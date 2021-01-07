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

namespace Swm.Model
{
    /// <summary>
    /// 表示为出库单明细在库存项中进行的一次分配。
    /// </summary>
    public class OutboundLineAllocation : IEquatable<OutboundLineAllocation>
    {
        public OutboundLineAllocation()
        {
        }

        /// <summary>
        /// 获取或设置本次分配是从哪个库存项中进行的。
        /// </summary>
        [Required]
        public virtual UnitloadItem UnitloadItem { get; internal protected set; }


        /// <summary>
        /// 获取或设置本次分配的数量。
        /// </summary>
        public virtual decimal Quantity { get; internal protected set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as OutboundLineAllocation);
        }

        public bool Equals(OutboundLineAllocation other)
        {
            return other != null &&
                   EqualityComparer<UnitloadItem>.Default.Equals(UnitloadItem, other.UnitloadItem) &&
                   Quantity == other.Quantity;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UnitloadItem, Quantity);
        }

        public static bool operator ==(OutboundLineAllocation left, OutboundLineAllocation right)
        {
            return EqualityComparer<OutboundLineAllocation>.Default.Equals(left, right);
        }

        public static bool operator !=(OutboundLineAllocation left, OutboundLineAllocation right)
        {
            return !(left == right);
        }
    }



}