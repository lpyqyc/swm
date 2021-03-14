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

using Arctic.Auditing;
using Swm.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Swm.Users
{
    // TODO 使用 asp.net core identity 代替

    /// <summary>
    /// 表示角色。
    /// </summary>
    public class Role : IHasCtime, IHasMtime
    {
        /// <summary>
        /// 初始化此类的新实例。
        /// </summary>
        public Role()
        {
            this.ctime = DateTime.Now;
            this.mtime = DateTime.Now;
            this.AllowedOpTypes = new HashSet<string>();
        }

        /// <summary>
        /// Id
        /// </summary>
        public virtual Int32 RoleId { get; protected set; }

        /// <summary>
        /// 角色名称。自然键。
        /// </summary>
        [Required]
        [MaxLength(FIELD_LENGTH.ROLENAME)]
        public virtual string RoleName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime ctime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public virtual DateTime mtime { get; set; }

        /// <summary>
        /// 是否内置角色
        /// </summary>
        public virtual bool IsBuiltIn { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public virtual string Comment { get; set; }

        /// <summary>
        /// 返回表示此实例的字符串。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.RoleName;
        }

        /// <summary>
        /// 此角色可以执行的操作。
        /// </summary>
        public virtual ISet<string> AllowedOpTypes { get; set; }

    }

}
