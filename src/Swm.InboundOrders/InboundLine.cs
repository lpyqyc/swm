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

using Swm.Constants;
using Swm.Materials;
using System;
using System.ComponentModel.DataAnnotations;

namespace Swm.InboundOrders
{
    /// <summary>
    /// 表示入库单中的一行。
    /// </summary>
    public class InboundLine : IHasStockKey
    {
        /// <summary>
        /// 初始化此类的新实例。
        /// </summary>
        public InboundLine()
        {
        }

        /// <summary>
        /// 主键
        /// </summary>
        public virtual int InboundLineId { get; protected set; }


        /// <summary>
        /// 获取或设置此行所属的入库单。
        /// </summary>
        [Required]
        public virtual InboundOrder InboundOrder { get; internal protected set; } = default!;

        /// <summary>
        /// 收货物料。
        /// </summary>
        [Required]
        public virtual Material Material { get; set; } = default!;

        /// <summary>
        /// 收货批号。
        /// </summary>
        [Required]
        [MaxLength(FIELD_LENGTH.BATCH)]
        public virtual string Batch { get; set; } = default!;

        [Required]
        [MaxLength(FIELD_LENGTH.STOCK_STATUS)]
        public virtual string StockStatus { get; set; } = default!;

        /// <summary>
        /// 计量单位
        /// </summary>
        [Required]
        [MaxLength(FIELD_LENGTH.UOM)]
        public virtual string Uom { get; set; } = default!;

        /// <summary>
        /// 应收数。
        /// </summary>
        public virtual decimal QuantityExpected { get; set; }

        /// <summary>
        /// 实收数。
        /// </summary>
        public virtual decimal QuantityReceived { get; set; }


        // TODO 在收货时维护此属性
        /// <summary>
        /// 指示此明细行是否发生过入库操作。发生过入库操作的入库明细不能被删除。
        /// 在第一次拣货时，此属性变为 true，此后不会变回 false。
        /// </summary>
        public virtual bool Dirty { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public virtual string? Comment { get; set; }

    }

}