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

using Arctic.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using Serilog;
using Swm.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 安装工具。
    /// </summary>
    [ApiController]
    [Route("api/setup")]
    public class SetupController : ControllerBase
    {
        readonly Configuration _nhConfiguration;
        readonly IWebHostEnvironment _env;
        readonly ILogger _logger;
        readonly ISession _session;
        readonly Func<Location> _locationFactory;
        readonly LocationHelper _locationHelper;
        readonly UserManager<ApplicationUser> _userManager;
        readonly RoleManager<ApplicationRole> _roleManager;
        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="locationFactory"></param>
        /// <param name="locationHelper"></param>
        /// <param name="session"></param>
        /// <param name="nhConfiguration"></param>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        public SetupController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            Func<Location> locationFactory, 
            LocationHelper locationHelper,
            ISession session, 
            Configuration nhConfiguration, 
            IWebHostEnvironment env, 
            ILogger logger
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _locationFactory = locationFactory;
            _session = session;
            _nhConfiguration = nhConfiguration;
            _env = env;
            _locationHelper = locationHelper;
            _logger = logger;
        }

        /// <summary>
        /// 根据已注册到容器的 nh 映射信息向数据库导出表结构。此方法不是动态迁移表结构，而是删除旧表并创建新表，仅用于开发环境。
        /// </summary>
        [HttpPost("export-schema")]
        [AutoTransaction]
        public async Task<ApiData> ExportSchema()
        {
            if (_env.IsDevelopment() == false)
            {
                throw new InvalidOperationException("只能在开发环境运行此工具");
            }
            _logger.Warning("正在导出数据库架构，所有表结构将重建");
            _logger.Information("当前是开发环境");
            SchemaExport export = new SchemaExport(_nhConfiguration);
            await export.CreateAsync(true, true);
            _logger.Information("已导出数据库架构");

            _logger.Information("正在创建 N 位置");
            Location loc = _locationFactory.Invoke();
            loc.LocationCode = LocationCodes.N;
            loc.LocationType = LocationTypes.N;
            loc.InboundLimit = 999;
            loc.OutboundLimit = 999;
            await _session.SaveAsync(loc).ConfigureAwait(false);
            _logger.Information("已创建 N 位置");

            return this.Success();
        }

        /// <summary>
        /// 生成测试数据，包含一个5列2层单深巷道，和一个5列2层双深巷道。
        /// </summary>
        /// <returns></returns>
        [HttpPost("generate-test-data")]
        [AutoTransaction]
        public async Task<ApiData> GenerateTestData()
        {
            if (_env.IsDevelopment() == false)
            {
                throw new InvalidOperationException("只能在开发环境运行此工具");
            }
            _logger.Information("正在生成测试数据");
            _logger.Information("当前是开发环境");

            await GenerateLaneway(new GenerateLanewayArgs { LanewayCode = "S1", Columns = 5, Levels = 2, DoubleDeep = false });
            await GenerateLaneway(new GenerateLanewayArgs { LanewayCode = "S2", Columns = 5, Levels = 2, DoubleDeep = true });

            _logger.Information("已生成测试数据");

            return this.Success();
        }

        /// <summary>
        /// 生成管理员用户和管理员角色
        /// </summary>
        /// <returns></returns>
        [HttpPost("generate-admin-user")]
        public async Task<ApiData> GenerateAdminUser()
        {
            var role = new ApplicationRole { Name = "admin", IsBuiltIn = true };
            await _roleManager.CreateAsync(role);

            ApplicationUser user = new ApplicationUser { UserName = "admin", IsBuiltIn = true };
            await _userManager.CreateAsync(user, "123456");

            await _userManager.AddToRolesAsync(user, new[] { "admin" });

            return this.Success();
        }

        /// <summary>
        /// 生成巷道
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("generate-laneway")]
        [AutoTransaction]
        public async Task<ApiData> GenerateLaneway(GenerateLanewayArgs args)
        {
            string lanewayCode = args.LanewayCode;
            bool doubleDeep = args.DoubleDeep;
            int columns = args.Columns;
            int levels = args.Levels;

            Laneway laneway = new Laneway(args.LanewayCode, args.DoubleDeep, "默认区域");
            await _session.SaveAsync(laneway).ConfigureAwait(false);
            List<(string rackCode, RackSide side, int deep)> racks = new List<(string rack, RackSide side, int deep)>();
            if (args.DoubleDeep)
            {
                racks.Add(($"{args.LanewayCode}1", RackSide.Left, 2));
                racks.Add(($"{args.LanewayCode}2", RackSide.Left, 1));
                racks.Add(($"{args.LanewayCode}3", RackSide.Right, 1));
                racks.Add(($"{args.LanewayCode}4", RackSide.Right, 2));
            }
            else
            {
                racks.Add(($"{args.LanewayCode}1", RackSide.Left, 1));
                racks.Add(($"{args.LanewayCode}2", RackSide.Right, 1));
            }
            int k = 0;
            for (int i = 0; i < levels; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    foreach (var side in racks.GroupBy(x => x.side))
                    {
                        k++;
                        int col = j + 1;
                        int lv = i + 1;
                        Cell cell = new Cell(laneway)
                        {
                            Side = side.Key,
                            Column = col,
                            Level = lv,
                            i1 = laneway.LanewayId * 10000 + k,
                            o1 = laneway.LanewayId * 10000 + k
                        };

                        foreach (var rack in side)
                        {
                            string locCode = string.Format("{0}-{1:000}-{2:000}", rack.rackCode, col, lv);
                            Location loc = _locationFactory.Invoke();
                            loc.LocationCode = locCode;
                            loc.LocationType = LocationTypes.S;
                            loc.Laneway = laneway;
                            loc.Column = col;
                            loc.Level = lv;
                            loc.InboundLimit = 1;
                            loc.OutboundLimit = 1;
                            loc.Side = rack.side;
                            loc.Deep = rack.deep;
                            loc.StorageGroup = "普通";
                            loc.Specification = "普通";
                            loc.Cell = cell;
                            cell.Locations.Add(loc);
                        }

                        cell.UpdateState();
                        await _session.SaveAsync(cell).ConfigureAwait(false);
                        foreach (var loc in cell.Locations)
                        {
                            await _session.SaveAsync(loc).ConfigureAwait(false);
                        }
                    }
                }
            }
            
            await _session.FlushAsync().ConfigureAwait(false);


            ISQLQuery q1 = _session.CreateSQLQuery(@"
MERGE Cells c
USING (SELECT CellId, ROW_NUMBER() OVER(ORDER BY [Level], [Column], Side) + :lanewayId * 10000 AS i1
		FROM Cells
        WHERE LanewayId = :lanewayId
    ) AS t
ON c.CellId = t.CellId
WHEN MATCHED THEN UPDATE SET c.i1 = t.i1;");
            await q1
                .SetInt32("lanewayId", laneway.LanewayId)
                .ExecuteUpdateAsync()
                .ConfigureAwait(false);

            ISQLQuery q2 = _session.CreateSQLQuery(@"
MERGE Cells c
USING (SELECT CellId, ROW_NUMBER() OVER(ORDER BY [Level], [Column], Side) + :lanewayId * 10000 AS o1
		FROM Cells
        WHERE LanewayId = :lanewayId
    ) AS t
ON c.CellId = t.CellId
WHEN MATCHED THEN UPDATE SET c.o1 = t.o1;");
            await q2
                .SetInt32("lanewayId", laneway.LanewayId)
                .ExecuteUpdateAsync()
                .ConfigureAwait(false);

            await _locationHelper.RebuildLanewayStatAsync(laneway).ConfigureAwait(false);

            return this.Success(laneway.LanewayCode);
        }
    }
}
