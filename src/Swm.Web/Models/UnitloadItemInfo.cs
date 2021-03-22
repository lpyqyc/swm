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
using System.Collections.Generic;
using System.Linq;

namespace Swm.Web.Controllers
{
    // TODO 多处引用，写转换函数
    /// <summary>
    /// 货载明细信息
    /// </summary>
    public class UnitloadItemInfo
    {
        /// <summary>
        /// 货载项Id
        /// </summary>
        public int UnitloadItemId { get; set; }

        /// <summary>
        /// 物料Id
        /// </summary>
        public int MaterialId { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string? MaterialCode { get; set; }

        /// <summary>
        /// 物料类型
        /// </summary>
        public string? MaterialType { get; set; }

        /// <summary>
        /// 物料描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 物料规格
        /// </summary>
        public string? Specification { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string? Batch { get; set; }

        /// <summary>
        /// 库存状态
        /// </summary>
        public string? StockStatus { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// 已分配给出库单的数量
        /// </summary>
        public decimal QuantityAllocatedToOutboundOrder
        {
            get
            {
                return this.AllocationsToOutboundOrder == null || this.AllocationsToOutboundOrder.Length == 0 
                    ? 0m 
                    : this.AllocationsToOutboundOrder.Sum(x => x.QuantityAllocated);
            }
        }

        /// <summary>
        /// 分配给出库单行的数量明细，字典的键表示出库单明细的Id，字典的值表示分配的数量
        /// </summary>
        public AllocationInfoToOutboundOrder[]? AllocationsToOutboundOrder { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        public string? Uom { get; set; }


        /// <summary>
        /// 分配信息
        /// </summary>
        public class AllocationInfoToOutboundOrder
        {
            /// <summary>
            /// 分配信息Id
            /// </summary>
            public int UnitloadItemAllocationId { get; set; }

            /// <summary>
            /// 出库单明细Id
            /// </summary>
            public int OutboundLineId { get; set; }

            /// <summary>
            /// 分配数量
            /// </summary>
            public decimal QuantityAllocated { get; set; }
        }
    }


}
