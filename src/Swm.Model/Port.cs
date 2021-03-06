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

using Arctic.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Swm.Model
{

    public class Port : IHasCtime
    {
        public Port()
        {
            this.ctime = DateTime.Now;
            this.Laneways = new HashSet<Laneway>();
        }

        public virtual Int32 PortId { get; internal protected set; }

        [Required]
        [MaxLength(20)]
        public virtual string PortCode { get; set; }

        public virtual Int32 v { get; set; }

        public virtual DateTime ctime { get; set; }

        [Required]
        public virtual Location KP1 { get; set; }

        public virtual Location? KP2 { get; set; }

        public virtual string? Comment { get; set; }


        public virtual ISet<Laneway> Laneways { get; protected set; }

        // TODO 重命名
        public virtual IUnitloadAllocationTable? CurrentUat { get; protected set; }

        public virtual DateTime CheckedAt { get; set; } = default;

        public virtual string? CheckMessage { get; set; }

        // TODO 重命名
        public virtual void SetCurrentUat(IUnitloadAllocationTable uat)
        {
            if (uat == null)
            {
                throw new ArgumentNullException(nameof(uat));
            }

            if (this.CurrentUat == uat)
            {
                return;
            }

            if (this.CurrentUat != null)
            {
                throw new InvalidOperationException($"出口已被占用。【{this.CurrentUat}】");
            }

            this.CurrentUat = uat;
        }

        // TODO 重命名
        public virtual void ResetCurrentUat()
        {
            this.CurrentUat = null;
        }

        public override string ToString()
        {
            return this.PortCode;
        }

    }

}
