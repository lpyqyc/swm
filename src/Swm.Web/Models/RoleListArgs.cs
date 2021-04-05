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

using Arctic.NHibernateExtensions;
using System.Linq;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 角色列表的查询参数
    /// </summary>
    public class RoleListArgs
    {
        /// <summary>
        /// 角色名，支持模糊查询
        /// </summary>
        public string? RoleName { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string? Sort { get; set; }

        /// <summary>
        /// 基于 1 的当前页面，默认值为 1。
        /// </summary>
        public int? Current { get; set; } = 1;

        /// <summary>
        /// 每页大小，默认值为 10。
        /// </summary>
        public int? PageSize { get; set; }


        /// <summary>
        /// 在查询对象上应用筛选条件
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public IQueryable<ApplicationRole> Filter(IQueryable<ApplicationRole> q)
        {
            RoleName = trim(RoleName);

            if (RoleName != null)
            {
                q = q.Where(x => x.UserName.Contains(RoleName));
            }

            return q;

            static string? trim(string? str)
            {
                str = str?.Trim();
                if (string.IsNullOrEmpty(str))
                {
                    str = null;
                }
                return str;
            }
        }


    }

}
