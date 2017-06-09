using System.Collections.Generic;
using MusicCore;

namespace MusicComposer
{
    public static class Compositions
    {
        public static Melody12Tone CMajorPreludeBach()
        {
            MelodyAtomic melody = new MelodyAtomic(new object[] { 0, 1, 2, 3, 4, 2, 3, 4 }, 2);

            MelodyComposite composite = new MelodyComposite("AA", melody);
            const int C = 0, D = 1, E = 2, /*F = 3,*/ G = 4, A = 5, B = 6, C2 = 7, D2 = 8, E2 = 9, F2 = 10, G2 = 11, A2 = 12;
            const int Bsm = -1, Asm = -2, Gsm = -3, /*Fsm = -4, Esm = -5,*/ Dsm = -6;   // sm - small octave
            string FSharp = "3+";

            MelodySequencer seq = new MelodySequencer(composite,
            new object[] { C, E, G, C2, E2 },   //C
            new object[] { C, D, A, D2, F2 },   //C\d
            new object[] { Bsm, D, G, D2, F2 }, //B\G
            new object[] { C, E, G, C2, E2 },   //C

            new object[] { C, E, A, E2, A2 },       //C\a
            new object[] { C, D, FSharp, A, D2 },   //C\D
            new object[] { Bsm, D, G, D2, G2 },     //B\G
            new object[] { Bsm, C, E, G, C2 },      //B\C = Cmaj7
            new object[] { Asm, C, E, G, C2 },      //A\C = a7
            new object[] { Dsm, Asm, D, FSharp, C2 },   //D7
            new object[] { Gsm, Bsm, D, G, B }      //G
            );

            return new Melody12Tone(seq, MusicalModes.Major, 60, 80);
        }

        public static Melody12Tone DanceOfClocks()
        {
            RhythmPatternBase rhythm = new RhythmPattern(4, new bool[] { true, true, true, true, false, false, false, false });
            MelodyAtomic melody = new MelodyAtomic(rhythm, new object[] { 0, 1, 1, 2 });
            MelodySequencer seq = new MelodySequencer(melody,
                new object[] { 2, 4, 5 },
                new object[] { 5, 7, 6 },
                new object[] { 4, 5, 6 },
                new object[] { 6, 10, 9 },

                new object[] { 2, 9, 8 },
                new object[] { 8, 6, 5 },
                new object[] { 5, 3, 2 }
                );

            return new Melody12Tone(seq, MusicalModes.Major, 64, 80);
        }

        public static Melody12Tone ChopinAccompany()
        {
            RhythmPatternBase rhythm = new RhythmPattern(3, new bool[] { true, true, true });
            MelodyAtomic melody = new MelodyAtomic(rhythm, new object[] { 0, "1/3", "2/4" });
            MelodySequencer seq = new MelodySequencer(melody,
                new object[] { 0, 2, 4, 7, 9 },
                new object[] { 0, 2, 4, 7, 9 }
                );

            return new Melody12Tone(seq, MusicalModes.Major, 64, 60);
        }

        public static Melody12Tone OggyMelody()
        {
            RhythmPattern rhythm = new RhythmPattern(3, 2, "hh11");
            MelodyBase melody = new MelodyAtomic(rhythm, new object[] { 0, 1, 2, 0 });
            MelodyBase melodyEnd = new MelodyAtomic(new RhythmPattern(3, 2, "3"), new object[] { 0 });

            MelodyComposite melody2 = new MelodyComposite("AA-A--B", melody, melodyEnd);

            Melody12Tone m12tone = new Melody12Tone(melody2, MusicalModes.Minor, 64, 100);
            return m12tone;
        }

        public static Melody12Tone MrSandman()
        {
            MelodyBase melody = new MelodyAtomic(new object[] { 0, 2, 4, 6 }, 2);
            var melody2 = new MelodyReversed(new MelodyDiffEnd(melody, new[] { new NoteWithDuration(5, new Fraction(1, 2)) }));
            MelodyComposite melodyComp = new MelodyComposite("ABA+", melody, melody2);
            Melody12Tone m12tone = new Melody12Tone(melodyComp, MusicalModes.Major, 60, 150);
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
            Melody12Tone m12tone = new Melody12Tone(melodyComp, MusicalModes.MinorHarmonic, 64, 100);
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
            return new Melody12Tone(melodyComp, MusicalModes.Major, 60, 50);
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

        public static Melody12Tone GMajorMenuet2nd()
        {
            RhythmPattern rhythm = new RhythmPattern(3, 2, "1hhhh");
            ParameterizedMelody pmelody = new ParameterizedMelody("0' 0 1 2 0", rhythm);
            MelodyComposite mcomposote = new MelodyComposite("A(0,2) A(-3,1) A(-2,0)", pmelody);
            return new Melody12Tone(mcomposote, MusicalModes.Major, 60);
        }

        public static Melody12Tone CSharpValseChopin()
        {
            RhythmPattern rhythm = new RhythmPattern(3, 2, "hhhhhh");
            ParameterizedMelody pmelody = new ParameterizedMelody("0 1 0 -1 -3 0'", rhythm);
            MelodyComposite mcomposite = new MelodyComposite("A(4,-3) A(3,-3) A(2,-4) A(1,-5)", pmelody);
            return new Melody12Tone(mcomposite, MusicalModes.MinorHarmonic, 60);
        }

        public static Melody12Tone WeWishYouAMerryChristmas()
        {
            RhythmPattern rhythm = new RhythmPattern(3, 2, "1hhhh111");
            MelodyAtomic melody = new MelodyAtomic(rhythm, new object[] { 0, 0, 1, 0, -1, -2, -4, -2 });
            melody.anacrusis = new List<NoteWithDuration>() { new NoteWithDuration(-3, new Fraction(1, 1)) };
            MelodyAtomic melody2 = new MelodyAtomic(new RhythmPattern(3, 2, "1113"), new object[] { -2, 1, -1, 0 });
            MelodyComposite meloComp = new MelodyComposite("AA+A++B", melody, melody2);
            return new Melody12Tone(meloComp, MusicalModes.Major, 65, 140);
        }

        public static Melody12Tone AnotherWhoopy()
        {
            MelodyAtomic melody = new MelodyAtomic(new RhythmPattern(4, 1, "4"), new object[] { 5 });
            melody.anacrusis = new List<NoteWithDuration>()
            {
                new NoteWithDuration(0, new Fraction(1, 3)),
                new NoteWithDuration(1, new Fraction(2, 3)),
                new NoteWithDuration(0, new Fraction(1, 3)),
            };

            MelodyComposite mc = new MelodyComposite("AA", melody);
            return new Melody12Tone(mc, MusicalModes.Major, 64, 100);
        }
    }
}