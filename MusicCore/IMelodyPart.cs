using NAudio.Midi;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MusicCore
{
    public interface IMelodyPart
    {
        Fraction duration { get; }
        IEnumerable<NoteWithDuration> GetNotes();
    }

    public class MelodyPart : IMelodyPart
    {
        protected int offset;

        protected MelodyPartList list;

        public MelodyPart(int offset, MelodyPartList list)
        {
            this.offset = offset;
            this.list = list;
        }

        public Fraction duration => throw new System.NotImplementedException();

        public IEnumerable<NoteWithDuration> GetNotes()
        {
            foreach (var part in list)
                foreach (var note in part.GetNotes())
                    yield return note + offset;
        }

        public void PlayRecursive(int offset)
        {
            list.PlayRecursive(this.offset + offset);
        }
    }

    public class MelodyPartList : List<IMelodyPart>
    {
        //todo: add int stepsPerOctave;

        Fraction duration
        {
            get
            {
                Fraction sum = 0;
                foreach (var el in this)
                    sum += el.duration;
                return sum;
            }
        }

        public int RecursiveCount => GetNotes().Count();

        public IEnumerable<NoteWithDuration> GetNotes()
        {
            foreach (var mp in this)
                foreach (var note in mp.GetNotes())
                    yield return note;
        }

        public void Play(int offset = 0)
        {
            int? previousNote = null;

            foreach (NoteWithDuration note in GetNotes())
            {
                if (previousNote.HasValue)
                    StaticAndExtensionMethods.midiOut.Send(MidiMessage.StopNote(previousNote.Value + offset, 100, 1).RawData);

                previousNote = note.note;
                NoteWithDuration noteToPlay = note + offset;
                Debug.Assert(noteToPlay.IsPause || noteToPlay.note > 10 && noteToPlay.note < 100);
                StaticAndExtensionMethods.midiOut.Send(MidiMessage.StartNote(noteToPlay.note, 100, 1).RawData);
                var fract = note.Duration;
                Thread.Sleep(15 * 1000 * fract.p / fract.q / 60);
            }
        }

        internal void PlayRecursive(int offset = 0)
        {
            foreach (IMelodyPart melodyPart in this)
            {
                if (melodyPart is NoteWithDuration note)
                {
                    var noteToPlay = note + offset;
                    if (!noteToPlay.IsPause)
                        StaticAndExtensionMethods.midiOut.Send(MidiMessage.StartNote(noteToPlay.note, 100, 1).RawData);
                    var fract = note.Duration;
                    Thread.Sleep(15 * 1000 * fract.p / fract.q / 60);
                    if (!noteToPlay.IsPause)
                        StaticAndExtensionMethods.midiOut.Send(MidiMessage.StopNote(noteToPlay.note, 100, 1).RawData);
                }
                else if (melodyPart is MelodyPart part)
                {
                    part.PlayRecursive(offset);
                }
                else Debug.Fail($"unrecognized {melodyPart}");
            }
        }
    }
}