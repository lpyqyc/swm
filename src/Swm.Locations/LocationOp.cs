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

namespace Swm.Locations
{
    /// <summary>
    /// 表示货位上的操作记录。
    /// </summary>
    public class LocationOp : IHasCtime, IHasCuser
    {
        public virtual int LocationOpId { get; internal protected set; }

        [Required]
        public virtual Location Location { get; set; } = default!;

        [Required]
        [MaxLength(FIELD_LENGTH.OPERATION_TYPE)]
        public virtual string OpType { get; set; } = default!;

        public virtual DateTime ctime { get; set; }

        [MaxLength(FIELD_LENGTH.USERNAME)]
        public virtual string cuser { get; set; } = default!;

        [MaxLength(9999)]
        public virtual string? Comment { get; set; }

    }

}
