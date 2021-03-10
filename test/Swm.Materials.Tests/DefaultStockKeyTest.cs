using Xunit;

namespace Swm.Materials.Tests
{
    public class DefaultStockKeyTest
    {
        [Fact]
        public void TestEquals()
        {
            DefaultStockKey k1 = new DefaultStockKey(new Material(), "B", "合格", "PCS");
            DefaultStockKey k2 = new DefaultStockKey(k1.Material, "B", "合格", "PCS");
            DefaultStockKey k3 = new DefaultStockKey(new Material(), "B", "合格", "PCS");

            Assert.True(k1.Equals(k2));
            Assert.True(k1 == k2);
            Assert.False(k1.Equals(k3));
        }

        [Fact]
        public void TestGetHashCode()
        {
            DefaultStockKey k1 = new DefaultStockKey(new Material(), "B", "合格", "PCS");
            DefaultStockKey k2 = new DefaultStockKey(k1.Material, "B", "合格", "PCS");
            DefaultStockKey k3 = new DefaultStockKey(new Material(), "B", "合格", "PCS");

            Assert.Equal(k1.GetHashCode(), k2.GetHashCode());
            Assert.NotEqual(k1.GetHashCode(), k3.GetHashCode());
        }
    }

}
