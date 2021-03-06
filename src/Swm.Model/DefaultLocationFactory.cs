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

using NHibernate.Mapping;

namespace Swm.Model
{
    public class DefaultLocationFactory : ILocationFactory
    {
        public Location CreateLocation(string locationCode, string locationType, Rack rack, int column, int level)
        {
            Location obj = new Location();
            obj.LocationCode = locationCode;
            obj.LocationType = locationType;
            obj.Rack = rack;
            if (rack != null)
            {
                rack.Locations.Add(obj);
            }
            obj.Column = column;
            obj.Level = level;
            return obj;
        }
    }
}
