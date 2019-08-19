using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MusicCore.Tests
{
    [TestClass]
    public class TwelveToneSetTests
    {
        [TestMethod]
        public void DiatonicToChromatic()
        {
            Assert.AreEqual(-1, TwelveToneSet.majorScale.ChromaticToDiatonic(-1));
        }

        [TestMethod]
        public void TwelveToneSetScaleTest()
        {
            // Check major and minor scale
            Assert.AreEqual(new TwelveToneSet(new int[] { 0, 2, 4, 5, 7, 9, 11 }), TwelveToneSet.majorScale);
            Assert.AreEqual(new TwelveToneSet(new int[] { 0, 2, 3, 5, 7, 8, 10 }), TwelveToneSet.minorScale);

            //// Major and minor scale are different but similar
            Assert.AreNotEqual(TwelveToneSet.majorScale, TwelveToneSet.minorScale);
            Assert.IsTrue(TwelveToneSet.majorScale.IsSimilar(TwelveToneSet.minorScale));

            // Minor, minor harmonic, minor melodic are NOT similar
            Assert.IsFalse(TwelveToneSet.minorScale.IsSimilar(TwelveToneSet.minorHarmonicScale));
            Assert.IsFalse(TwelveToneSet.minorScale.IsSimilar(TwelveToneSet.minorMelodicScale));
            Assert.IsFalse(TwelveToneSet.minorHarmonicScale.IsSimilar(TwelveToneSet.minorMelodicScale));

            // Ionian, Dorian, Phrygian, Lydian, Mixolydian, Aoelian, Locrian
            for (MusicalModes mode = MusicalModes.Ionian; mode<=MusicalModes.Locrian; mode++)
            {
                TwelveToneSet set = new TwelveToneSet(mode);
                Assert.IsTrue(set.IsSimilar(TwelveToneSet.majorScale));
                Assert.IsFalse(set.IsSimilar(TwelveToneSet.minorHarmonicScale));
                Assert.IsFalse(set.IsSimilar(TwelveToneSet.minorMelodicScale));
            }
        }

        [TestMethod]
        public void MeasureHarmonyByFifthsTest()
        {
            // Major and minor thirds
            Assert.AreEqual(4, new TwelveToneSet("CE").MeasureHarmonyByFifths());
            Assert.AreEqual(3, new TwelveToneSet("AC").MeasureHarmonyByFifths());

            // Pentatonics
            Assert.AreEqual(4, new TwelveToneSet("CDEGA").MeasureHarmonyByFifths());

            // Major and minor fifths
            Assert.AreEqual(4, new TwelveToneSet("CEG").MeasureHarmonyByFifths());
            Assert.AreEqual(4, new TwelveToneSet("ACE").MeasureHarmonyByFifths());

            // Sevenths
            Assert.AreEqual(5, new TwelveToneSet("CEGB").MeasureHarmonyByFifths());
            Assert.AreEqual(6, new TwelveToneSet("GHDF").MeasureHarmonyByFifths());
            Assert.AreEqual(6, new TwelveToneSet("CEGB♭").MeasureHarmonyByFifths());

            // Major/minor scale
            Assert.AreEqual(6, new TwelveToneSet("CDEFGAB").MeasureHarmonyByFifths());

            // Augmented and diminished
            Assert.AreEqual(6, new TwelveToneSet("F#AC").MeasureHarmonyByFifths());
            Assert.AreEqual(8, new TwelveToneSet("DF#A#").MeasureHarmonyByFifths());
            Assert.AreEqual(9, new TwelveToneSet("D#F#AC").MeasureHarmonyByFifths());

            // Whole-tone scale
            Assert.AreEqual(10, new TwelveToneSet("CDEF#G#A#").MeasureHarmonyByFifths());
        }

        [TestMethod]
        public void TwelveToneSetCtr()
        {
            // Major and Minor scales
            Assert.AreEqual(TwelveToneSet.majorScale, new TwelveToneSet(MusicalModes.Major));
            Assert.AreEqual(TwelveToneSet.minorScale, new TwelveToneSet(MusicalModes.Minor));

            // Harmonic and Melodic minor scales
            Assert.AreNotEqual(TwelveToneSet.minorScale, new TwelveToneSet(MusicalModes.MinorHarmonic));
            Assert.AreNotEqual(TwelveToneSet.minorScale, new TwelveToneSet(MusicalModes.MinorMelodic));

            foreach (var mode in Enum.GetValues(typeof(MusicalModes)))
            {
                TwelveToneSet toneset = new TwelveToneSet((MusicalModes)mode);
                Assert.IsTrue(toneset.IsRooted);
                if ((MusicalModes)mode == MusicalModes.Chromatic)
                    Assert.AreEqual(TwelveToneSet.TWELVE, toneset.Count);
                else
                    Assert.AreEqual(7, toneset.Count);
            }
        }

        [TestMethod]
        public void TwelveToneSetConsts()
        {
            // Covered by major scale
            Assert.IsTrue(TwelveToneSet.majorTriad.CoveredByAnySimilar(TwelveToneSet.majorScale));
            Assert.IsTrue(TwelveToneSet.minorTriad.CoveredByAnySimilar(TwelveToneSet.majorScale));
            Assert.IsTrue(TwelveToneSet.major7.CoveredByAnySimilar(TwelveToneSet.majorScale));
            Assert.IsTrue(TwelveToneSet.minor7.CoveredByAnySimilar(TwelveToneSet.majorScale));
            Assert.IsTrue(TwelveToneSet.halfDiminished.CoveredByAnySimilar(TwelveToneSet.majorScale));
            Assert.IsFalse(TwelveToneSet.fullDiminished.CoveredByAnySimilar(TwelveToneSet.majorScale));
            Assert.IsFalse(TwelveToneSet.augmented.CoveredByAnySimilar(TwelveToneSet.majorScale));

            // Covered by minor harmonic scale
            Assert.IsTrue(TwelveToneSet.fullDiminished.CoveredByAnySimilar(TwelveToneSet.minorHarmonicScale));
            Assert.IsTrue(TwelveToneSet.augmented.CoveredByAnySimilar(TwelveToneSet.minorHarmonicScale));
        }

        [TestMethod]
        public void Tone12Test()
        {
            Assert.AreEqual<tone12>(0, 12);
            Assert.AreEqual<tone12>(3, 15);
            Assert.AreEqual<tone12>(5, -7);
        }

        [TestMethod]
        public void GetRootTest()
        {
            Assert.AreEqual<int>(0, TwelveToneSet.majorTriad.GetRoot());
            Assert.AreEqual<int>(0, TwelveToneSet.major7.GetRoot());
            Assert.AreEqual<int>(0, TwelveToneSet.minorTriad.GetRoot());
            Assert.AreEqual<int>(new tone12("F"), TwelveToneSet.halfDiminished.GetRoot());

            // No roots:
            Assert.ThrowsException<Exception>(() => TwelveToneSet.augmented.GetRoot());
            Assert.ThrowsException<Exception>(() => TwelveToneSet.chromatic.GetRoot());
            Assert.ThrowsException<Exception>(() => TwelveToneSet.fullDiminished.GetRoot());
            // minor7 can be interpreted as major 6th, therefore two roots i.e. no roots
            Assert.ThrowsException<Exception>(() => TwelveToneSet.minor7.GetRoot());

            // No roots for scales:
            Assert.ThrowsException<Exception>(() => TwelveToneSet.majorScale.GetRoot());
            Assert.ThrowsException<Exception>(() => TwelveToneSet.minorScale.GetRoot());
            Assert.ThrowsException<Exception>(() => TwelveToneSet.minorHarmonicScale.GetRoot());
            Assert.ThrowsException<Exception>(() => TwelveToneSet.minorMelodicScale.GetRoot());
            Assert.ThrowsException<Exception>(() => TwelveToneSet.chromatic.GetRoot());
        }

        [TestMethod]
        public void DiatonicToChromaticTest()
        {
            Assert.AreEqual(12, TwelveToneSet.majorScale.DiatonicToChromatic(7));
        }

        [TestMethod]
        public void ShiftInScaleTest_InScale()
        {
            Assert.AreEqual(
                new TwelveToneSet("DFA"),
                new TwelveToneSet("CEG").ShiftInScale(1, TwelveToneSet.majorScale));
            Assert.AreEqual(
                new TwelveToneSet("CD"),
                new TwelveToneSet("EF").ShiftInScale(5, TwelveToneSet.majorScale));
            Assert.AreEqual(
                TwelveToneSet.majorScale,
                TwelveToneSet.majorScale.ShiftInScale(5, TwelveToneSet.majorScale));
        }

        [TestMethod]
        public void ShiftInScaleTest_OutScale()
        {
            Assert.AreEqual(
                new TwelveToneSet("BD#"),
                new TwelveToneSet("AC#").ShiftInScale(1, TwelveToneSet.majorScale));
            TwelveToneSet AMajor = new TwelveToneSet("AC#E");
            Assert.AreEqual(
                new TwelveToneSet("BDF"),
                AMajor.ShiftInScale(1, TwelveToneSet.majorScale));
            Assert.AreEqual(
                new TwelveToneSet("GBD"),
                AMajor.ShiftInScale(-1, TwelveToneSet.majorScale));
            Assert.AreEqual(
                new TwelveToneSet("FAC"),
                AMajor.ShiftInScale(-2, TwelveToneSet.majorScale));
            Assert.AreEqual(
                new TwelveToneSet("EG#B"),
                AMajor.ShiftInScale(-3, TwelveToneSet.majorScale));

            TwelveToneSet CSharp = new TwelveToneSet("C#EGB");
            Assert.AreEqual(
                new TwelveToneSet("DFAC"),
                CSharp.ShiftInScale(1, TwelveToneSet.majorScale));
            Assert.AreEqual(
                new TwelveToneSet("F#ACE"),
                CSharp.ShiftInScale(3, TwelveToneSet.majorScale));
            Assert.AreEqual(
                new TwelveToneSet("BDFA"),
                CSharp.ShiftInScale(-1, TwelveToneSet.majorScale));
            Assert.AreEqual(
                new TwelveToneSet("ACEG"),
                CSharp.ShiftInScale(-2, TwelveToneSet.majorScale));

            TwelveToneSet FSharp = new TwelveToneSet("F#G");
            Assert.AreEqual(
                new TwelveToneSet("EF"),
                FSharp.ShiftInScale(-1, TwelveToneSet.majorScale));
            Assert.AreEqual(
                new TwelveToneSet("G#A"),
                FSharp.ShiftInScale(1, TwelveToneSet.majorScale));
            Assert.AreEqual(
                new TwelveToneSet("A#B"),
                FSharp.ShiftInScale(2, TwelveToneSet.majorScale));
            Assert.AreEqual(
                new TwelveToneSet("BC"),
                FSharp.ShiftInScale(3, TwelveToneSet.majorScale));
        }

        [TestMethod, Ignore]
        public void ShiftInScaleTest_0shift()
        {
            Random rand = new Random(0);
            for (int i=0; i<100; i++)
            {
                TwelveToneSet set = new TwelveToneSet(rand, rand.Next(3,6), TwelveToneSet.chromatic);
                TwelveToneSet setShifted = set.ShiftInScale(0, TwelveToneSet.majorScale);
                Assert.AreEqual(set, setShifted);
            }
        }
    }

    [TestClass]
    public class ToneSetTest
    {
        [TestMethod, Ignore]
        public void GetHighestCommonSubHarmonicTest()
        {
            //ToneSet majorChord = new ToneSet("C4", TwelveToneSet.majorTriad);
            //Assert.AreEqual<int>(Tone.FromString("C2"), majorChord.GetHighestCommonSubHarmonic());
            //Assert.AreEqual<int>(86, majorChord.GetLowestCommonHarmonic());

            //ToneSet minorChord = new ToneSet(48, TwelveToneSet.minorTriad);
            //Assert.AreEqual<int>(17, minorChord.GetHighestCommonSubHarmonic());
            //Assert.AreEqual<int>(79, minorChord.GetLowestCommonHarmonic());
        }

        [TestMethod, Ignore]
        public void GetDisharmony()
        {
            //ToneSet octave = new ToneSet(48, 60);
            //Assert.AreEqual<int>(12, octave.GetDisharmony(1, 1));

            //ToneSet twooctave2 = new ToneSet(48, 60, 72);
            //Assert.AreEqual<int>(12 + 12, twooctave2.GetDisharmony(1, 1));

            //ToneSet first3harms = new ToneSet(48, 60, 67);
            //Assert.AreEqual<int>(12 + 12 + 7, first3harms.GetDisharmony(1, 1));
        }
    }
}