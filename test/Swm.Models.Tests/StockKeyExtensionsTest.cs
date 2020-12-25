using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Swm.Model.Tests
{
    public class StockKeyExtensionsTest
    {
        private class Foo : IHasStockKey
        {
            public Material Material { get; set; }
            
            public  string Batch { get; set; }
            
            public string StockStatus { get; set; }

            public  string Uom { get; set; }

            public  string OtherKeyProp { get; set; }

            public string NormalProp { get; set; }
        }

        private record FooStockKey(Material Material, string Batch, string StockStatus, string Uom, string OtherKeyProp) : StockKeyBase;


        [Fact]
        public void GetStockKey_可从IHasStockKey对象读取库存键()
        {
            Foo foo = new Foo
            {
                Material = new Material(),
                Batch = "1513",
                StockStatus = "合格",
                Uom = "PCS",
                OtherKeyProp = "X",
                NormalProp = "300"
            };

            FooStockKey stockKey = foo.GetStockKey<FooStockKey>();

            Assert.Same(foo.Material, stockKey.Material);
            Assert.Equal(foo.Batch, stockKey.Batch);
            Assert.Equal(foo.StockStatus, stockKey.StockStatus);
            Assert.Equal(foo.Uom, stockKey.Uom);
            Assert.Equal(foo.OtherKeyProp, stockKey.OtherKeyProp);
            Assert.Equal(new FooStockKey(foo.Material, foo.Batch, foo.StockStatus, foo.Uom, foo.OtherKeyProp), stockKey);
        }


        [Fact]
        public void SetStockKey_可向IHasStockKey对象设置库存键()
        {
            FooStockKey stockKey = new FooStockKey(new Material(), "1513", "合格", "PCS", "X");

            Foo foo = new Foo();
            foo.SetStockKey(stockKey);

            Assert.Same(stockKey.Material, foo.Material);
            Assert.Equal(stockKey.Batch, foo.Batch);
            Assert.Equal(stockKey.StockStatus, foo.StockStatus);
            Assert.Equal(stockKey.Uom, foo.Uom);
            Assert.Equal(stockKey.OtherKeyProp, foo.OtherKeyProp);

            Assert.Null(foo.NormalProp);
        }


        [Fact]
        public void OfStockKey_能够得到正确的筛选结果()
        {
            Foo foo1 = new Foo { Material = new Material(), Batch = "1513", StockStatus = "合格", Uom = "PCS", OtherKeyProp = "X", NormalProp = "300" };
            Foo foo2 = new Foo { Material = new Material(), Batch = "1514", StockStatus = "合格", Uom = "PCS", OtherKeyProp = "X", NormalProp = "300" };
            Foo foo3 = new Foo { Material = new Material(), Batch = "1515", StockStatus = "合格", Uom = "PCS", OtherKeyProp = "X", NormalProp = "300" };

            IQueryable<Foo> q = new List<Foo> { foo1, foo2, foo3 }.AsQueryable();
            FooStockKey stockKey = foo2.GetStockKey<FooStockKey>();

            var q2 = q.OfStockKey(stockKey);

            Assert.Equal(3, q.Count());
            Assert.Equal(1, q2.Count());
            Assert.Same(foo2, q2.Single());
        }


        [Fact]
        public void BuildWhereClause_能够得到正确的Where语句()
        {

            StockKeyComponent[] arr = new StockKeyComponent[] {
                new StockKeyComponent("A", 100),
                new StockKeyComponent("B", 200),
                new StockKeyComponent("C", 300),
            };

            string where = Queries.BuildWhereClause(arr);

            const string expected = @"A = @0 AND B = @1 AND C = @2";
            Assert.Equal(expected, where);
        }

    }
}
