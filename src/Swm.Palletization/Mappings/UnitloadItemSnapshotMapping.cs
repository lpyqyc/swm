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
using Swm.Materials;

namespace Swm.Palletization.Mappings
{
    internal class UnitloadItemSnapshotMapping : ClassMapping<UnitloadItemSnapshot>
    {
        public UnitloadItemSnapshotMapping()
        {
            Table("UnitloadItemSnapshots");
            BatchSize(10);
            Lazy(false);
            DiscriminatorValue(1);

            Id(cl => cl.UnitloadItemSnapshotId, id => id.Generator(Generators.Identity));
            Discriminator(dm => {
                dm.NotNullable(true);
            });

            Property(cl => cl.UnitloadItemId, m => m.Update(false));
            ManyToOne(cl => cl.Unitload, m => {
                m.Column(nameof(UnitloadSnapshot.UnitloadSnapshotId));
                m.Update(false);
            });
            ManyToOne(cl => cl.Material, m => {
                m.Column(nameof(Material.MaterialId));
                m.Update(false);
            });

            Property(cl => cl.Batch, m => m.Update(false));
            Property(cl => cl.StockStatus, m => m.Update(false));
            Property(cl => cl.Quantity, m => m.Update(false));
            Property(cl => cl.Uom, m => m.Update(false));
            Property(cl => cl.ProductionTime, m => m.Update(false));

        }
    }

}
