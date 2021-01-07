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
using System.ComponentModel.DataAnnotations;

namespace Swm.Model
{
    /// <summary>
    /// 表示单元货物明细。
    /// </summary>
    public class UnitloadItem : IHasStockKey
    {
        internal protected UnitloadItem()
        {
            this.Uom = Cst.None;
        }

        public virtual Int32 UnitloadItemId { get; set; }

        [Required]
        public virtual Unitload Unitload { get; internal protected set; }


        [Required]
        public virtual Material Material { get; set; }

        /// <summary>
        /// TODO 重命名为 Batch 或者 Lot，前端需一同更新
        /// </summary>
        [Required]
        [MaxLength(FIELD_LENGTH.BATCH)]
        public virtual string Batch { get; set; }

        [Required]
        [MaxLength(FIELD_LENGTH.APP_CODE)]
        public virtual string StockStatus { get; set; }

        public virtual decimal Quantity { get; set; }

        [Required]
        [MaxLength(FIELD_LENGTH.UOM)]
        public virtual string Uom { get; set; }

        public virtual DateTime ProductionTime { get; set; }

        [MaxLength(20)]
        [Required]
        public virtual string OutOrdering { get; set; }


        public override string ToString()
        {
            return $"{this.Unitload?.PalletCode}#{this.UnitloadItemId}";
        }
    }


}
