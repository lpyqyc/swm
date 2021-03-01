// Copyright 2020-2021 王建军
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

using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        readonly LogDbContext _context;

        public LogsController(LogDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 日志列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet("get-log-list")]
        public async Task<ListData<LogEntry>> GetLogList([FromQuery] LogListArgs args)
        {
            var pagedList = await SearchLogsAsync(_context.Logs.AsNoTracking(), args.Filter, args.Sort, args.Current, args.PageSize);

            foreach (var item in pagedList.List)
            {
                item.Time = DateTime.SpecifyKind(item.Time, DateTimeKind.Local);
            }

            return this.ListData(pagedList);
        }

        /// <summary>
        /// 跟踪日志
        /// </summary>
        /// <param name="args">跟踪参数</param>
        /// <returns></returns>
        [HttpGet("trace-log")]
        public async Task<ListData<LogEntry>> TraceLog([FromQuery] LogTraceArgs args)
        {
            var pagedList = await SearchLogsAsync(_context.Logs.AsNoTracking(), args.Filter, args.Sort, args.Current, args.PageSize);

            foreach (var item in pagedList.List)
            {
                item.Time = DateTime.SpecifyKind(item.Time, DateTimeKind.Local);
            }

            return this.ListData(pagedList);
        }

        private static async Task<PagedList<LogEntry>> SearchLogsAsync(IQueryable<LogEntry> q, Func<IQueryable<LogEntry>, IQueryable<LogEntry>> filter, string? sort, int? current, int? pageSize)
        {
            if (current == null || current.Value < 1)
            {
                current = 1;
            }
            if (pageSize == null || pageSize.Value < 1)
            {
                pageSize = 20;
            }
            q = filter(q);

            if (!string.IsNullOrWhiteSpace(sort))
            {
                q = q.OrderBy(sort);
            }

            var totalItemCount = q.Count();

            if (totalItemCount == 0)
            {
                return new PagedList<LogEntry>(new List<LogEntry>(), 1, pageSize.Value, 0);
            }

            int start = (current.Value - 1) * pageSize.Value;
            var list = await q.Skip(start)
                .Take(pageSize.Value)
                .ToListAsync()
                .ConfigureAwait(false);
            return new PagedList<LogEntry>(list, 1, pageSize.Value, totalItemCount);

        }
    }
}
