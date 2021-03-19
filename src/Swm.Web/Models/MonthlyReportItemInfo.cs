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

using System;
using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 库存月报的数据项
    /// </summary>
    public class MonthlyReportItemInfo
    {
        /// <summary>
        /// 月份
        /// </summary>
        public DateTime Month { get; set; }

        /// <summary>
        /// 物料代码
        /// </summary>
        [Required]
        public string? MaterialCode { get; set; }

        /// <summary>
        /// 物料描述
        /// </summary>
        [Required]
        public string? Description { get; set; }


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
        /// 计量单位
        /// </summary>
        [Required]
        public string Uom { get; set; } = default!;

        /// <summary>
        /// 期初数量。期初数量 = 上期期末数量。
        /// </summary>
        public decimal Beginning { get; set; }

        /// <summary>
        /// 流入数量。
        /// </summary>
        public decimal Incoming { get; set; }

        /// <summary>
        /// 流出数量。
        /// </summary>
        public decimal Outgoing { get; set; }

        /// <summary>
        /// 期末数量。期末数量 = 期初数量 + 流入数量 - 流出数量。
        /// </summary>
        public decimal Ending { get; set; }



    }


}