using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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


        static NoteWithDuration Transform(NoteWithDuration note, Func<NoteWithDuration, NoteWithDuration> trans)
        {
            NoteWithDuration noteReturn = trans(note);
            if (note.otherNote != null)
                noteReturn.otherNote = Transform(note.otherNote as NoteWithDuration, trans);

            return noteReturn;
        }

        public override IEnumerable<NoteWithDuration> Notes()
        {
            foreach (var notewd in melodyBase.Notes())
            {
                yield return Transform(notewd, note =>
                    new NoteWithDuration(
                    note.IsPause ? Note.PAUSE : key + toneset.Calculate(note.note) + (int)note.alter,
                    note.duration));
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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Note note in notes)
            {
                sb.Append($"{note} ");
            }
            return sb.ToString();
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

                    Note currNote = note;

                    NoteWithDuration noteReturn = null, noteTail = null;


                    do
                    {
                        Alteration alter = (Alteration)((int)currNote.alter + (int)seq[currNote.note].alter);
                        if (noteReturn == null)
                            noteReturn = noteTail = new NoteWithDuration(number, alter, note.duration);
                        else
                        {
                            noteTail.otherNote = new NoteWithDuration(number, alter, note.duration);
                            noteTail = noteTail.otherNote as NoteWithDuration;
                        }

                        currNote = currNote.otherNote;
                    } while (currNote != null);

                    yield return noteReturn;
                }
            }
        }
    }
}