using NAudio.Midi;
using System;

namespace MusicCore
{
    public static class MidiFileReader
    {
        private static void AddNote(MelodyPartList list, NoteWithDuration nwd, Fraction? prec = null, long absoluteTime = 0)
        {
            if (prec != null)
            {
                Fraction times = (nwd.Duration / prec.Value);
                if (!times.IsWhole())
                    throw new Exception($"Duration of Note {nwd} not divisible by {prec.Value} at absoluteTime={absoluteTime}");
            }

            list.Add(nwd);
        }

        public static Composition Read(string filename, Fraction? prec = null)
        {
            MidiFile midi = new MidiFile(filename, true);
            Composition composition = new Composition();
            int quarterNoteInTicks = midi.DeltaTicksPerQuarterNote;

            for (int track = 0; track < midi.Tracks; track++)
            {
                MelodyPartList list = new MelodyPartList(MelodyPartList.Type.Voice);
                foreach (MidiEvent midievent in midi.Events[track])
                {
                    if (midievent is NoteOnEvent noteonevent)
                    {
                        if (noteonevent.OffEvent != null)
                        {
                            if (noteonevent.DeltaTime > 0)
                                AddNote(list, new NoteWithDuration(new Fraction(noteonevent.DeltaTime, quarterNoteInTicks)), prec, noteonevent.AbsoluteTime);

                            AddNote(
                                list,
                                new NoteWithDuration(
                                    noteonevent.NoteNumber,
                                    Alteration.Natural,
                                    new Fraction(noteonevent.NoteLength, quarterNoteInTicks)),
                                prec,
                                noteonevent.AbsoluteTime);
                        }
                    }
                    else
                    {

                    }
                }

                if (list.Count > 0)
                    composition.AddVoice(list);
            }

            return composition;
        }
    }
}