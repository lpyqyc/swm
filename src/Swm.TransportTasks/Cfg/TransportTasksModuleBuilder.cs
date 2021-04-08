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

using System;
using System.Collections.Generic;

namespace Swm.TransportTasks
{
    public class TransportTasksModuleBuilder
    {
        internal Dictionary<string, Type> _requestHandlerTypes = new Dictionary<string, Type>();
        internal Dictionary<string, Type> _completedTaskHandlerTypes = new Dictionary<string, Type>();
        internal Type? _taskSenderType;

        internal TransportTasksModuleBuilder()
        {
        }

        public TransportTasksModuleBuilder AddRequestHandler<T>(string requestType)
            where T : IRequestHandler
        {
            if (string.IsNullOrWhiteSpace(requestType))
            {
                throw new ArgumentException("请求类型不能为空", nameof(requestType));
            }
            _requestHandlerTypes.Add(requestType.Trim(), typeof(T));
            return this;
        }

        public TransportTasksModuleBuilder AddCompletedTaskHandler<T>(string taskType)
            where T : ICompletedTaskHandler
        {
            if (string.IsNullOrWhiteSpace(taskType))
            {
                throw new ArgumentException("任务类型不能为空", nameof(taskType));
            }
            _completedTaskHandlerTypes.Add(taskType.Trim(), typeof(T));
            return this;
        }

        public TransportTasksModuleBuilder UseTaskSender<T>()
            where T : ITaskSender
        {
            _taskSenderType = typeof(T);
            return this;
        }


        internal TransportTasksModule Build()
        {
            return new TransportTasksModule(this);
        }
    }
}
