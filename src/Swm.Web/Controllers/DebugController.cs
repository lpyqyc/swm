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
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 提供调试功能。
    /// </summary>
    [ApiController]
    [Route("[controller]")]
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
        [HttpPost]
        [Route("simulate-request")]
        [OperationType(OperationTypes.模拟请求)]
        [AutoTransaction]
        public async Task<OperationResult> SimulateRequestAsync(RequestInfo requestInfo)
        {
            _logger.Debug("正在模拟请求");

            await _eventBus.FireEventAsync(EventTypes.PreRequest, requestInfo);
            await _eventBus.FireEventAsync(EventTypes.Request, requestInfo);

            var op = await _opHelper.SaveOpAsync("模拟请求信息【{0}】", requestInfo);

            _logger.Debug("已模拟请求");

            return new OperationResult
            {
                Success = true,
                Message = "操作成功",
            };
        }

        /// <summary>
        /// 模拟完成
        /// </summary>
        /// <param name="taskInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("simulate-task-completion")]
        [OperationType(OperationTypes.模拟完成)]
        [AutoTransaction]
        public async Task<OperationResult> SimulateTaskCompletionAsync(CompletedTaskInfo taskInfo)
        {
            _logger.Debug("正在模拟任务完成。");

            await _eventBus.FireEventAsync(EventTypes.TaskCompleted, taskInfo);

            _logger.Debug("已模拟任务完成。");

            var op = await _opHelper.SaveOpAsync("模拟完成信息【{0}】。", taskInfo);

            return new OperationResult
            {
                Success = true,
                Message = "操作成功",
            };
        }

    }


}
