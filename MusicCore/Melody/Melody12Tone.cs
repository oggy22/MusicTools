using System;
using System.Collections.Generic;

namespace MusicCore
{
    public class Melody12Tone : MelodyBase
    {
        MelodyBase melodyBase;
        TwelveToneSet toneset;
        int key = 0;

        public override Fraction StartPause
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IEnumerable<NoteWithDuration> Anacrusis => melodyBase.Anacrusis;

        public override Fraction Duration => melodyBase.Duration;

        public Melody12Tone(MelodyBase melodyBase, MusicalModes mode, int key = 0)
        {
            this.melodyBase = melodyBase;
            toneset = new TwelveToneSet(mode);
            this.key = key;
        }

        public override IEnumerable<NoteWithDuration> Notes()
        {
            foreach (var notewd in melodyBase.Notes())
            {
                yield return new NoteWithDuration()
                {
                    note = notewd.IsPause ? Note.PAUSE : key + toneset.Calculate(notewd.note),
                    duration = notewd.duration
                };
            }
        }

        public override IEnumerable<Tuple<Note, Fraction>> NotesOld()
        {
            foreach (var tuple in melodyBase.NotesOld())
            {
                Note note = new Note()
                {
                    note = key + toneset.Calculate(tuple.Item1.note)
                };

                yield return new Tuple<Note, Fraction>(note, tuple.Item2);
            }
        }
    }
}
