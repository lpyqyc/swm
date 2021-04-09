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
using Autofac;
using Serilog;
using System;

namespace Swm.InboundOrders
{
    /// <summary>
    /// 
    /// </summary>
    public class InboundOrdersModule : Module
    {
        static ILogger _logger = Log.ForContext<InboundOrdersModule>();
        InboundOrdersModuleBuilder _moduleBuilder;
        internal InboundOrdersModule(InboundOrdersModuleBuilder moduleBuilder)
        {
            _moduleBuilder = moduleBuilder;
        }


        protected override void Load(ContainerBuilder builder)
        {
            builder.AddModelMapperConfigurer(new ModelMapperConfigurer());
            if (_moduleBuilder.ExtensionModelMapperConfigurer != null)
            {
                builder.AddModelMapperConfigurer(_moduleBuilder.ExtensionModelMapperConfigurer);
            }

            builder.RegisterEntityFactory(_moduleBuilder.InboundOrderFactory);
            builder.RegisterEntityFactory(_moduleBuilder.InboundLineFactory);

        }

    }

}
