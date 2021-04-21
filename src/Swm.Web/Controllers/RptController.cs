using Arctic.AspNetCore;
using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;
using Serilog;
using Swm.Materials;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Swm.Web.Controllers.DashboardData;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 提供报表 api
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RptController : ControllerBase
    {
        readonly ISession _session;
        readonly ILogger _logger;

        /// <summary>
        /// 初始化新实例。
        /// </summary>
        /// <param name="session"></param>
        /// <param name="logger"></param>
        public RptController(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger;
        }

        /// <summary>
        /// 实时库存
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet("get-inventory-report")]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.实时库存)]
        public async Task<ListData<InventoryReprotItemInfo>> GetInventoryReport([FromQuery] InventoryReportArgs args)
        {
            var pagedList = await _session.Query<Stock>().Where(x => x.Quantity != 0).SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return this.ListData(pagedList, x => new InventoryReprotItemInfo
            {
                StockId = x.StockId,
                mtime = x.mtime,
                MaterialCode = x.Material.MaterialCode,
                Description = x.Material.Description,
                Batch = x.Batch,
                StockStatus = x.StockStatus,
                Quantity = x.Quantity,
                Uom = x.Uom,
                Fifo = x.Fifo,
                AgeBaseline = x.AgeBaseline,
            });
        }




        /// <summary>
        /// 获取月报列表
        /// </summary>
        /// <param name="month">表示月份的时间，例如 2021-01</param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-monthly-report/{month}")]
        public async Task<ListData<MonthlyReportItemInfo>> GetMonthlyReport(DateTime month)
        {
            month = month.AddDays(1 - month.Day).Date; // 取月初 0 点
            MonthlyReport? monthlyReport = await _session.GetAsync<MonthlyReport>(month);
            PagedList<MonthlyReportItemInfo> pagedList;
            if (monthlyReport == null)
            {
                pagedList = new PagedList<MonthlyReportItemInfo>(new List<MonthlyReportItemInfo>(), 1, 1, 0);
            }
            else
            {
                pagedList = new PagedList<MonthlyReportItemInfo>(
                       monthlyReport.Items.Select(x => new MonthlyReportItemInfo
                       {
                           Month = monthlyReport.Month,
                           MaterialCode = x.Material?.MaterialCode,
                           Description = x.Material?.Description,
                           Batch = x.Batch,
                           StockStatus = x.StockStatus,
                           Uom = x.Uom,
                           Beginning = x.Beginning,
                           Ending = x.Ending,
                           Incoming = x.Incoming,
                           Outgoing = x.Outgoing,
                       }).ToList(), 1, monthlyReport.Items.Count, monthlyReport.Items.Count);
            }

            return this.ListData(pagedList);
        }


        /// <summary>
        /// 生成月报
        /// </summary>
        /// <returns></returns>
        [HttpPost("build-monthly-report")]
        [AutoTransaction]
        public async Task<ApiData> BuildMonthlyReport()
        {
            // 每次生成一个月的月报

            _logger.Information("正在生成月报");

            var initDate = await _session.Query<Flow>().MinAsync(x => (DateTime?)x.ctime);
            if (initDate == null)
            {
                const string msg = "尚未产生流水";
                _logger.Debug(msg);
                return this.Success();
            }

            initDate = initDate.Value.Date;
            var initMonth = new DateTime(initDate.Value.Year, initDate.Value.Month, 1);
            var thisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var nextMonth = thisMonth.Date.AddMonths(1);

            var prev = await _session.Query<MonthlyReport>()
                .OrderByDescending(x => x.Month)
                .FirstOrDefaultAsync();
            if (prev == null)
            {
                prev = new MonthlyReport
                {
                    Month = initMonth.AddMonths(-1)
                };
            }

            if (prev.Month.AddMonths(1) >= thisMonth)
            {
                const string msg = "当月的月报要到下个月才生成";
                _logger.Debug(msg);
                return this.Success();
            }

            // 顺着已经生成的月报向后生成

            // 这里采用 sql，
            // 等价的 linq 和 hql 写法在运行时会报错

            string sql =
@"SELECT MaterialId, Batch, StockStatus, Uom, 
    SUM (CASE WHEN Direction = 1 THEN Quantity ELSE 0 END) Incoming,
    SUM (CASE WHEN Direction = -1 THEN Quantity ELSE 0 END) Outgoing
FROM Flows
WHERE ctime >= :start
AND ctime < :end
GROUP BY MaterialId, Batch, StockStatus, Uom";
            var rows = await _session.CreateSQLQuery(sql)
                .SetParameter("start", prev.Month.AddMonths(1))  // 左闭合区间
                .SetParameter("end", prev.Month.AddMonths(2))
                .SetResultTransformer(NHibernate.Transform.Transformers.AliasToEntityMap)
                .ListAsync<Hashtable>();

            var list = rows.Select(x => new MonthlyReportItem
            {
                Material = _session.Load<Material>(x["MaterialId"]),
                Batch = (string)x["Batch"]!,
                StockStatus = (string)x["StockStatus"]!,
                Uom = (string)x["Uom"]!,
                Beginning = 0, // 期初数量稍后计算
                Incoming = (decimal)x["Incoming"]!,
                Outgoing = (decimal)x["Outgoing"]!,
            }).ToList();

            // 合并上月条目，并计算期初数量
            foreach (var prevItem in prev.Items)
            {
                if (Appears<DefaultStockKey>(prevItem, list, out MonthlyReportItem? found) == false)
                {
                    // 上个月的条目，在本月未出现，也要添加到本月
                    if (prevItem.Ending != 0)
                    {
                        list.Add(new MonthlyReportItem
                        {
                            Material = prevItem.Material,
                            Batch = prevItem.Batch,
                            StockStatus = prevItem.StockStatus,
                            Uom = prevItem.Uom,
                            Beginning = prevItem.Ending,
                            Incoming = 0,
                            Outgoing = 0,
                        });
                    }
                }
                else
                {
                    found!.Beginning = prevItem.Ending;
                }
            }

            MonthlyReport report = new MonthlyReport
            {
                Month = prev.Month.AddMonths(1)
            };
            foreach (var entry in list)
            {
                report.Items.Add(entry);
                entry.MonthlyReport = report;
            }

            await _session.SaveAsync(report);
            _logger.Information("已生成 {month} 的月报", report.Month.ToString("yyyy-MM}"));
            return this.Success();

            bool Appears<TStockKey>(MonthlyReportItem entry, IEnumerable<MonthlyReportItem> entries, out MonthlyReportItem? found) where TStockKey : StockKeyBase
            {
                found = entries.FirstOrDefault(x => x.GetStockKey<TStockKey>() == entry.GetStockKey<TStockKey>());
                return found != null;
            }
        }

        /// <summary>
        /// 库龄报表
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-age-report")]
        [AutoTransaction]
        public async Task<ListData<AgeReportItemInfo>> GetAgeReport()
        {
            const string ageCase = @"
CASE 
    WHEN DATEDIFF(MINUTE, i.ProductionTime, GETDATE()) / 1440.0 < 7 THEN '7'
    WHEN DATEDIFF(MINUTE, i.ProductionTime, GETDATE()) / 1440.0 < 30 THEN '30'
    WHEN DATEDIFF(MINUTE, i.ProductionTime, GETDATE()) / 1440.0 < 90 THEN '90'
    ELSE '90+'
END";
            string hql = @$"
SELECT 
    m.MaterialCode AS MaterialCode, 
    m.Description AS Description, 
    m.Specification AS Specification, 
    i.Batch AS Batch, 
    i.StockStatus AS StockStatus, 
    i.Uom AS Uom, 
    SUM(i.Quantity) AS Quantity, 
    {ageCase} AS Age
FROM UnitloadItem i
JOIN i.Material m
GROUP BY m.MaterialCode, m.Description, m.Specification, i.Batch, i.StockStatus, i.Uom, {ageCase}    
";

            var rows = await _session.CreateQuery(hql).SetResultTransformer(Transformers.AliasToEntityMap).ListAsync<Hashtable>();
            List<AgeReportItemInfo> ages = new List<AgeReportItemInfo>();
            foreach (var row in rows)
            {
                var item = new
                {
                    MaterialCode = Convert.ToString(row["MaterialCode"]),
                    Description = Convert.ToString(row["Description"]),
                    Specification = Convert.ToString(row["Specification"]),
                    Batch = Convert.ToString(row["Batch"]),
                    StockStatus = Convert.ToString(row["StockStatus"]),
                    Uom = Convert.ToString(row["Uom"]),
                    Quantity = Convert.ToDecimal(row["Quantity"]),
                    Age = Convert.ToString(row["Age"]),
                };
                var age = ages.SingleOrDefault(x =>
                    x.MaterialCode == item.MaterialCode
                    && x.Batch == item.Batch
                    && x.StockStatus == item.StockStatus
                    && x.Uom == item.Uom
                );

                if (age == null)
                {
                    age = new AgeReportItemInfo
                    {
                        MaterialCode = item.MaterialCode!,
                        Description = item.Description!,
                        Specification = item.Specification!,
                        Batch = item.Batch!,
                        StockStatus = item.StockStatus!,
                        Uom = item.Uom!,
                    };
                    ages.Add(age);
                }

                switch (item.Age)
                {
                    case "7":
                        age.ZeroToSevenDays = item.Quantity;
                        break;
                    case "30":
                        age.SevenToThirtyDays = item.Quantity;
                        break;
                    case "90":
                        age.ThirtyToNinetyDays = item.Quantity;
                        break;
                    case "90+":
                        age.MoreThanNinetyDays = item.Quantity;
                        break;
                    default:
                        break;
                }
            }

            return new ListData<AgeReportItemInfo>
            {
                Success = true,
                Data = ages,
                Total = ages.Count,
            };
        }

        // TODO 使用真实数据替换假数据
        // TODO 使用缓存提升性能
        /// <summary>
        /// 获取仪表盘数据
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-dashboard-data")]
        public async Task<ApiData<DashboardData>> GetDashboardData()
        {
            Random random = new Random();
            DashboardData dashboardData = new DashboardData
            {
                Location = new LocationData
                {
                    StreetletCount = 5,
                    AvailableLocationCount = 392,
                    DisabledLocationCount = 29,
                    LocationCount = 7349,
                    LocationUsageRate = 0.235f,
                    LocationUsageRate7 = Enumerable.Range(-7, 7)
                        .Select(x => new DateValuePair<double>()
                        {
                            Date = DateTime.Now.Date.AddDays(x),
                            Value = random.NextDouble()
                        })
                        .ToList(),
                },
                InboundOrder = new InboundOrderData
                {
                    InboundOrderCount7 = Enumerable.Range(-7, 7)
                        .Select(x => new DateValuePair<int>
                        {
                            Date = DateTime.Now.Date.AddDays(x),
                            Value = random.Next(0, 100)
                        })
                        .ToList(),
                    OpenInboundOrderCount = 12,
                },
                OutboundOrder = new OutboundOrderData
                {
                    OpenOutboundOrderCount = 8,
                    OutboundOrderCount7 = Enumerable.Range(-7, 7)
                        .Select(x => new DateValuePair<int>
                        {
                            Date = DateTime.Now.Date.AddDays(x),
                            Value = random.Next(0, 100)
                        })
                        .ToList(),
                },
                Stock = new StockData
                {
                    UnitloadCount = 1499,
                    EmptyPalletCount = 362,
                    FlowInCount7 = Enumerable.Range(-7, 7)
                        .Select(x => new DateValuePair<int>
                        {
                            Date = DateTime.Now.Date.AddDays(x),
                            Value = random.Next(0, 3000)
                        })
                        .ToList(),
                    FlowOutCount7 = Enumerable.Range(-7, 7)
                        .Select(x => new DateValuePair<int>
                        {
                            Date = DateTime.Now.Date.AddDays(x),
                            Value = random.Next(0, 3000)
                        })
                        .ToList(),
                },
                Task = new TaskData
                {
                    TaskCount = 29,
                    TaskCount7 = Enumerable.Range(-7, 7)
                        .Select(x => new DateValuePair<int>
                        {
                            Date = DateTime.Now.Date.AddDays(x),
                            Value = random.Next(1000, 4000)
                        }).ToList(),
                },
            };

            await Task.CompletedTask;
            return this.Success(dashboardData);


            /*

            DashboardData dashboardData = new DashboardData();
            dashboardData.Location.StreetletCount = await _session.Query<Streetlet>().CountAsync();
            dashboardData.Location.LocationCount = await _session.Query<Location>()
                .Where(x => x.LocationType == LocationTypes.S
                    && x.Exists)
                .CountAsync();
            dashboardData.Location.AvailableLocationCount = await _session.Query<Location>()
                .Where(x => x.LocationType == LocationTypes.S
                    && x.Exists
                    && x.InboundDisabled == false)
                .CountAsync();
            dashboardData.Location.DisabledLocationCount = await _session.Query<Location>()
                .Where(x => x.LocationType == LocationTypes.S
                    && x.Exists
                    && (x.InboundDisabled || x.OutboundDisabled))
                .CountAsync();

             
             
             */
        }


    }

}
