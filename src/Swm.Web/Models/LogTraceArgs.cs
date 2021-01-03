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

using System;
using System.Collections.Specialized;
using System.Linq;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 日志跟踪参数
    /// </summary>
    public class LogTraceArgs
    {
        /// <summary>
        /// 查询开始时间
        /// </summary>
        public DateTime TimeFrom { get; set; }

        /// <summary>
        /// 取多少秒
        /// </summary>
        public int? Seconds { get; set; }

        /// <summary>
        /// 计算得到的查询结束时间
        /// </summary>
        internal DateTime? TimeTo
        {
            get
            {
                if (this.Seconds.HasValue)
                {
                    return this.TimeFrom.AddSeconds(Seconds.Value);
                }
                return null;
            }
        }

        /// <summary>
        /// 请求Id
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// 要查询的关键字
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 在查询对象上应用筛选条件
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public IQueryable<LogEntry> Filter(IQueryable<LogEntry> q)
        {
            RequestId = trim(RequestId);
            Keyword = trim(Keyword);

            if (RequestId != null)
            {
                q = q.Where(x => x.RequestId == RequestId);
            }

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                q = q.Where(x => x.Message.Contains(Keyword));
            }

            q = q.Where(x => x.Time >= TimeFrom);

            if (TimeTo != null)
            {
                q = q.Where(x => x.Time <= TimeTo);
            }

            return q;

            static string? trim(string? str)
            {
                str = str?.Trim();
                if (string.IsNullOrEmpty(str))
                {
                    str = null;
                }
                return str;
            }
        }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string? Sort { get; set; }

        /// <summary>
        /// 基于 1 的当前页面，默认值为 1。
        /// </summary>
        public int? Current { get; set; } = 1;

        /// <summary>
        /// 每页大小，默认值为 10。
        /// </summary>
        public int? PageSize { get; set; }

    }


}
