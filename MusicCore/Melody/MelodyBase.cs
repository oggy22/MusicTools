using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MusicCore
{
    public enum Alteration { Flat=-1, Natural = 0, Sharp = 1}

    public class Note
    {
        public int note;
        public Alteration alter;
        public const int PAUSE = int.MinValue;
        public bool IsPause => note == PAUSE;
    }

    public class NoteWithDuration : Note
    {
        public Fraction duration;

        static public NoteWithDuration MakePause(Fraction duration)
        {
            return new NoteWithDuration() { note = Note.PAUSE, duration = duration };
        }
    }

    public abstract class MelodyBase
    {
        public abstract IEnumerable<Tuple<Note, Fraction>> NotesOld();
        public abstract IEnumerable<NoteWithDuration> Notes();

        public abstract Fraction StartPause { get; }

        public abstract IEnumerable<NoteWithDuration> Anacrusis { get; }

        public abstract Fraction Duration { get; }
    }
}
