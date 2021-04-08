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
        internal Func<Unitload>? _unitloadFactory;
        internal Func<UnitloadItem>? _unitloadItemFactory;
        internal Func<UnitloadSnapshot>? _unitloadSnapshotFactory;
        internal Func<UnitloadItemSnapshot>? _unitloadItemSnapshotFactory;
        internal IPalletCodeValidator?  palletCodeValidator;
        internal XModelMapper? _extensionModelMapper;
        internal Type? _unitloadStorageInfoProviderType;

        internal PalletizationModuleBuilder()
        {
        }


        public PalletizationModuleBuilder UseUnitload<T>()
            where T : Unitload, new()
        {
            _unitloadFactory = () => new T();
            return this;
        }

        public PalletizationModuleBuilder UseUnitloadItem<T>()
            where T : UnitloadItem, new()
        {
            _unitloadItemFactory = () => new T();
            return this;
        }

        public PalletizationModuleBuilder UseUnitloadSnapshot<T>()
            where T : UnitloadSnapshot, new()
        {
            _unitloadSnapshotFactory = () => new T();
            return this;
        }

        public PalletizationModuleBuilder UseUnitloadItemSnapshot<T>()
            where T : UnitloadItemSnapshot, new()
        {
            _unitloadItemSnapshotFactory = () => new T();
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
            this._unitloadStorageInfoProviderType = typeof(T);
            return this;
        }
        /// <summary>
        /// 如果使用子类扩展了实体模型，则使用此方法添加扩展部分的模型映射类，将实体添加到 NHibernate 中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extensionModelMapper"></param>
        /// <returns></returns>
        public PalletizationModuleBuilder AddExtensionModelMapper<T>(XModelMapper extensionModelMapper)
        {
            _extensionModelMapper = extensionModelMapper;
            return this;
        }
        internal PalletizationModule Build()
        {
            return new PalletizationModule(this);
        }
    }


}