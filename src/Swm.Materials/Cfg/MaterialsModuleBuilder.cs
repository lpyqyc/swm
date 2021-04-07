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
using System.Collections.Generic;

namespace Swm.Materials
{
    public class MaterialsModuleBuilder
    {
        internal List<MaterialTypeInfo> _materialTypes = new List<MaterialTypeInfo>();
        internal List<BizTypeInfo> _bizTypes = new List<BizTypeInfo>();
        internal List<StockStatusInfo> _stockStatus = new List<StockStatusInfo>();
        internal Type? _stockKeyType;

        public MaterialsModuleBuilder UseMaterialType(MaterialTypeInfo materialType)
        {
            _materialTypes.Add(materialType);
            return this;
        }

        public MaterialsModuleBuilder UseBizType(BizTypeInfo bizType)
        {
            _bizTypes.Add(bizType);
            return this;
        }

        public MaterialsModuleBuilder UseStockStatus(StockStatusInfo stockStatus)
        {
            _stockStatus.Add(stockStatus);
            return this;
        }

        public MaterialsModuleBuilder UseStockKey<TStockKey>() where TStockKey : StockKeyBase
        {
            _stockKeyType = typeof(TStockKey);
            return this;
        }

        public MaterialsModule Build()
        {
            return new MaterialsModule
            {
                MaterialsConfig = new MaterialsConfig(_materialTypes, _stockStatus, _stockKeyType, _bizTypes)
            };
        }
    }
}
