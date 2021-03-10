using Serilog;
using Swm.Locations;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Swm.Palletization.Tests
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
        public void Enter_����Sλ��()
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
        public void Enter_����Kλ��()
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
        public void Enter_����Nλ��()
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
        public void Enter_���������Sλ����_����׳��쳣()
        {
            Location loc1 = NewS();
            Location loc2 = NewN();
            Unitload u1 = new Unitload();

            u1.Enter(loc1);

            Assert.Throws<InvalidOperationException>(() => u1.Enter(loc2));
        }

        [Fact]
        public void Enter_���������Kλ����_����׳��쳣()
        {
            Location loc1 = NewK();
            Location loc2 = NewN();
            Unitload u1 = new Unitload();

            u1.Enter(loc1);

            Assert.Throws<InvalidOperationException>(() => u1.Enter(loc2));
        }

        [Fact]
        public void Enter_���������Nλ����_�򲻻��׳��쳣()
        {
            Location loc1 = NewN();
            Location loc2 = NewS();
            Unitload u1 = new Unitload();

            u1.Enter(loc1);

            Assert.Throws<InvalidOperationException>(() => u1.Enter(loc2));
        }

        [Fact]
        public void LeaveCurrentLocation_�뿪Sλ��()
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
        public void LeaveCurrentLocation_�뿪Kλ��()
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
        public void LeaveCurrentLocation_�뿪Nλ��()
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
