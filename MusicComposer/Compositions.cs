using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicCore;

namespace MusicComposer
{
    public static class Compositions
    {
        public static Melody12Tone OggyMelody()
        {
            RhythmPattern rhythm = new RhythmPattern(3, 2, "hh11");
            MelodyBase melody = new MelodyAtomic(rhythm, new object[] { 0, 1, 2, 0 });
            MelodyBase melodyEnd = new MelodyAtomic(new RhythmPattern(3, 2, "3"), new object[] { 0 });

            MelodyComposite melody2 = new MelodyComposite("AA-A--B", melody, melodyEnd);

            Melody12Tone m12tone = new Melody12Tone(melody2, MusicalModes.Minor, 64);
            return m12tone;
        }

        public static Melody12Tone MrSandman()
        {
            MelodyBase melody = new MelodyAtomic(new object[] { 0, 2, 4, 6 }, 2);
            var melody2 = new MelodyReversed(new MelodyDiffEnd(melody, new[] { new NoteWithDuration(5, new Fraction(1, 2)) }));
            MelodyComposite melodyComp = new MelodyComposite("ABA+", melody, melody2);
            Melody12Tone m12tone = new Melody12Tone(melodyComp, MusicalModes.Major, 64);
            return m12tone;
        }

        public static object Pratnja()
        {
            MelodyBase melody = new MelodyAtomic(new object[] { 0, 2 }, 2);
            MelodyComposite melodyComp = new MelodyComposite("A A1 A2 Ar1", melody);
            Melody12Tone m12tone = new Melody12Tone(melodyComp, MusicalModes.Major, 64);
            return m12tone;
        }

        public static Melody12Tone Albinoni()
        {
            RhythmPattern rhythm = new RhythmPattern(3, 4, "1h.qh.q 1 2");
            MelodyBase melody = new MelodyAtomic(rhythm, new object[] { 4, 3, 2, 1, 0, 0, -1 });
            MelodyComposite melodyComp = new MelodyComposite("AA+", melody);
            Melody12Tone m12tone = new Melody12Tone(melodyComp, MusicalModes.MinorHarmonic, 64);
            return m12tone;
        }

        public static Melody12Tone Rach2ndSymphAdagio()
        {
            MelodyAtomic melodyCEGB = new MelodyAtomic(new object[] { null, 0, 2, 4, 6, 7, 5, null }, 4);
            MelodyAtomic melodyEnd = new MelodyAtomic(new object[] { 1, null, null, 2, 1, null, 0, null }, 4);
            melodyEnd.anacrusis = new List<NoteWithDuration>()
            {
                new NoteWithDuration(2, new Fraction(1, 2))
            };
            MelodyComposite melodyComp = new MelodyComposite("AA-A--B", melodyCEGB, melodyEnd);
            return new Melody12Tone(melodyComp, MusicalModes.Major, 64);
        }

        public static Melody12Tone GabrielaImprov()
        {
            MelodyAtomic melody = new MelodyAtomic(new object[] { 2, null, 2, null, 2, null, 1, 0, 1, null, null, null}, 3);
            melody.anacrusis = new List<NoteWithDuration>()
            {
                new NoteWithDuration(0, new Fraction(1, 4)),
                new NoteWithDuration(1, new Fraction(1, 4))
            };
            MelodyComposite melodyComp = new MelodyComposite("AA+A++", melody);
            return new Melody12Tone(melodyComp, MusicalModes.Minor, 64);
        }

        public static Melody12Tone GMajorMenuet()
        {
            MelodyAtomic melody = new MelodyAtomic(new object[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 4);
            return new Melody12Tone(melody, MusicalModes.Major, 60);
        }

        public static Melody12Tone WeWishYouAMerryChristmas()
        {
            RhythmPattern rhythm = new RhythmPattern(3, 2, "1hhhh111");
            MelodyAtomic melody = new MelodyAtomic(rhythm, new object[] { 0, 0, 1, 0, -1, -2, -4, -2 });
            melody.anacrusis = new List<NoteWithDuration>() { new NoteWithDuration(-3, new Fraction(1, 1)) };
            MelodyAtomic melody2 = new MelodyAtomic(new RhythmPattern(3, 2, "1113"), new object[] { -2, 1, -1, 0 });
            MelodyComposite meloComp = new MelodyComposite("AA+A++B", melody, melody2);
            return new Melody12Tone(meloComp, MusicalModes.Major, 65);
        }
    }
}