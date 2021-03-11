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
    internal class StockMapping : ClassMapping<Stock>
    {
        public StockMapping()
        {
            Table("stocks");
            DynamicUpdate(true);
            BatchSize(10);
            Lazy(false);
            DiscriminatorValue(1);

            Id(cl => cl.StockId, id => id.Generator(Generators.Identity));
            Discriminator(dm => {
                dm.NotNullable(true);
            });

            Version(cl => cl.v, v => v.Column("v"));

            Property(cl => cl.ctime, prop => prop.Update(false));
            Property(cl => cl.mtime);

            ManyToOne(cl => cl.Material, m => {
                m.Column("MaterialId");
                m.Update(false);
                m.UniqueKey(nameof(StockKeyBase));
            });

            Property(cl => cl.Batch, prop =>
            {
                prop.UniqueKey(nameof(StockKeyBase));
                prop.Update(false);
            });
            Property(cl => cl.StockStatus, prop =>
            {
                prop.UniqueKey(nameof(StockKeyBase));
                prop.Update(false);
            });
            Property(cl => cl.Quantity);
            Property(cl => cl.Uom, prop =>
            {
                prop.UniqueKey(nameof(StockKeyBase));
                prop.Update(false);
            });


            Property(cl => cl.Quantity);
            Property(cl => cl.Fifo);

            Property(cl => cl.Stocktaking);
            Property(cl => cl.AgeBaseline);
        }
    }
}
