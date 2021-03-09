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
        static ILogger _logger = Log.ForContext<PalletizationModule>();

        /// <summary>
        /// 为 <see cref="RegexPalletCodeValidator"/> 配置正则表达式。
        /// </summary>
        public string PalletCodePattern { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.AddModelMapper<Mapper>();

            RegisterBySuffix("Factory");
            RegisterBySuffix("Helper");
            RegisterBySuffix("Provider");
            RegisterBySuffix("Service");

            builder.RegisterInstance(new RegexPalletCodeValidator(PalletCodePattern))
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
        }

    }

}