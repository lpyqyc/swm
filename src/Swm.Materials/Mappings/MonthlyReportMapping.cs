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
    internal class MonthlyReportMapping : ClassMapping<MonthlyReport>
    {
        public MonthlyReportMapping()
        {
            Table("MonthlyReports");
            DynamicUpdate(true);
            BatchSize(10);

            Id(cl => cl.Month, id => id.Generator(Generators.Assigned));
            Version("v", m => { });
            Property(cl => cl.ctime, prop =>
            {
                prop.Column("ctime");
            });

            Set(cl => cl.Items, set =>
            {
                set.Inverse(true);
                set.BatchSize(10);
                set.Key(key => {
                    key.Column("Month");
                    key.Update(false);
                    key.NotNullable(true);
                });
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
            }, rel => rel.OneToMany());
        }
    }
}
