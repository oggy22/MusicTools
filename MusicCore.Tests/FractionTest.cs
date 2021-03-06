﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MusicCore.Tests
{
    [TestClass]
    public class FractionTest
    {
        [TestMethod]
        public void FractionCtrs()
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

        [TestMethod]
        public void TestMin()
        {
            Assert.AreEqual(new Fraction(3, 4), Fraction.Min(new Fraction(3, 4), new Fraction(4, 5)));
            Fraction.Min(0, 1);
            Assert.AreEqual<Fraction>(0, Fraction.Min(0, 1));
            Assert.AreEqual<Fraction>(-2, Fraction.Min(-1, -2));
        }
    }
}
