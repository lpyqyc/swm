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
using System.Linq;

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
            builder.AddModelMapperConfigurer(new ModelMapperConfigurer());

            builder.RegisterType<TaskHelper>();
            builder.RegisterType(_moduleBuilder.TaskSenderType).As<ITaskSender>();

            ConfigureRequestHandlers(builder);
            ConfigureCompletedTaskHandlers(builder);
        }

        void ConfigureRequestHandlers(ContainerBuilder builder)
        {
            _logger.Information("正在配置请求处理程序");


            foreach (var (requestType, handlerType) in _moduleBuilder.RequestHandlerTypes)
            {
                builder.RegisterType(handlerType ?? throw new())
                    .Keyed<IRequestHandler>(requestType ?? throw new());

                _logger.Information("  请求类型 {requestType} --> 处理程序类型 {handlerType}", requestType, handlerType);
            }

            _logger.Information("已配置请求处理程序。");
        }


        void ConfigureCompletedTaskHandlers(ContainerBuilder builder)
        {
            _logger.Information("正在配置完成处理程序");

            foreach (var (taskType, handlerType) in _moduleBuilder.CompletedTaskHandlerTypes)
            {
                builder.RegisterType(handlerType ?? throw new())
                    .Keyed<ICompletedTaskHandler>(taskType ?? throw new());

                _logger.Information("  任务类型 {taskType} --> 处理程序类型 {handlerType}", taskType, handlerType);
            }

            builder.RegisterInstance(new TaskTypesProvider
            {
                TaskTypes = _moduleBuilder.CompletedTaskHandlerTypes
                    .Select(x => x.taskType)
                    .ToList()
                    .AsReadOnly(),
            }).SingleInstance();

            _logger.Information("已配置完成处理程序");
        }

    }

}
