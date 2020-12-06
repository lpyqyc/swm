namespace Swm.Model
{
    public interface ILocationFactory
    {
        Location CreateLocation(string locationCode, string locationType, Rack rack, int column, int level);
    }


}
