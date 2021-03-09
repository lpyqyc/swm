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

namespace Swm.Palletization.Mappings
{
    internal class UnitloadItemAllocationMapping : ClassMapping<UnitloadItemAllocation>
    {
        public UnitloadItemAllocationMapping()
        {
            Table("unitload_item_allocations");
            BatchSize(10);
            DynamicUpdate(true);
            Lazy(false);

            Id(cl => cl.UnitloadItemAllocationId, id => id.Generator(Generators.Identity));

            ManyToOne(cl => cl.UnitloadItem, m => {
                m.Column(nameof(UnitloadItem.UnitloadItemId));
                m.Update(false);
            });

            Any(cl => cl.OutboundDemand, typeof(int), m =>
            {
                m.Lazy(false);
                m.Columns(x =>
                {
                    x.Name("OutboundDemandId");
                }, x =>
                {
                    x.Name("OutboundDemandType");
                    x.Length(30);
                });
            });
            Property(cl => cl.OutboundDemandRootType);
            Property(cl => cl.QuantityAllocated);
            Property(cl => cl.Comment);
        }
    }

}
