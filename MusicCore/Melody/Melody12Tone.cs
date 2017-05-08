using System;
using System.Collections.Generic;

namespace MusicCore
{
    public class Melody12Tone : MelodyBase
    {
        MelodyBase melodyBase;
        TwelveToneSet toneset;
        int key = 0;
        public readonly int tempo;

        public override Fraction StartPause
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IEnumerable<NoteWithDuration> Anacrusis => melodyBase.Anacrusis;

        public override Fraction Duration => melodyBase.Duration;

        public Melody12Tone(MelodyBase melodyBase, MusicalModes mode, int key = 0, int tempo = 60)
        {
            this.melodyBase = melodyBase;
            toneset = new TwelveToneSet(mode);
            this.key = key;
            this.tempo = tempo;
        }

        public override IEnumerable<NoteWithDuration> Notes()
        {
            foreach (var notewd in melodyBase.Notes())
            {
                yield return new NoteWithDuration(notewd.IsPause ? Note.PAUSE : key + toneset.Calculate(notewd.note), notewd.duration);
            }
        }
    }
}
