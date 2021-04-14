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

using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 提供用户和角色管理
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RgvController : ControllerBase
    {
        readonly ILogger _logger;
        readonly RgvService _rgvService;

        /// <summary>
        /// 初始化新实例。
        /// </summary>
        /// <param name="rgvService"></param>
        /// <param name="logger"></param>
        public RgvController(
            RgvService rgvService,
            ILogger logger
            )
        {
            _rgvService = rgvService;
            _logger = logger;
        }




        /// <summary>
        /// 连接到设备
        /// </summary>
        /// <returns></returns>
        [HttpGet("start")]
        public async Task<ApiData> Start()
        {
            foreach (var rgv in _rgvService.RgvList)
            {
                await rgv.ConnectAsync();
            }
            return this.Success();
        }

        /// <summary>
        /// 行走任务
        /// </summary>
        /// <param name="toStation"></param>
        /// <returns></returns>
        [HttpGet("walk")]
        public async Task<ApiData> Walk(int toStation)
        {
            await _rgvService.RgvList.First().WalkWithoutPalletAsync(toStation);
            return this.Success();
        }

        /// <summary>
        /// 查看统计数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("stat")]
        public ApiData Stat()
        {
            var str = _rgvService.RgvList.First().Statistics.ToString();
            return this.Success(str);
        }

        /// <summary>
        /// 与设备断开连接
        /// </summary>
        /// <returns></returns>
        [HttpGet("stop")]
        public async Task<ApiData> Stop()
        {
            foreach (var rgv in _rgvService.RgvList)
            {
                await rgv.DisconnectAsync();
            }

            return this.Success();
        }

    }
}
