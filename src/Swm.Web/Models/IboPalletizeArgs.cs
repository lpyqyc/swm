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

using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 无单据组盘的操作参数。
    /// </summary>
    public class IboPalletizeArgs
    {
        /// <summary>
        /// 托盘号
        /// </summary>
        [Required]
        public string PalletCode { get; set; } = default!;

        /// <summary>
        /// 入库单号
        /// </summary>
        public string InboundOrderCode { get; set; } = default!;

        /// <summary>
        /// 物料编码
        /// </summary>
        [Required]
        public string MaterialCode { get; set; } = default!;

        /// <summary>
        /// 批号
        /// </summary>
        [Required]
        public string Batch { get; set; } = default!;

        /// <summary>
        /// 库存状态
        /// </summary>
        [Required]
        public string StockStatus { get; set; } = default!;

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        [Required]
        public string Uom { get; set; } = default!;
    }


}

