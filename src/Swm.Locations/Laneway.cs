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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Swm.Locations
{
    public class Laneway : IHasCtime, IHasMtime
    {
        protected Laneway()
            : this(default!, default, default!)
        {
        }

        public Laneway(string lanewayCode, bool doubleDeep, string area)
        {
            this.Area = area;
            this.LanewayCode = lanewayCode;
            this.DoubleDeep = doubleDeep;
            this.ctime = DateTime.Now;
            this.mtime = DateTime.Now;
            this.Automated = true;
            this.Locations = new HashSet<Location>();
            this.Ports = new HashSet<Port>();
            this.Usage = new Dictionary<LanewayUsageKey, LanewayUsageData>();
        }

        public virtual int LanewayId { get; internal protected set; }

        [Required]
        [MaxLength(4)]
        public virtual string LanewayCode { get; internal protected set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "特殊属性")]
        public virtual int v { get; set; }

        public virtual DateTime ctime { get; set; }

        public virtual DateTime mtime { get; set; }

        [Required]
        [MaxLength(16)]
        public virtual string Area { get; set; }

        public virtual string? Comment { get; set; }

        public virtual ISet<Location> Locations { get; protected set; }

        public virtual bool Automated { get; set; }

        public virtual bool Offline { get; set; }

        public virtual string? OfflineComment { get; set; }

        public virtual DateTime TakeOfflineTime { get; set; }

        public virtual double TotalOfflineHours { get; set; }


        public virtual bool DoubleDeep { get; protected set; }

        public virtual int ReservedLocationCount { get; set; }

        /// <summary>
        /// 获取此巷道能够到达的出口
        /// </summary>
        public virtual ISet<Port> Ports { get; protected set; }

        /// <summary>
        /// 备用字段
        /// </summary>
        [MaxLength(9999)]
        public virtual string? ex1 { get; set; }

        /// <summary>
        /// 备用字段
        /// </summary>
        [MaxLength(9999)]
        public virtual string? ex2 { get; set; }


        public override string ToString()
        {
            return LanewayCode;
        }

        public virtual IDictionary<LanewayUsageKey, LanewayUsageData> Usage { get; protected set; }

        public virtual int GetTotalLocationCount()
        {
            if (this.Usage.Any())
            {
                return this.Usage.Sum(x => x.Value.Total);
            }

            return 0;
        }

        public virtual int GetAvailableLocationCount()
        {
            if (this.Usage.Any())
            {
                return this.Usage.Sum(x => x.Value.Available);
            }

            return 0;
        }
    }

}
