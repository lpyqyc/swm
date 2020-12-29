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
    internal class RoleMapping : ClassMapping<Role>
    {
        public RoleMapping()
        {
            Table("roles");
            BatchSize(10);
            DynamicUpdate(true);
            Cache(cache => cache.Usage(CacheUsage.NonstrictReadWrite));

            Id(cl => cl.RoleId, id => id.Generator(Generators.Identity));

            NaturalId(npm => {
                npm.Property(cl => cl.RoleName);
            }, m => m.Mutable(true));

            Property(cl => cl.ctime, prop => prop.Update(false));
            Property(cl => cl.mtime);

            Property(cl => cl.IsBuiltIn);
            Property(cl => cl.Comment);

            Set(cl => cl.AllowedOpTypes, set => {
                set.Table("ROLE_OPTYPE");
                set.BatchSize(10);
                set.Cache(cache => cache.Usage(CacheUsage.NonstrictReadWrite));
                set.Key(key => {
                    key.Column("RoleId");
                });
            }, rel => rel.Element(el => {
                el.Column("OpType");
                el.NotNullable(true);
            })
            );
        }
    }

}
