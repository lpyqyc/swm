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

namespace Swm.Locations
{
    /// <summary>
    /// 巷道使用数据。
    /// </summary>
    public class LanewayUsageData : ICloneable
    {
        public LanewayUsageData()
        {
        }

        /// <summary>
        /// 数据更新时间。
        /// </summary>
        public virtual DateTime mtime { get; set; }


        /// <summary>
        /// 获取或设置总货位数。
        /// <see cref="Location.Exists"/> 为 false 的货位不参与统计。
        /// </summary>
        public virtual int Total { get; set; }

        /// <summary>
        /// 获取或设置当前可用的货位数。
        /// 可用货位是指 <see cref="Location.Available"/> 方法返回 true 的货位。
        /// <see cref="Location.Exists"/> 为 false 的货位不参与统计。
        /// </summary>
        public virtual int Available { get; set; }

        /// <summary>
        /// 获取或设置当前有货的货位数。
        /// 有货的货位是指 <see cref="Location.Loaded"/> 方法返回 true 的货位。
        /// <see cref="Location.Exists"/> 为 false 的货位不参与统计。
        /// </summary>
        public virtual int Loaded { get; set; }

        /// <summary>
        /// 获取或设置当前已禁止入站的货位数，
        /// <see cref="Location.Exists"/> 为 false 的货位不参与统计。
        /// </summary>
        public virtual int InboundDisabled { get; set; }


        public LanewayUsageData Clone()
        {
            return (LanewayUsageData)this.MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }

}
