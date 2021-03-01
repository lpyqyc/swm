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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Swm.Model
{
    // TODO 使用成熟解决方案代替自己编写

    /// <summary>
    /// 表示用户。
    /// </summary>
    public class User : IHasCtime, IHasMtime
    {
        /// <summary>
        /// 初始化 User 类的新实例。
        /// </summary>
        public User()
        {
            this.ctime = DateTime.Now;
            this.mtime = DateTime.Now;

            this.Roles = new HashSet<Role>();
        }

        /// <summary>
        /// Id
        /// </summary>
        public virtual Int32 UserId { get; protected set; }


        /// <summary>
        /// 用户名。自然键。
        /// </summary>
        [MaxLength(FIELD_LENGTH.USERNAME)]
        [Required]
        public virtual string UserName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime ctime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public virtual DateTime mtime { get; set; }

        /// <summary>
        /// 密码的散列值
        /// </summary>
        [MaxLength(50)]
        [Required]
        public virtual string PasswordHash { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        [MaxLength(50)]
        public virtual string RealName { get; set; }

        /// <summary>
        /// 密码盐度
        /// </summary>
        [MaxLength(50)]
        [Required]
        public virtual string PasswordSalt { get; set; }
        /// <summary>
        /// 是否内置用户
        /// </summary>
        public virtual Boolean IsBuiltIn { get; set; }

        /// <summary>
        /// 是否锁定
        /// </summary>
        public virtual Boolean IsLocked { get; set; }

        /// <summary>
        /// 锁定原因
        /// </summary>
        [MaxLength(50)]
        public virtual string LockedReason { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        public virtual string Comment { get; set; }

        /// <summary>
        /// 电子邮件
        /// </summary>
        [MaxLength(50)]
        public virtual string Email { get; set; }

        /// <summary>
        /// 此用户具有的角色。
        /// </summary>
        public virtual ISet<Role> Roles { get; protected set; }

        /// <summary>
        /// 将此实例添加到角色。
        /// </summary>
        /// <param name="role"></param>
        public virtual void AddToRole(Role role)
        {
            this.Roles.Add(role);
        }

        /// <summary>
        /// 将此实例从角色移除。
        /// </summary>
        /// <param name="role"></param>
        public virtual void RemoveFromRole(Role role)
        {
            this.Roles.Remove(role);
        }

        /// <summary>
        /// 将此实例添加到角色。
        /// </summary>
        /// <param name="roles"></param>
        public virtual void AddToRoles(Role[] roles)
        {
            foreach (var role in roles)
            {
                this.Roles.Add(role);
            }
        }

        /// <summary>
        /// 将此实例从角色移除。
        /// </summary>
        /// <param name="roles"></param>
        public virtual void RemoveFromRoles(Role[] roles)
        {
            foreach (var role in roles)
            {
                this.Roles.Remove(role);
            }
        }
        /// <summary>
        /// 用户是否在指定的角色中
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public virtual Boolean IsInRole(string roleName)
        {
            return Roles.Select(x => x.RoleName)
                .ToList()
                .Any(x => x.Equals(roleName, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// 返回表示此 User 的字符串。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.UserName;
        }
    }

}
