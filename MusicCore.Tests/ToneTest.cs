using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MusicCore.Tests
{
    [TestClass]
    public class ToneTest
    {
        [TestMethod]
        public void TestFrequency()
        {
            Assert.AreEqual(220.0, new Tone("A4").Frequency, 0.00001);
            Assert.AreEqual(261.6, new Tone("C5").Frequency, 0.1);
            Assert.AreEqual(440.0, new Tone("A5").Frequency, 0.00001);
        }

        [TestMethod]
        public void TestCurveA()
        {
            Assert.AreEqual(-25.60, new Tone("C3").CurveA, 0.005);
            Assert.AreEqual(-15.62, new Tone("C4").CurveA, 0.005);
            Assert.AreEqual(-8.26, new Tone("C5").CurveA, 0.005);
            Assert.AreEqual(-2.96, new Tone("C6").CurveA, 0.005);
            Assert.AreEqual(0.13, new Tone("C7").CurveA, 0.005);
        }
    }
}