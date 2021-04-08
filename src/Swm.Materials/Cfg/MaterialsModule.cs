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
using Swm.Materials.Mappings;
using System;
using System.Reflection;

namespace Swm.Materials
{
    /// <summary>
    /// 
    /// </summary>
    internal class MaterialsModule : Autofac.Module
    {
        MaterialsModuleBuilder _moduleBuilder;
        internal MaterialsModule(MaterialsModuleBuilder moduleBuilder)
        {
            _moduleBuilder = moduleBuilder;
        }

        static ILogger _logger = Log.ForContext<MaterialsModule>();
        
        protected override void Load(ContainerBuilder builder)
        {
            builder.AddModelMapper(new Mapper());
            if (_moduleBuilder._extensionModelMapper != null)
            {
                builder.AddModelMapper(_moduleBuilder._extensionModelMapper);
            }

            RegisterFactory(_moduleBuilder._materialFactory);
            RegisterFactory(_moduleBuilder._flowFactory);
            RegisterFactory(_moduleBuilder._stockFactory);
            RegisterFactory(_moduleBuilder._monthlyReportItemFactory);

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
