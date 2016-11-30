using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicCore;

namespace Tests.MusicCore
{
    [TestClass]
    public class FractionTest
    {
        [TestMethod]
        public void Ctrs()
        {
            Fraction fract = new Fraction(4, 6);
            Assert.AreEqual(2, fract.p);
            Assert.AreEqual(3, fract.q);
        }

        [TestMethod]
        public void TestGCD()
        {
            Assert.AreEqual(5, Fraction.GCD(20, 15));
            Assert.AreEqual(1, Fraction.GCD(17, 25));
            Assert.AreEqual(2, Fraction.GCD(4, 6));
        }
    }
}
