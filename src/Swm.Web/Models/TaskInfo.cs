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

using Swm.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 任务信息
    /// </summary>
    public class TaskInfo
    {
        /// <summary>
        /// 任务Id
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// 任务号
        /// </summary>
        public string TaskCode { get; set; } = default!;

        /// <summary>
        /// 任务类型
        /// </summary>
        public string TaskType { get; set; } = default!;

        /// <summary>
        /// 托盘号
        /// </summary>
        public string PalletCode { get; set; } = default!;

        /// <summary>
        /// 起点
        /// </summary>
        public string StartLocationCode { get; set; } = default!;

        /// <summary>
        /// 终点
        /// </summary>
        public string EndLocationCode { get; set; } = default!;

        /// <summary>
        /// 任务下发给Wcs的时间
        /// </summary>
        public DateTime SendTime { get; set; }

        /// <summary>
        /// 货载明细
        /// </summary>
        public List<UnitloadItemInfo> Items { get; set; } = default!;

        /// <summary>
        /// 单号
        /// </summary>
        public string OrderCode { get; set; } = default!;

        /// <summary>
        /// 备注
        /// </summary>
        public string Comment { get; set; } = default!;


    }

}