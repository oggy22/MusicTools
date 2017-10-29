using MusicCore;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MusicComposer
{
    partial class Program
    {
        /// <summary>
        /// Generated a random sequence of tones
        /// </summary>
        /// <param name="alightWithLast">The number of previous tones, the next should be aligned with in a chord</param>
        /// <param name="diffFromLast">The number of the previous tones it sohuld be different from</param>
        /// <param name="apartFromLast">Maximal distance from the last tone in semitones</param>
        static void RandomSequencer(int alightWithLast = 5, int diffFromLast = 1, int apartFromLast = 6)
        {
            Random rand = new Random();
            List<int> list = new List<int>();
            int lastTone = 0;
            while (true)
            {
                int tone;
                while (true)
                {
                    // todo: Normal distribution would be more appropriate here
                    tone = rand.Next(40, 80);

                    // Apart from the last tone?
                    if (list.Count > 0 && Math.Abs(list.Last() - tone) > apartFromLast)
                        continue;

                    // Different from last few?
                    bool diff = true;
                    for (int i = list.Count - 1; i >= 0 && i >= list.Count - diffFromLast; i--)
                        if (list[i] == tone)
                        {
                            diff = false;
                            break;
                        }
                    if (!diff)
                        continue;

                    // Last notes contained by a major scale
                    TwelveToneSet set = new TwelveToneSet();
                    set.Add(tone % 12);
                    for (int i = list.Count - 1; i >= list.Count - alightWithLast && i >= 0; i--)
                        set.Add(list[i] % 12);
                    if (!CoveredByChordOfInterefest(set))
                        continue;

                    break;
                }

                midiOut.Send(MidiMessage.StartNote(tone, 100, 1).RawData);
                Console.Write(new tone12(tone).ToString() + " ");
                list.Add(tone);
                midiOut.Send(MidiMessage.StartNote(lastTone, 0, 1).RawData);
                lastTone = tone;
                Thread.Sleep(200);
            }
        }

        static bool CoveredByChordOfInterefest(TwelveToneSet set)
        {
            if (set.CoveredByAnySimilar(TwelveToneSet.major7))
                return true;
            if (set.CoveredByAnySimilar(TwelveToneSet.minor7))
                return true;
            if (set.CoveredByAnySimilar(TwelveToneSet.augmented))
                return true;
            if (set.CoveredByAnySimilar(TwelveToneSet.halfDiminished))
                return true;
            if (set.CoveredByAnySimilar(TwelveToneSet.fullDiminished))
                return true;

            return false;
        }
    }
}
