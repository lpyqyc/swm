﻿// 参考：https://blog.csdn.net/sd7o95o/article/details/114504446

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
    public class AccountDebugController : ControllerBase
    {
        readonly IOptions<JwtSetting> _jwtSetting;
        readonly ILogger _logger;

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="logger"></param>
        public AccountDebugController(
            IOptions<JwtSetting> jwtSetting,
            ILogger logger)
        {
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

            var result = model.UserName == "debug";
            if (result)
            {

                //创建用户身份标识，可按需要添加更多信息
                bool admin = true;
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("id", "1", ClaimValueTypes.String), // 用户id
                    new Claim("name", "debug"), // 用户名
                    new Claim("admin", admin.ToString() ,ClaimValueTypes.Boolean), // 是否是管理员
                };

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
                    currentAuthority = new string[] { "fa", "admin", "user" },
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
            if (args.Password == "1")
            {
                throw new Exception("修改密码失败");
            }
            return this.Success();
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
            AntProCurrentUserInfo userInfo = new AntProCurrentUserInfo();
            userInfo.Name = "user.Name";
            userInfo.Userid = "user.Id";
            userInfo.Email = "user.Email";
            userInfo.Phone = "user.PhoneNumber";


            return userInfo;
        }

    }



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
