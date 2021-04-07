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

        internal PalletizationModule Build()
        {
            return new PalletizationModule(this);
        }
    }


}