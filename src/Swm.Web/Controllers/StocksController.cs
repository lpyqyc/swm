using Arctic.AppCodes;
using Arctic.AspNetCore;
using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;
using Serilog;
using Swm.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        readonly ISession _session;
        readonly ILogger _logger;

        public StocksController(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger;
        }

        /// <summary>
        /// 实时库存
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.实时库存)]
        public async Task<ListResult<StockListItem>> List([FromQuery] StockListArgs args)
        {
            var pagedList = await _session.Query<Stock>().Where(x => x.Quantity != 0).SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return new ListResult<StockListItem>
            {
                Success = true,
                Data = pagedList.List.Select(x => new StockListItem
                {
                    StockId = x.StockId,
                    mtime = x.mtime,
                    MaterialCode = x.Material.MaterialCode,
                    Description = x.Material.Description,
                    Batch = x.Batch,
                    StockStatus = x.StockStatus,
                    Quantity = x.Quantity,
                    Uom = x.Uom,
                    OutOrdering = x.OutOrdering,
                    AgeBaseline = x.AgeBaseline,
                }),
                Total = pagedList.Total
            };
        }

        /// <summary>
        /// 获取库存状态的选择列表
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet]
        [Route("stock-status-select-list")]
        public async Task<List<StockStatusSelectListItem>> StockStatusSelectList()
        {
            var list = await _session.Query<AppCode>().GetAppCodesAsync(AppCodeTypes.StockStatus);

            var items = list.Select(x => new StockStatusSelectListItem
            {
                StockStatus = x.AppCodeValue,
                Description = x.Description,
                Scope = x.Scope,
                DisplayOrder = x.DisplayOrder,
            }).ToList();

            return items;
        }


        /// <summary>
        /// 获取月报列表
        /// </summary>
        /// <param name="month">表示月份的时间，例如 2021-01</param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("monthly-reports/{month}")]
        public async Task<ActionResult<ListResult<MonthlyReportItemInfo>>> GetMonthlyReport(DateTime month)
        {
            month = month.AddDays(1 - month.Day).Date; // 取月初 0 点
            MonthlyReport? monthlyReport = await _session.GetAsync<MonthlyReport>(month);
            if (monthlyReport == null)
            {
                return NotFound();
            }

            return new ListResult<MonthlyReportItemInfo>
            {
                Success = true,
                Data = monthlyReport.Items.Select(x => new MonthlyReportItemInfo
                {
                    Month = monthlyReport.Month,
                    MaterialCode = x.Material.MaterialCode,
                    Description = x.Material.Description,
                    Batch = x.Batch,
                    StockStatus = x.StockStatus,
                    Uom = x.Uom,
                    Beginning = x.Beginning,
                    Ending = x.Ending,
                    Incoming = x.Incoming,
                    Outgoing = x.Outgoing,
                }).ToArray(),
                Total = monthlyReport.Items.Count
            };
        }


        /// <summary>
        /// 生成月报
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AutoTransaction]
        public async Task<ActionResult> BuildMonthlyReport()
        {
            // 每次生成一个月的月报

            _logger.Information("正在生成月报");

            var initDate = await _session.Query<Flow>().MinAsync(x => (DateTime?)x.ctime);
            if (initDate == null)
            {
                const string msg = "尚未产生流水";
                _logger.Debug(msg);
                return this.Success(msg);
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
                prev = new MonthlyReport();
                prev.Month = initMonth.AddMonths(-1);
            }

            if (prev.Month.AddMonths(1) >= thisMonth)
            {
                const string msg = "当月的月报要到下个月才生成";
                _logger.Debug(msg);
                return this.Success(msg);
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

            MonthlyReport report = new MonthlyReport();
            report.Month = prev.Month.AddMonths(1);
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
        [HttpGet("age-report")]
        [AutoTransaction]
        public async Task<ListResult<AgeReportListItem>> GetAges()
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
            List<AgeReportListItem> ages = new List<AgeReportListItem>();
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
                    age = new AgeReportListItem
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

            return new ListResult<AgeReportListItem>
            {
                Success = true,
                Data = ages,
                Total = ages.Count,
            };
        }


    }

}
