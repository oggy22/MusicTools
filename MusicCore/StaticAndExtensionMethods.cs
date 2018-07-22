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
    }
}
