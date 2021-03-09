using NSubstitute;
using Swm.Locations;
using static NSubstitute.Substitute;

namespace Swm.Palletization.Tests
{
    public static class TestDataUtil
    {
        static readonly DefaultLocationFactory _locationFactory = new DefaultLocationFactory();
        public static Location NewS()
        {
            Location loc = _locationFactory.CreateLocation(
                default,
                LocationTypes.S,
                For<Laneway>(),
                default,
                default
                );
            loc.Cell = For<Cell>();
            loc.Laneway.Automated.Returns(true);
            loc.InboundLimit = 1;
            loc.OutboundLimit = 1;
            return loc;
        }

        public static Location NewK()
        {
            Location loc = _locationFactory.CreateLocation(
                default,
                LocationTypes.K,
                default,
                default,
                default
                );
            loc.InboundLimit = 999;
            loc.OutboundLimit = 999;
            return loc;
        }

        public static Location NewN()
        {
            Location loc = _locationFactory.CreateLocation(
                default,
                LocationTypes.N,
                default,
                default,
                default
                );
            return loc;
        }
    }

}
