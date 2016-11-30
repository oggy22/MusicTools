using System.Diagnostics;

namespace MusicCore
{
    public class NoteOld
    {
        public short note;
        static NoteOld Pause = new NoteOld { note = short.MinValue };
    }

    public class MelodyOld
    {
        public int Measures
        {
            get
            {
                return notes.Length / (BeatsPerMeasure * BeatsPerUnit);
            }
        }

        NoteOld[] notes;
        public readonly int BeatsPerMeasure, BeatsPerUnit;

        public MelodyOld(int bpm, int bpu, string st)
        {
            Debug.Assert(bpm >= 2 && bpm <= 4);
            Debug.Assert(bpu == 1 || bpu == 2 || bpu == 4);
            BeatsPerMeasure = bpm;
            BeatsPerUnit = bpu;
        }

        public void Inverse()
        {
            Debug.Assert(notes[0] != null);
            NoteOld[] notesNew = new NoteOld[notes.Length];
            for (int i=notes.Length-1, j=0; i>= 0; i--, j++)
            {
                int oldJ = j;
                while (notes[i]==null)
                {
                    i--; j++;
                }
                notesNew[oldJ] = notes[i];
            }
            notes = notesNew;
            Debug.Assert(notes[0] != null);
        }
    }
}