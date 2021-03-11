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

using Arctic.Auditing;
using Swm.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace Swm.Materials
{
    /// <summary>
    /// 表示库存记录。
    /// </summary>
    public class Stock : IHasCtime, IHasMtime, IHasStockKey
    {
        internal protected Stock()
        {

        }

        public virtual int StockId { get; protected set; }

        public virtual int v { get; set; }

        public virtual DateTime ctime { get; set; }

        public virtual DateTime mtime { get; set; }

        [Required]
        public virtual Material Material { get; set; } = default!;

        [Required]
        [MaxLength(FIELD_LENGTH.BATCH)]
        public virtual string Batch { get; set; } = default!;

        [MaxLength(FIELD_LENGTH.STOCK_STATUS)]
        [Required]
        public virtual string StockStatus { get; set; } = default!;

        public virtual decimal Quantity { get; set; }

        [Required]
        [MaxLength(FIELD_LENGTH.UOM)]
        public virtual string Uom { get; set; } = default!;

        [MaxLength(20)]
        [Required]
        public virtual string Fifo { get; set; } = default!;

        public virtual bool Stocktaking { get; set; }

        public virtual DateTime AgeBaseline { get; set; }

    }
}
