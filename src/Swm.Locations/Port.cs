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

namespace Swm.Locations
{

    public class Port : IHasCtime
    {
        public Port(string portCode)
        {
            this.ctime = DateTime.Now;
            this.Laneways = new HashSet<Laneway>();
            this.PortCode = portCode;
        }

        protected Port()
        {
            this.Laneways = new HashSet<Laneway>();
            this.PortCode = default!;
        }

        public virtual int PortId { get; internal protected set; }

        [Required]
        [MaxLength(20)]
        public virtual string PortCode { get; protected set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "特殊属性")]
        public virtual int v { get; set; }

        public virtual DateTime ctime { get; set; }

        [Required]
        public virtual Location? KP1 { get; set; }

        public virtual Location? KP2 { get; set; }

        public virtual string? Comment { get; set; }


        public virtual ISet<Laneway> Laneways { get; protected set; }

        // TODO 重命名
        public virtual object? CurrentUat { get; protected set; }

        public virtual DateTime CheckedAt { get; set; } = default;

        public virtual string? CheckMessage { get; set; }


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


        // TODO 考虑引入接口，并重命名，
        public virtual void SetCurrentUat(object uat)
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

        public override string? ToString()
        {
            return this.PortCode;
        }

    }

}
