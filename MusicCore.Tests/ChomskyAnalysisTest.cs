﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MusicCore.Tests
{
    [TestClass]
    public class ChomskyAnalysisTest
    {
        [DataRow(@"Albinoni_Adagio.mid")]
        [DataTestMethod]
        public void Albinoni_Adagio(string filename)
        {
            Test(filename, int.MaxValue, new Fraction(1,8));
        }

        [DataRow(@"Bach_invention_1_Cmajor.mid")]
        [DataRow(@"Bach_invention_4_Dminor.mid")]
        [DataRow(@"Bach_invention_8_Fmajor.mid")]
        [DataRow(@"Bach_invention_10_Gmajor.mid")]
        [DataRow(@"Bach_invention_13_Aminor.mid")]
        //[DataRow(@"Bach_invention_14_Bbmajor.mid")]
        [DataTestMethod]
        public void Bach_inventions(string filename)
        {
            Test(filename, int.MaxValue, new Fraction(1, 96));
        }

        [DataRow(@"Bach_Air_on_G_String_BWV1068.mid")]
        [DataTestMethod]
        public void Bach_Air_on_G_String(string filename)
        {
            Test(filename, int.MaxValue, new Fraction(1, 24));
        }

        [DataRow(@"Mozart_Symphony40_Allegro.mid")]
        [DataTestMethod, Ignore]
        public void Mozart_Symphony40_Allegro(string filename)
        {
            Test(filename, 2);
        }

        private void Test(string filename, int take = int.MaxValue, Fraction? prec = null)
        {
            var lists = MidiFileReader.Read(filename, prec).GetVoices().Take(take).ToList();

            // Save copy
            var copy = FlatCopy(lists);
            //AssertEqual(lists, copy);

            // Perform Chomsky reduction
            var allNodes = ChomskyGrammarAnalysis.Reduce(lists, new List<TwelveToneSet>() { TwelveToneSet.majorScale });
            AssertEqual(lists, copy);

            // At least one diatonic?
            //Assert.IsTrue(allNodes.Exists(mpl => mpl.IsDiatonic));

            // Post analysis
            ChomskyGrammarAnalysis.PostAnalysis(lists);
            ChomskyGrammarAnalysis.Print(allNodes);
            CheckPostAnalysis(allNodes, lists.Count);

            int totalNotesBefore = copy.Sum(list => list.Count);
            int totalNotesAfter = allNodes.Sum(list => list.children.Count(imp => imp is NoteWithDuration));
            int totalLeafs = allNodes.Sum(list => list.IsLeaf ? 1 : 0);
            int totalLeafNotes = allNodes.Sum(list => list.IsLeaf ? list.Count : 0);

            Debug.WriteLine(filename);
            Debug.WriteLine($"Total notes before: {totalNotesBefore}");
            Debug.WriteLine($"Total notes after: {totalNotesAfter}");
            Debug.WriteLine($"After/before: {100 * totalNotesAfter / totalNotesBefore}%");
            Debug.WriteLine($"Total leaf notes: {totalLeafNotes}");
            Debug.WriteLine($"Leaf notes/before: {100 * totalLeafNotes / totalNotesBefore}%");
            Debug.WriteLine($"Total leafs: {totalLeafs}");
        }

        #region Helper methods
        static void AssertEqual(IEnumerable<MelodyPartList> lists1, IEnumerable<MelodyPartList> lists2)
        {
            //lists2.First().Play();

            var enumerator = lists1.GetEnumerator();
            int i = 0;
            foreach (var mpl in lists2)
            {
                Assert.IsTrue(enumerator.MoveNext());

                var mplCopy = enumerator.Current;

                var list = mpl.GetNotes().ToList();
                var listCopy = new List<NoteWithDuration>();
                foreach (var nwd in mplCopy.GetNotes())
                {
                    listCopy.Add(nwd);
                }

                for (int j = 0; j < list.Count; j++)
                {
                    Debug.WriteLine($"{list[j]} {listCopy[j]}");
                    Assert.AreEqual(
                        list[j],
                        listCopy[j],
                        $"At {j}"
                        );
                }

                Assert.IsTrue(mpl.GetNotes().SequenceEqual(mplCopy.GetNotes()));
                i++;
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
                if (list.type == MelodyPartList.Type.Melody)
                {
                    switch (list.Count)
                    {
                        case 0 : Assert.Fail("No parents"); break;
                        case 1: Assert.IsTrue(list[0] is MelodyPartDtoC); break;
                        default: break;
                    }
                }
        }
        #endregion
    }
}