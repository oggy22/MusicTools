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
        private int offset;

        internal MelodyPartList node;

        public MelodyPart(int offset, MelodyPartList node)
        {
            this.offset = offset;
            this.node = node;
        }

        public Fraction duration => throw new System.NotImplementedException();

        public IEnumerable<NoteWithDuration> GetNotes()
        {
            foreach (var part in node)
                foreach (var note in part.GetNotes())
                    yield return note + offset;
        }

        public void PlayRecursive(int offset)
        {
            node.PlayRecursive(this.offset + offset);
        }
    }

    public class MelodyPartList : List<IMelodyPart>
    {
        private static int idVoice = 0, idMelody = 0;
        public readonly int id;

        public enum Type { Voice, Melody, Copy }
        public Type type;

        public MelodyPartList(Type type)
        {
            this.type = type;

            switch (type)
            {
                case Type.Melody: id = ++idMelody; return;
                case Type.Voice: id = ++idVoice; return;
                default: Debug.Fail($"Unrecognized {type}"); return;
            }
        }

        /// <summary>
        /// Flat copy constructor
        /// </summary>
        public MelodyPartList(MelodyPartList mpl)
        {
            this.type = Type.Copy;

            foreach (var nwd in mpl)
                Add(nwd);
        }

        public override string ToString()
        {
            string st =  $"{type}/{id}: Notes = {this.GetNotes().Count()}; Total occurances={TotalOccurances}; Voices={TotalVoices}; ";

            foreach (var impl in this)
            {
                if (impl is NoteWithDuration nwd)
                    st += $"{nwd},";
                else if (impl is MelodyPart mp)
                    st += $"{mp.node.type}/{id}{mp.node.id},";
                else
                    Debug.Fail("Unrecognized part");
            }

            return st;
        }

        //todo: add int stepsPerOctave;
        public Fraction duration
        {
            get
            {
                Fraction sum = 0;
                foreach (var el in this)
                    sum += el.duration;

                return sum;
            }
        }

        public bool IsIdentical(MelodyPartList node) => this.SequenceEqual(node);

        public bool IsLeaf => !GetChildren().Any();

        public IEnumerable<MelodyPartList> GetChildren() =>
            this
            .OfType<MelodyPart>()
            .Select(imp => imp.node);

        public int RecursiveCount => GetNotes().Count();

        public IEnumerable<NoteWithDuration> GetNotes()
        {
            foreach (var mp in this)
                foreach (var note in mp.GetNotes())
                    yield return note;
        }

        public int TotalOccurances { get; set; }

        public int TotalVoices { get; set; }

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