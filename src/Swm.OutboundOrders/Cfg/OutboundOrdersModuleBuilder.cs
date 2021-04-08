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
        internal Func<OutboundOrder>? _outboundOrderFactory;
        internal Func<OutboundLine>? _outboundLineFactory;
        internal XModelMapper? _extensionModelMapper;
        internal Type? _outboundOrderAllocatorType;

        internal OutboundOrdersModuleBuilder()
        {
        }

        public OutboundOrdersModuleBuilder UseOutboundOrder<T>()
            where T : OutboundOrder, new()
        {
            _outboundOrderFactory = () => new T();
            return this;
        }

        public OutboundOrdersModuleBuilder UseOutboundLine<T>()
            where T : OutboundLine, new()
        {
            _outboundLineFactory = () => new T();
            return this;
        }

        public OutboundOrdersModuleBuilder UseOutboundOrderAllocator<T>()
            where T : IOutboundOrderAllocator
        {
            _outboundOrderAllocatorType = typeof(T);
            return this;
        }

        /// <summary>
        /// 如果使用子类扩展了实体模型，则使用此方法添加扩展部分的模型映射类，将实体添加到 NHibernate 中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extensionModelMapper"></param>
        /// <returns></returns>
        public OutboundOrdersModuleBuilder AddExtensionModelMapper<T>(XModelMapper extensionModelMapper)
        {
            _extensionModelMapper = extensionModelMapper;
            return this;
        }

        internal OutboundOrdersModule Build()
        {
            return new OutboundOrdersModule(this);
        }
    }


}
