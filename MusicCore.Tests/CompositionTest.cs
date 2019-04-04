using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MusicCore.Tests
{
    [TestClass]
    public class CompositionTest
    {
        [DataRow(@"Resources\Bach_invention_1_Cmajor.mid")]
        [DataRow(@"Resources\Bach_invention_4_Dminor.mid")]
        [DataRow(@"Resources\Bach_invention_8_Fmajor.mid")]
        [DataRow(@"Resources\Bach_invention_10_Gmajor.mid")]
        [DataRow(@"Resources\Bach_invention_13_Aminor.mid")]
        [DataRow(@"Resources\Bach_Air_on_G_String_BWV1068.mid")]
        [DataRow(@"Resources\Mozart_Symphony40_Allegro.mid")]
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
                IncludeDurations: false
                );

            // At the end all tones are released
            foreach (int i in tones)
                Assert.AreEqual(0, tones[0]);
        }
    }
}