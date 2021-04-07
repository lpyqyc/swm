using Serilog;
using Swm.Materials;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static NSubstitute.Substitute;

namespace Swm.Palletization.Tests
{
    public class UnitloadSnapshopHelperTest
    {

        public UnitloadSnapshopHelperTest(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.TestOutput(output)
                .CreateLogger();
        }


        [Fact]
        public void GetSnapshot_可以复制属性()
        {
            Unitload unitload = new()
            {
                UnitloadId = 1,
                PalletCode = "PalletCode",
                Comment = "Comment",
                HasCountingError = true
            };
            unitload.AddItem(new UnitloadItem
            {
                UnitloadItemId = 1,
                Batch = "B1",
                Material = new Material(),
                Fifo = "111",
                Quantity = 100,
                StockStatus = "合格",
                Uom = "PCS",
            });
            unitload.AddItem(new UnitloadItem
            {
                UnitloadItemId = 2,
                Batch = "B2",
                Material = new Material(),
                Fifo = "222",
                Quantity = 200,
                StockStatus = "不合格",
                Uom = "PCS",
            });


            UnitloadSnapshopHelper unitloadSnapshopHelper = new(() => new UnitloadSnapshot(), () => new UnitloadItemSnapshot(), For<ILogger>());

            UnitloadSnapshot snapshot = unitloadSnapshopHelper.GetSnapshot(unitload);
            UnitloadItem item1 = unitload.Items.Single(x => x.UnitloadItemId == 1);
            UnitloadItem item2 = unitload.Items.Single(x => x.UnitloadItemId == 2);
            UnitloadItemSnapshot itemSnapshot1 = snapshot.Items.Single(x => x.UnitloadItemId == 1);
            UnitloadItemSnapshot itemSnapshot2 = snapshot.Items.Single(x => x.UnitloadItemId == 2);
            Assert.Equal(0, snapshot.UnitloadSnapshotId);
            Assert.Equal(unitload.UnitloadId, snapshot.UnitloadId);
            Assert.Equal(unitload.PalletCode, snapshot.PalletCode);
            Assert.Same(unitload.StorageInfo, snapshot.StorageInfo);
            Assert.Equal(unitload.Comment, snapshot.Comment);
            Assert.Equal(unitload.HasCountingError, snapshot.HasCountingError);
            Assert.Equal(unitload.HasCountingError, snapshot.HasCountingError);

            Assert.Equal(0, itemSnapshot1.UnitloadItemSnapshotId);
            Assert.Same(snapshot, itemSnapshot1.Unitload);
            Assert.Equal(item1.UnitloadItemId, item1.UnitloadItemId);
            Assert.Equal(item1.Batch, item1.Batch);
            Assert.Same(item1.Material, item1.Material);
            Assert.Equal(item1.Fifo, item1.Fifo);
            Assert.Equal(item1.Quantity, item1.Quantity);
            Assert.Equal(item1.StockStatus, item1.StockStatus);
            Assert.Equal(item1.Uom, item1.Uom);

            Assert.Equal(0, itemSnapshot2.UnitloadItemSnapshotId);
            Assert.Same(snapshot, itemSnapshot2.Unitload);
            Assert.Equal(item2.UnitloadItemId, item2.UnitloadItemId);
            Assert.Equal(item2.Batch, item2.Batch);
            Assert.Same(item2.Material, item2.Material);
            Assert.Equal(item2.Fifo, item2.Fifo);
            Assert.Equal(item2.Quantity, item2.Quantity);
            Assert.Equal(item2.StockStatus, item2.StockStatus);
            Assert.Equal(item2.Uom, item2.Uom);
        }

        //[Fact]
        //public void UnitloadItemGetSnapshot_可以复制属性()
        //{
        //    UnitloadItem item = new UnitloadItem
        //    {
        //        UnitloadItemId = 2,
        //        Material = new Material(),
        //        Batch = "Batch",
        //        StockStatus = "StockStatus",
        //        Quantity = 2m,
        //        Uom = "Uom",
        //        ProductionTime = DateTime.Now,
        //    };

        //    UnitloadItemSnapshot snapshot = item.GetSnapshot(new DefaultUnitloadFactory());

        //    Assert.Equal(item.UnitloadItemId, snapshot.UnitloadItemId);
        //    Assert.Null(snapshot.Unitload);
        //    Assert.Same(item.Material, snapshot.Material);
        //    Assert.Equal(item.Batch, snapshot.Batch);
        //    Assert.Equal(item.StockStatus, snapshot.StockStatus);
        //    Assert.Equal(item.Quantity, snapshot.Quantity);
        //    Assert.Equal(item.Uom, snapshot.Uom);
        //    Assert.Equal(item.ProductionTime, snapshot.ProductionTime);

        //}


    }

}
