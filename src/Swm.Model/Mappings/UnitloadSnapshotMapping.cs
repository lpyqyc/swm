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
    internal class UnitloadSnapshotMapping : ClassMapping<UnitloadSnapshot>
    {
        public UnitloadSnapshotMapping()
        {
            Table("unitload_snapshots");
            BatchSize(10);
            Lazy(false);
            DiscriminatorValue(1);

            Id(cl => cl.UnitloadSnapshotId, id => id.Generator(Generators.Identity));
            Discriminator(dm => {
                dm.NotNullable(true);
            });

            Property(cl => cl.UnitloadId, m => m.Update(false));
            Property(cl => cl.PalletCode, m => m.Update(false));
            Property(cl => cl.ctime, m => m.Update(false));
            Property(cl => cl.mtime, m => m.Update(false));
            Property(cl => cl.cuser, m => m.Update(false));
            Component(cl => cl.StorageInfo, m => m.Update(false));
            Property(cl => cl.HasCountingError, m => m.Update(false));

            Set(cl => cl.Items, set => {
                set.Inverse(true);
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
                set.BatchSize(10);
                set.Key(key => {
                    key.Column(nameof(UnitloadSnapshot.UnitloadSnapshotId));
                    key.NotNullable(true);
                    key.Update(false);
                });
            }, rel => rel.OneToMany());

            Property(cl => cl.Comment, m => m.Update(false));
        }
    }

}
