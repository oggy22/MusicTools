using System;
using MusicCore;
using NAudio.Midi;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using static MusicCore.StaticAndExtensionMethods;

namespace MusicComposer
{
    partial class Program
    {
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
            if (args.Length < 1)
            {
                Console.WriteLine("Playing all songs:");
                foreach (MethodInfo mi in typeof(Compositions).GetMethods())
                {
                    if (mi.GetParameters().Length == 0 && mi.ReturnType == typeof(Melody12Tone))
                    {
                        Console.Write(mi.Name + " ");
                        Main(new[] { mi.Name });
                        Console.WriteLine();
                        Console.WriteLine("Wait for 1 second...");
                        Thread.Sleep(1000);
                    }
                }
                Console.WriteLine();
                return;
            }

            if (args.Length == 2 && args[0] == "midi")
            {
                var list = MidiFileReader.Read(args[1]);
                int previousNote = 0;
                foreach (NoteWithDuration note in list[0].GetNotes())
                {
                    if (previousNote != 0)
                        midiOut.Send(MidiMessage.StopNote(previousNote, 100, 1).RawData);

                    previousNote = note.note;
                    midiOut.Send(MidiMessage.StartNote(note.note, 100, 1).RawData);
                    var fract = note.Duration;
                    Thread.Sleep(15 * 1000 * fract.p / fract.q / 60);
                }

                return;
            }

            if (args[0] == "RandomSequencer")
            {
                RandomSequencer();
                return;
            }

            MethodInfo miStatic = typeof(Compositions).GetMethod(args[0]);
            Func<Melody12Tone> dgComposition = Delegate.CreateDelegate(typeof(Func<Melody12Tone>), miStatic) as Func<Melody12Tone>;
            Melody12Tone m12tone = dgComposition();

            List<int> lastnotes = new List<int>();
            foreach (var nwd in m12tone.Notes())
            {
                foreach (int note in lastnotes)
                    midiOut.Send(MidiMessage.StopNote(note, 100, 1).RawData);

                // Play the note, and if it's pause mute the last note
                if (!nwd.IsPause)
                {
                    Note note = nwd;
                    lastnotes = new List<int>();
                    do
                    {
                        midiOut.Send(MidiMessage.StartNote(note.note, 100, 1).RawData);
                        lastnotes.Add(note.note);
                        //note = note.otherNote;
                    } while (false /*note != null*/);
                }
                else
                    foreach (var note in lastnotes)
                        midiOut.Send(MidiMessage.StartNote(note, 0, 1).RawData);

                Fraction fract = nwd.Duration;
                Console.Write(fract + " ");
                Thread.Sleep(60 * 1000 * fract.p / fract.q / m12tone.tempo);
            }

            // Mute the last note(s)
            foreach (var note in lastnotes)
                midiOut.Send(MidiMessage.StartNote(note, 0, 1).RawData);
        }

        static int Distance(TwelveToneSet chord1, TwelveToneSet chord2)
        {
            return chord1.TotalDistance(chord2) + chord2.InScale();
        }
    }
}