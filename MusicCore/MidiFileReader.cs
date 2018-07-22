using System.Collections.Generic;
using NAudio.Midi;

namespace MusicCore
{
    public static class MidiFileReader
    {
        public static List<MelodyPartList> Read(string filename)
        {
            MidiFile midi = new MidiFile(filename, true);
            List<MelodyPartList> ret = new List<MelodyPartList>();

            for (int track = 0; track < midi.Tracks; track++)
            {
                MelodyPartList list = new MelodyPartList();
                foreach (MidiEvent midievent in midi.Events[track])
                {
                    if (midievent is NoteOnEvent noteonevent)
                    {
                        if (noteonevent.OffEvent != null)
                        {
                            if (noteonevent.DeltaTime > 0)
                                list.Add(new NoteWithDuration(new Fraction(noteonevent.DeltaTime, 120)));

                            list.Add(
                                new NoteWithDuration(
                                    noteonevent.NoteNumber,
                                    Alteration.Natural,
                                    new Fraction(noteonevent.NoteLength, 120)));

                        }
                    }
                    else
                    {

                    }
                }

                if (list.Count > 0)
                    ret.Add(list);
            }

            return ret;
        }
    }
}