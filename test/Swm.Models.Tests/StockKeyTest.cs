//using Arctic.Models.Materials;
//using Xunit;

//namespace Swm.Model.Tests
//{
//    public class StockKeyTest
//    {
//        [Fact]
//        public void Equals_具有单个相等的分量_返回True()
//        {
//            StockKey stockKey1 = new StockKey(new StockKeyComponent("COMP-1", 100));
//            StockKey stockKey2 = new StockKey(new StockKeyComponent("COMP-1", 100));

//            Assert.True(stockKey1.Equals(stockKey2));
//            Assert.True(stockKey1 == stockKey2);
//        }

//        [Fact]
//        public void Equals_具有单个不相等的分量_返回False()
//        {
//            StockKey stockKey1 = new StockKey(new StockKeyComponent("COMP-1", 100));
//            StockKey stockKey2 = new StockKey(new StockKeyComponent("COMP-1", 200));

//            Assert.False(stockKey1.Equals(stockKey2));
//            Assert.False(stockKey1 == stockKey2);
//        }

//        [Fact]
//        public void Equals_具有多个相等但次序不同的分量_返回True()
//        {
//            StockKey stockKey1 = new StockKey(new StockKeyComponent("COMP-1", 100), new StockKeyComponent("COMP-2", 200));
//            StockKey stockKey2 = new StockKey(new StockKeyComponent("COMP-2", 200), new StockKeyComponent("COMP-1", 100));

//            Assert.True(stockKey1.Equals(stockKey2));
//            Assert.True(stockKey1 == stockKey2);
//        }

//        [Fact]
//        public void Equals_具有不同个数的分量_返回False()
//        {
//            StockKey stockKey1 = new StockKey(new StockKeyComponent("COMP-1", 100), new StockKeyComponent("COMP-2", 200));
//            StockKey stockKey2 = new StockKey(new StockKeyComponent("COMP-1", 100));

//            Assert.False(stockKey1.Equals(stockKey2));
//            Assert.False(stockKey1 == stockKey2);
//        }

//        [Fact]
//        public void GetHashCode_两个StockKey相等_哈希值相同()
//        {
//            StockKey stockKey1 = new StockKey(new StockKeyComponent("COMP-1", 100), new StockKeyComponent("COMP-2", 200));
//            StockKey stockKey2 = new StockKey(new StockKeyComponent("COMP-2", 200), new StockKeyComponent("COMP-1", 100));

//            int hashCode1 = stockKey1.GetHashCode();
//            int hashCode2 = stockKey2.GetHashCode();

//            Assert.Equal(hashCode1, hashCode2);
//        }

//        [Fact]
//        public void ComfirmTo_符合架构时_返回True()
//        {
//            StockKeySchema schema = new StockKeySchema("COMP-1", "COMP-2");
//            StockKey stockKey = new StockKey(new StockKeyComponent("COMP-2", 200), new StockKeyComponent("COMP-1", 100));

//            bool b = stockKey.ConformTo(schema);

//            Assert.True(b);
//        }

//        [Fact]
//        public void ComfirmTo_不符合架构时_返回False()
//        {
//            StockKeySchema schema = new StockKeySchema("COMP-1", "COMP-2");
//            StockKey stockKey = new StockKey(new StockKeyComponent("COMP-2", 200));

//            bool b = stockKey.ConformTo(schema);

//            Assert.False(b);
//        }
//    }

//}
