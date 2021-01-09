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

using System;
using System.Collections.Generic;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 货载列表的数据项
    /// </summary>
    public class UnitloadListItem
    {
        /// <summary>
        /// 货载Id
        /// </summary>
        public int UnitloadId { get; set; }
        
        /// <summary>
        /// 托盘号
        /// </summary>
        public string PalletCode { get; set; } = default!;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime ctime { get; set; }

        /// <summary>
        /// 所在货位编码
        /// </summary>
        public string LocationCode { get; set; } = default!;


        /// <summary>
        /// 所在巷道编码
        /// </summary>
        public string? LanewayCode { get; set; }

        /// <summary>
        /// 托盘是否已分配
        /// </summary>
        public bool Allocated { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// 是否有任务
        /// </summary>
        public bool BeingMoved { get; set; }

        /// <summary>
        /// 货载明细列表
        /// </summary>
        public List<UnitloadItemInfo> Items { get; set; } = new List<UnitloadItemInfo>();
    }
}
