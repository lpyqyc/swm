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
    /// 用户信息
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; } = default!;

        /// <summary>
        /// 是否内置用户，内置用户不能删除
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 所属角色
        /// </summary>
        public IEnumerable<string>? Roles { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime ctime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// 是否锁定，锁定用户不能登录
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// 锁定原因
        /// </summary>
        public string? LockedReason { get; set; }
    }

}
