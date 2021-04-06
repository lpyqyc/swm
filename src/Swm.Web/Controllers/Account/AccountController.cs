// 参考：https://blog.csdn.net/sd7o95o/article/details/114504446

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 登录和已登录用户的相关功能
    /// </summary>
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        readonly IOptions<JwtSetting> _jwtSetting;
        readonly UserManager<ApplicationUser> _userManager;
        readonly RoleManager<ApplicationRole> _roleManager;
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly ILogger _logger;

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="logger"></param>
        public AccountController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<JwtSetting> jwtSetting,
            ILogger logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _jwtSetting = jwtSetting;
            _logger = logger;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [DebugShowArgs]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginArgs model)
        {
            // TODO 增加操作记录
            // TODO 打印日志

            if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
            {
                return Ok(new
                {
                    status = "error",
                    message = "用户名和密码不能为空"
                });
            }

            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                ApplicationUser user = await _userManager.FindByNameAsync(model.UserName);
                var roles = await _userManager.GetRolesAsync(user);

                //创建用户身份标识，可按需要添加更多信息
                bool admin = await _userManager.IsInRoleAsync(user, "admin");
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id, ClaimValueTypes.String), // 用户id
                    new Claim(ClaimTypes.Name, user.UserName), // 用户名
                    new Claim("admin", admin.ToString() ,ClaimValueTypes.Boolean), // 是否是管理员
                };

                claims.AddRange(await _userManager.GetClaimsAsync(user));

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));

                    ApplicationRole appRole = await _roleManager.FindByNameAsync(role);
                    claims.AddRange(await _roleManager.GetClaimsAsync(appRole));
                }

                var key = Encoding.UTF8.GetBytes(_jwtSetting.Value.SecurityKey);

                //创建令牌
                var token = new JwtSecurityToken(
                  issuer: _jwtSetting.Value.Issuer,
                  audience: _jwtSetting.Value.Audience,
                  signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                  claims: claims,
                  notBefore: DateTime.Now,
                  expires: DateTime.Now.AddMinutes(_jwtSetting.Value.TokenExpiry)
                );

                string jwt = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new
                {
                    status = "ok",
                    token = jwt,
                    type = "Bearer",
                    currentAuthority = roles
                });
            }

            return Ok(new
            {
                status = "error",
                message = "用户名或密码错误"
            });

        }



        /// <summary>
        /// 已登录用户更改自己的密码
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [DebugShowArgs]
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ApiData> ChangePassword(ChangePasswordArgs args)
        {
            ApplicationUser? user = await _userManager.FindByNameAsync(User?.Identity?.Name);

            if (user == null)
            {
                throw new Exception("用户不存在");
            }

            var result = await _userManager.ChangePasswordAsync(user, args.OriginalPassword, args.Password);
            if (result.Succeeded)
            {
                return this.Success();
            }

            return this.Failure(string.Join(", ", result.Errors.Select(x => x.Description)));
        }

        /// <summary>
        /// 或者当前登录用户信息
        /// </summary>
        /// <returns></returns>
        [DebugShowArgs]
        [HttpGet("currentUser")]
        [Authorize]
        public async Task<AntProCurrentUserInfo> GetCurrentUser()
        {

            ApplicationUser? user = await _userManager.FindByNameAsync(User?.Identity?.Name);

            AntProCurrentUserInfo userInfo = new AntProCurrentUserInfo();
            if (user != null)
            {
                userInfo.Name = user.UserName;
                userInfo.Userid = user.Id;
                userInfo.Email = user.Email;
                userInfo.Phone = user.PhoneNumber;
            }

            return userInfo;
        }

    }


    // TODO 处理下面的代码
    public class ClaimRequirementAttribute : TypeFilterAttribute
    {
        public ClaimRequirementAttribute(string claimType, string claimValue) : base(typeof(ClaimRequirementFilter))
        {
            Arguments = new object[] { new Claim(claimType, claimValue) };
        }
    }

    public class ClaimRequirementFilter : IAuthorizationFilter
    {
        readonly Claim _claim;

        public ClaimRequirementFilter(Claim claim)
        {
            _claim = claim;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var hasClaim = context.HttpContext.User.Claims.Any(c => c.Type == _claim.Type && c.Value == _claim.Value);
            if (!hasClaim)
            {
                context.Result = new ForbidResult();
            }
        }
    }

}
