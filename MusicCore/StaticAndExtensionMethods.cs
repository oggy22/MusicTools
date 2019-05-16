using System.Linq;
using NAudio.Midi;
using System.Collections.Generic;

namespace MusicCore
{
    public enum SortBy
    {
        None = 0,
        Duration,
        Count,
        Repetitions
    }

    public static class StaticAndExtensionMethods
    {
        public static MidiOut midiOut = new MidiOut(0);

        static void Query(this List<MelodyPartList> composition,
            int minCount=0, int maxCount=int.MaxValue,
            int minDuration=0, int maxDuration=int.MaxValue,
            bool leafsOnly = false,
            SortBy sortBy = SortBy.None)
        {
            //todo: perform query
        }

        public static int NoteTo12Tone(int x)
        {
            if (x > 0)
                return x % 12;

            return 0;
        }

        public static List<NoteWithDuration> ChromaticToDiatonic(List<int> notes)
        {
            notes.Distinct().ToList().Sort();

            //HashSet<int> hash;

            return null;
        }

        public static List<int> DiatonicToChromatic(List<NoteWithDuration> notes)
        {
            //HashSet<int> hash;

            return null;
        }
    }
}
