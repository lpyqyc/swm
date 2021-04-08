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

namespace Swm.Locations.Mappings
{
    internal class CellMapping : ClassMapping<Cell>
    {
        public CellMapping()
        {
            Table("Cells");
            DynamicUpdate(true);
            BatchSize(100);

            Id(cl => cl.CellId, id => id.Generator(Generators.Identity));
            
            ManyToOne(cl => cl.Laneway, m => {
                m.Column("LanewayId");
                m.Update(false);
            });

            Property(cl => cl.Side, prop => prop.Update(false));
            Property(cl => cl.Column);
            Property(cl => cl.Level);

            Property(cl => cl.Shape);
            Property(cl => cl.oByShape);
            Property(cl => cl.iByShape);


            Set(cl => cl.Locations, set => {
                set.Inverse(true);
                set.BatchSize(10);
                set.Cache(cache => cache.Usage(CacheUsage.ReadWrite));
                set.Key(key => { 
                    key.Column("CellId");
                    key.Update(false);
                });                
            }, rel => rel.OneToMany());

            Property(cl => cl.i1);
            Property(cl => cl.o1);

            Property(cl => cl.i2);
            Property(cl => cl.o2);

            Property(cl => cl.i3);
            Property(cl => cl.o3);
        }
    }
}
