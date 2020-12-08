using NSubstitute;
using static NSubstitute.Substitute;

namespace Swm.Model.Tests
{
    public static class TestDataUtil
    {
        public static Location NewS()
        {
            Location loc = new Location();
            loc.LocationType = LocationTypes.S;
            loc.Cell = For<Cell>();
            loc.Rack = For<Rack>();
            loc.Rack.Laneway.Automated.Returns(true);
            loc.InboundLimit = 1;
            loc.OutboundLimit = 1;
            return loc;
        }

        public static Location NewK()
        {
            Location loc = new Location();
            loc.LocationType = LocationTypes.K;
            loc.InboundLimit = 999;
            loc.OutboundLimit = 999;
            return loc;
        }

        public static Location NewN()
        {
            Location loc = new Location();
            loc.LocationType = LocationTypes.N;
            return loc;
        }
    }

}
