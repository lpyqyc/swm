﻿// Copyright 2020-2021 王建军
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
        internal Func<InboundOrder>? _inboundOrderFactory;
        internal Func<InboundLine>? _inboundLineFactory;
        internal XModelMapper? _extensionModelMapper;


        internal InboundOrdersModuleBuilder()
        {
        }

        public InboundOrdersModuleBuilder UseInboundOrder<T>()
            where T : InboundOrder, new()
        {
            _inboundOrderFactory = () => new T();
            return this;
        }

        public InboundOrdersModuleBuilder UseInboundLine<T>()
            where T : InboundLine, new()
        {
            _inboundLineFactory = () => new T();
            return this;
        }

        /// <summary>
        /// 如果使用子类扩展了实体模型，则使用此方法添加扩展部分的模型映射类，将实体添加到 NHibernate 中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extensionModelMapper"></param>
        /// <returns></returns>
        public InboundOrdersModuleBuilder AddExtensionModelMapper<T>(XModelMapper extensionModelMapper)
        {
            _extensionModelMapper = extensionModelMapper;
            return this;
        }

        internal InboundOrdersModule Build()
        {
            return new InboundOrdersModule(this);
        }
    }
}
