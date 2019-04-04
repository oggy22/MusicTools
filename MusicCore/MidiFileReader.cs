using System.Collections.Generic;
using NAudio.Midi;

namespace MusicCore
{
    public static class MidiFileReader
    {
        public static Composition Read(string filename)
        {
            MidiFile midi = new MidiFile(filename, true);
            Composition composition = new Composition();

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
                    composition.AddVoice(list);
            }

            return composition;
        }
    }
}