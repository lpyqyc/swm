using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Swm.Materials.Tests
{
    public class StockKeyExtensionsTest
    {
        private class Foo : IHasStockKey
        {
            public Material? Material { get; set; }

            public string? Batch { get; set; }

            public string? StockStatus { get; set; }

            public string? Uom { get; set; }

            public string? ExtraKeyProp { get; set; }

            public string? NormalProp { get; set; }
        }

        private record FooStockKey(Material Material, string Batch, string StockStatus, string Uom, string OtherKeyProp) : StockKeyBase;


        [Fact]
        public void GetStockKey_�ɴ�IHasStockKey�����ȡ����()
        {
            Foo foo = new Foo
            {
                Material = new Material(),
                Batch = "1513",
                StockStatus = "�ϸ�",
                Uom = "PCS",
                ExtraKeyProp = "X",
                NormalProp = "300"
            };

            FooStockKey stockKey = foo.GetStockKey<FooStockKey>();

            Assert.Same(foo.Material, stockKey.Material);
            Assert.Equal(foo.Batch, stockKey.Batch);
            Assert.Equal(foo.StockStatus, stockKey.StockStatus);
            Assert.Equal(foo.Uom, stockKey.Uom);
            Assert.Equal(foo.ExtraKeyProp, stockKey.OtherKeyProp);
            Assert.Equal(new FooStockKey(foo.Material, foo.Batch, foo.StockStatus, foo.Uom, foo.ExtraKeyProp), stockKey);
        }


        [Fact]
        public void SetStockKey_����IHasStockKey�������ÿ���()
        {
            FooStockKey stockKey = new FooStockKey(new Material(), "1513", "�ϸ�", "PCS", "X");

            Foo foo = new Foo();
            foo.SetStockKey(stockKey);

            Assert.Same(stockKey.Material, foo.Material);
            Assert.Equal(stockKey.Batch, foo.Batch);
            Assert.Equal(stockKey.StockStatus, foo.StockStatus);
            Assert.Equal(stockKey.Uom, foo.Uom);
            Assert.Equal(stockKey.OtherKeyProp, foo.ExtraKeyProp);

            Assert.Null(foo.NormalProp);
        }


        [Fact]
        public void OfStockKey_�ܹ��õ���ȷ��ɸѡ���()
        {
            Foo foo1 = new Foo { Material = new Material(), Batch = "1513", StockStatus = "�ϸ�", Uom = "PCS", ExtraKeyProp = "X", NormalProp = "300" };
            Foo foo2 = new Foo { Material = new Material(), Batch = "1514", StockStatus = "�ϸ�", Uom = "PCS", ExtraKeyProp = "X", NormalProp = "300" };
            Foo foo3 = new Foo { Material = new Material(), Batch = "1515", StockStatus = "�ϸ�", Uom = "PCS", ExtraKeyProp = "X", NormalProp = "300" };

            IQueryable<Foo> q = new List<Foo> { foo1, foo2, foo3 }.AsQueryable();
            FooStockKey stockKey = foo2.GetStockKey<FooStockKey>();

            var q2 = q.OfStockKey(stockKey);

            Assert.Equal(3, q.Count());
            Assert.Equal(1, q2.Count());
            Assert.Same(foo2, q2.Single());
        }


        [Fact]
        public void BuildWhereClause_�ܹ��õ���ȷ��Where���()
        {
            var arr = new (string name, object? value)[] 
            {
                ("A", 100),
                ("B", 200),
                ("C", 300),
            };

            string where = StockKeyExtensions.BuildWhereClause(arr);

            const string expected = @"A = @0 AND B = @1 AND C = @2";
            Assert.Equal(expected, where);
        }


        [Fact]
        public void TestStockKeyComponent()
        {
            (string name, object value) a = ("A", 100);
            (string name, object value) b = ("A", 100);
            (string name, object value) c = ("A", 200);
            Assert.Equal(a, b);
            Assert.NotEqual(a, c);
        }
    }
}
