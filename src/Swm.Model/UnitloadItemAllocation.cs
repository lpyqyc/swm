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

namespace Swm.Model
{
    /// <summary>
    /// 表示为出库需求在货载明细的一次分配信息
    /// </summary>
    public class UnitloadItemAllocation
    {
        /// <summary>
        /// 分配信息的Id
        /// </summary>
        public virtual int UnitloadItemAllocationId { get; set; }

        /// <summary>
        /// 此分配属于哪个货载明细
        /// </summary>
        public virtual UnitloadItem UnitloadItem { get; set; }

        /// <summary>
        /// 此分配属于哪个出库需求
        /// </summary>
        public virtual IOutboundDemand OutboundDemand { get; set; }

        /// <summary>
        /// 获取或设置出库需求的根类型
        /// </summary>
        [Required]
        [MaxLength(30)]
        internal protected virtual string OutboundDemandRootType { get; set; }

        /// <summary>
        /// 获取或设置本次分配的数量。
        /// </summary>
        public virtual decimal QuantityAllocated { get; internal protected set; }

        /// <summary>
        /// 备注
        /// </summary>
        public virtual string Comment { get; set; }

    }

}