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

using Arctic.NHibernateExtensions;
using Swm.Model;
using System;
using System.Linq.Expressions;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 关键点列表的查询参数
    /// </summary>
    public class KeyPointListArgs
    {
        /// <summary>
        /// 货位类型，始终是 <see cref="LocationTypes.K"/>
        /// </summary>
        [SearchArg]
        public string LocationType { get; } = LocationTypes.K;

        /// <summary>
        /// 支持模糊查找，使用 ? 表示单个字符，使用 * 表示任意个字符
        /// </summary>
        [SearchArg(SearchMode.Like)]
        public string? LocationCode { get; set; }


        /// <summary>
        /// 支持模糊查找，使用 ? 表示单个字符，使用 * 表示任意个字符
        /// </summary>
        [SearchArg(SearchMode.Like)]
        public string? Tag { get; set; }

        /// <summary>
        /// 支持模糊查找，使用 ? 表示单个字符，使用 * 表示任意个字符
        /// </summary>
        [SearchArg(SearchMode.Like)]
        public string? RequestType { get; set; }



        /// <summary>
        /// 是否禁止入站
        /// </summary>
        [SearchArg]
        public bool? InboundDisabled { get; set; }

        /// <summary>
        /// 是否禁止出站
        /// </summary>
        [SearchArg]
        public bool? OutboundDisabled { get; set; }

        /// <summary>
        /// 是否有入站任务
        /// </summary>
        [SearchArg(SearchMode.Expression)]
        public bool? HasInboundMoves { get; set; }

        internal Expression<Func<Location, bool>>? HasInboundMovesExpr
        {
            get
            {
                return HasInboundMoves switch
                {
                    true => x => x.InboundCount > 0,
                    false => x => x.InboundCount == 0,
                    null => null,
                };
            }
        }

        /// <summary>
        /// 是否有出站任务
        /// </summary>
        [SearchArg(SearchMode.Expression)]
        public bool? HasOutboundMoves { get; set; }

        internal Expression<Func<Location, bool>>? HasOutboundMovesExpr
        {
            get
            {
                return HasOutboundMoves switch
                {
                    true => x => x.OutboundCount > 0,
                    false => x => x.OutboundCount == 0,
                    null => null,
                };
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
