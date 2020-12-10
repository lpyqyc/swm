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

using Swm.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 列表查询参数
    /// </summary>
    public class StorageLocationListArgs : IListArgs<Location>
    {
        /// <summary>
        /// 货位类型，始终是 <see cref="LocationTypes.S"/>
        /// </summary>
        [ListFilter]
        internal string LocationType { get; } = LocationTypes.S;

        /// <summary>
        /// 支持模糊查找，使用 ? 表示单个字符，使用 * 表示任意个字符
        /// </summary>
        [ListFilter(ListFilterOperator.Like)]
        public string? LocationCode { get; set; }

        /// <summary>
        /// 货位所在巷道
        /// </summary>
        [ListFilter(ListFilterOperator.IN, "Rack.Laneway.LanewayId")]
        public int[]? LanewayIdList { get; set; }

        /// <summary>
        /// 货位是否有货
        /// </summary>
        [ListFilter(ListFilterOperator.Linq)]
        public bool? Loaded { get; set; }

        internal Expression<Func<Location, bool>>? LoadedExpr
        {
            get
            {
                return Loaded switch
                {
                    true => x => x.UnitloadCount > 0,
                    false => x => x.UnitloadCount == 0,
                    null => null,
                };
            }
        }


        /// <summary>
        /// 支持模糊查找，使用 ? 表示单个字符，使用 * 表示任意个字符
        /// </summary>
        [ListFilter(op: ListFilterOperator.Like)]
        public string? StorageGroup { get; set; }

        /// <summary>
        /// 是否禁止入站
        /// </summary>
        [ListFilter]
        public bool? InboundDisabled { get; set; }

        /// <summary>
        /// 是否禁止出站
        /// </summary>
        [ListFilter]
        public bool? OutboundDisabled { get; set; }

        /// <summary>
        /// 是否有入站任务
        /// </summary>
        [ListFilter(ListFilterOperator.Linq)]
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
        public OrderedDictionary? Sort { get; set; }

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
