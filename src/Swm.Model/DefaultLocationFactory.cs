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
