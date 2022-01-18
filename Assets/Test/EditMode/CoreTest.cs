using NUnit.Framework;
using Core;

namespace Test.EditMode
{
    public class CoreTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void FractionTest()
        {
            var x = new Fraction(1, 2);
            Assert.AreEqual(x.ToString(), "1 / 2");
        }

    }
}