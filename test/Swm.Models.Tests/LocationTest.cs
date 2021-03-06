using NSubstitute;
using Serilog;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Swm.Model.Tests
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
        public void IncreaseUnitloadCount_S位置()
        {
            Location loc = new Location
            {
                LocationType = LocationTypes.S, 
                Cell = Substitute.For<Cell>(),
                Rack = Substitute.For<Rack>(),
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
        public void EnterLocation_K位置()
        {
            Location loc = new Location
            {
                LocationType = LocationTypes.K,
                Cell = Substitute.For<Cell>(),
                Rack = Substitute.For<Rack>(),
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
        public void IncreaseUnitloadCount_N位置_会抛出异常()
        {
            Location loc = new Location
            {
                LocationType = LocationTypes.N,
            };

            Assert.Throws<InvalidOperationException>(loc.IncreaseUnitloadCount);
        }



        [Fact]
        public void DecreaseUnitloadCount_K位置()
        {
            Location loc = new Location
            {
                LocationType = LocationTypes.K,
                Cell = Substitute.For<Cell>(),
                Rack = Substitute.For<Rack>(),
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
        public void DecreaseUnitloadCount_S位置()
        {
            Location loc = new Location
            {
                LocationType = LocationTypes.S,
                Cell = Substitute.For<Cell>(),
                Rack = Substitute.For<Rack>(),
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
        public void DecreaseUnitloadCount_小于0_会抛出异常()
        {
            Location loc = new Location
            {
                LocationType = LocationTypes.S,
                Cell = Substitute.For<Cell>(),
                Rack = Substitute.For<Rack>(),
                UnitloadCount = 1,
            };

            Assert.True(loc.Loaded());

            loc.DecreaseUnitloadCount();
            Assert.Equal(0, loc.UnitloadCount);
            Assert.False(loc.Loaded());

            Assert.Throws<InvalidOperationException>(loc.DecreaseUnitloadCount);
        }



        [Fact]
        public void DecreaseUnitloadCount_N位置_会抛出异常()
        {
            Location loc = new Location
            {
                LocationType = LocationTypes.N,
            };

            Assert.Throws<InvalidOperationException>(loc.DecreaseUnitloadCount);


        }
    }

}
