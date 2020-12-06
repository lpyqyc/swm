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
    internal class LocationOpMapping : ClassMapping<LocationOp>
    {
        public LocationOpMapping()
        {
            Table("location_ops");
            Mutable(false);
            BatchSize(10);

            Id(cl => cl.LocationOpId, id => id.Generator(Generators.Identity));
            
            ManyToOne(cl => cl.Location, m => {
                m.Column("LocationId");
                m.Update(false);
            });

            Property(cl => cl.ctime, prop => prop.Update(false));
            Property(cl => cl.cuser, prop => prop.Update(false));

            Property(cl => cl.OpType);
            Property(cl => cl.Comment);
        }
    }
}
