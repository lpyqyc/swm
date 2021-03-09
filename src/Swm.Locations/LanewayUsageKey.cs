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
using System.ComponentModel.DataAnnotations;

namespace Swm.Locations
{
    /// <summary>
    /// 巷道使用信息的 Key。
    /// </summary>
    public class LanewayUsageKey : IEquatable<LanewayUsageKey>
    {
        public LanewayUsageKey()
        {
        }

        /// <summary>
        /// 获取统计信息的货位存储分组。
        /// </summary>
        [Required]
        [MaxLength(10)]
        public virtual string StorageGroup { get; set; }

        /// <summary>
        /// 获取统计信息的货位规格。
        /// </summary>
        [Required]
        [MaxLength(16)]
        public virtual string Specification { get; set; }

        /// <summary>
        /// 获取统计信息的货位限重。
        /// </summary>
        public virtual decimal WeightLimit { get; set; }

        /// <summary>
        /// 获取统计信息的货位限高。
        /// </summary>
        public virtual decimal HeightLimit { get; set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as LanewayUsageKey);
        }

        public virtual bool Equals(LanewayUsageKey other)
        {
            if (other == null)
            {
                return false;
            }
            return this.StorageGroup == other.StorageGroup
                && this.Specification == other.Specification
                && this.WeightLimit == other.WeightLimit
                && this.HeightLimit == other.HeightLimit;
        }

        public override int GetHashCode()
        {
            int hashCode = 868850908;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(StorageGroup);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Specification);
            hashCode = hashCode * -1521134295 + WeightLimit.GetHashCode();
            hashCode = hashCode * -1521134295 + HeightLimit.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(LanewayUsageKey left, LanewayUsageKey right)
        {
            return EqualityComparer<LanewayUsageKey>.Default.Equals(left, right);
        }

        public static bool operator !=(LanewayUsageKey left, LanewayUsageKey right)
        {
            return !(left == right);
        }
    }

}
