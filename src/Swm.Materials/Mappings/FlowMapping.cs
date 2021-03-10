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

using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Swm.Materials.Mappings
{
    internal class FlowMapping : ClassMapping<Flow>
    {
        public FlowMapping()
        {
            Table("flows");
            BatchSize(10);
            Lazy(false);
            DiscriminatorValue(1);

            Id(cl => cl.FlowId, id => id.Generator(Generators.Identity));
            Discriminator(dm => {
                dm.NotNullable(true);
            });

            Property(cl => cl.ctime, prop => prop.Update(false));
            Property(cl => cl.cuser, prop => prop.Update(false));

            ManyToOne(cl => cl.Material, m => {
                m.Column("MaterialId");
                m.Update(false);
            });

            Property(cl => cl.Batch);
            Property(cl => cl.StockStatus);
            Property(cl => cl.Quantity);
            Property(cl => cl.Uom);


            Property(cl => cl.BizType);
            Property(cl => cl.Direction);
            Property(cl => cl.OpType);
            Property(cl => cl.TxNo);

            Property(cl => cl.OrderCode);
            Property(cl => cl.BizOrder);
            Property(cl => cl.PalletCode);

            Property(cl => cl.Balance);
            Property(cl => cl.Comment);
        }
    }
}
