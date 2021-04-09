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

namespace Swm.OutboundOrders
{
    public class OutboundOrdersModuleBuilder
    {
        public Func<OutboundOrder> OutboundOrderFactory { get; private set; } = () => new OutboundOrder();

        public Func<OutboundLine> OutboundLineFactory { get; private set; } = () => new OutboundLine();

        public Type OutboundOrderAllocatorType { get; private set; } = typeof(DefaultOutboundOrderAllocator);

        /// <summary>
        /// 获取实体扩展部分（如果有）的映射配置程序
        /// </summary>
        public IModelMapperConfigurer? ExtensionModelMapperConfigurer { get; private set; }

        internal OutboundOrdersModuleBuilder()
        {
        }

        public OutboundOrdersModuleBuilder UseOutboundOrder<T>()
            where T : OutboundOrder, new()
        {
            OutboundOrderFactory = () => new T();
            return this;
        }

        public OutboundOrdersModuleBuilder UseOutboundLine<T>()
            where T : OutboundLine, new()
        {
            OutboundLineFactory = () => new T();
            return this;
        }

        public OutboundOrdersModuleBuilder UseOutboundOrderAllocator<T>()
            where T : IOutboundOrderAllocator
        {
            OutboundOrderAllocatorType = typeof(T);
            return this;
        }

        /// <summary>
        /// 为实体的扩展部分（如果有）添加映射配置程序
        /// </summary>
        /// <param name="modelMapperConfigurer"></param>
        /// <returns></returns>
        public OutboundOrdersModuleBuilder AddExtensionModelMapperConfigurer(IModelMapperConfigurer modelMapperConfigurer)
        {
            ExtensionModelMapperConfigurer = modelMapperConfigurer;
            return this;
        }


        internal OutboundOrdersModule Build()
        {
            return new OutboundOrdersModule(this);
        }
    }


}
