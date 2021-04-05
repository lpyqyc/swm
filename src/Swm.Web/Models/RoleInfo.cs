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

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 角色信息
    /// </summary>
    public class RoleInfo
    {
        /// <summary>
        /// 角色Id
        /// </summary>
        [Required]
        public string? RoleId { get; set; }

        /// <summary>
        /// 角色名
        /// </summary>
        [Required]
        public string? RoleName { get; set; }

        /// <summary>
        /// 是否内置角色，内置角色不能删除
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 允许的操作
        /// </summary>
        public IEnumerable<string>? AllowedOpTypes { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        public string? Comment { get; set; }

    }

}
