// Copyright 2020 王建军
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Arctic.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using Serilog;
using Swm.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    [ApiController]
    [Route("monthly-reports")]
    public class MonthlyReportsController : ControllerBase
    {
        readonly ILogger _logger;
        readonly ISession _session;

        public MonthlyReportsController(ISession session, ILogger logger)
        {
            _logger = logger;
            _session = session;
        }

        /// <summary>
        /// 获取月报列表
        /// </summary>
        /// <param name="month">表示月份的时间，例如 2021-01-12</param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("{month}")]
        public async Task<ActionResult<MonthlyReportDetail>> Detail(DateTime month)
        {
            month = month.AddDays(1 - month.Day).Date; // 取月初 0 点
            MonthlyReport? monthlyReport = await _session.GetAsync<MonthlyReport>(month);
            if (monthlyReport == null)
            {
                return NotFound();
            }

            return new MonthlyReportDetail
            {
                Month = monthlyReport.Month,
                Items = monthlyReport.Items.Select(x => new MonthlyReportItemInfo
                {
                    MaterialCode = x.Material.MaterialCode,
                    Description = x.Material.Description,
                    Batch = x.Batch,
                    StockStatus = x.StockStatus,
                    Uom = x.Uom,
                    Beginning = x.Beginning,
                    Ending = x.Ending,
                    Incoming = x.Incoming,
                    Outgoing = x.Outgoing,
                }).ToArray()
            };
        }


        /// <summary>
        /// 生成月报
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AutoTransaction]
        public async Task<ActionResult> Build()
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
    }
}
