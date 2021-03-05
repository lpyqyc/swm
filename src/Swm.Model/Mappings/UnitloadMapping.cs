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
    internal class UnitloadMapping : ClassMapping<Unitload>
    {
        public UnitloadMapping()
        {
            Table("unitloads");
            DynamicUpdate(true);
            BatchSize(10);
            Lazy(false);
            DiscriminatorValue(1);

            Id(cl => cl.UnitloadId, id => id.Generator(Generators.Identity));
            Discriminator(dm => {
                dm.NotNullable(true);
            });

            NaturalId(npm => {
                npm.Property(c => c.PalletCode, prop => prop.Update(false));
            }, m => m.Mutable(false));

            Version(cl => cl.v, v => v.Column("v"));

            Property(cl => cl.ctime, prop => prop.Update(false));
            Property(cl => cl.mtime);
            Property(cl => cl.cuser, prop => prop.Update(false));

            Component(cl => cl.StorageInfo);

            Property(cl => cl.HasCountingError);
            Property(cl => cl.Odd);

            Set(cl => cl.Items, set => {
                set.Inverse(true);
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
                set.BatchSize(10);
                set.Key(key => {
                    key.Column(nameof(Unitload.UnitloadId));
                    key.NotNullable(true);
                    key.Update(false);
                });
            }, rel => rel.OneToMany());

            Property(cl => cl.BeingMoved);
            ManyToOne(cl => cl.CurrentLocation, m => {
                m.Column("CurrentLocationId");
            });
            Property(cl => cl.CurrentLocationTime);

            // TODO 重命名
            Property(cl => cl.CurrentUatRootType);
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


           Set(cl => cl.CurrentTasks, set => {
                set.Inverse(true);
                set.BatchSize(10);
                set.Key(key =>
                {
                    key.Column("UnitloadId");
                    key.NotNullable(true);
                });
            }, rel => rel.OneToMany());

            Property(cl => cl.OpHintType);
            Property(cl => cl.OpHintInfo);
            Property(cl => cl.Comment);
        }
    }

}
