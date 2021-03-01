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
using Swm.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 列表查询参数
    /// </summary>
    public class InboundOrderListArgs
    {
        /// <summary>
        /// 支持模糊查找，使用 ? 表示单个字符，使用 * 表示任意个字符
        /// </summary>
        [SearchArg(SearchMode.Like)]
        public string? InboundOrderCode { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        [SearchArg(SearchMode.Like)]
        public string? BizType { get; set; }

        // TODO 改为 Expression 方式
        /// <summary>
        /// 是否显示已关闭的出库单
        /// </summary>
        public bool? ShowClosed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public IQueryable<InboundOrder> Filter(IQueryable<InboundOrder> q)
        {
            if (ShowClosed != true)
            {
                q = q.Where(x => x.Closed == false);
            }
            return q;
        }

        /// <summary>
        /// 排序字段，例如 F1 DESC, F2 ASC, F3
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

