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
using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 货载明细列表的查询参数
    /// </summary>
    public class GetUnitloadItemsToChangeStockStatusArgs
    {
        /// <summary>
        /// 业务类型
        /// </summary>
        [Required]
        public string? BizType { get; set; }



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

        // TODO 重复的代码
        /// <summary>
        /// 库存状态
        /// </summary>
        [SearchArg]
        public string? StockStatus => BizType switch
        {
            "待检转合格" => "待检",
            "待检转不合格" => "待检",
            "不合格转合格" => "不合格",
            "合格转不合格" => "合格",
            _ => throw new(),
        };


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
