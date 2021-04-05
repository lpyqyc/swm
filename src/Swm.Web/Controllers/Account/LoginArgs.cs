﻿// 参考：https://blog.csdn.net/sd7o95o/article/details/114504446

using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 登录的参照参数
    /// </summary>
    public class LoginArgs
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        public string? UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required]
        public string? Password { get; set; }
    }

}