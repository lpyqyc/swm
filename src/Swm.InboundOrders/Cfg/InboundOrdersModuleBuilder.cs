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

namespace Swm.InboundOrders
{
    public class InboundOrdersModuleBuilder
    {
        public Func<InboundOrder> InboundOrderFactory { get; private set; } = () => new InboundOrder();
        public Func<InboundLine> InboundLineFactory { get; private set; } = () => new InboundLine();


        /// <summary>
        /// 获取实体扩展部分（如果有）的映射配置程序
        /// </summary>
        public IModelMapperConfigurer? ExtensionModelMapperConfigurer { get; private set; }


        internal InboundOrdersModuleBuilder()
        {
        }

        public InboundOrdersModuleBuilder UseInboundOrder<T>()
            where T : InboundOrder, new()
        {
            InboundOrderFactory = () => new T();
            return this;
        }

        public InboundOrdersModuleBuilder UseInboundLine<T>()
            where T : InboundLine, new()
        {
            InboundLineFactory = () => new T();
            return this;
        }

        /// <summary>
        /// 为实体的扩展部分（如果有）添加映射配置程序
        /// </summary>
        /// <param name="modelMapperConfigurer"></param>
        /// <returns></returns>
        public InboundOrdersModuleBuilder AddExtensionModelMapperConfigurer(IModelMapperConfigurer modelMapperConfigurer)
        {
            ExtensionModelMapperConfigurer = modelMapperConfigurer;
            return this;
        }


        internal InboundOrdersModule Build()
        {
            return new InboundOrdersModule(this);
        }
    }
}
