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
using Swm.InboundOrders.Mappings;
using System;
using System.Reflection;

namespace Swm.InboundOrders
{
    /// <summary>
    /// 
    /// </summary>
    public class InboundOrdersModule : Autofac.Module
    {
        static ILogger _logger = Log.ForContext<InboundOrdersModule>();
        InboundOrdersModuleBuilder _moduleBuilder;
        internal InboundOrdersModule(InboundOrdersModuleBuilder moduleBuilder)
        {
            _moduleBuilder = moduleBuilder;
        }


        protected override void Load(ContainerBuilder builder)
        {
            builder.AddModelMapper(new Mapper());

            if (_moduleBuilder._extensionModelMapper != null)
            {
                builder.AddModelMapper(_moduleBuilder._extensionModelMapper);
            }

            RegisterFactory(_moduleBuilder._inboundOrderFactory);
            RegisterFactory(_moduleBuilder._inboundLineFactory);

            RegisterBySuffix("Helper");
            RegisterBySuffix("Provider");
            RegisterBySuffix("Service");

            void RegisterBySuffix(string suffix)
            {
                var asm = Assembly.GetExecutingAssembly();
                builder.RegisterAssemblyTypes(asm)
                    .Where(t => t.IsAbstract == false && t.Name.EndsWith(suffix, StringComparison.Ordinal))
                    .AsImplementedInterfaces()
                    .AsSelf();
                _logger.Information("已注册后缀 {suffix}", suffix);
            }

            void RegisterFactory<T>(Func<T>? factory) where T : notnull
            {
                if (factory != null)
                {
                    builder.Register(c => factory.Invoke()).As<T>().InstancePerDependency();
                }
            }
        }

    }
}
