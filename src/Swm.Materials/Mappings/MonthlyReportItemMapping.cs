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

using NHibernate.Mapping.ByCode.Conformist;

namespace Swm.Materials.Mappings
{
    internal class MonthlyReportItemMapping : ClassMapping<MonthlyReportItem>
    {
        public MonthlyReportItemMapping()
        {
            Table("MonthlyReportItems");
            DynamicUpdate(true);
            BatchSize(10);
            ComposedId(idm =>
            {
                idm.ManyToOne(cl => cl.MonthlyReport, m =>
                {
                    m.Column("Month");
                    m.Update(false);
                    m.NotNullable(true);
                });

                idm.ManyToOne(cl => cl.Material);
                idm.Property(cl => cl.Batch);
                idm.Property(cl => cl.StockStatus);
                idm.Property(cl => cl.Uom);
            });
            Version("v", m => { });

            Property(cl => cl.Beginning);
            Property(cl => cl.Incoming);
            Property(cl => cl.Outgoing);
            Property(cl => cl.Ending);
        }
    }
}
