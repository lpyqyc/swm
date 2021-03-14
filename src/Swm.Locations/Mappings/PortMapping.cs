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
    internal class PortMapping : ClassMapping<Port>
    {
        public PortMapping()
        {
            Table("ports");
            DynamicUpdate(true);
            BatchSize(10);
            Cache(cache => cache.Usage(CacheUsage.ReadWrite));

            Id(cl => cl.PortId, id => id.Generator(Generators.Identity));

            NaturalId(npm => {
                npm.Property(cl => cl.PortCode, prop => prop.Update(false));
            }, m => m.Mutable(false));

            Version(cl => cl.v, v => v.Column("v"));

            Property(cl => cl.ctime, prop => prop.Update(false));

            ManyToOne(cl => cl.KP1, m => {
                m.Column("KP1");
                m.NotNullable(true);
                m.Update(false);
            });

            ManyToOne(cl => cl.KP2, m => {
                m.Column("KP2");
                m.Update(false);
            });
            Property(cl => cl.Comment);


            Set(cl => cl.Laneways, set => {
                set.Table("LANEWAY_PORT");
                set.Cache(cache => cache.Usage(CacheUsage.ReadWrite));
                set.BatchSize(10);
                set.Key(key => { 
                    key.Column("PortId");
                    key.NotNullable(true);
                });
            }, rel => {
                rel.ManyToMany(m2m =>
                {
                    m2m.Column("LanewayId");
                });
            });

            // TODO 重命名
            Any(cl => cl.CurrentUat, typeof(int), m =>
            {
                m.Lazy(false);
                m.Columns(x =>
                {
                    x.Name("CurrentUatId");
                }, x =>
                {
                    x.Name("CurrentUatType");
                    x.Length(30);
                });
            });


            Property(cl => cl.CheckedAt);
            Property(cl => cl.CheckMessage);

            Property(cl => cl.ex1);
            Property(cl => cl.ex2);

        }
    }
}
