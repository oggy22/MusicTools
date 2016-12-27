using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MusicCore
{
    public class MelodyAtomic : MelodyBase
    {
        RhythmPatternBase rhythm;
        object[] notes;
        int bpm;

        public override Fraction StartPause => rhythm.StartPause;

        public IEnumerable<NoteWithDuration> anacrusis = null;

        public override IEnumerable<NoteWithDuration> Anacrusis
        {
            get
            {
                return anacrusis ?? Enumerable.Empty<NoteWithDuration>();
            }
        }

        public override Fraction Duration => new Fraction(rhythm.Length, 1);

        public override IEnumerable<NoteWithDuration> Notes()
        {
            foreach (var nwd in Anacrusis)
                yield return nwd;

            int i = 0;
            foreach (var fract in rhythm.Notes())
            {
                yield return new NoteWithDuration((int)notes[i], fract);
                i++;
            }
        }

        public override IEnumerable<NoteWithDuration> Notes(int[] coeffs)
        {
            Debug.Assert(coeffs.Length == 1);
            int i = 0;
            foreach (var fract in rhythm.Notes())
            {
                yield return new NoteWithDuration((int)notes[i], fract);
                i++;
            }
        }

        public MelodyAtomic(RhythmPatternBase rhythm, object[] notes)
        {
            Debug.Assert(rhythm.CountNotes() == notes.Length);
            this.rhythm = rhythm;
            Debug.Assert(notes.Length == rhythm.CountNotes());
            foreach (var note in notes)
            {
                Debug.Assert(note is int || note is Note);
            }
            this.notes = (object[])notes.Clone();
        }

        public MelodyAtomic(object[] notes, int bpm)
        {
            Debug.Assert(notes.Length % bpm == 0);
            rhythm = new RhythmPattern(bpm, notes.Select(obj => obj != null).ToArray());
            this.notes = notes.Where(obj => obj != null).ToArray();
            this.bpm = bpm;
        }
    }

    public class ParameterizedMelody : MelodyBase
    {
        int[] bases = new int[0];
        List<Tuple<NoteWithDuration, int>> notes = new List<Tuple<NoteWithDuration, int>>();

        public override IEnumerable<NoteWithDuration> Anacrusis => Enumerable.Empty<NoteWithDuration>();

        /// <summary>
        /// E.g. "0' 0 1 2 3 4"
        /// </summary>
        public ParameterizedMelody(string st, RhythmPattern rp, params int[] bases)
        {
            int maxnumber = 0;
            var @enum = rp.Notes().GetEnumerator();
            foreach (string stNote in st.Split(' '))
            {
                Debug.Assert(stNote.Length > 0);

                // Based on the nubmer of "'" it pertains to appropriate base
                string stNoteP = stNote.TrimEnd('\'');
                int number = stNote.Length - stNoteP.Length;
                if (number > maxnumber)
                    maxnumber = number;

                // The note is written as a number
                int note = int.Parse(stNoteP);
                Debug.Assert(@enum.MoveNext());

                // Add to the note list
                notes.Add(new Tuple<NoteWithDuration, int>(new NoteWithDuration(note, @enum.Current), number));
            }
            Debug.Assert(!@enum.MoveNext());
        }

        public override Fraction Duration
        {
            get
            {
                Fraction sum = new Fraction(0, 1);
                foreach (var note in notes)
                    sum += note.Item1.duration;

                return sum;
            }
        }

        public override Fraction StartPause
        {
            get
            {
                return new Fraction(0, 1);
            }
        }

        public override IEnumerable<NoteWithDuration> Notes(int[] coeffs)
        {
            foreach (var pair in notes)
            {
                yield return new NoteWithDuration(
                    pair.Item1.note + coeffs[pair.Item2],
                    pair.Item1.alter,
                    pair.Item1.duration);
            }
        }

        public override IEnumerable<NoteWithDuration> Notes()
        {
            throw new NotImplementedException();
        }
    }
}
