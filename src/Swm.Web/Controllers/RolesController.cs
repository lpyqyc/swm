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
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly ILogger _logger;
        readonly OpHelper _opHelper;
        private readonly ISession _session;

        public RolesController(ISession session, OpHelper opHelper, ILogger logger)
        {
            _logger = logger;
            _opHelper = opHelper;
            _session = session;
        }

        // TODO 挪走
        [HttpGet]
        [Route("get-app-info")]
        public async Task<ActionResult> GetAppInfoAsync()
        {
            return await Task.FromResult(Ok(new
            {
                AppName = "Arctic",
                Version = "1.0",
            }));
        }


        /// <summary>
        /// 角色列表
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet]
        [OperationType(OperationTypes.查看角色)]
        public async Task<ListResult<RoleListItem>> Get([FromQuery]RoleListArgs args)
        {
            var pagedList = await _session.Query<Role>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return new ListResult<RoleListItem>
            {
                Success = true,
                Data = pagedList.List.Select(x => new RoleListItem
                {
                    RoleId = x.RoleId,
                    RoleName = x.RoleName,
                    IsBuiltIn = x.IsBuiltIn,
                    AllowedOpTypes = x.AllowedOpTypes,
                    ctime = x.ctime,
                    Comment = x.Comment,
                })
            };
        }

        /// <summary>
        /// 获取角色的选择列表
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet]
        [Route("select-list")]
        public async Task<List<RoleSelectListItem>> GetSelectList()
        {
            var items = await _session.Query<Role>()
                .Select(x => new RoleSelectListItem
                {
                    RoleId = x.RoleId,
                    RoleName = x.RoleName,
                    IsBuiltIn = x.IsBuiltIn,
                })
                .WrappedToListAsync();
            return items;
        }

        [AutoTransaction]
        [HttpPost]
        [OperationType(OperationTypes.创建角色)]
        public async Task<IActionResult> Create(CreateRoleArgs args)
        {
            if (_session.Query<Role>().Any(x => x.RoleName == args.RoleName))
            {
                throw new InvalidOperationException("名称重复");
            }

            Role role = new Role();
            role.RoleName = args.RoleName;
            role.Comment = args.Comment;

            await _session.SaveAsync(role);
            await _opHelper.SaveOpAsync("RoleId: {0}，角色名：{1}。", role.RoleId, role.RoleName);

            return this.Success();
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="id">角色id</param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpDelete("{id}")]
        [OperationType(OperationTypes.删除角色)]
        public async Task<IActionResult> Delete(int id)
        {
            Role role = await _session.GetAsync<Role>(id);
            if (role.IsBuiltIn)
            {
                return NotFound(id);
            }

            var users = await _session.Query<User>().Where(x => x.Roles.Contains(role)).WrappedToListAsync();
            foreach (var user in users)
            {
                user.RemoveFromRole(role);
            }
            await _session.DeleteAsync(role);
            await _opHelper.SaveOpAsync("RoleId: {0}，角色名：{1}", role.RoleId, role.RoleName);

            return this.Success();
        }

        /// <summary>
        /// 编辑角色
        /// </summary>
        /// <param name="id">角色id</param>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPut("{id}")]
        [OperationType(OperationTypes.编辑角色)]
        public async Task<IActionResult> Edit(int id, EditRoleArgs args)
        {
            Role role = await _session.GetAsync<Role>(id);
            if (role == null)
            {
                return NotFound(id);
            }
            if (_session.Query<Role>().Any(x => x.RoleId != id && x.RoleName == args.RoleName))
            {
                throw new InvalidOperationException("名称重复");
            }

            role.RoleName = args.RoleName;
            role.Comment = args.Comment;

            await _session.SaveAsync(role);
            await _opHelper.SaveOpAsync("RoleId: {0}，角色名：{1}", role.RoleId, role.RoleName);

            return this.Success();
        }
    }
}
