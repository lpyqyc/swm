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
    /// 表示物料主数据。
    /// </summary>
    public class Material : IHasCtime, IHasCuser, IHasMtime, IHasMuser
    {
        internal protected Material()
        {
            this.ctime = DateTime.Now;
            this.mtime = DateTime.Now;
            this.DefaultStorageGroup = "普通";
        }

        public virtual int MaterialId { get; set; }

        [Required]
        [MaxLength(30)]
        public virtual string? MaterialCode { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "特殊属性")]
        public virtual int v { get; set; }

        public virtual DateTime ctime { get; set; }

        [MaxLength(FIELD_LENGTH.USERNAME)]
        public virtual string? cuser { get; set; } = default!;

        public virtual DateTime mtime { get; set; }

        [MaxLength(FIELD_LENGTH.USERNAME)]
        public virtual string? muser { get; set; }


        /// <summary>
        /// 获取物料类型。
        /// </summary>
        [MaxLength(8)]
        public virtual string? MaterialType { get; set; }

        [Required]
        [MaxLength(255)]
        public virtual string? Description { get; set; }

        [MaxLength(30)]
        public virtual string? SpareCode { get; set; }

        [MaxLength(64)]
        public virtual string? Specification { get; set; }

        [MaxLength(20)]
        public virtual string? MnemonicCode { get; set; }

        public virtual bool BatchEnabled { get; set; } = true;

        [MaxLength(50)]
        public virtual string? MaterialGroup { get; set; }

        public virtual decimal ValidDays { get; set; }

        /// <summary>
        /// 此物料的静置时间（以小时为单位）。
        /// </summary>
        public virtual decimal StandingTime { get; set; }

        [MaxLength(1)]
        public virtual string? AbcClass { get; set; }

        [Required]
        [MaxLength(FIELD_LENGTH.UOM)]
        public virtual string? Uom { get; set; }



        public virtual decimal LowerBound { get; set; } = -1;

        public virtual decimal UpperBound { get; set; } = 99999999;


        public virtual decimal DefaultQuantity { get; set; }

        [Required]
        [MaxLength(8)]
        public virtual string? DefaultStorageGroup { get; set; }

        public virtual string? Comment { get; set; }

    }
}
