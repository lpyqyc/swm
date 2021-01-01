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
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 货载列表的查询参数
    /// </summary>
    public class UnitloadListArgs
    {
        /// <summary>
        /// 托盘号，支持模糊查找
        /// </summary>
        [SearchArg(SearchMode.Like)]
        public string? PalletCode { get; set; }

        /// <summary>
        /// 物料类型
        /// </summary>
        [SearchArg(SearchMode.Expression)]
        public string? MaterialType { get; set; }

        internal Expression<Func<Unitload, bool>>? MaterialTypeExpr
        {
            get
            {
                return this.MaterialType switch
                {
                    null => null,
                    _ => x => x.Items.Any(i => i.Material.MaterialType == this.MaterialType)
                };
                return null;
            }
        }

        /// <summary>
        /// 物料编码，支持模糊查找
        /// </summary>
        [SearchArg(SearchMode.Expression)]
        public string? MaterialCode { get; set; }

        internal Expression<Func<Unitload, bool>>? MaterialCodeExpr
        {
            get
            {
                return this.MaterialCode switch
                {
                    null => null,
                    _ => x => x.Items.Select(i => i.Material)
                        .Any(m => SqlMethods.Like(m.MaterialCode, this.MaterialCode))
                };
            }
        }

        /// <summary>
        /// 批号，支持模糊查找
        /// </summary>
        [SearchArg(SearchMode.Like)]
        public string? Batch { get; set; }

        internal Expression<Func<Unitload, bool>>? BatchExpr
        {
            get
            {
                return this.Batch switch
                {
                    null => null,
                    _ => x => x.Items.Any(i => SqlMethods.Like(i.Batch, Batch))
                };
            }
        }

        /// <summary>
        /// 库存状态
        /// </summary>
        [SearchArg]
        public string? StockStatus { get; set; }

        /// <summary>
        /// 托盘所在巷道
        /// </summary>
        [SearchArg]
        public int? LanewayId { get; set; }

        /// <summary>
        /// 托盘所在货位号，支持模糊查找
        /// </summary>
        [SearchArg(SearchMode.Like)]
        [SourceProperty("CurrentLocation.LocationCode")]
        public string? LocationCode { get; set; }

        /// <summary>
        /// 托盘是否有任务
        /// </summary>
        [SearchArg]
        public bool? BeingMoved { get; set; }

        /// <summary>
        /// 托盘的操作提示
        /// </summary>
        [SearchArg]
        public string? OpHintType { get; set; }

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
