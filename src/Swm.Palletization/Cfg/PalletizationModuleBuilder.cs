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
using System;

namespace Swm.Palletization
{
    public class PalletizationModuleBuilder
    {
        public Func<Unitload> UnitloadFactory { get; private set; } = () => new Unitload();
        public Func<UnitloadItem> UnitloadItemFactory { get; private set; } = () => new UnitloadItem();
        public Func<UnitloadSnapshot> UnitloadSnapshotFactory { get; private set; } = () => new UnitloadSnapshot();
        public Func<UnitloadItemSnapshot> UnitloadItemSnapshotFactory { get; private set; } = () => new UnitloadItemSnapshot();

        /// <summary>
        /// 获取实体扩展部分（如果有）的映射配置程序
        /// </summary>
        public IModelMapperConfigurer? ExtensionModelMapperConfigurer { get; private set; }

        public IPalletCodeValidator?  palletCodeValidator;

        public Type UnitloadStorageInfoProviderType { get; private set; } = typeof(DefaultUnitloadStorageInfoProvider);

        internal PalletizationModuleBuilder()
        {
        }


        public PalletizationModuleBuilder UseUnitload<T>()
            where T : Unitload, new()
        {
            UnitloadFactory = () => new T();
            return this;
        }

        public PalletizationModuleBuilder UseUnitloadItem<T>()
            where T : UnitloadItem, new()
        {
            UnitloadItemFactory = () => new T();
            return this;
        }

        public PalletizationModuleBuilder UseUnitloadSnapshot<T>()
            where T : UnitloadSnapshot, new()
        {
            UnitloadSnapshotFactory = () => new T();
            return this;
        }

        public PalletizationModuleBuilder UseUnitloadItemSnapshot<T>()
            where T : UnitloadItemSnapshot, new()
        {
            UnitloadItemSnapshotFactory = () => new T();
            return this;
        }

        public PalletizationModuleBuilder UsePalletCodeValidator(IPalletCodeValidator palletCodeValidator)
        {
            this.palletCodeValidator = palletCodeValidator;
            return this;
        }

        public PalletizationModuleBuilder UseUnitloadStorageInfoProvider<T>()
            where T : IUnitloadStorageInfoProvider
        {
            this.UnitloadStorageInfoProviderType = typeof(T);
            return this;
        }

        /// <summary>
        /// 为实体的扩展部分（如果有）添加映射配置程序
        /// </summary>
        /// <param name="modelMapperConfigurer"></param>
        /// <returns></returns>
        public PalletizationModuleBuilder AddExtensionModelMapperConfigurer(IModelMapperConfigurer modelMapperConfigurer)
        {
            ExtensionModelMapperConfigurer = modelMapperConfigurer;
            return this;
        }



        internal PalletizationModule Build()
        {
            return new PalletizationModule(this);
        }
    }


}