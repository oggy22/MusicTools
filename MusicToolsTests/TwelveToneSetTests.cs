using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicCore;
using System;

namespace MusicComposer.Tests
{
    [TestClass]
    public class TwelveToneSetTests
    {
        [TestMethod]
        public void TwelveToneSetScaleTest()
        {
            // Check major and minor scale
            Assert.AreEqual(new TwelveToneSet(new int[] { 0, 2, 4, 5, 7, 9, 11 }), TwelveToneSet.majorScale);
            Assert.AreEqual(new TwelveToneSet(new int[] { 0, 2, 3, 5, 7, 8, 10 }), TwelveToneSet.minorScale);

            // Major and minor scale are different but similar
            Assert.AreNotEqual(TwelveToneSet.majorScale, TwelveToneSet.minorScale);
            Assert.IsTrue(TwelveToneSet.majorScale.Similar(TwelveToneSet.minorScale));

            // Minor, minor harmonic, minor melodic are NOT similar
            Assert.IsFalse(TwelveToneSet.minorScale.Similar(TwelveToneSet.minorHarmonicScale));
            Assert.IsFalse(TwelveToneSet.minorScale.Similar(TwelveToneSet.minorMelodicScale));
            Assert.IsFalse(TwelveToneSet.minorHarmonicScale.Similar(TwelveToneSet.minorMelodicScale));
        }

        [TestMethod]
        public void MeasureHarmonyByFifthsTest()
        {
            // Major and minor thirds
            Assert.AreEqual(4, new TwelveToneSet("CE").MeasureHarmonyByFifths());
            Assert.AreEqual(3, new TwelveToneSet("AC").MeasureHarmonyByFifths());

            // Major and minor fifths
            Assert.AreEqual(4, new TwelveToneSet("CEG").MeasureHarmonyByFifths());
            Assert.AreEqual(4, new TwelveToneSet("ACE").MeasureHarmonyByFifths());

            // Sevenths
            Assert.AreEqual(5, new TwelveToneSet("CEGB").MeasureHarmonyByFifths());
            Assert.AreEqual(6, new TwelveToneSet("GHDF").MeasureHarmonyByFifths());
            Assert.AreEqual(6, new TwelveToneSet("CEGB♭").MeasureHarmonyByFifths());

            // Augmented and diminished
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
                Assert.AreEqual(7, toneset.Count);
            }
        }
    }
}