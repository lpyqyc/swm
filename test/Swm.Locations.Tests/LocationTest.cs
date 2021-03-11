using NSubstitute;
using Serilog;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Swm.Locations.Tests
{
    public class LocationTest
    {
        public LocationTest(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.TestOutput(output)
                .CreateLogger();
        }

        [Fact]
        public void IncreaseUnitloadCount_Sλ��()
        {
            Location loc = new Location("", LocationTypes.S)
            {
                Cell = Substitute.For<Cell>(),
                Laneway = Substitute.For<Laneway>(),
            };

            Assert.False(loc.Loaded());

            loc.IncreaseUnitloadCount();
            Assert.Equal(1, loc.UnitloadCount);
            Assert.True(loc.Loaded());

            loc.IncreaseUnitloadCount();
            Assert.Equal(2, loc.UnitloadCount);
            Assert.True(loc.Loaded());

            loc.Cell.Received(2).UpdateState();
        }

        [Fact]
        public void EnterLocation_Kλ��()
        {
            Location loc = new Location("", LocationTypes.K)
            {
                Cell = Substitute.For<Cell>(),
                Laneway = Substitute.For<Laneway>(),
            };

            Assert.False(loc.Loaded());


            loc.IncreaseUnitloadCount();
            Assert.Equal(1, loc.UnitloadCount);
            Assert.True(loc.Loaded());

            loc.IncreaseUnitloadCount();
            Assert.Equal(2, loc.UnitloadCount);
            Assert.True(loc.Loaded());

            loc.Cell.DidNotReceive().UpdateState();
        }

        [Fact]
        public void IncreaseUnitloadCount_Nλ��_���׳��쳣()
        {
            Location loc = new Location("", LocationTypes.N)
            {
                LocationType = LocationTypes.N,
            };

            Assert.Throws<InvalidOperationException>(loc.IncreaseUnitloadCount);
        }



        [Fact]
        public void DecreaseUnitloadCount_Kλ��()
        {
            Location loc = new Location("", LocationTypes.K)
            {
                Cell = Substitute.For<Cell>(),
                Laneway = Substitute.For<Laneway>(),
                UnitloadCount = 2,
            };

            Assert.True(loc.Loaded());

            loc.DecreaseUnitloadCount();
            Assert.Equal(1, loc.UnitloadCount);
            Assert.True(loc.Loaded());

            loc.DecreaseUnitloadCount();
            Assert.Equal(0, loc.UnitloadCount);
            Assert.False(loc.Loaded());

            loc.Cell.DidNotReceive().UpdateState();
        }


        [Fact]
        public void DecreaseUnitloadCount_Sλ��()
        {
            Location loc = new Location("", LocationTypes.S)
            {
                Cell = Substitute.For<Cell>(),
                Laneway = Substitute.For<Laneway>(),
                UnitloadCount = 2,
            };

            Assert.True(loc.Loaded());

            loc.DecreaseUnitloadCount();
            Assert.Equal(1, loc.UnitloadCount);
            Assert.True(loc.Loaded());

            loc.DecreaseUnitloadCount();
            Assert.Equal(0, loc.UnitloadCount);
            Assert.False(loc.Loaded());

            loc.Cell.Received(2).UpdateState();

        }


        [Fact]
        public void DecreaseUnitloadCount_С��0_���׳��쳣()
        {
            Location loc = new Location("", LocationTypes.S)
            {
                Cell = Substitute.For<Cell>(),
                Laneway = Substitute.For<Laneway>(),
                UnitloadCount = 1,
            };

            Assert.True(loc.Loaded());

            loc.DecreaseUnitloadCount();
            Assert.Equal(0, loc.UnitloadCount);
            Assert.False(loc.Loaded());

            Assert.Throws<InvalidOperationException>(loc.DecreaseUnitloadCount);
        }



        [Fact]
        public void DecreaseUnitloadCount_Nλ��_���׳��쳣()
        {
            Location loc = new Location("", LocationTypes.N);

            Assert.Throws<InvalidOperationException>(loc.DecreaseUnitloadCount);


        }
    }

}