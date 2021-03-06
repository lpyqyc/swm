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
    /// 货载详情
    /// </summary>
    public class UnitloadDetails
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
        /// 操作提示类型
        /// </summary>
        public string OpHintType { get; set; } = default!;

        /// <summary>
        /// 操作提示信息
        /// </summary>
        public string OpHintInfo { get; set; } = default!;

        /// <summary>
        /// 备注
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// 是否有任务
        /// </summary>
        public bool BeingMoved { get; set; }

        /// <summary>
        /// 当前任务
        /// </summary>
        public CurrentTaskInfo? CurrentTask { get; set; }

        /// <summary>
        /// 当前分配到的单据
        /// </summary>
        public string? CurrentUat { get; set; }

        /// <summary>
        /// 货载明细列表
        /// </summary>
        public List<UnitloadItemInfo> Items { get; set; } = new List<UnitloadItemInfo>();

        /// <summary>
        /// 当前任务信息
        /// </summary>
        public class CurrentTaskInfo
        {
            /// <summary>
            /// 当前任务号
            /// </summary>
            public string TaskCode { get; set; } = default!;

            /// <summary>
            /// 当前任务类型
            /// </summary>
            public string TaskType { get; set; } = default!;

            /// <summary>
            /// 起点位置编码
            /// </summary>
            public string StartLocationCode { get; set; } = default!;

            /// <summary>
            /// 终点位置编码
            /// </summary>
            public string EndLocationCode { get; set; } = default!;
        }
    }


}
