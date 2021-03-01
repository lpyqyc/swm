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
using System.ComponentModel.DataAnnotations;

namespace Swm.Model
{
    public class ArchivedTransportTask
    {
        public ArchivedTransportTask()
        {
        }

        public virtual int TaskId { get; set; }

        [Required]
        [MaxLength(20)]
        public virtual string TaskCode { get; internal protected set; }

        [Required]
        [MaxLength(20)]
        public virtual string TaskType { get; set; }

        public virtual DateTime ctime { get; set; }


        [Required]
        public virtual UnitloadSnapshot Unitload { get; internal protected set; }

        [Required]
        public virtual Location Start { get; set; }

        [Required]
        public virtual Location End { get; set; }

        [Required]
        public virtual Location ActualEnd { get; internal protected set; }


        public virtual bool ForWcs { get; set; }

        public virtual bool WasSentToWcs { get; set; }

        public virtual DateTime SendTime { get; set; }

        [MaxLength(20)]
        public virtual string OrderCode { get; set; }

        public virtual string Comment { get; set; }

        public virtual string ex1 { get; set; }

        public virtual string ex2 { get; set; }

        public virtual DateTime ArchivedAt { get; set; }

        public virtual bool Cancelled { get; set; }

    }


}
