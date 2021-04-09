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

namespace Swm.Materials
{
    public class MaterialsModuleBuilder
    {
        internal Type? _stockKeyType;

        /// <summary>
        /// 获取实体扩展部分（如果有）的映射配置程序
        /// </summary>
        public IModelMapperConfigurer? ExtensionModelMapperConfigurer { get; private set; }

        /// <summary>
        /// 获取 <see cref="Material"/> 的实体工厂
        /// </summary>
        public Func<Material> MaterialFactory { get; private set; } = () => new Material();

        /// <summary>
        /// 获取 <see cref="Flow"/ 的实体工厂
        /// </summary>
        public Func<Flow> FlowFactory { get; private set; } = () => new Flow();

        /// <summary>
        /// 获取 <see cref="Stock"/ 的实体工厂
        /// </summary>
        public Func<Stock> StockFactory { get; private set; } = () => new Stock();

        /// <summary>
        /// 获取 <see cref="MonthlyReportItem"/ 的实体工厂
        /// </summary>
        public Func<MonthlyReportItem> MonthlyReportItemFactory { get; private set; } = () => new MonthlyReportItem();

        /// <summary>
        /// 获取 <see cref="IFifoProvider"/> 的类型
        /// </summary>
        public Type? FifoProviderType { get; private set; } = typeof(DefaultFifoProvider);

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
            MaterialFactory = () => new T();
            return this;
        }

        public MaterialsModuleBuilder UseFlow<T>()
            where T : Flow, new()
        {
            FlowFactory = () => new T();
            return this;
        }

        public MaterialsModuleBuilder UseStock<T>()
            where T : Stock, new()
        {
            StockFactory = () => new T();
            return this;
        }

        public MaterialsModuleBuilder UseMonthlyReportItem<T>()
            where T : MonthlyReportItem, new()
        {
            MonthlyReportItemFactory = () => new T();
            return this;
        }

        public MaterialsModuleBuilder UseFifoProvider<T>()
            where T : IFifoProvider
        {
            FifoProviderType = typeof(T);
            return this;
        }

        /// <summary>
        /// 为实体的扩展部分（如果有）添加映射配置程序
        /// </summary>
        /// <param name="modelMapperConfigurer"></param>
        /// <returns></returns>
        public MaterialsModuleBuilder AddExtensionModelMapperConfigurer(IModelMapperConfigurer modelMapperConfigurer)
        {
            ExtensionModelMapperConfigurer = modelMapperConfigurer;
            return this;
        }



        internal MaterialsModule Build()
        {
            return new MaterialsModule(this);
        }
    }
}
