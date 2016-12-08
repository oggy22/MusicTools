using System;
using MusicCore;
using NAudio.Midi;
using System.Threading;

namespace MusicComposer
{
    class Program
    {
        static MidiOut midiOut = new MidiOut(0);

        static void Play(TwelveToneSet chord, int startFrom)
        {
            for (int i=startFrom; i< TwelveToneSet.TWELVE + startFrom; i++)
            {
                if (chord[(i+10*TwelveToneSet.TWELVE) % TwelveToneSet.TWELVE])
                {
                    midiOut.Send(MidiMessage.StartNote(48 + i, 100, 1).RawData);
                    Thread.Sleep(100);
                }
            }
        }

        static void Main2(string[] args)
        {
            Random rand = new Random();
            TwelveToneSet chord = TwelveToneSet.major7;
            TwelveToneSet scale = new TwelveToneSet(TwelveToneSet.majorScale);
            int dist = 0;
            Console.WriteLine($"Chord: {chord.ToString()}");
            int startFrom = chord.First;
            do
            {
                Play(chord, startFrom);
                Thread.Sleep(2000);
                TwelveToneSet chordNew = new TwelveToneSet(rand, 4, scale.ShiftRight(rand.Next(TwelveToneSet.TWELVE)));
                dist = Distance(chord, chordNew);
                for (int i=0; i<10; i++)
                {
                    TwelveToneSet chord2 = new TwelveToneSet(rand, 4, scale.ShiftRight(rand.Next(TwelveToneSet.TWELVE)));
                    if (chord.Equals(chord2))
                        continue;

                    int dist2 = Distance(chord, chord2);
                    if (dist2 < dist)
                    {
                        dist = dist2;
                        chordNew = chord2;
                    }
                }
                Console.Write($"Distance: {dist}={chord.DistanceScales(chordNew)}+{chord.DistanceCommonTones(chordNew)}+{chordNew.InScale()}  ");
                Console.WriteLine($"Chord: { chordNew.ToString()}");

                chord = chordNew;
            } while (true);
        }

        static void Main3(string[] args)
        {
            RhythmPatternBase pattern = new RhytmPatternComposite("AAAA", RhythmMaker.CreateRhytmPattern());

            int tempo = 60;

            foreach (var fract in pattern.Notes())
            {
                midiOut.Send(MidiMessage.StartNote(48, 100, 1).RawData);
                Console.Write(fract + " ");
                Thread.Sleep(60 * 1000 * fract.p / fract.q / tempo);
            }
        }

        static void Main(string[] args)
        {
            Melody12Tone m12tone = Compositions.WeWishYouAMerryChristmas();
            int tempo = 140;
            int lastnote = 0;
            foreach (var nwd in m12tone.Notes())
            {
                midiOut.Send(MidiMessage.StopNote(lastnote, 100, 1).RawData);

                if (!nwd.IsPause)
                    midiOut.Send(MidiMessage.StartNote(lastnote = nwd.note, 100, 1).RawData);
                Fraction fract = nwd.duration;
                Console.Write(fract + " ");
                Thread.Sleep(60 * 1000 * fract.p / fract.q / tempo);
            }
        }

        static int Distance(TwelveToneSet chord1, TwelveToneSet chord2)
        {
            return chord1.TotalDistance(chord2) + chord2.InScale();
        }
    }
}