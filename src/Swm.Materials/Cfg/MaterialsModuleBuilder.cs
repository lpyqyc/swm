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
using System.Linq;

namespace Swm.Materials
{
    public class MaterialsModuleBuilder
    {
        internal Type? _stockKeyType;

        internal Func<Material>? _materialFactory;
        internal Func<Flow>? _flowFactory;
        internal Func<Stock>? _stockFactory;
        internal Func<MonthlyReportItem>? _monthlyReportItemFactory;

        internal MaterialsModuleBuilder()
        {
        }


        public MaterialsModuleBuilder UseStockKey<TStockKey>() where TStockKey : StockKeyBase
        {
            _stockKeyType = typeof(TStockKey);
            return this;
        }


        public MaterialsModuleBuilder UseMaterial<T>() 
            where T : Material, new()
        {
            _materialFactory = () => new T();
            return this;
        }

        public MaterialsModuleBuilder UseFlow<T>()
            where T : Flow, new()
        {
            _flowFactory = () => new T();
            return this;
        }

        public MaterialsModuleBuilder UseStock<T>()
            where T : Stock, new()
        {
            _stockFactory = () => new T();
            return this;
        }

        public MaterialsModuleBuilder UseMonthlyReportItem<T>()
            where T : MonthlyReportItem, new()
        {
            _monthlyReportItemFactory = () => new T();
            return this;
        }


        internal MaterialsModule Build()
        {
            return new MaterialsModule(this);
        }
    }
}
