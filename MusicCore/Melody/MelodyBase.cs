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

        public string NoteToString()
        {
            return string.Format($"{note}{StAlter(alter)}");
        }

        public override string ToString()
        {
            return NoteToString();
        }
    }

    public class NoteWithDuration : Note, IEquatable<NoteWithDuration>
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
            return $"{NoteToString()}:{duration}";
        }

        public bool Equals(NoteWithDuration other)
        {
            return duration == other.duration &&
                note == other.note &&
                alter == other.alter;
        }

        public override bool Equals(object obj)
        {
            NoteWithDuration node = obj as NoteWithDuration;
            if (node == null) return false;
            return Equals(node);
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

    public class NoteList
    {
        private List<Note> notes;

        public NoteList(int[] ints)
        {
            notes = new List<Note>();
            foreach (int i in ints)
                notes.Add(new Note(i));
        }

        public NoteList(object[] objs)
        {
            notes = new List<Note>();
            foreach (object obj in objs)
            {
                if (obj is int) notes.Add(new Note((int)obj));
                else if (obj is Note) notes.Add(obj as Note);
                else Debug.Fail("unrecognized");
            }
        }

        public IEnumerable<Note> Notes() => notes;
    }
}
