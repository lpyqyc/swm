using NSubstitute;
using Swm.Locations;
using static NSubstitute.Substitute;

namespace Swm.Palletization.Tests
{
    public static class TestDataUtil
    {
        public static Location NewS()
        {
            Location loc = new Location();
            loc.LocationType = LocationTypes.S;
            loc.Laneway = For<Laneway>();
            loc.Cell = For<Cell>();
            loc.Laneway?.Automated.Returns(true);
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
