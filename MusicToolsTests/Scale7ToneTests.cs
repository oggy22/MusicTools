using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MusicComposer.TonesAndIntervals.Tests
{
    [TestClass]
    public class Scale7ToneTests
    {
        [TestMethod]
        public void ToStringTest()
        {
            var x = new Scale7Tone(0);
            Assert.AreEqual("C0", new Scale7Tone(0).ToString());
            Assert.AreEqual("D0", new Scale7Tone(1).ToString());
            Assert.AreEqual("C1", new Scale7Tone(7).ToString());
            Assert.AreEqual("D1", new Scale7Tone(8).ToString());
            Assert.AreEqual("C-1", new Scale7Tone(-7).ToString());
            Assert.AreEqual("D-1", new Scale7Tone(-6).ToString());
        }
    }
}