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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Swm.Model
{
    public class Laneway : IHasCtime, IHasMtime
    {
        protected Laneway()
        {
            this.ctime = DateTime.Now;
            this.mtime = DateTime.Now;
            this.Automated = true;
            this.Locations = new HashSet<Location>();
            this.Ports = new HashSet<Port>();
            this.Usage = new Dictionary<LanewayUsageKey, LanewayUsageData>();
        }

        public Laneway(bool doubleDeep)
            : this()
        {
            this.DoubleDeep = doubleDeep;
            this.Area = "NA";
        }

        public virtual Int32 LanewayId { get; internal protected set; }

        [Required]
        [MaxLength(4)]
        public virtual String LanewayCode { get; set; }

        public virtual Int32 v { get; set; }

        public virtual DateTime ctime { get; set; }

        public virtual DateTime mtime { get; set; }

        [Required]
        [MaxLength(16)]
        public virtual string Area { get; set; }

        public virtual String Comment { get; set; }

        public virtual ISet<Location> Locations { get; protected set; }

        public virtual bool Automated { get; set; }

        public virtual Boolean Offline { get; set; }

        public virtual string OfflineComment { get; set; }

        public virtual DateTime TakeOfflineTime { get; set; }

        public virtual double TotalOfflineHours { get; set; }


        public virtual bool DoubleDeep { get; protected set; }

        public virtual int ReservedLocationCount { get; set; }

        public virtual ISet<Port> Ports { get; protected set; }

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
