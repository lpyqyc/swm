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

using Swm.Model;
using System;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 出库单明细
    /// </summary>
    public class OutboundLineInfo
    {
        /// <summary>
        /// 出库单明细Id。
        /// </summary>
        public int OutboundLineId { get; set; }

        /// <summary>
        /// 物料Id
        /// </summary>
        public int MaterialId { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; } = default!;

        /// <summary>
        /// 物料类型
        /// </summary>
        public string MaterialType { get; set; } = default!;

        /// <summary>
        /// 物料描述
        /// </summary>
        public string Description { get; set; } = default!;

        /// <summary>
        /// 物料规格
        /// </summary>
        public string Specification { get; set; } = default!;


        /// <summary>
        /// 要出库的批号，可以为空
        /// </summary>
        public string? Batch { get; set; }


        /// <summary>
        /// 要出库的库存状态。
        /// </summary>
        public string StockStatus { get; set; }

        /// <summary>
        /// 计量单位。
        /// </summary>
        public string Uom { get; set; } = Cst.None;

        /// <summary>
        /// 需求数量。
        /// </summary>
        public decimal QuantityRequired { get; set; }

        /// <summary>
        /// 已出数量
        /// </summary>
        public decimal QuantityDelivered { get; set; }

        /// <summary>
        /// 未出数量，MAX(应出-已出, 0)
        /// </summary>
        public decimal QuantityUndelivered { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Comment { get; set; }
    }

}

