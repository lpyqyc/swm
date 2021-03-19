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

using Arctic.EventBus;
using NHibernate;
using Serilog;
using Swm.TransportTasks.Cfg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.TransportTasks
{
    public class TaskCompletedEventHandler : IEventHandler
    {
        readonly IEnumerable<Lazy<ICompletedTaskHandler, CompletedTaskHandlerMeta>> _completedTaskHandlers;
        readonly ILogger _logger;
        readonly ISession _session;
        public TaskCompletedEventHandler(IEnumerable<Lazy<ICompletedTaskHandler, CompletedTaskHandlerMeta>> completedTaskHandlers, ISession session, ILogger logger)
        {
            _completedTaskHandlers = completedTaskHandlers;
            _session = session;
            _logger = logger;
        }
        public async Task ProcessAsync(string eventType, object? eventData)
        {
            await ProcessCompletedTaskAsync((CompletedTaskInfo)(eventData ?? throw new Exception("未提供任务信息")));
        }

        private async Task ProcessCompletedTaskAsync(CompletedTaskInfo taskInfo)
        {            
            string taskCode = taskInfo.TaskCode ?? throw new Exception("任务号不能为 null");

            _logger.Information("正在引发任务完成事件，任务类型 {taskType}，任务号 {taskCode}", taskInfo.TaskType, taskCode);

            if (taskInfo.TaskType == null)
            {
                string msg = string.Format("未提供任务类型。任务编号 {0}。", taskCode);
                throw new ApplicationException(msg);
            }

            ICompletedTaskHandler? handler = GetCompletedTaskHandler(taskInfo.TaskType);
            if (handler == null)
            {
                throw new ApplicationException($"没有找到可用的完成处理程序。任务类型：{taskInfo.TaskType}。");
            }

            _logger.Debug("完成处理程序是 {handlerType}。", handler.GetType());

            // 判断任务是否存在
            TransportTask task = await _session.Query<TransportTask>().GetTaskAsync(taskInfo.TaskCode);
            if (task != null)
            {
                await handler.ProcessCompletedTaskAsync(taskInfo, task);
            }
            else
            {
                _logger.Warning("未找到任务 {taskCode}，已忽略", taskInfo.TaskCode);
            }
        }

        /// <summary>
        /// 为指定的任务类型创建处理程序。
        /// </summary>
        /// <param name="taskType"></param>
        /// <returns></returns>
        private ICompletedTaskHandler? GetCompletedTaskHandler(string taskType)
        {
            var lazy = _completedTaskHandlers
                .Where(x => string.Equals(x.Metadata.TaskType, taskType, StringComparison.InvariantCultureIgnoreCase))
                .LastOrDefault();
            if (lazy == null)
            {
                return null;
            }
            return lazy.Value;
        }

    }

}
