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
    internal class LocationMapping : ClassMapping<Location>
    {
        public LocationMapping()
        {
            Table("locations");
            DynamicUpdate(true);
            BatchSize(10);
            Lazy(false);
            DiscriminatorValue(1);

            Id(cl => cl.LocationId, id => id.Generator(Generators.Identity));
            Discriminator(dm => {
                dm.NotNullable(true);
            });

            NaturalId(npm => {
                npm.Property(cl => cl.LocationCode, prop => prop.Update(false));
            }, m => m.Mutable(false));

            Version(cl => cl.v, v => v.Column("v"));

            Property(cl => cl.ctime, prop => prop.Update(false));
            Property(cl => cl.mtime);

            Property(cl => cl.LocationType, prop => prop.Update(false));

            Property(cl => cl.InboundCount);
            Property(cl => cl.InboundLimit);
            Property(cl => cl.InboundDisabled);
            Property(cl => cl.InboundDisabledComment);

            Property(cl => cl.OutboundCount);
            Property(cl => cl.OutboundLimit);
            Property(cl => cl.OutboundDisabled);
            Property(cl => cl.OutboundDisabledComment);

            Property(cl => cl.Exists);
            Property(cl => cl.WeightLimit);
            Property(cl => cl.HeightLimit);
            Property(cl => cl.Specification);

            ManyToOne(cl => cl.Laneway, m => {
                m.Column("LanewayId");
                m.Update(false);
            });
            Property(cl => cl.Side);
            Property(cl => cl.Deep);

            Property(cl => cl.Column, prop => prop.Update(false));
            Property(cl => cl.Level, prop => prop.Update(false));
            Property(cl => cl.StorageGroup);

            Property(cl => cl.UnitloadCount);
            ManyToOne(cl => cl.Cell, m => {
                m.Column("CellId");
                m.Update(false);
            });

            Property(cl => cl.Tag);
            Property(cl => cl.RequestType);
        }
    }
}
