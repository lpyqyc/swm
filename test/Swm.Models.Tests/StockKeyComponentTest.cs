using Xunit;

namespace Swm.Model.Tests
{
    public class StockKeyComponentTest
    {
        [Fact]
        public void Equals_分量名相同且分量值相同_返回True()
        {
            const string name1 = "COMP-1";
            object value1 = new object();
            StockKeyComponent comp1 = new StockKeyComponent(name1, value1);
            StockKeyComponent comp2 = new StockKeyComponent(name1, value1);

            bool b = comp1.Equals(comp2);

            Assert.True(b);
        }

        [Fact]
        public void Equals_分量名相同但分量值不同_返回False()
        {
            const string name1 = "COMP-1";
            object value1 = new object();
            object value2 = new object();
            StockKeyComponent comp1 = new StockKeyComponent(name1, value1);
            StockKeyComponent comp2 = new StockKeyComponent(name1, value2);


            Assert.False(comp1.Equals(comp2));
            Assert.False(comp1 == comp2);

        }



    }
}
