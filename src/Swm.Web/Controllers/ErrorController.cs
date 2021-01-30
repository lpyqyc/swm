// 代码来源：https://docs.microsoft.com/zh-cn/aspnet/core/web-api/handle-errors?view=aspnetcore-5.0#exception-handler

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Swm.Web;
using System;

namespace Arctic.Web.Controllers
{
    /// <summary>
    /// 错误处理程序
    /// </summary>
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        /// <summary>
        /// 生产环境错误处理
        /// </summary>
        /// <returns></returns>
        [Route("/error")]
        public ApiData Error()
        {
            var errorContext = HttpContext.Features.Get<IExceptionHandlerFeature>();
            return this.Error2(errorContext.Error.Message);
        }

        /// <summary>
        /// 开发环境错误处理
        /// </summary>
        /// <param name="webHostEnvironment"></param>
        /// <returns></returns>
        [Route("/error-local-development")]
        public ApiData ErrorLocalDevelopment([FromServices] IWebHostEnvironment webHostEnvironment)
        {
            if (webHostEnvironment.EnvironmentName != "Development")
            {
                throw new InvalidOperationException("This shouldn't be invoked in non-development environments.");
            }

            var errorContext = HttpContext.Features.Get<IExceptionHandlerFeature>();
            return this.Error2(errorContext.Error.Message);
        }
    }
}
