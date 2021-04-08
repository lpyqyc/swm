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
using Swm.Palletization.Mappings;
using System;
using System.Reflection;

namespace Swm.Palletization
{
    /// <summary>
    /// 用于向容器注册类型的扩展方法。在 Startup.ConfigureContainer 方法中调用。
    /// </summary>
    public class PalletizationModule : Autofac.Module
    {

        static readonly ILogger _logger = Log.ForContext<PalletizationModule>();
        PalletizationModuleBuilder _moduleBuilder;
        
        internal PalletizationModule(PalletizationModuleBuilder moduleBuilder)
        {
            _moduleBuilder = moduleBuilder;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.AddModelMapper(new Mapper());

            RegisterBySuffix("Helper");
            RegisterBySuffix("Provider");

            RegisterFactory(_moduleBuilder._unitloadFactory);
            RegisterFactory(_moduleBuilder._unitloadItemFactory);
            RegisterFactory(_moduleBuilder._unitloadSnapshotFactory);
            RegisterFactory(_moduleBuilder._unitloadItemSnapshotFactory);
            builder.RegisterInstance(_moduleBuilder.palletCodeValidator ?? throw new())
                .AsImplementedInterfaces()
                .SingleInstance();

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