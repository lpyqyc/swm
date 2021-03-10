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

namespace Swm.TransportTasks.Mappings
{
    internal class TransportTaskMapping : ClassMapping<TransportTask>
    {
        public TransportTaskMapping()
        {
            Table("transport_tasks");
            DynamicUpdate(true);
            BatchSize(10);

            Id(cl => cl.TaskId, id => id.Generator(Generators.Identity));

            NaturalId(npm => {
                npm.Property(cl => cl.TaskCode, prop => prop.Update(false));
            }, m => m.Mutable(false));

            Property(cl => cl.TaskType, prop => prop.Update(false));
            Property(cl => cl.ctime, prop => prop.Update(false));

            ManyToOne(cl => cl.Unitload, m => {
                m.Column("UnitloadId");
                m.Update(false);
                m.Unique(true);
            });

            ManyToOne(cl => cl.Start, m => {
                m.Column("StartLocationId");
                m.Update(false);
            });
            ManyToOne(cl => cl.End, m => {
                m.Column("EndLocationId");
            });

            Property(cl => cl.ForWcs);
            Property(cl => cl.WasSentToWcs);
            Property(cl => cl.SendTime);

            Property(cl => cl.OrderCode);
            Property(cl => cl.Comment);

            Property(cl => cl.ex1);
            Property(cl => cl.ex2);
        }
    }

}
