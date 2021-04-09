using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Swm.Web
{
    // TODO 文件位置
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }

    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// 是否内置用户
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// 刷新令牌的生成时间
        /// </summary>
        public DateTime? RefreshTokenTime { get; set; }

        /// <summary>
        /// 刷新令牌的过期时间
        /// </summary>
        public DateTime? RefreshTokenExpireTime { get; set; }

    }

    public class ApplicationRole : IdentityRole
    {
        public bool IsBuiltIn { get; set; }

        public string? Comment { get; set; }
    }
}
