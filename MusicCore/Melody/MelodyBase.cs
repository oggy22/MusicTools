using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MusicCore
{
    public enum Alteration { Flat=-1, Natural = 0, Sharp = 1}

    public class Note
    {
        public readonly int note;
        public readonly Alteration alter;
        public const int PAUSE = int.MinValue;
        public bool IsPause => note == PAUSE;
        public Note otherNote;

        public Note(int note, Alteration alter = Alteration.Natural)
        {
            this.note = note;
            this.alter = alter;
        }

        public Note(string st)
        {
            int index = st.IndexOf('/');

            if (index != -1)
            {
                otherNote = new Note(st.Substring(index + 1));
                st = st.Substring(0, index);
            }

            note = int.Parse(st.TrimEnd('+', '-'));
            char last = st[st.Length - 1];
            if (last == '+')
                alter = Alteration.Sharp;
            else if (last == '-')
                alter = Alteration.Flat;
            else
                alter = Alteration.Natural;
        }

        private static string StAlter(Alteration alter)
        {
            switch (alter)
            {
                case Alteration.Flat: return "-";
                case Alteration.Sharp: return "+";
            }
            return string.Empty;
        }

        public override string ToString()
        {
            return string.Format($"{note}{StAlter(alter)}");
        }
    }

    public class NoteWithDuration : Note
    {
        public readonly Fraction duration;

        public NoteWithDuration(int note, Alteration alter, Fraction duration) : base(note, alter)
        {
            Debug.Assert(duration.p > 0);
            this.duration = duration;
        }

        public NoteWithDuration(int note, Fraction duration) : this(note, Alteration.Natural, duration)
        {
        }

        static public NoteWithDuration MakePause(Fraction duration)
        {
            return new NoteWithDuration(PAUSE, Alteration.Natural, duration);
        }

        public NoteWithDuration ChangedDuration(Fraction dur)
        {
            return new NoteWithDuration(note, alter, dur);
        }

        public override string ToString()
        {
            return string.Format($"{this as Note}{duration}");
        }
    }

    public abstract class MelodyBase
    {
        public abstract IEnumerable<NoteWithDuration> Notes();

        public virtual IEnumerable<NoteWithDuration> Notes(int[] coeffs)
        {
            throw new NotImplementedException();
        }

        public abstract Fraction StartPause { get; }

        public abstract IEnumerable<NoteWithDuration> Anacrusis { get; }

        public abstract Fraction Duration { get; }
    }
}
