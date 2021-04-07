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
using Swm.Locations.Mappings;
using System;
using System.Reflection;

namespace Swm.Locations
{
    /// <summary>
    /// 
    /// </summary>
    public class LocationsModule : Autofac.Module
    {
        static ILogger _logger = Log.ForContext<LocationsModule>();

        internal LocationsModule()
        {

        }

        public Type? LocationType { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.AddModelMapper<Mapper>();

            builder.RegisterType(LocationType ?? throw new InvalidOperationException("未提供 LocationType")).As<Location>().InstancePerDependency();

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

        }

    }
}
