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
using Swm.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// TODO 重要：安装工具不应作为程序的一部分，应拆分为独立的工具

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
        /// <param name="userManager"></param>
        /// <param name="roleManager"></param>
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
            await _session.SaveAsync(loc);
            _logger.Information("已创建 N 位置");

            string aspnetIdentity = @"
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserTokens]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserTokens] DROP CONSTRAINT IF EXISTS [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserRoles] DROP CONSTRAINT IF EXISTS [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserRoles] DROP CONSTRAINT IF EXISTS [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserLogins] DROP CONSTRAINT IF EXISTS [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserClaims] DROP CONSTRAINT IF EXISTS [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetRoleClaims]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetRoleClaims] DROP CONSTRAINT IF EXISTS [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
/****** Object:  Index [UserNameIndex]    Script Date: 2021/4/16 10:23:42 ******/
DROP INDEX IF EXISTS [UserNameIndex] ON [dbo].[AspNetUsers]
GO
/****** Object:  Index [EmailIndex]    Script Date: 2021/4/16 10:23:42 ******/
DROP INDEX IF EXISTS [EmailIndex] ON [dbo].[AspNetUsers]
GO
/****** Object:  Index [IX_AspNetUserRoles_RoleId]    Script Date: 2021/4/16 10:23:42 ******/
DROP INDEX IF EXISTS [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]
GO
/****** Object:  Index [IX_AspNetUserLogins_UserId]    Script Date: 2021/4/16 10:23:42 ******/
DROP INDEX IF EXISTS [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]
GO
/****** Object:  Index [IX_AspNetUserClaims_UserId]    Script Date: 2021/4/16 10:23:42 ******/
DROP INDEX IF EXISTS [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]
GO
/****** Object:  Index [RoleNameIndex]    Script Date: 2021/4/16 10:23:42 ******/
DROP INDEX IF EXISTS [RoleNameIndex] ON [dbo].[AspNetRoles]
GO
/****** Object:  Index [IX_AspNetRoleClaims_RoleId]    Script Date: 2021/4/16 10:23:42 ******/
DROP INDEX IF EXISTS [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 2021/4/16 10:23:42 ******/
DROP TABLE IF EXISTS [dbo].[AspNetUserTokens]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 2021/4/16 10:23:42 ******/
DROP TABLE IF EXISTS [dbo].[AspNetUsers]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 2021/4/16 10:23:42 ******/
DROP TABLE IF EXISTS [dbo].[AspNetUserRoles]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 2021/4/16 10:23:42 ******/
DROP TABLE IF EXISTS [dbo].[AspNetUserLogins]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 2021/4/16 10:23:42 ******/
DROP TABLE IF EXISTS [dbo].[AspNetUserClaims]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 2021/4/16 10:23:42 ******/
DROP TABLE IF EXISTS [dbo].[AspNetRoles]
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 2021/4/16 10:23:42 ******/
DROP TABLE IF EXISTS [dbo].[AspNetRoleClaims]
GO

/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 2021/4/16 10:23:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 2021/4/16 10:23:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 2021/4/16 10:23:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 2021/4/16 10:23:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 2021/4/16 10:23:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](450) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 2021/4/16 10:23:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](450) NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 2021/4/16 10:23:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [nvarchar](450) NOT NULL,
	[LoginProvider] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetRoleClaims_RoleId]    Script Date: 2021/4/16 10:23:42 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [RoleNameIndex]    Script Date: 2021/4/16 10:23:42 ******/
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
(
	[NormalizedName] ASC
)
WHERE ([NormalizedName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserClaims_UserId]    Script Date: 2021/4/16 10:23:42 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserLogins_UserId]    Script Date: 2021/4/16 10:23:42 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserRoles_RoleId]    Script Date: 2021/4/16 10:23:42 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [EmailIndex]    Script Date: 2021/4/16 10:23:42 ******/
CREATE NONCLUSTERED INDEX [EmailIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedEmail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UserNameIndex]    Script Date: 2021/4/16 10:23:42 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedUserName] ASC
)
WHERE ([NormalizedUserName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO

ALTER TABLE [dbo].[AspNetUsers] ADD IsBuiltIn bit NOT NULL
GO
ALTER TABLE [dbo].[AspNetUsers] ADD Comment nvarchar(255)
GO
ALTER TABLE [dbo].[AspNetUsers] ADD RefreshToken nvarchar(255)
GO
ALTER TABLE [dbo].[AspNetUsers] ADD RefreshTokenTime datetime2
GO
ALTER TABLE [dbo].[AspNetUsers] ADD RefreshTokenExpireTime datetime2
GO
ALTER TABLE [dbo].[AspNetRoles] ADD IsBuiltIn bit NOT NULL
GO
ALTER TABLE [dbo].[AspNetRoles] ADD Comment nvarchar(255)
GO
";

            foreach (var stmt in Regex.Split(aspnetIdentity, @"\bGO\b"))
            {
                if (string.IsNullOrWhiteSpace(stmt) == false)
                {
                    await _session.CreateSQLQuery(stmt).ExecuteUpdateAsync();
                }
            }

            _logger.Information("已创建 AspNetCore.Identity 表结构");

            await _session.SaveAsync(new MaterialTypeInfo { MaterialType = "原材料", DisplayName = "原材料", DisplayOrder = 1,  Visible = true });
            await _session.SaveAsync(new MaterialTypeInfo { MaterialType = "成品", DisplayName = "成品", DisplayOrder = 2, Visible = true });
            _logger.Information("已写入默认的物料类型数据");

            await _session.SaveAsync(new BizTypeInfo { BizType = "独立入库", DisplayName = "独立入库", DisplayOrder = 1, Scope = "入库单", Visible = true });
            await _session.SaveAsync(new BizTypeInfo { BizType = "独立出库", DisplayName = "独立出库", DisplayOrder = 2, Scope = "出库单", Visible = true });
            _logger.Information("已写入默认的业务类型数据");

            await _session.SaveAsync(new StockStatusInfo { StockStatus = "待检", DisplayName = "待检", DisplayOrder = 1, Visible = true });
            await _session.SaveAsync(new StockStatusInfo { StockStatus = "合格", DisplayName = "合格", DisplayOrder = 2, Visible = true });
            await _session.SaveAsync(new StockStatusInfo { StockStatus = "不合格", DisplayName = "不合格", DisplayOrder = 3, Visible = true });
            _logger.Information("已写入默认的库存状态数据");

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

            await GenerateStreetlet(new GenerateStreetletArgs { StreetletCode = "S1", Columns = 5, Levels = 2, DoubleDeep = false });
            await GenerateStreetlet(new GenerateStreetletArgs { StreetletCode = "S2", Columns = 5, Levels = 2, DoubleDeep = true });

            _logger.Information("已生成测试数据");

            return this.Success();
        }

        /// <summary>
        /// 生成管理员用户和管理员角色
        /// </summary>
        /// <param name="password">密码</param>
        /// <returns></returns>
        [HttpPost("generate-admin-user")]
        public async Task<ApiData> GenerateAdminUser([FromBody] string password)
        {
            var role = new ApplicationRole { Name = "admin", IsBuiltIn = true };
            await _roleManager.CreateAsync(role);

            ApplicationUser user = new ApplicationUser { UserName = "admin", IsBuiltIn = true };
            await _userManager.CreateAsync(user, password);

            await _userManager.AddToRolesAsync(user, new[] { "admin" });

            return this.Success();
        }



        /// <summary>
        /// 生成巷道
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("generate-streetlet")]
        [AutoTransaction]
        public async Task<ApiData> GenerateStreetlet(GenerateStreetletArgs args)
        {
            string streetletCode = args.StreetletCode;
            bool doubleDeep = args.DoubleDeep;
            int columns = args.Columns;
            int levels = args.Levels;

            Streetlet streetlet = new Streetlet(args.StreetletCode, args.DoubleDeep, "默认区域");
            await _session.SaveAsync(streetlet).ConfigureAwait(false);
            List<(string rackCode, RackSide side, int deep)> racks = new List<(string rack, RackSide side, int deep)>();
            if (args.DoubleDeep)
            {
                racks.Add(($"{args.StreetletCode}1", RackSide.Left, 2));
                racks.Add(($"{args.StreetletCode}2", RackSide.Left, 1));
                racks.Add(($"{args.StreetletCode}3", RackSide.Right, 1));
                racks.Add(($"{args.StreetletCode}4", RackSide.Right, 2));
            }
            else
            {
                racks.Add(($"{args.StreetletCode}1", RackSide.Left, 1));
                racks.Add(($"{args.StreetletCode}2", RackSide.Right, 1));
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
                        Cell cell = new Cell(streetlet)
                        {
                            Side = side.Key,
                            Column = col,
                            Level = lv,
                            i1 = streetlet.StreetletId * 10000 + k,
                            o1 = streetlet.StreetletId * 10000 + k
                        };

                        foreach (var rack in side)
                        {
                            string locCode = string.Format("{0}-{1:000}-{2:000}", rack.rackCode, col, lv);
                            Location loc = _locationFactory.Invoke();
                            loc.LocationCode = locCode;
                            loc.LocationType = LocationTypes.S;
                            loc.Streetlet = streetlet;
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
USING (SELECT CellId, ROW_NUMBER() OVER(ORDER BY [Level], [Column], Side) + :streetletId * 10000 AS i1
		FROM Cells
        WHERE StreetletId = :streetletId
    ) AS t
ON c.CellId = t.CellId
WHEN MATCHED THEN UPDATE SET c.i1 = t.i1;");
            await q1
                .SetInt32("streetletId", streetlet.StreetletId)
                .ExecuteUpdateAsync()
                .ConfigureAwait(false);

            ISQLQuery q2 = _session.CreateSQLQuery(@"
MERGE Cells c
USING (SELECT CellId, ROW_NUMBER() OVER(ORDER BY [Level], [Column], Side) + :streetletId * 10000 AS o1
		FROM Cells
        WHERE StreetletId = :streetletId
    ) AS t
ON c.CellId = t.CellId
WHEN MATCHED THEN UPDATE SET c.o1 = t.o1;");
            await q2
                .SetInt32("streetletId", streetlet.StreetletId)
                .ExecuteUpdateAsync()
                .ConfigureAwait(false);

            await _locationHelper.RebuildStreetletStatAsync(streetlet).ConfigureAwait(false);

            return this.Success(streetlet.StreetletCode);
        }
    }
}
