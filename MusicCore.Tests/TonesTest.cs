using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MusicCore.Tests
{
    [TestClass]
    public class TonesTest
    {
        [TestMethod]
        public void GetHarmonicsTest()
        {
            int count = 0, previous = -1;
            foreach (var d in Tone.GetHarmonics())
            {
                Assert.IsTrue((count == 0) == (d == 0));
                Assert.IsTrue(d > previous);
                previous = d;
                count++;
                if (count >= 100)
                    break;
            }
        }
    }
}
