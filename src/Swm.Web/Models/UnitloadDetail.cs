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

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 货载详情
    /// </summary>
    public class UnitloadDetail : UnitloadInfo
    {
        /// <summary>
        /// 货载到达当前位置的时间
        /// </summary>
        public DateTime LocationTime { get; set; }


        /// <summary>
        /// 操作提示类型
        /// </summary>
        public string? OpHintType { get; set; }

        /// <summary>
        /// 操作提示信息
        /// </summary>
        public string? OpHintInfo { get; set; }



        /// <summary>
        /// 当前分配到的单据
        /// </summary>
        public string? CurrentUat { get; set; }

        /// <summary>
        /// 当前任务号
        /// </summary>
        public string? CurrentTaskCode { get; set; }

        /// <summary>
        /// 当前任务类型
        /// </summary>
        public string? CurrentTaskType { get; set; }

        /// <summary>
        /// 当前任务起点位置编码
        /// </summary>
        public string? CurrentTaskStartLocationCode { get; set; }

        /// <summary>
        /// 当前任务终点位置编码
        /// </summary>
        public string? CurrentTaskEndLocationCode { get; set; }


    }

}
