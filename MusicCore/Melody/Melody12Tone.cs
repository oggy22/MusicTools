using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
                yield return new NoteWithDuration(
                    notewd.IsPause ? Note.PAUSE : key + toneset.Calculate(notewd.note) + (int)notewd.alter,
                    notewd.duration);
            }
        }
    }

    public class NoteSequence
    {
        private List<Note> notes;
        public NoteSequence(object[] objs)
        {
            notes = objs.Select<object, Note>(obj =>
            {
                if (obj is Note)
                    return (Note)obj;

                if (obj is int)
                    return new Note((int)obj);

                if (obj is string)
                    return new Note((string)obj);

                Debug.Assert(obj == null);
                return new Note(Note.PAUSE);
            }).ToList();
        }

        public Note this[int index]
        {
            get
            {
                return notes[index];
            }
        }

        public static implicit operator NoteSequence(object[] objs)
        {
            return new NoteSequence(objs);
        }
    }

    public class MelodySequencer : MelodyBase
    {
        private List<NoteSequence> sequences;
        private MelodyBase melody;

        public MelodySequencer(MelodyBase melody, params NoteSequence[] sequence)
        {
            sequences = sequence.ToList();
            this.melody = melody;
        }

        public override Fraction StartPause => Fraction.ZERO;

        public override IEnumerable<NoteWithDuration> Anacrusis => Enumerable.Empty<NoteWithDuration>();

        public override Fraction Duration => sequences.Count * melody.Duration;

        public override IEnumerable<NoteWithDuration> Notes()
        {
            foreach (NoteSequence seq in sequences)
            {
                foreach (var note in melody.Notes())
                {
                    int number = seq[note.note].note;
                    Alteration alter = (Alteration)((int)note.alter + (int)seq[note.note].alter);
                    NoteWithDuration noteReturn = new NoteWithDuration(number, alter, note.duration);
                    yield return noteReturn;
                }
            }
        }
    }
}