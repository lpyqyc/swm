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
    public class MaterialsModule : Autofac.Module
    {
        static ILogger _logger = Log.ForContext<MaterialsModule>();
        
        public MaterialsConfig? MaterialsConfig { get; set; }

        public Type? MaterialType { get; set; }
        public Type? StockKeyType { get; set; }
        public Type? StockType { get; set; }
        public Type? FlowType { get; set; }
        public Type? MonthlyReportItemType { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.AddModelMapper<Mapper>();

            builder.RegisterInstance(MaterialsConfig ?? throw new InvalidOperationException("未提供 MaterialsConfig"));
            builder.RegisterType(MaterialType ?? throw new InvalidOperationException("未提供 MaterialType")).As<Material>().InstancePerDependency();
            builder.RegisterType(FlowType ?? throw new InvalidOperationException("未提供 FlowType")).As<Flow>().InstancePerDependency();
            builder.RegisterType(StockType ?? throw new InvalidOperationException("未提供 StockType")).As<Stock>().InstancePerDependency();
            builder.RegisterType(MonthlyReportItemType ?? throw new InvalidOperationException("未提供 MonthlyReportItemType")).As<MonthlyReportItem>().InstancePerDependency();

            RegisterBySuffix("Factory");
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
