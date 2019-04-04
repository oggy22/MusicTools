using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace MusicCore
{
    /// <summary>
    /// Represents polyphonic composition. Contains list of voices/melodies.
    /// </summary>
    public class Composition
    {
        public readonly int tempo;
        public readonly int beatCount;

        struct Voice
        {
            public readonly Fraction offset;
            public readonly MelodyPartList mpl;
            //todo: instrument

            public Voice(MelodyPartList mpl, Fraction offset)
            {
                this.mpl = mpl;
                this.offset = offset;
            }
        }

        public int Count => mpls.Count;

        public IEnumerable<MelodyPartList> GetVoices() => mpls.Select(voice => voice.mpl);

        List<Voice> mpls = new List<Voice>();

        public void AddVoice(MelodyPartList mpl)
        {
            AddVoice(mpl, 0);
        }

        public void AddVoice(MelodyPartList mpl, Fraction offset)
        {
            mpls.Add(new Voice(mpl, offset));
        }

        public void PlayBack(MidiOut midiOut)
        {
            PlayBack(
                note => midiOut.Send(MidiMessage.StartNote(note, 100, 1).RawData),
                    note => midiOut.Send(MidiMessage.StopNote(note, 100, 1).RawData));
        }

        public void PlayBack(Action<int> playNote, Action<int> stopNote, bool IncludeDurations = true)
        {
            List<MelodyPartList> mpls = GetVoices().ToList();
            var iterators = mpls.Select(mpl =>
            {
                var enumerator = mpl.GetNotes().GetEnumerator();

                bool NotEnd = enumerator.MoveNext();
                Debug.Assert(NotEnd);

                if (!enumerator.Current.IsPause)
                    playNote(enumerator.Current.note);

                return new Iterator(enumerator, enumerator.Current.Duration);
            }).ToList();

            do
            {
                // Find the shortest note left to play
                Fraction min = new Fraction(int.MaxValue, 1);
                iterators.ForEach(it => min = Fraction.Min(min, it.durLeft));

                Console.WriteLine(min);
                if (IncludeDurations)
                    Thread.Sleep((int)(200 * min));

                // Update durations
                iterators.ForEach(it =>
                {
                    it.durLeft = it.durLeft - min;
                    Debug.Assert(it.durLeft >= 0.0);
                    if (it.durLeft == 0)
                    {
                        if (!it.Current.IsPause)
                            stopNote(it.Current.note);

                        if (it.MoveNext())
                        {
                            it.durLeft = it.Current.Duration;
                            if (!it.Current.IsPause)
                                playNote(it.note);
                        }
                    }
                });

                iterators.RemoveAll(it => it.Finished);
            } while (iterators.Count > 0);
        }

        /// <summary>
        /// Used to iterate over notes in playback
        /// </summary>
        class Iterator
        {
            public Iterator(IEnumerator<NoteWithDuration> enumerator, Fraction durLeft)
            {
                this.enumerator = enumerator;
                this.durLeft = durLeft;
                this.Finished = false;
            }

            public bool MoveNext()
            {
                bool moveNext = enumerator.MoveNext();
                if (!moveNext)
                    Finished = true;
                return moveNext;
            }

            public bool Finished { get; private set; }
            public readonly IEnumerator<NoteWithDuration> enumerator;
            public Fraction durLeft;
            public NoteWithDuration Current => enumerator.Current;
            public int note => Current.note;
        };
    }
}
