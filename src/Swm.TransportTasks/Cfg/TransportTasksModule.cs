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

using Arctic.NHibernateExtensions;
using Autofac;
using Serilog;
using Swm.TransportTasks.Mappings;
using System;
using System.Collections.Generic;

namespace Swm.TransportTasks
{
    /// <summary>
    /// 
    /// </summary>
    public class TransportTasksModule : Module
    {
        static ILogger _logger = Log.ForContext<TransportTasksModule>();

        TransportTasksModuleBuilder _moduleBuilder;

        internal TransportTasksModule(TransportTasksModuleBuilder moduleBuilder)
        {
            _moduleBuilder = moduleBuilder;
        }


        protected override void Load(ContainerBuilder builder)
        {
            builder.AddModelMapper(new Mapper());
            builder.RegisterType(_moduleBuilder._taskSenderType).AsImplementedInterfaces();
            builder.RegisterType<TaskHelper>();

            ConfigureRequestHandlers(builder);
            ConfigureCompletedTaskHandlers(builder);
        }

        void ConfigureRequestHandlers(ContainerBuilder builder)
        {
            _logger.Information("正在配置请求处理程序");


            foreach (var (requestType, handlerType) in _moduleBuilder._requestHandlerTypes)
            {
                _logger.Debug("正在配置请求处理程序：请求类型 {requestType} --> 处理程序类型 {handlerType}", requestType, handlerType);

                if (string.IsNullOrEmpty(requestType))
                {
                    throw new ApplicationException("配置错误，请求类型不能为空。");
                }

                if (handlerType == null)
                {
                    throw new ApplicationException("配置错误，处理程序类型不能为空。");
                }

                builder.RegisterType(handlerType)
                    .Keyed<IRequestHandler>(requestType);

                _logger.Information("已配置请求处理程序：请求类型 {requestType} --> 处理程序类型 {handlerType}", requestType, handlerType);

            }

            _logger.Information("已配置请求处理程序。");
        }


        void ConfigureCompletedTaskHandlers(ContainerBuilder builder)
        {
            _logger.Information("正在配置完成处理程序");

            foreach (var (taskType, handlerType) in _moduleBuilder._completedTaskHandlerTypes)
            {
                _logger.Debug("正在配置完成处理程序：任务类型 {taskType} --> 处理程序类型 {handlerType}", taskType, handlerType);

                if (string.IsNullOrEmpty(taskType))
                {
                    throw new ApplicationException("配置错误，任务类型不能为空。");
                }

                if (handlerType == null)
                {
                    throw new ApplicationException("配置错误，处理程序类型不能为空。");
                }



                builder.RegisterType(handlerType)
                    .Keyed<ICompletedTaskHandler>(taskType);

                _logger.Information("已配置完成处理程序：任务类型 {taskType} --> 处理程序类型 {handlerType}", taskType, handlerType);

            }

            builder.RegisterInstance(new TaskTypesProvider
            {
                TaskTypes = _moduleBuilder._completedTaskHandlerTypes.Keys,
            }).SingleInstance();

            _logger.Information("已配置完成处理程序");
        }

    }
}
