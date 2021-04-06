using Arctic.AspNetCore;
using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 提供用户和角色管理
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsrController : ControllerBase
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly RoleManager<ApplicationRole> _roleManager;
        readonly ApplicationDbContext _applicationDbContext;
        readonly ILogger _logger;

        /// <summary>
        /// 初始化新实例。
        /// </summary>
        /// <param name="logger"></param>
        public UsrController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger logger
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }




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
            var pagedList = await SearchAsync(_applicationDbContext.Users, args.Filter, args.Sort, args.Current, args.PageSize);

            Dictionary<ApplicationUser, UserInfo> userInfos = new();
            foreach (var user in pagedList.List)
            {
                UserInfo userInfo = new();
                userInfo.UserId = user.Id;
                userInfo.UserName = user.UserName;
                userInfo.IsBuiltIn = user.IsBuiltIn;
                userInfo.Roles = await _userManager.GetRolesAsync(user);

                userInfos.Add(user, userInfo);
            }

            return this.ListData(pagedList, x => userInfos[x]);
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
            if (_applicationDbContext.Users.Any(x => x.UserName == args.UserName))
            {
                throw new InvalidOperationException("用户名重复。");
            }

            var user = new ApplicationUser { UserName = args.UserName };
            var result = await _userManager.CreateAsync(user, args.Password);
            if (result.Succeeded == false)
            {
                throw new InvalidOperationException(string.Join(", " , result.Errors.Select(x => x.Description)));
            }
            await _userManager.AddToRolesAsync(user, args.Roles);

            
            return this.Success();
        }



        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("delete-user/{id}")]
        [OperationType(OperationTypes.删除用户)]
        public async Task<ApiData> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user.IsBuiltIn)
            {
                throw new InvalidOperationException("不能删除内置用户。");
            }

            await _userManager.DeleteAsync(user);
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
        public async Task<ApiData> UpdateUser(string id, UpdateUserArgs args)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                throw new InvalidOperationException("用户不存在。");
            }

            if (_applicationDbContext.Users.Any(x => x.Id != id && x.UserName == args.UserName))
            {
                throw new InvalidOperationException("用户名重复。");
            }

            user.UserName = args.UserName;

            if (string.IsNullOrEmpty(args.Password) == false)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ChangePasswordAsync(user, token, args.Password);
            }

            await SetRolesAsync(user, args.Roles);


            return this.Success();
        }


        private async Task SetRolesAsync(ApplicationUser user, string[]? roles)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));

            if (user.UserName == "admin")
            {
                await _userManager.AddToRoleAsync(user, "admin");
            }

            if (roles != null)
            {
                await _userManager.AddToRolesAsync(user, roles);
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
            var pagedList = await SearchAsync(_applicationDbContext.Roles, args.Filter, args.Sort, args.Current, args.PageSize);

            Dictionary<ApplicationRole, RoleInfo> roleInfos = new();
            foreach (var role in pagedList.List)
            {
                RoleInfo roleInfo = new();
                roleInfo.RoleId = role.Id;
                roleInfo.RoleName = role.Name;
                roleInfo.IsBuiltIn = role.IsBuiltIn;
                roleInfo.AllowedOpTypes = new string[0];  // TODO 
                roleInfos.Add(role, roleInfo);
            }

            return this.ListData(pagedList, x => roleInfos[x]);
        }

        /// <summary>
        /// 获取角色的选项列表
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-role-options")]
        public async Task<OptionsData<RoleInfo>> GetRoleOptions()
        {
            var items = await _applicationDbContext.Roles
                .Select(x => new RoleInfo
                {
                    RoleId = x.Id,
                    RoleName = x.Name,
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
            args.RoleName = args.RoleName.Trim();
            if (_applicationDbContext.Roles.Any(x => x.Name == args.RoleName))
            {
                throw new InvalidOperationException("角色名重复。");
            }
            var role = new ApplicationRole { Name = args.RoleName, Comment = args.Comment };
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded == false)
            {
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(x => x.Description)));
            }

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
        public async Task<ApiData> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role.IsBuiltIn)
            {
                throw new InvalidOperationException("不能删除内置用户。");
            }
            
            var users = await _userManager.GetUsersInRoleAsync(role.Name);
            foreach (var user in users)
            {
                await _userManager.RemoveFromRoleAsync(user, role.Name);
            }
            await _roleManager.DeleteAsync(role);


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
        public async Task<ApiData> UpdateRole(string id, CreateUpdateRoleArgs args)
        {
            ApplicationRole role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                throw new InvalidOperationException("角色不存在。");
            }

            if (_applicationDbContext.Roles.Any(x => x.Id != id && x.Name == args.RoleName))
            {
                throw new InvalidOperationException("角色名重复。");
            }

            role.Name = args.RoleName;
            role.Comment = args.Comment;

            await _roleManager.UpdateAsync(role);

            return this.Success();
        }


        private static async Task<PagedList<T>> SearchAsync<T>(IQueryable<T> q, Func<IQueryable<T>, IQueryable<T>> filter, string? sort, int? current, int? pageSize)
        {
            if (current == null || current.Value < 1)
            {
                current = 1;
            }
            if (pageSize == null || pageSize.Value < 1)
            {
                pageSize = 20;
            }
            q = filter(q);

            if (!string.IsNullOrWhiteSpace(sort))
            {
                q = q.OrderBy(sort);
            }

            var totalItemCount = q.Count();

            if (totalItemCount == 0)
            {
                return new PagedList<T>(new List<T>(), 1, pageSize.Value, 0);
            }

            int start = (current.Value - 1) * pageSize.Value;
            var list = await q.Skip(start)
                .Take(pageSize.Value)
                .ToListAsync()
                .ConfigureAwait(false);
            return new PagedList<T>(list, 1, pageSize.Value, totalItemCount);

        }

    }
}
