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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Swm.Palletization
{
    /// <summary>
    /// 存储信息。这些信息决定了入库分配货位的结果。
    /// </summary>
    public class StorageInfo : IEquatable<StorageInfo>
    {
        public StorageInfo()
        {
            this.StorageGroup = default!;
            this.OutFlag = default!;
            this.ContainerSpecification = default!;
        }

        /// <summary>
        /// 获取货载的重量，单位千克。<see cref="Location.WeightLimit" />。
        /// </summary>
        public virtual decimal Weight { get; set; }

        /// <summary>
        /// 获取货载的高度，单位米。<see cref="Location.HeightLimit" />。
        /// </summary>
        public virtual decimal Height { get; set; }

        /// <summary>
        /// 获取此货载的存储分组。存储分组影响上架时的货位分配。<see cref="Location.StorageGroup"/>。
        /// </summary>
        /// <remarks>
        /// 属性值由 <see cref="IUnitloadStorageInfoProvider.GetStorageGroup(Unitload)"/> 方法提供。
        /// </remarks>
        [Required]
        [MaxLength(10)]
        public virtual string StorageGroup { get; set; }

        /// <summary>
        /// 获取或设置此货载的出库标记。
        /// 具有相同出库标记的货载在出库时应可互换。
        /// 对于双深位巷道，框架在分配货位时，会尽量使远端和近端存放出库标记相同的货载，
        /// 以减少出库时让路的可能性。
        /// 属性值由 <see cref="IUnitloadStorageInfoProvider.GetOutFlag(Unitload)"/> 方法提供。
        /// </summary>
        [Required]
        [MaxLength(50)]
        public virtual string OutFlag { get; set; }

        /// <summary>
        /// 获取或设置此货载的容器规格。此属性与 <see cref="Location.Specification"/> 配合使用。
        /// </summary>
        [Required]
        [MaxLength(10)]
        public virtual string ContainerSpecification { get; set; }


        public override string? ToString()
        {
            return JsonSerializer.Serialize(this);
        }

        public override bool Equals(object? obj)
        {
            return obj is StorageInfo info &&
                   Weight == info.Weight &&
                   Height == info.Height &&
                   StorageGroup == info.StorageGroup &&
                   OutFlag == info.OutFlag &&
                   ContainerSpecification == info.ContainerSpecification;
        }

        public override int GetHashCode()
        {
            int hashCode = 637653034;
            hashCode = hashCode * -1521134295 + Weight.GetHashCode();
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(StorageGroup);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(OutFlag);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContainerSpecification);
            return hashCode;
        }

        public virtual bool Equals(StorageInfo? info)
        {
            return info != null &&
                   Weight == info.Weight &&
                   Height == info.Height &&
                   StorageGroup == info.StorageGroup &&
                   OutFlag == info.OutFlag &&
                   ContainerSpecification == info.ContainerSpecification;
        }

        public static bool operator ==(StorageInfo? left, StorageInfo? right)
        {
            return EqualityComparer<StorageInfo>.Default.Equals(left, right);
        }

        public static bool operator !=(StorageInfo? left, StorageInfo? right)
        {
            return !(left == right);
        }
    }


}
