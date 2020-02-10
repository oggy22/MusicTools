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

    public class MelodyPartDtoC : MelodyPart
    {
        private TwelveToneSet scale;

        public MelodyPartDtoC(int offset, TwelveToneSet scale, MelodyPartList node) //todo: scale should be 3rd arg
            : base(offset, node)
        {
            Debug.Assert(node.IsDiatonic);
            this.scale = scale;
        }

        public override IEnumerable<NoteWithDuration> GetNotes()
        {
            foreach (var part in node.children)
                foreach (var note in part.GetNotes())
                    yield return note.IsPause ?
                        new NoteWithDuration(note.Duration) :
                        new NoteWithDuration(
                        scale.DiatonicToChromatic(note.note) + offset,
                        note.Duration
                        );
        }
    }

    /// <summary>
    /// Pointer to MelodyPartList
    /// </summary>
    public class MelodyPart : IMelodyPart
    {
        protected int offset;

        internal MelodyPartList node;

        public MelodyPart(int offset, MelodyPartList node)
        {
            this.offset = offset;
            this.node = node;
        }

        public Fraction duration
        {
            get
            {
                Fraction sum = 0;
                foreach (var part in node.children)
                    sum += part.duration;

                return sum;
            }
        }

        public virtual IEnumerable<NoteWithDuration> GetNotes()
        {
            foreach (var part in node.children)
                foreach (var note in part.GetNotes())
                    yield return note + offset;
        }

        public void PlayRecursive(int offset)
        {
            node.PlayRecursive(this.offset + offset);
        }
    }

    public class MelodyPartList
    {
        public bool Changed { get; private set; }
        public bool IsDiatonic;
        internal List<IMelodyPart> children = new List<IMelodyPart>();
        public int Count => children.Count;
        public IMelodyPart this[int index] => children[index];
        private static int idVoice = 0, idMelody = 0;
        public readonly int id;
        public void Add(IMelodyPart imp) => children.Add(imp);
        public void RemoveRange(int i, int count) => children.RemoveRange(i, count);
        public void Insert(int i, IMelodyPart imp) => children.Insert(i, imp);

        public enum Type { Voice, Melody, Copy }
        public Type type;

        public MelodyPartList(Type type, bool isDiatonic = false)
        {
            this.type = type;
            this.IsDiatonic = isDiatonic;

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

            foreach (var nwd in mpl.children)
                children.Add(nwd);
        }

        public override string ToString()
        {
            string dorc = IsDiatonic ? "diat" : "chrom";
            string st =  $"{type}/{id}/{dorc}: Notes = {this.GetNotes().Count()}; Total occurances={TotalOccurances}; Voices={TotalVoices}; ";
            
            foreach (var impl in this.children)
            {
                if (impl is NoteWithDuration nwd)
                    st += $"{nwd},";
                else if (impl is MelodyPart mp)
                    st += $"{mp.node.type}/{mp.node.id},";
                else
                    Debug.Fail("Unrecognized part");
            }

            if (Changed)
                st += " (CHANGED)";

            return st;
        }

        //todo: add int stepsPerOctave;
        Fraction duration
        {
            get
            {
                Fraction sum = 0;
                foreach (var el in this.children)
                    sum += el.duration;

                return sum;
            }
        }

        public bool IsIdentical(MelodyPartList node) => this.children.SequenceEqual(node.children) && this.IsDiatonic == node.IsDiatonic;

        public bool IsLeaf => !GetChildren().Any();

        public IEnumerable<MelodyPartList> GetChildren() =>
            this.children
            .OfType<MelodyPart>()
            .Select(imp => imp.node);

        public int RecursiveCount => GetNotes().Count();

        public IEnumerable<NoteWithDuration> GetNotes()
        {
            foreach (var mp in this.children)
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

                previousNote = note.IsPause ? null : (int?)(note.note);
                NoteWithDuration noteToPlay = note + offset;
                Debug.Assert(noteToPlay.IsPause || noteToPlay.note > 10 && noteToPlay.note < 100);
                if (!noteToPlay.IsPause)
                    StaticAndExtensionMethods.midiOut.Send(MidiMessage.StartNote(noteToPlay.note, 100, 1).RawData);
                var fract = note.Duration;
                Thread.Sleep(15 * 1000 * fract.p / fract.q / 60);
            }
        }

        internal void PlayRecursive(int offset = 0)
        {
            foreach (IMelodyPart melodyPart in this.children)
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

        public void ReshuffleNoteBlocks()
        {
            const int outside = -1;
            int start = outside;
            int i;
            for (i=0; i<children.Count; i++)
            {
                if (children[i] is NoteWithDuration)
                {
                    if (start == outside)
                        start = i;
                }
                else
                {
                    if (start != outside)
                        ReshuffleRegion(start, i - start);
                }
            }
            if (start != outside)
                ReshuffleRegion(start, i - start);

            Changed = true;
        }

        private void ReshuffleRegion(int first, int length)
        {
            if (length < 2)
                return;

            var random = new System.Random(0);
            for (int i = 0; i<length; i++)
            {
                int j = random.Next(i, first + length);
                if (i == j)
                    continue;

                var temp = children[i];
                children[i] = children[j];
                children[j] = temp;
            }
        }
    }

    class Permutation
    {

    }
}