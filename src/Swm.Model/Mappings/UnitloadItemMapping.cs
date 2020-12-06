// Copyright 2020 王建军
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

namespace Swm.Model.Mappings
{
    internal class UnitloadItemMapping : ClassMapping<UnitloadItem>
    {
        public UnitloadItemMapping()
        {
            Table("unitload_items");
            BatchSize(10);
            DynamicUpdate(true);
            Lazy(false);
            DiscriminatorValue(1);

            Id(cl => cl.UnitloadItemId, id => id.Generator(Generators.Identity));
            Discriminator(dm => {
                dm.NotNullable(true);
            });

            ManyToOne(cl => cl.Unitload, m => {
                m.Column(nameof(Unitload.UnitloadId));
                m.Update(false);
            });

            ManyToOne(cl => cl.Material, m => {
                m.Column(nameof(Material.MaterialId));
                m.Update(false);
            });

            Property(cl => cl.Batch);
            Property(cl => cl.OutOrdering);
            Property(cl => cl.Quantity);
            Property(cl => cl.Uom);

            Property(cl => cl.ProductionTime);
            Property(cl => cl.StockStatus);

        }
    }

}
