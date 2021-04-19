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
        PalletizationModuleBuilder _moduleBuilder;
        
        internal PalletizationModule(PalletizationModuleBuilder moduleBuilder)
        {
            _moduleBuilder = moduleBuilder;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.AddModelMapperConfigurer(new ModelMapperConfigurer());
            if (_moduleBuilder.ExtensionModelMapperConfigurer != null)
            {
                builder.AddModelMapperConfigurer(_moduleBuilder.ExtensionModelMapperConfigurer);
            }

            builder.RegisterType<PalletizationHelper>();
            builder.RegisterType<UnitloadSnapshopHelper>();
            builder.RegisterType(_moduleBuilder.UnitloadStorageInfoProviderType).As<IUnitloadStorageInfoProvider>();

            builder.RegisterEntityFactory(_moduleBuilder.UnitloadFactory);
            builder.RegisterEntityFactory(_moduleBuilder.UnitloadItemFactory);
            builder.RegisterEntityFactory(_moduleBuilder.UnitloadSnapshotFactory);
            builder.RegisterEntityFactory(_moduleBuilder.UnitloadItemSnapshotFactory);
            builder.RegisterInstance(_moduleBuilder.palletCodeValidator ?? throw new())
                .As<IPalletCodeValidator>()
                .SingleInstance();
        }
    }

}