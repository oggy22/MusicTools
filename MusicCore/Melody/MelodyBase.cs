﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace MusicCore
{
    public enum Alteration { Flat=-1, Natural = 0, Sharp = 1}

    public struct Note
    {
        public readonly int note;
        public readonly Alteration alter;
        public const int PAUSE = int.MinValue;
        public bool IsPause => note == PAUSE;

        public Note(int note, Alteration alter = Alteration.Natural)
        {
            Debug.Assert(-128 <= note && note < 128 || note==PAUSE);

            this.note = note;
            this.alter = alter;
            //otherNote = null;
        }

        public Note(string st)
        {
            int index = st.IndexOf('/');

            if (index != -1)
            {
                //otherNote = new Note(st.Substring(index + 1));
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
            if (IsPause)
                return "Pause";

            return string.Format($"{note}{StAlter(alter)}");
        }

        public override string ToString()
        {
            return NoteToString();
        }

        static public implicit operator Note(NoteWithDuration nwd)
        {
            return new Note(nwd.note, nwd.alter);
        }
    }

    public struct NoteWithDuration : IEquatable<NoteWithDuration>, IMelodyPart
    {
        public readonly int note;
        public readonly Alteration alter;
        public const int PAUSE = int.MinValue;
        public bool IsPause => note == PAUSE;

        public Fraction Duration { get; }

        Fraction IMelodyPart.duration => Duration;

        public NoteWithDuration(Fraction duration)
        {
            Debug.Assert(duration.p > 0);
            note = PAUSE;
            alter = Alteration.Natural;
            this.Duration = duration;
        }

        public NoteWithDuration(int note, Alteration alter, Fraction duration)
        {
            this.note = note;
            this.alter = alter;
            Debug.Assert(duration.p > 0);
            this.Duration = duration;
        }

        public NoteWithDuration(int note, Fraction duration)
        {
            this.note = note;
            this.alter = Alteration.Natural;
            this.Duration = duration;
        }

        public static NoteWithDuration operator +(NoteWithDuration note, int offset)
        {
            if (note.IsPause)
                return new NoteWithDuration(note.Duration);

            return new NoteWithDuration(note.note + offset, note.alter, note.Duration);
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
            string stNote = note == PAUSE ? "PAUSE" : note.ToString();
            string stAlter = "";
            if (alter != Alteration.Natural)
                stAlter = alter == Alteration.Flat ? "b" : "#";

            return $"{stNote}{stAlter}:{Duration}";
        }

        public bool Equals(NoteWithDuration other)
        {
            return Duration == other.Duration &&
                note == other.note &&
                alter == other.alter;
        }

        public override int GetHashCode()
        {
            return note.GetHashCode() + Duration.GetHashCode();
        }

        IEnumerable<NoteWithDuration> IMelodyPart.GetNotes()
        {
            yield return this;
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

        //public NoteList(object[] objs)
        //{
        //    notes = new List<Note>();
        //    foreach (object obj in objs)
        //    {
        //        if (obj is int) notes.Add(new Note((int)obj));
        //        else if (obj is Note) notes.Add(obj as Note);
        //        else Debug.Fail("unrecognized");
        //    }
        //}

        public IEnumerable<Note> Notes() => notes;
    }
}
