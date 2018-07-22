using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
//using MSTest.TestFramework;

namespace MusicCore.Tests
{
    [TestClass]
    public class ChomskyAnalysisTest
    {
        [DataRow(@"Resources\Bach_invention_1_Cmajor.mid")]
        [DataRow(@"Resources\Bach_invention_4_Dminor.mid")]
        [DataRow(@"Resources\Bach_invention_8_Fmajor.mid")]
        [DataRow(@"Resources\Bach_invention_10_Gmajor.mid")]
        [DataRow(@"Resources\Bach_invention_13_Aminor.mid")]
        [DataTestMethod]
        public void Bach_inventions(string filename)
        {
            var lists = MidiFileReader.Read(filename);

            ChomskyGrammarAnalysis.Reduce(lists);
        }

        [DataRow(@"Resources\Mozart_Symphony40_Allegro.mid")]
        [DataTestMethod]
        public void Mozart_Symphony40_Allegro(string filename)
        {
            var lists = MidiFileReader.Read(filename).Take(2).ToList();

            ChomskyGrammarAnalysis.Reduce(lists);
        }
    }
}