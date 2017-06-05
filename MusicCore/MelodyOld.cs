using System.Diagnostics;

namespace MusicCore
{
    public class NoteOld
    {
        public short note;
        static NoteOld Pause = new NoteOld { note = short.MinValue };
    }
}