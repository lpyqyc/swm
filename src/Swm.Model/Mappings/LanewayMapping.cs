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
    internal class LanewayMapping : ClassMapping<Laneway>
    {
        public LanewayMapping()
        {
            Table("laneways");
            DynamicUpdate(true);
            BatchSize(10);

            Id(cl => cl.LanewayId, id => id.Generator(Generators.Identity));

            NaturalId(npm => {
                npm.Property(cl => cl.LanewayCode, prop => prop.Update(false));
            }, m => m.Mutable(false));

            Version(cl => cl.v, v => v.Column("v"));

            Property(cl => cl.ctime, prop => prop.Update(false));
            Property(cl => cl.mtime);

            Property(cl => cl.Area);

            Property(cl => cl.Comment);

            Set(cl => cl.Racks, set => {
                set.Inverse(true);
                set.BatchSize(10);
                set.Cache(cache => cache.Usage(CacheUsage.ReadWrite));
                set.Key(key => { 
                    key.Column("LanewayId");
                    key.NotNullable(true);
                    key.Update(false);
                });
            }, rel => rel.OneToMany());

            Property(cl => cl.Automated, prop => prop.Update(false));
            Property(cl => cl.Offline);
            Property(cl => cl.OfflineComment);
            Property(cl => cl.TakeOfflineTime);

            Property(cl => cl.DoubleDeep, prop => prop.Update(false));
            Property(cl => cl.ReservedLocationCount);

            Set(cl => cl.Ports, set => {
                set.Table("LANEWAY_PORT");
                set.BatchSize(10);
                set.Cache(cache => cache.Usage(CacheUsage.ReadWrite));
                set.Key(key => {
                    key.Column("LanewayId");
                    key.NotNullable(true);
                    key.Update(false);
                });
            }, rel => {
                rel.ManyToMany(m2m => m2m.Column("PortId"));
            });

            Map(
                cl => cl.Usage, 
                map =>
                {
                    map.Table("laneway_usage");
                    map.BatchSize(100);
                    map.Cascade(Cascade.All | Cascade.DeleteOrphans);
                    map.Key(key =>
                    {
                        key.Column("LanewayId");
                        key.NotNullable(true);
                    });
                },
                key => key.Component(comp => { }),
                el => el.Component(comp => comp.Class<LanewayUsageData>()));

            Property(cl => cl.TotalOfflineHours);
            


        }
    }
}
