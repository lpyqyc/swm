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

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 货载项列表的查询参数
    /// </summary>
    public class UnitloadItemListArgs
    {
        /// <summary>
        /// 托盘号
        /// </summary>
        [SearchArg(SearchMode.Like)]
        public string? PalletCode { get; set; }

        /// <summary>
        /// 物料类型
        /// </summary>
        [SearchArg]
        public string? MaterialType { get; set; }


        /// <summary>
        /// 物料编码
        /// </summary>
        [SearchArg]
        [SourceProperty("Material.MaterialCode")]
        public string? MaterialCode { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        [SearchArg]
        public string? Batch { get; set; }

        /// <summary>
        /// 库存状态
        /// </summary>
        [SearchArg]
        public string? StockStatus { get; set; }


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
