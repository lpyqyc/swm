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
using Swm.Model.Cfg;
using Swm.Model.Mappings;
using System;
using System.Reflection;

namespace Swm.Model
{
    /// <summary>
    /// 用于向容器注册类型的扩展方法。在 Startup.ConfigureContainer 方法中调用。
    /// </summary>
    public static class SwmContainerBuilderExtensions
    {
        static ILogger _logger = Log.ForContext(typeof(SwmContainerBuilderExtensions));

        public static void AddSwm(this ContainerBuilder builder, SwmOptions options)
        {
            builder.AddModelMapper<SwmModelMapper>();

            RegisterBySuffix("Factory");
            RegisterBySuffix("Helper");
            RegisterBySuffix("Provider");
            RegisterBySuffix("Service");
            RegisterByPrefix("SSRule");
            RegisterByPrefix("SDRule");

            builder.RegisterInstance(new RegexPalletCodeValidator(options.PalletCodePattern))
                .AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterType<FakeTaskSender>().As<ITaskSender>();
            builder.RegisterType<DefaultOutboundOrderAllocator>().As<IOutboundOrderAllocator>();

            ConfigureRequestHandlers(builder, options.RequestHandlers);
            ConfigureCompletedTaskHandlers(builder, options.CompletedTaskHandlers);

            void RegisterBySuffix(string suffix)
            {
                var asm = Assembly.GetExecutingAssembly();
                builder.RegisterAssemblyTypes(asm)
                    .Where(t => t.IsAbstract == false && t.Name.EndsWith(suffix, StringComparison.Ordinal))
                    .AsImplementedInterfaces()
                    .AsSelf();
                _logger.Information("已注册后缀 {suffix}", suffix);
            }


            void RegisterByPrefix(string prefix)
            {
                var asm = Assembly.GetExecutingAssembly();
                builder.RegisterAssemblyTypes(asm)
                    .Where(t => t.IsAbstract == false && t.Name.StartsWith(prefix, StringComparison.Ordinal))
                    .AsImplementedInterfaces()
                    .AsSelf();
                _logger.Information("已注册前缀 {suffix}", prefix);
            }
        }

        static void ConfigureRequestHandlers(ContainerBuilder builder, RequestHandler[] handlers)
        {
            _logger.Information("正在配置请求处理程序");

            foreach (var handlerConfig in handlers)
            {
                _logger.Information("请求类型 {requestType} --> 处理程序 {handlerType}", handlerConfig.RequestType, handlerConfig.HandlerType);

                var handlerType = Type.GetType(handlerConfig.HandlerType);

                if (handlerType == null)
                {
                    string msg = string.Format("配置错误，请求处理程序类型 {0} 不存在。", handlerConfig.HandlerType);
                    throw new ApplicationException(msg);
                }
                if (typeof(IRequestHandler).IsAssignableFrom(handlerType) == false)
                {
                    throw new ApplicationException($"配置错误，类型【{handlerType}】未实现【{typeof(IRequestHandler)}】接口。");
                }

                string requestType = handlerConfig.RequestType?.Trim();
                if (string.IsNullOrEmpty(requestType))
                {
                    throw new ApplicationException("配置错误，请求类型不能为空。");
                }
                builder.RegisterType(handlerType)
                    .As<IRequestHandler>()
                    .PropertiesAutowired()
                    .WithMetadata<RequestHandlerMeta>(cfg =>
                        cfg.For(meta => meta.RequestType, handlerConfig.RequestType));
            }

            _logger.Information("已配置请求处理程序。");
        }


        static void ConfigureCompletedTaskHandlers(ContainerBuilder builder, CompletedTaskHandler[] handlers)
        {
            _logger.Information("正在配置完成处理程序");

            foreach (var handlerConfig in handlers)
            {
                _logger.Information("任务类型 {taskType} --> 处理程序 {handlerType}", handlerConfig.TaskType, handlerConfig.HandlerType);

                var handlerType = Type.GetType(handlerConfig.HandlerType);

                if (handlerType == null)
                {
                    string msg = string.Format("配置错误，完成处理程序类型 {0} 不存在。", handlerConfig.HandlerType);
                    throw new ApplicationException(msg);
                }
                if (typeof(ICompletedTaskHandler).IsAssignableFrom(handlerType) == false)
                {
                    throw new ApplicationException($"配置错误，类型【{handlerType}】未实现【{typeof(ICompletedTaskHandler)}】接口。");
                }

                string taskType = handlerConfig.TaskType?.Trim();
                if (string.IsNullOrEmpty(taskType))
                {
                    throw new ApplicationException("配置错误，任务类型不能为空。");
                }
                builder.RegisterType(handlerType)
                    .As<ICompletedTaskHandler>()
                    .PropertiesAutowired()
                    .WithMetadata<CompletedTaskHandlerMeta>(cfg =>
                        cfg.For(meta => meta.TaskType, handlerConfig.TaskType));
            }

            _logger.Information("已配置完成处理程序");
        }

    }
}
