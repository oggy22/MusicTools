using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

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

            var copy = FlatCopy(lists);
            AssertEqual(lists, copy);

            ChomskyGrammarAnalysis.Reduce(lists);
            AssertEqual(lists, copy);
        }

        [DataRow(@"Resources\Mozart_Symphony40_Allegro.mid")]
        [DataTestMethod]
        public void Mozart_Symphony40_Allegro(string filename)
        {
            var lists = MidiFileReader.Read(filename).Take(2).ToList();

            var copy = FlatCopy(lists);
            AssertEqual(lists, copy);

            ChomskyGrammarAnalysis.Reduce(lists);
            AssertEqual(lists, copy);
        }


        #region Helper methods
        static void AssertEqual(IEnumerable<MelodyPartList> lists1, IEnumerable<MelodyPartList> lists2)
        {
            var enumerator = lists1.GetEnumerator();
            foreach (var mpl in lists2)
            {
                Assert.IsTrue(enumerator.MoveNext());

                var mplCopy = enumerator.Current;
                Assert.IsTrue(mpl.GetNotes().SequenceEqual(mplCopy.GetNotes()));
            }

            Assert.IsFalse(enumerator.MoveNext());
        }

        static List<MelodyPartList> FlatCopy(List<MelodyPartList> lists) =>
            lists.ConvertAll(mpl =>
            {
                MelodyPartList mplCopy = new MelodyPartList();
                foreach (var nwd in mpl)
                    mplCopy.Add(nwd);

                return mplCopy;
            });
        #endregion
    }
}