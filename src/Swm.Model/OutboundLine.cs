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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Swm.Model
{
    /// <summary>
    /// 表示出库单明细。
    /// </summary>
    public class OutboundLine
    {
        public OutboundLine()
        {
            this.Uom = Cst.None;
        }

        /// <summary>
        /// 出库单明细Id。
        /// </summary>
        public virtual int OutboundLineId { get; set; }

        /// <summary>
        /// 所属出库单。
        /// </summary>
        [Required]
        public virtual OutboundOrder OutboundOrder { get; internal protected set; }

        /// <summary>
        /// 要出库的物料。
        /// </summary>
        [Required]
        public virtual Material Material { get; set; }


        /// <summary>
        /// 要出库的批号，可以为空
        /// </summary>
        [MaxLength(FIELD_LENGTH.BATCH)]
        public virtual string? Batch { get; set; }


        /// <summary>
        /// 要出库的库存状态。
        /// </summary>
        [MaxLength(FIELD_LENGTH.STOCK_STATUS)]
        public virtual string StockStatus { get; set; }

        /// <summary>
        /// 计量单位。
        /// </summary>
        [Required]
        [MaxLength(FIELD_LENGTH.UOM)]
        public virtual string Uom { get; set; } = Cst.None;

        /// <summary>
        /// 需求数量。
        /// </summary>
        public virtual decimal QuantityRequired { get; set; }

        /// <summary>
        /// 已出数量
        /// </summary>
        public virtual decimal QuantityDelivered { get; set; }


        /// <summary>
        /// 未出数量，MAX(应出-已出, 0)
        /// </summary>
        public virtual decimal QuantityUndelivered
        {
            get
            {
                return Math.Max(QuantityRequired - QuantityDelivered, 0);
            }
            protected set
            {
            }
        }

        /// <summary>
        /// 计算此出库单明细的分配欠数
        /// </summary>
        /// <returns></returns>
        public virtual decimal ComputeShortage()
        {
            return QuantityRequired - QuantityDelivered - Allocations.Select(x => x.Quantity).Sum();
        }


        // TODO 在拣货时维护此属性
        /// <summary>
        /// 指示此明细行是否发生过出库操作。发生过出库操作的出库明细不能被删除。
        /// 在第一次拣货时，此属性变为 true，此后不会变回 false。
        /// </summary>
        public virtual bool Dirty { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public virtual string Comment { get; set; }

        /// <summary>
        /// 获取此出库单明细当前的分配信息。此属性由分配程序使用，不要直接添加和移除元素。
        /// </summary>
        public virtual ISet<OutboundLineAllocation> Allocations { get; protected set; } = new HashSet<OutboundLineAllocation>();

        public override string ToString()
        {
            return $"{this.OutboundOrder?.OutboundOrderCode}#{this.OutboundLineId}";
        }
    }
}