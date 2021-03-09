using Arctic.AppSettings;
using Arctic.AspNetCore;
using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using Serilog;
using Swm.Model;
using Swm.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 提供系统 api
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SysController : ControllerBase
    {
        readonly ISession _session;
        readonly ILogger _logger;
        readonly OpHelper _opHelper;
        readonly IAppSettingService _appSettingService;

        public SysController(IAppSettingService appSettingService, ISession session, OpHelper opHelper, ILogger logger)
        {
            _appSettingService = appSettingService;
            _session = session;
            _opHelper = opHelper;
            _logger = logger;
        }

        /// <summary>
        /// 系统参数列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet("get-app-setting-list")]
        [DebugShowArgs]
        [AutoTransaction]
        public async Task<ListData<AppSetting>> GetAppSettingList([FromQuery] AppSettingListArgs args)
        {
            var pagedList = await _session.Query<AppSetting>().SearchAsync(args, "settingName ASC", 1, 9999);
            return this.ListData(pagedList);
        }


        /// <summary>
        /// 更改参数
        /// </summary>
        /// <param name="settingName">参数名称</param>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("update-app-setting/{settingName}")]
        [OperationType(OperationTypes.更改系统参数)]
        public async Task<ApiData> UpdateAppSetting([Required] string? settingName, UpdateAppSettingArgs args)
        {
            settingName = settingName?.Trim();
            if (string.IsNullOrEmpty(settingName))
            {
                throw new InvalidOperationException("参数名不能为空。");
            }
            var setting = await _appSettingService.GetAsync(settingName);
            if (setting == null)
            {
                throw new InvalidOperationException("参数不存在");
            }

            var prevValue = setting.SettingValue;

            switch (setting.SettingType)
            {
                case AppSettingTypes.字符串:
                    await _appSettingService.SetStringAsync(settingName, args.SettingValue);
                    break;
                case AppSettingTypes.布尔:
                    await _appSettingService.SetBooleanAsync(settingName, Convert.ToBoolean(args.SettingValue));
                    break;
                case AppSettingTypes.数字:
                    await _appSettingService.SetNumberAsync(settingName, Convert.ToDecimal(args.SettingValue));
                    break;
                default:
                    break;
            }

            _logger.Information("将参数 {settingName} 的值由 {prevValue} 改为 {value}", settingName, prevValue, args.SettingValue);
            await _opHelper.SaveOpAsync($"参数名 {settingName}，前值 {prevValue}，新值 {args.SettingValue}", settingName, prevValue, args.SettingValue);

            return this.Success();
        }

        /// <summary>
        /// 更改参数
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("create-app-setting")]
        [OperationType(OperationTypes.更改系统参数)]
        public async Task<ApiData> Create(CreateAppSettingArgs args)
        {
            var settingName = args.SettingName?.Trim();
            if (string.IsNullOrEmpty(settingName))
            {
                throw new InvalidOperationException("参数名不能为空。");
            }

            var setting = await _appSettingService.GetAsync(settingName);
            if (setting != null)
            {
                throw new InvalidOperationException("餐宿已存在。");
            }

            switch (args.SettingType)
            {
                case AppSettingTypes.字符串:
                    await _appSettingService.SetStringAsync(settingName, args.SettingValue);
                    break;
                case AppSettingTypes.布尔:
                    await _appSettingService.SetBooleanAsync(settingName, Convert.ToBoolean(args.SettingValue));
                    break;
                case AppSettingTypes.数字:
                    await _appSettingService.SetNumberAsync(settingName, Convert.ToDecimal(args.SettingValue));
                    break;
                default:
                    break;
            }

            _logger.Information("创建参数 {settingName}，值为 {value}", settingName, args.SettingValue);
            await _opHelper.SaveOpAsync($"参数名 {settingName}，值 {args.SettingValue}", settingName, args.SettingValue);

            return this.Success();
        }

        #region 用户和角色

        /// <summary>
        /// 用户列表
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-user-list")]
        [OperationType(OperationTypes.查看用户)]
        public async Task<ListData<UserInfo>> GetUserList([FromQuery] UserListArgs args)
        {
            var pagedList = await _session.Query<User>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

            return this.ListData(pagedList, x => new UserInfo
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
        [HttpPost("create-user")]
        [OperationType(OperationTypes.创建用户)]
        public async Task<ApiData> CreateUser(CreateUserArgs args)
        {
            args.UserName = args.UserName.Trim();
            var dup = await _session.Query<User>().AnyAsync(x => x.UserName == args.UserName);
            if (dup)
            {
                throw new InvalidOperationException("用户名重复。");
            }
            User user = new User
            {
                UserName = args.UserName,
                PasswordSalt = Guid.NewGuid().ToString()
            };
            user.PasswordHash = HashPassword(args.Password, user.PasswordSalt);

            SetRoles(user, args.Roles);

            await _session.SaveAsync(user);
            _ = await _opHelper.SaveOpAsync("UserId: {0}，用户名：{1}", user.UserId, user.UserName);
            
            return this.Success();
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id">用户id</param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("delete-user/{id}")]
        [OperationType(OperationTypes.删除用户)]
        public async Task<ApiData> DeleteUser(int id)
        {
            User user = await _session.GetAsync<User>(id);
            if (user.IsBuiltIn)
            {
                throw new InvalidOperationException("不能删除内置用户。");
            }

            await _session.DeleteAsync(user);
            await _opHelper.SaveOpAsync("UserId: {0}，用户名：{1}", user.UserId, user.UserName);
            return this.Success();
        }

        /// <summary>
        /// 编辑用户
        /// </summary>
        /// <param name="id">用户id</param>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("update-user/{id}")]
        [OperationType(OperationTypes.编辑用户)]
        public async Task<ApiData> UpdateUser(int id, UpdateUserArgs args)
        {
            User user = await _session.GetAsync<User>(id);
            if (user == null)
            {
                throw new InvalidOperationException("用户不存在。");
            }

            if (_session.Query<User>().Any(x => x.UserId != id && x.UserName == args.UserName))
            {
                throw new InvalidOperationException("用户名重复。");
            }

            user.UserName = args.UserName;

            if (string.IsNullOrEmpty(args.Password) == false)
            {
                user.PasswordSalt = Guid.NewGuid().ToString();
                user.PasswordHash = HashPassword(args.Password, user.PasswordSalt);
            }

            SetRoles(user, args.Roles);

            await _session.SaveAsync(user);

            await _opHelper.SaveOpAsync("UserId: {0}，用户名：{1}", user.UserId, user.UserName);

            return this.Success();
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
            using MD5 md5Hasher = MD5.Create();

            byte[] data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }


        /// <summary>
        /// 角色列表
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-role-list")]
        [OperationType(OperationTypes.查看角色)]
        public async Task<ListData<RoleInfo>> GetRoleList([FromQuery] RoleListArgs args)
        {
            var pagedList = await _session.Query<Role>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return this.ListData(pagedList, x => new RoleInfo
            {
                RoleId = x.RoleId,
                RoleName = x.RoleName,
                IsBuiltIn = x.IsBuiltIn,
                AllowedOpTypes = x.AllowedOpTypes,
                ctime = x.ctime,
                Comment = x.Comment,
            });
        }

        /// <summary>
        /// 获取角色的选项列表
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-role-options")]
        public async Task<OptionsData<RoleInfo>> GetRoleOptions()
        {
            var items = await _session.Query<Role>()
                .Select(x => new RoleInfo
                {
                    RoleId = x.RoleId,
                    RoleName = x.RoleName,
                    IsBuiltIn = x.IsBuiltIn,
                })
                .ToListAsync();
            return this.OptionsData(items);
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("create-role")]
        [OperationType(OperationTypes.创建角色)]
        public async Task<ApiData> CreateRole(CreateUpdateRoleArgs args)
        {
            if (_session.Query<Role>().Any(x => x.RoleName == args.RoleName))
            {
                throw new InvalidOperationException("角色名重复。");
            }

            Role role = new Role
            {
                RoleName = args.RoleName,
                Comment = args.Comment
            };

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
        [HttpPost("delete-role/{id}")]
        [OperationType(OperationTypes.删除角色)]
        public async Task<ApiData> DeleteRole(int id)
        {
            Role role = await _session.GetAsync<Role>(id);
            if (role.IsBuiltIn)
            {
                throw new InvalidOperationException("不能删除内置角色。");
            }

            var users = await _session.Query<User>().Where(x => x.Roles.Contains(role)).ToListAsync();
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
        [HttpPost("update-role/{id}")]
        [OperationType(OperationTypes.编辑角色)]
        public async Task<ApiData> UpdateRole(int id, CreateUpdateRoleArgs args)
        {
            Role role = await _session.GetAsync<Role>(id);
            if (role == null)
            {
                throw new InvalidOperationException("角色不存在。");
            }
            if (_session.Query<Role>().Any(x => x.RoleId != id && x.RoleName == args.RoleName))
            {
                throw new InvalidOperationException("角色名重复。");
            }

            role.RoleName = args.RoleName;
            role.Comment = args.Comment;

            await _session.SaveAsync(role);
            await _opHelper.SaveOpAsync("RoleId: {0}，角色名：{1}", role.RoleId, role.RoleName);

            return this.Success();
        }

        #endregion

        /// <summary>
        /// 操作记录列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet("get-op-list")]
        [DebugShowArgs]
        [AutoTransaction]
        public async Task<ListData<OpListInfo>> GetOpList([FromQuery] OpListArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Sort))
            {
                args.Sort = "opId desc";
            }
            var pagedList = await _session.Query<Op>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return this.ListData(pagedList, x => new OpListInfo
            {
                OpId = x.OpId,
                ctime = x.ctime,
                cuser = x.cuser,
                OperationType = x.OperationType,
                Url = x.Url,
                Comment = x.Comment
            });
        }

    }
}
