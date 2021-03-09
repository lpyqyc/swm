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

namespace Swm.Materials.Mappings
{
    internal class MaterialMapping : ClassMapping<Material>
    {
        public MaterialMapping()
        {
            Table("materials");
            DynamicUpdate(true);
            Lazy(false);
            BatchSize(10);
            DiscriminatorValue(1);

            Id(cl => cl.MaterialId, id => id.Generator(Generators.Identity));
            Discriminator(dm => {
                dm.NotNullable(true);
            });

            NaturalId(npm => {
                npm.Property(cl => cl.MaterialCode);
            });
            Version(cl => cl.v, v => v.Column("v"));

            Property(cl => cl.ctime, prop => prop.Update(false));
            Property(cl => cl.cuser, prop => prop.Update(false));
            Property(cl => cl.mtime);
            Property(cl => cl.muser);

            Property(cl => cl.MaterialType);

            Property(cl => cl.Description);
            Property(cl => cl.SpareCode);
            Property(cl => cl.Specification);
            Property(cl => cl.MnemonicCode);

            Property(cl => cl.BatchEnabled);
            Property(cl => cl.MaterialGroup);

            Property(cl => cl.ValidDays);
            Property(cl => cl.StandingTime);
            Property(cl => cl.AbcClass);

            Property(cl => cl.Uom);


            Property(cl => cl.LowerBound);
            Property(cl => cl.UpperBound);

            Property(cl => cl.DefaultQuantity);
            Property(cl => cl.DefaultStorageGroup);

            Property(cl => cl.Comment);
        }
    }

}
