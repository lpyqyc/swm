using NHibernate;
using Serilog;
using Swm.Materials;
using Swm.Palletization;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static NSubstitute.Substitute;
using System.Linq;

namespace Swm.OutboundOrders.Tests
{
    public class DefaultOutboundOrderAllocatorTest
    {
        public DefaultOutboundOrderAllocatorTest(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.TestOutput(output)
                .CreateLogger();
        }


        [Fact]
        public async Task AllocateItem_尾托分配数量在需求数量以内()
        {
            ISession session = For<ISession>();
            ILogger logger = For<ILogger>();
            DefaultOutboundOrderAllocator allocator = new DefaultOutboundOrderAllocator(session, logger);

            Material material = new Material();
            OutboundOrder outboundOrder = new OutboundOrder();
            OutboundLine line1 = new OutboundLine
            {
                OutboundLineId = 1,
                Material = material,
                StockStatus = "合格",
                Uom = "PCS",
                QuantityDemanded = 100,
                QuantityFulfilled = 0,
            };
            outboundOrder.AddLine(line1);

            DefaultUnitloadFactory factory = new DefaultUnitloadFactory();
            Unitload p1 = factory.CreateUnitload();
            p1.PalletCode = "P1";
            p1.ResetCurrentUat();
            UnitloadItem i1 = factory.CreateUnitloadItem();
            i1.Material = material;
            i1.Batch = "B";
            i1.StockStatus = "合格";
            i1.Uom = "PCS";
            i1.Quantity = 60;
            p1.AddItem(i1);

            Unitload p2 = factory.CreateUnitload();
            p2.PalletCode = "P2";
            p2.ResetCurrentUat();
            UnitloadItem i2 = factory.CreateUnitloadItem();
            i2.Material = material;
            i2.Batch = "B";
            i2.StockStatus = "合格";
            i2.Uom = "PCS";
            i2.Quantity = 70;
            p2.AddItem(i2);

            await allocator.AllocateItemAsync(line1, i1, null);
            await allocator.AllocateItemAsync(line1, i2, null);

            Assert.Single(i1.Allocations);
            Assert.Same(outboundOrder, p1.CurrentUat);
            Assert.Equal(60, i1.Allocations.Single().QuantityAllocated);

            Assert.Single(i2.Allocations);
            Assert.Same(outboundOrder, p2.CurrentUat);
            Assert.Equal(40, i2.Allocations.Single().QuantityAllocated);

            Assert.Equal(2, line1.Allocations.Count);
        }


        [Fact]
        public async Task AllocateItem_物料不匹配时不分配()
        {
            ISession session = For<ISession>();
            ILogger logger = For<ILogger>();
            DefaultOutboundOrderAllocator allocator = new DefaultOutboundOrderAllocator(session, logger);

            Material material = new Material();
            OutboundOrder outboundOrder = new OutboundOrder();
            OutboundLine line1 = new OutboundLine
            {
                OutboundLineId = 1,
                Material = material,
                StockStatus = "合格",
                Uom = "PCS",
                QuantityDemanded = 100,
                QuantityFulfilled = 0,
            };
            outboundOrder.AddLine(line1);

            DefaultUnitloadFactory factory = new DefaultUnitloadFactory();
            Unitload p1 = factory.CreateUnitload();
            p1.PalletCode = "P1";
            p1.ResetCurrentUat();
            UnitloadItem i1 = factory.CreateUnitloadItem();
            i1.Material = new Material();
            i1.Batch = "B";
            i1.StockStatus = "合格";
            i1.Uom = "PCS";
            i1.Quantity = 60;
            p1.AddItem(i1);

            await allocator.AllocateItemAsync(line1, i1, null);

            Assert.Empty(i1.Allocations);
            Assert.Null(p1.CurrentUat);
            Assert.Empty(line1.Allocations);

        }


        [Fact]
        public async Task AllocateItem_库存状态不匹配时不分配()
        {
            ISession session = For<ISession>();
            ILogger logger = For<ILogger>();
            DefaultOutboundOrderAllocator allocator = new DefaultOutboundOrderAllocator(session, logger);

            Material material = new Material();
            OutboundOrder outboundOrder = new OutboundOrder();
            OutboundLine line1 = new OutboundLine
            {
                OutboundLineId = 1,
                Material = material,
                StockStatus = "合格",
                Uom = "PCS",
                QuantityDemanded = 100,
                QuantityFulfilled = 0,
            };
            outboundOrder.AddLine(line1);

            DefaultUnitloadFactory factory = new DefaultUnitloadFactory();
            Unitload p1 = factory.CreateUnitload();
            p1.PalletCode = "P1";
            p1.ResetCurrentUat();
            UnitloadItem i1 = factory.CreateUnitloadItem();
            i1.Material = material;
            i1.Batch = "B";
            i1.StockStatus = "不合格";
            i1.Uom = "PCS";
            i1.Quantity = 60;
            p1.AddItem(i1);

            await allocator.AllocateItemAsync(line1, i1, null);

            Assert.Empty(i1.Allocations);
            Assert.Null(p1.CurrentUat);
            Assert.Empty(line1.Allocations);
        }

        [Fact]
        public async Task AllocateItem_计量单位不匹配时不分配()
        {
            ISession session = For<ISession>();
            ILogger logger = For<ILogger>();
            DefaultOutboundOrderAllocator allocator = new DefaultOutboundOrderAllocator(session, logger);

            Material material = new Material();
            OutboundOrder outboundOrder = new OutboundOrder();
            OutboundLine line1 = new OutboundLine
            {
                OutboundLineId = 1,
                Material = material,
                StockStatus = "合格",
                Uom = "PCS",
                QuantityDemanded = 100,
                QuantityFulfilled = 0,
            };
            outboundOrder.AddLine(line1);

            DefaultUnitloadFactory factory = new DefaultUnitloadFactory();
            Unitload p1 = factory.CreateUnitload();
            p1.PalletCode = "P1";
            p1.ResetCurrentUat();
            UnitloadItem i1 = factory.CreateUnitloadItem();
            i1.Material = material;
            i1.Batch = "B";
            i1.StockStatus = "合格";
            i1.Uom = "米";
            i1.Quantity = 60;
            p1.AddItem(i1);

            await allocator.AllocateItemAsync(line1, i1, null);

            Assert.Empty(i1.Allocations);
            Assert.Null(p1.CurrentUat);
            Assert.Empty(line1.Allocations);
        }


        [Fact]
        public async Task AllocateItem_批号不匹配时不分配()
        {
            ISession session = For<ISession>();
            ILogger logger = For<ILogger>();
            DefaultOutboundOrderAllocator allocator = new DefaultOutboundOrderAllocator(session, logger);

            Material material = new Material();
            OutboundOrder outboundOrder = new OutboundOrder();
            OutboundLine line1 = new OutboundLine
            {
                OutboundLineId = 1,
                Material = material,
                StockStatus = "合格",
                Uom = "PCS",
                Batch = "L",
                QuantityDemanded = 100,
                QuantityFulfilled = 0,
            };
            outboundOrder.AddLine(line1);

            DefaultUnitloadFactory factory = new DefaultUnitloadFactory();
            Unitload p1 = factory.CreateUnitload();
            p1.PalletCode = "P1";
            p1.ResetCurrentUat();
            UnitloadItem i1 = factory.CreateUnitloadItem();
            i1.Material = material;
            i1.Batch = "B";
            i1.StockStatus = "合格";
            i1.Uom = "PCS";
            i1.Quantity = 60;
            p1.AddItem(i1);

            await allocator.AllocateItemAsync(line1, i1, null);

            Assert.Empty(i1.Allocations);
            Assert.Null(p1.CurrentUat);
            Assert.Empty(line1.Allocations);
        }

        [Fact]
        public async Task AllocateItem_可以分配到一个出库单下的两个明细()
        {
            ISession session = For<ISession>();
            ILogger logger = For<ILogger>();
            DefaultOutboundOrderAllocator allocator = new DefaultOutboundOrderAllocator(session, logger);

            Material material = new Material();
            OutboundOrder outboundOrder = new OutboundOrder();
            OutboundLine line1 = new OutboundLine
            {
                OutboundLineId = 1,
                Material = material,
                StockStatus = "合格",
                Uom = "PCS",
                QuantityDemanded = 45,
                QuantityFulfilled = 0,
            };
            outboundOrder.AddLine(line1);

            OutboundLine line2 = new OutboundLine
            {
                OutboundLineId = 1,
                Material = material,
                StockStatus = "合格",
                Uom = "PCS",
                QuantityDemanded = 55,
                QuantityFulfilled = 0,
            };
            outboundOrder.AddLine(line2);

            DefaultUnitloadFactory factory = new DefaultUnitloadFactory();
            Unitload p1 = factory.CreateUnitload();
            p1.PalletCode = "P1";
            p1.ResetCurrentUat();
            UnitloadItem i1 = factory.CreateUnitloadItem();
            i1.Material = material;
            i1.Batch = "B";
            i1.StockStatus = "合格";
            i1.Uom = "PCS";
            i1.Quantity = 60;
            p1.AddItem(i1);

            await allocator.AllocateItemAsync(line1, i1, null);
            await allocator.AllocateItemAsync(line2, i1, null);

            Assert.Equal(2, i1.Allocations.Count);
            Assert.Same(outboundOrder, p1.CurrentUat);
            Assert.Equal(45, i1.Allocations.First().QuantityAllocated);
            Assert.Equal(15, i1.Allocations.Last().QuantityAllocated);
            Assert.Equal(1, line1.Allocations.Count);
            Assert.Equal(1, line2.Allocations.Count);

        }

        [Fact]
        public async Task AllocateItem_不可以分配到多个出库单()
        {
            ISession session = For<ISession>();
            ILogger logger = For<ILogger>();
            DefaultOutboundOrderAllocator allocator = new DefaultOutboundOrderAllocator(session, logger);

            Material material = new Material();
            OutboundOrder outboundOrder = new OutboundOrder();
            OutboundLine line1 = new OutboundLine
            {
                OutboundLineId = 1,
                Material = material,
                StockStatus = "合格",
                Uom = "PCS",
                QuantityDemanded = 45,
                QuantityFulfilled = 0,
            };
            outboundOrder.AddLine(line1);

            OutboundOrder outboundOrder2 = new OutboundOrder();
            OutboundLine line2 = new OutboundLine
            {
                OutboundLineId = 1,
                Material = material,
                StockStatus = "合格",
                Uom = "PCS",
                QuantityDemanded = 55,
                QuantityFulfilled = 0,
            };
            outboundOrder2.AddLine(line2);

            DefaultUnitloadFactory factory = new DefaultUnitloadFactory();
            Unitload p1 = factory.CreateUnitload();
            p1.PalletCode = "P1";
            p1.ResetCurrentUat();
            UnitloadItem i1 = factory.CreateUnitloadItem();
            i1.Material = material;
            i1.Batch = "B";
            i1.StockStatus = "合格";
            i1.Uom = "PCS";
            i1.Quantity = 60;
            p1.AddItem(i1);

            await allocator.AllocateItemAsync(line1, i1, null);
            await allocator.AllocateItemAsync(line2, i1, null);

            Assert.Single(i1.Allocations);
            Assert.Same(outboundOrder, p1.CurrentUat);
            Assert.Equal(45, i1.Allocations.Single().QuantityAllocated);
            Assert.Equal(1, line1.Allocations.Count);
            Assert.Empty(line2.Allocations);

        }
    }
}
