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

using Arctic.AspNetCore;
using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using Serilog;
using Swm.Model;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger _logger;
        readonly OpHelper _opHelper;
        private readonly ISession _session;

        public UsersController(ISession session, OpHelper opHelper, ILogger logger)
        {
            _logger = logger;
            _opHelper = opHelper;
            _session = session;
        }

        /// <summary>
        /// 用户列表
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet]
        [OperationType(OperationTypes.查看用户)]
        public async Task<ListData<UserListItem>> List([FromQuery]UserListArgs args)
        {
            var pagedList = await _session.Query<User>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

            return this.ListData(pagedList, x => new UserListItem
            {
                UserId = x.UserId,
                UserName = x.UserName,
                IsBuiltIn = x.IsBuiltIn,
                Roles = x.Roles.Select(x => x.RoleName),
                ctime = x.ctime,
                Comment = x.Comment,
                IsLocked = x.IsLocked,
                LockedReason = x.LockedReason
            });
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost]
        [OperationType(OperationTypes.创建用户)]
        public async Task<ApiData> Create(CreateUserArgs args)
        {
            User user = new User();
            user.UserName = args.UserName;
            user.PasswordSalt = Guid.NewGuid().ToString();
            user.PasswordHash = HashPassword(args.Password, user.PasswordSalt);

            SetRoles(user, args.Roles);

            await _session.SaveAsync(user);
            _ = await _opHelper.SaveOpAsync("UserId: {0}，用户名：{1}", user.UserId, user.UserName);

            return this.Success2();
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id"><用户id/param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("delete/{id}")]
        [OperationType(OperationTypes.删除用户)]
        public async Task<ApiData> Delete(int id)
        {
            User user = await _session.GetAsync<User>(id);
            if (user.IsBuiltIn)
            {
                throw new InvalidOperationException("不能删除内置用户。");
            }

            await _session.DeleteAsync(user);
            await _opHelper.SaveOpAsync("UserId: {0}，用户名：{1}", user.UserId, user.UserName);
            return this.Success2();
        }

        /// <summary>
        /// 编辑用户
        /// </summary>
        /// <param name="id">用户id</param>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("edit/{id}")]
        [OperationType(OperationTypes.编辑用户)]
        public async Task<ApiData> Edit(int id, EditUserArgs args)
        {
            User user = await _session.GetAsync<User>(id);
            if (user == null)
            {
                throw new InvalidOperationException("用户不存在。");
            }

            if (String.IsNullOrEmpty(args.Password) == false)
            {
                user.PasswordSalt = Guid.NewGuid().ToString();
                user.PasswordHash = HashPassword(args.Password, user.PasswordSalt);
            }

            SetRoles(user, args.Roles);

            await _session.SaveAsync(user);

            await _opHelper.SaveOpAsync("UserId: {0}，用户名：{1}", user.UserId, user.UserName);

            return this.Success2();
        }


        private void SetRoles(User user, string[]? roles)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.Roles.Clear();

            var rolelist = _session.Query<Role>().ToList();

            // TODO 提取到常数类
            if (user.UserName == "admin")
            {
                Role? adminRole = rolelist.SingleOrDefault(x => x.RoleName == "admin");
                if (adminRole != null)
                {
                    user.AddToRole(adminRole);
                }
            }

            if (roles != null)
            {
                foreach (var roleName in roles)
                {
                    Role? role = rolelist.SingleOrDefault(x => x.RoleName == roleName);
                    if (role != null)
                    {
                        user.AddToRole(role);
                    }
                }
            }
        }

        /// <summary>
        /// 对密码进行散列。
        /// </summary>
        /// <param name="password">密码明文</param>
        /// <param name="salt">密码盐度</param>
        /// <returns>散列后的密码</returns>
        internal static string HashPassword(string password, string salt)
        {
            string saltedPassword = salt + password + salt;
            string hash = GetMd5Hash(saltedPassword);
            return hash;
        }


        /// <summary>
        /// 使用 md5 算法获取输入字符串的 hash 值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static string GetMd5Hash(string input)
        {
            using (MD5 md5Hasher = MD5.Create())
            {
                byte[] data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(input));

                StringBuilder sBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }

    }



}
