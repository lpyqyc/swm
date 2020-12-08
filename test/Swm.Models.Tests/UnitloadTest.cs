using Serilog;
using System;
using Xunit;
using Xunit.Abstractions;
namespace Swm.Model.Tests
{
    using static TestDataUtil;

    public class UnitloadTest
    {

        public UnitloadTest(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.TestOutput(output)
                .CreateLogger();
        }



        [Fact]
        public void Enter_进入S位置()
        {
            Location loc = NewS();
            Unitload u1 = new Unitload();
            Unitload u2 = new Unitload();

            u1.Enter(loc);
            u2.Enter(loc);

            Assert.Same(loc, u1.CurrentLocation);
            Assert.Same(loc, u2.CurrentLocation);
            Assert.Equal(2, loc.UnitloadCount);
        }


        [Fact]
        public void Enter_进入K位置()
        {
            Location loc = NewK();
            Unitload u1 = new Unitload();
            Unitload u2 = new Unitload();

            u1.Enter(loc);
            u2.Enter(loc);

            Assert.Same(loc, u1.CurrentLocation);
            Assert.Same(loc, u2.CurrentLocation);
            Assert.Equal(2, loc.UnitloadCount);
        }


        [Fact]
        public void Enter_进入N位置()
        {
            Location loc = NewN();
            Unitload u1 = new Unitload();
            Unitload u2 = new Unitload();

            u1.Enter(loc);
            u2.Enter(loc);

            Assert.Same(loc, u1.CurrentLocation);
            Assert.Same(loc, u2.CurrentLocation);
            Assert.Equal(0, loc.UnitloadCount);
        }


        [Fact]
        public void Enter_如果货载在S位置上_则会抛出异常()
        {
            Location loc1 = NewS();
            Location loc2 = NewN();
            Unitload u1 = new Unitload();

            u1.Enter(loc1);

            Assert.Throws<InvalidOperationException>(() => u1.Enter(loc2));
        }

        [Fact]
        public void Enter_如果货载在K位置上_则会抛出异常()
        {
            Location loc1 = NewK();
            Location loc2 = NewN();
            Unitload u1 = new Unitload();

            u1.Enter(loc1);

            Assert.Throws<InvalidOperationException>(() => u1.Enter(loc2));
        }

        [Fact]
        public void Enter_如果货载在N位置上_则不会抛出异常()
        {
            Location loc1 = NewN();
            Location loc2 = NewS();
            Unitload u1 = new Unitload();

            u1.Enter(loc1);

            Assert.Throws<InvalidOperationException>(() => u1.Enter(loc2));
        }

        [Fact]
        public void LeaveCurrentLocation_离开S位置()
        {
            Location loc = NewS();
            Unitload u1 = new Unitload();
            Unitload u2 = new Unitload();
            u1.Enter(loc);
            u2.Enter(loc);

            u1.LeaveCurrentLocation();
            Assert.Null(u1.CurrentLocation);
            Assert.Equal(1, loc.UnitloadCount);

            u2.LeaveCurrentLocation();
            Assert.Null(u2.CurrentLocation);
            Assert.Equal(0, loc.UnitloadCount);
        }

        [Fact]
        public void LeaveCurrentLocation_离开K位置()
        {
            Location loc = NewK();
            Unitload u1 = new Unitload();
            Unitload u2 = new Unitload();
            u1.Enter(loc);
            u2.Enter(loc);

            u1.LeaveCurrentLocation();
            Assert.Null(u1.CurrentLocation);
            Assert.Equal(1, loc.UnitloadCount);

            u2.LeaveCurrentLocation();
            Assert.Null(u2.CurrentLocation);
            Assert.Equal(0, loc.UnitloadCount);
        }

        [Fact]
        public void LeaveCurrentLocation_离开N位置()
        {
            Location loc = NewN();
            Unitload u1 = new Unitload();
            Unitload u2 = new Unitload();
            u1.Enter(loc);
            u2.Enter(loc);

            u1.LeaveCurrentLocation();
            Assert.Null(u1.CurrentLocation);
            Assert.Equal(0, loc.UnitloadCount);

            u2.LeaveCurrentLocation();
            Assert.Null(u2.CurrentLocation);
            Assert.Equal(0, loc.UnitloadCount);
        }

    }

}
