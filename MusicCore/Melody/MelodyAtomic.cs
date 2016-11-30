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

        public override IEnumerable<Tuple<Note, Fraction>> NotesOld()
        {
            if (rhythm == null)
            {
                //TODO
                yield break;
            }

            int i = 0;
            foreach (var fract in rhythm.Notes())
            {
                Note note = notes[i] is Note ? (Note)notes[i] : new Note() { note = (int)(notes[i]) };
                yield return new Tuple<Note, Fraction>((Note)(note), fract);
                i++;
            }
            Debug.Assert(i == notes.Length);
        }

        public override IEnumerable<NoteWithDuration> Notes()
        {
            int i = 0;
            foreach (var fract in rhythm.Notes())
            {
                yield return new NoteWithDuration()
                {
                    note = (int)notes[i],
                    duration = fract
                };
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
}
