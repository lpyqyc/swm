﻿// Copyright 2020-2021 王建军
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

using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 创建手工任务的操作参数
    /// </summary>
    public class CreateManualTaskArgs
    {
        /// <summary>
        /// 托盘号
        /// </summary>
        [Required]
        public string? PalletCode { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        [Required]
        public string? TaskType { get; set; }
                
        /// <summary>
        /// 起点
        /// </summary>
        [Required]
        public string? FromLocationCode { get; set; }

        /// <summary>
        /// 终点
        /// </summary>
        [Required]
        public string? ToLocationCode { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Comment { get; set; }
    }

}