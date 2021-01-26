﻿// Copyright 2020 王建军
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
    /// 编辑程序设置操作的参数
    /// </summary>
    public class EditAppSettingArgs
    {
        /// <summary>
        /// 设置值
        /// </summary>
        [Required]
        public string SettingValue { get; set; } = default!;
    }
    /// <summary>
    /// 创建程序设置操作的参数
    /// </summary>
    public class CreateAppSettingArgs
    {
        /// <summary>
        /// 设置类型
        /// </summary>
        [Required]
        public string SettingType { get; set; } = default!;

        /// <summary>
        /// 设置值
        /// </summary>
        [Required]
        public string SettingValue { get; set; } = default!;
    }
}