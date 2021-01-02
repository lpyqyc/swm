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
    public class SysController : ControllerBase
    {
        private readonly ILogger _logger;
        readonly OpHelper _opHelper;
        private readonly ISession _session;

        public SysController(ISession session, OpHelper opHelper, ILogger logger)
        {
            _logger = logger;
            _opHelper = opHelper;
            _session = session;
        }


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
        [HttpPost]
        [Route("get-list-of-roles")]
        [OperationType(OperationTypes.查看角色)]
        public async Task<RoleList> GetListOfRolesAsync(RoleListArgs args)
        {
            var pagedList = await _session.Query<Role>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return new RoleList
            {
                Success = true,
                Message = "OK",
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

        [AutoTransaction]
        [HttpPost]
        [Route("get-select-list-of-roles")]
        public async Task<List<RoleSelectListItem>> GetSelectListOfRolesAsync()
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
        [Route("create-role")]
        [OperationType(OperationTypes.创建角色)]
        public async Task<OperationResult> CreateRoleAsync(CreateRoleArgs args)
        {
            Role role = new Role();
            role.RoleName = args.RoleName;
            role.Comment = args.Comment;

            await _session.SaveAsync(role);
            await _opHelper.SaveOpAsync("RoleId: {0}，角色名：{1}。", role.RoleId, role.RoleName);

            return new OperationResult
            {
                Success = true,
                Message = "操作成功",
            };
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost]
        [Route("delete-role")]
        [OperationType(OperationTypes.删除角色)]
        public async Task<OperationResult> DeleteRoleAsync(DeleteRoleArgs args)
        {
            Role role = await _session.GetAsync<Role>(args.RoleId);
            if (role.IsBuiltIn)
            {
                throw new InvalidOperationException("不能删除内置角色。");
            }

            var users = await _session.Query<User>().Where(x => x.Roles.Contains(role)).WrappedToListAsync();
            foreach (var user in users)
            {
                user.RemoveFromRole(role);
            }
            await _session.DeleteAsync(role);
            await _opHelper.SaveOpAsync("RoleId: {0}，角色名：{1}。", role.RoleId, role.RoleName);

            return new OperationResult
            {
                Success = true,
                Message = "操作成功",
            };
        }

        /// <summary>
        /// 编辑角色
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost]
        [Route("edit-role")]
        [OperationType(OperationTypes.编辑角色)]
        public async Task<OperationResult> EditRoleAsync(EditRoleArgs args)
        {
            if (args.RoleId == null)
            {
                throw new ArgumentException();
            }

            Role role = await _session.GetAsync<Role>(args.RoleId.Value);
            role.RoleName = args.RoleName;
            role.Comment = args.Comment;

            await _session.SaveAsync(role);
            await _opHelper.SaveOpAsync("RoleId: {0}，角色名：{1}。", role.RoleId, role.RoleName);

            return new OperationResult
            {
                Success = true,
                Message = "操作成功",
            };
        }
    }
}
