using Arctic.EventBus;
using NHibernate;
using NSubstitute;
using Serilog;
using Swm.Materials;
using Swm.Palletization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static NSubstitute.Substitute;

namespace Swm.OutboundOrders.Tests
{
    public class PickHelperTest
    {
        public PickHelperTest(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.TestOutput(output)
                .CreateLogger();
        }

        struct TestData
        {
            public ISession session;
            public DefaultOutboundOrderAllocator allocator;
            public OutboundLine line1;
            public Unitload p1;
            public UnitloadItem i1;
            public Unitload p2;
            public UnitloadItem i2;
        }


        /// <summary>
        /// 需求 100，i1 分配 60/60，i2 分配 40/70
        /// </summary>
        /// <param name="session"></param>
        /// <param name="allocator"></param>
        /// <param name="line1"></param>
        /// <param name="p1"></param>
        /// <param name="i1"></param>
        /// <param name="p2"></param>
        /// <param name="i2"></param>
        private static async Task<TestData> PrepareDataAsync()
        {
            ISession session = For<ISession>();
            var allocator = new DefaultOutboundOrderAllocator(session, For<ILogger>());
            Material material = new Material();
            OutboundOrder outboundOrder = new OutboundOrder();
            outboundOrder.BizType = "出库";
            var line1 = new OutboundLine
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
            var p1 = factory.CreateUnitload();
            p1.PalletCode = "P1";
            p1.ResetCurrentUat();
            var i1 = factory.CreateUnitloadItem();
            i1.Material = material;
            i1.Batch = "B";
            i1.StockStatus = "合格";
            i1.Uom = "PCS";
            i1.Quantity = 60;
            p1.AddItem(i1);

            var p2 = factory.CreateUnitload();
            p2.PalletCode = "P2";
            p2.ResetCurrentUat();
            var i2 = factory.CreateUnitloadItem();
            i2.Material = material;
            i2.Batch = "B";
            i2.StockStatus = "合格";
            i2.Uom = "PCS";
            i2.Quantity = 70;
            p2.AddItem(i2);

            await allocator.AllocateItemAsync(line1, i1, null);
            await allocator.AllocateItemAsync(line1, i2, null);
            i1.Allocations.Single().UnitloadItemAllocationId = 1;
            i2.Allocations.Single().UnitloadItemAllocationId = 2;

            return new TestData
            {
                session = session,
                allocator = allocator,
                line1 = line1,
                p1 = p1,
                i1 = i1,
                p2 = p2,
                i2 = i2
            };
        }


        [Fact]
        public async Task 出库单拣货后会从货载项扣数并删除分配信息()
        {
            var testData = await PrepareDataAsync();

            OutboundOrderPickHelper outboundOrderPickHelper = new OutboundOrderPickHelper(
                new FlowHelper(
                    testData.session,
                    () => new Flow(),
                    new SimpleEventBus(
                        new Lazy<IEventHandler, EventHandlerMeta>[0],
                        For<ILogger>()
                    )),
                testData.session,
                For<ILogger>()
                );

            await outboundOrderPickHelper.PickAsync<DefaultStockKey>(
                testData.p1,
                new OutboundOrderPickInfo { QuantityPicked = 60, UnitloadItemAllocationId = 1 },
                null
                );
            await outboundOrderPickHelper.PickAsync<DefaultStockKey>(
                testData.p2,
                new OutboundOrderPickInfo { QuantityPicked = 20, UnitloadItemAllocationId = 2 },
                null
                );

            Assert.Empty(testData.p1.Items);
            Assert.Empty(testData.i2.Allocations);
            Assert.Equal(50, testData.i2.Quantity);
        }

        [Fact]
        public async Task 出库单拣货后会更新出库明细的实出数量()
        {
            var testData = await PrepareDataAsync();

            OutboundOrderPickHelper outboundOrderPickHelper = new OutboundOrderPickHelper(
                new FlowHelper(
                    testData.session,
                    () => new Flow(),
                    new SimpleEventBus(
                        new Lazy<IEventHandler, EventHandlerMeta>[0],
                        For<ILogger>()
                    )),
                testData.session,
                For<ILogger>()
                );

            await outboundOrderPickHelper.PickAsync<DefaultStockKey>(
                testData.p1,
                new OutboundOrderPickInfo { QuantityPicked = 60, UnitloadItemAllocationId = 1 },
                null
                );
            Assert.Equal(60, testData.line1.QuantityFulfilled);
            Assert.Single(testData.line1.Allocations);

            await outboundOrderPickHelper.PickAsync<DefaultStockKey>(
                testData.p2,
                new OutboundOrderPickInfo { QuantityPicked = 20, UnitloadItemAllocationId = 2 },
                null
                );
            Assert.Equal(80, testData.line1.QuantityFulfilled);
            Assert.Empty(testData.line1.Allocations);

        }

        [Fact]
        public async Task 出库单拣货后不会删除空货载()
        {
            var testData = await PrepareDataAsync();

            OutboundOrderPickHelper outboundOrderPickHelper = new OutboundOrderPickHelper(
                new FlowHelper(
                    testData.session,
                    () => new Flow(),
                    new SimpleEventBus(
                        new Lazy<IEventHandler, EventHandlerMeta>[0],
                        For<ILogger>()
                    )),
                testData.session,
                For<ILogger>()
                );

            await outboundOrderPickHelper.PickAsync<DefaultStockKey>(
                testData.p1,
                new OutboundOrderPickInfo { QuantityPicked = 60, UnitloadItemAllocationId = 1 },
                null
                );


            Assert.Empty(testData.p1.Items);
            Assert.Null(testData.p1.CurrentUat);
            testData.session.DidNotReceive().Delete(testData.p1);
            _ = testData.session.DidNotReceive().DeleteAsync(testData.p1);

        }

    }

}
