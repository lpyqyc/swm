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

namespace Swm.Palletization
{
    /// <summary>
    /// 表示单元货物明细的快照。
    /// </summary>
    public class UnitloadItemSnapshot
    {
        internal protected UnitloadItemSnapshot()
        {
        }

        public virtual int UnitloadItemSnapshotId { get; internal protected set; }

        /// <summary>
        /// 源货载项的Id
        /// </summary>
        public virtual int UnitloadItemId { get; internal protected set; }


        [Required]
        public virtual UnitloadSnapshot? Unitload { get; internal protected set; }

        [Required]
        public virtual Material? Material { get; set; }

        [Required]
        [MaxLength(FIELD_LENGTH.BATCH)]
        public virtual string? Batch { get; set; }

        [Required]
        [MaxLength(FIELD_LENGTH.APP_CODE)]
        public virtual string? StockStatus { get; set; }


        public virtual decimal Quantity { get; set; }

        [MaxLength(FIELD_LENGTH.UOM)]
        [Required]
        public virtual string? Uom { get; set; }


        public virtual DateTime ProductionTime { get; set; }



    }


}
