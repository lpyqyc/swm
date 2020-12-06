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
    internal class RackMapping : ClassMapping<Rack>
    {
        public RackMapping()
        {
            Table("racks");
            DynamicUpdate(true);
            BatchSize(100);
            Cache(cache => cache.Usage(CacheUsage.ReadWrite));

            Id(cl => cl.RackId, im => im.Generator(Generators.Identity));

            NaturalId(npm => {
                npm.Property(cl => cl.RackCode, prop => prop.Update(false));
            }, m => m.Mutable(false));
            
            ManyToOne(cl => cl.Laneway, m => {
                m.Column("LanewayId");
                m.Update(false);
            });

            Property(cl => cl.Side, prop => prop.Update(false));
            Property(cl => cl.Deep, prop => prop.Update(false));

            Property(cl => cl.Columns, prop => prop.Update(false));
            Property(cl => cl.Levels, prop => prop.Update(false));
            Property(cl => cl.Comment);

            Set(cl => cl.Locations, set => {
                set.Inverse(true);
                set.BatchSize(10);
                set.Key(key => {
                    key.Column("RackId");
                    key.Update(false);
                });
            }, rel => rel.OneToMany());     
        }
    }
}
