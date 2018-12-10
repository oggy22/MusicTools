﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            Test(filename, 2);
        }

        [DataRow(@"Resources\Mozart_Symphony40_Allegro.mid")]
        [DataTestMethod]
        public void Mozart_Symphony40_Allegro(string filename)
        {
            Test(filename, 2);
        }

        private void Test(string filename, int take)
        {
            var lists = MidiFileReader.Read(filename).Take(take).ToList();

            // Save copy
            var copy = FlatCopy(lists);
            AssertEqual(lists, copy);

            // Perform Chomsky reduction
            var allNodes = ChomskyGrammarAnalysis.Reduce(lists);
            AssertEqual(lists, copy);

            // Post analysis
            ChomskyGrammarAnalysis.PostAnalysis(lists);
            ChomskyGrammarAnalysis.Print(allNodes);
            CheckPostAnalysis(allNodes, lists.Count);
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
                MelodyPartList mplCopy = new MelodyPartList(mpl);

                return mplCopy;
            });

        private void CheckPostAnalysis(List<MelodyPartList> lists, int numberOfLines)
        {
            // Top-nodes total occurances == 1
            for (int i = 0; i < numberOfLines; i++)
                Assert.AreEqual(1, lists[i].TotalOccurances);

            // Non-top-nodes total occurances >= 2, start with 0
            for (int i = numberOfLines; i < lists.Count; i++)
            {
                Assert.IsTrue(lists[i].TotalOccurances >= 2);
                Assert.AreEqual(0, lists[i].GetNotes().First().note);
            }

            // Child total occurances > parent total occurances
            foreach (var list in lists)
                foreach (var child in list.GetChildren())
                    Assert.IsTrue(child.TotalOccurances > list.TotalOccurances);

            // At least two elements
            foreach (var list in lists)
                Assert.IsTrue(list.Count >= 2);
        }
        #endregion
    }
}