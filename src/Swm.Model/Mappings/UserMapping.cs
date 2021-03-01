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

namespace Swm.Model.Mappings
{
    internal class UserMapping : ClassMapping<User>
    {
        public UserMapping()
        {
            Table("users");
            DynamicUpdate(true);
            BatchSize(10);
            Cache(cache => cache.Usage(CacheUsage.NonstrictReadWrite));

            Id(cl => cl.UserId, im => im.Generator(Generators.Identity));

            NaturalId(npm => {
                npm.Property(c => c.UserName);
            }, m => m.Mutable(true));

            Property(cl => cl.ctime, prop => prop.Update(false));
            Property(cl => cl.mtime);
            Property(cl => cl.PasswordHash);
            Property(cl => cl.RealName);
            Property(cl => cl.PasswordSalt);
            Property(cl => cl.IsBuiltIn);
            Property(cl => cl.IsLocked);
            Property(cl => cl.LockedReason);
            Property(cl => cl.Comment);
            Property(cl => cl.Email);

            Set(cl => cl.Roles, set => {
                set.Table("USER_ROLE");
                set.BatchSize(10);
                set.Cache(cache => cache.Usage(CacheUsage.NonstrictReadWrite));
                set.Key(key => {
                    key.Column("UserId");
                });
            }, rel => rel.ManyToMany(m2m => m2m.Column("RoleId")));
        }
    }

}
