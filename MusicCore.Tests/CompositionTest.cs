using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MusicCore.Tests
{
    [TestClass]
    public class CompositionTest
    {
        [DataRow(@"Bach_invention_1_Cmajor.mid")]
        [DataRow(@"Bach_invention_4_Dminor.mid")]
        [DataRow(@"Bach_invention_8_Fmajor.mid")]
        [DataRow(@"Bach_invention_10_Gmajor.mid")]
        [DataRow(@"Bach_invention_13_Aminor.mid")]
        [DataRow(@"Bach_Air_on_G_String_BWV1068.mid")]
        [DataRow(@"Mozart_Symphony40_Allegro.mid")]
        [DataTestMethod]
        public void PlayBackTest(string filename)
        {
            Composition composition = MidiFileReader.Read(filename);
            int[] tones = new int[128];

            // Play the composition keeping track of what nodes are on/off 
            composition.PlayBack(
                note =>
                {
                    tones[note]++;
                },
                note =>
                {
                    tones[note]--;
                    Assert.IsTrue(tones[note] >= 0);
                },
                0
                );

            // At the end all tones are released
            foreach (int i in tones)
                Assert.AreEqual(0, tones[0]);
        }
    }
}