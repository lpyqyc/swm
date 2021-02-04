// Copyright 2020 王建军
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
using Arctic.EventBus;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Swm.Model;
using System;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 提供调试功能。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        readonly ILogger _logger;
        readonly SimpleEventBus _eventBus;
        readonly OpHelper _opHelper;

        /// <summary>
        /// 初始化新实例。
        /// </summary>
        /// <param name="eventBus"></param>
        /// <param name="opHelper"></param>
        /// <param name="logger"></param>
        public DebugController(SimpleEventBus eventBus, OpHelper opHelper, ILogger logger)
        {
            _opHelper = opHelper;
            _logger = logger;
            _eventBus = eventBus;
        }


        /// <summary>
        /// 模拟请求
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <returns></returns>
        [HttpPost("simulate-request")]
        [OperationType(OperationTypes.模拟请求)]
        [AutoTransaction]
        public async Task<ApiData> SimulateRequest(RequestInfo requestInfo)
        {
            _logger.Information("正在模拟请求 {requestInfo}", requestInfo);
            await _eventBus.FireEventAsync(EventTypes.PreRequest, requestInfo);
            await _eventBus.FireEventAsync(EventTypes.Request, requestInfo);
            var op = await _opHelper.SaveOpAsync("{0}", requestInfo);
            _logger.Information("模拟请求成功");
            return this.Success();
        }

        /// <summary>
        /// 模拟完成
        /// </summary>
        /// <param name="taskInfo"></param>
        /// <returns></returns>
        [HttpPost("simulate-completion")]
        [OperationType(OperationTypes.模拟完成)]
        [AutoTransaction]
        public async Task<ApiData> SimulateCompletion(CompletedTaskInfo taskInfo)
        {
            _logger.Information("正在模拟任务完成 {taskInfo}", taskInfo);
            await _eventBus.FireEventAsync(EventTypes.TaskCompleted, taskInfo);
            _logger.Information("已模拟任务完成");
            var op = await _opHelper.SaveOpAsync("{0}", taskInfo);
            return this.Success();
        }
    }
}
