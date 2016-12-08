using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MusicCore
{
    public class MelodyDiffEnd : MelodyBase
    {
        private MelodyBase melodyBase;
        private List<NoteWithDuration> end;

        public MelodyDiffEnd(MelodyBase melodyBase, IEnumerable<NoteWithDuration> end)
        {
            this.end = end.ToList();
            this.melodyBase = melodyBase;
        }

        public override IEnumerable<NoteWithDuration> Anacrusis => melodyBase.Anacrusis;

        public override Fraction Duration => melodyBase.Duration;

        public override Fraction StartPause => melodyBase.StartPause;

        public override IEnumerable<NoteWithDuration> Notes()
        {
            Fraction dur = Duration;
            foreach (var nwd in end)
                dur = dur - nwd.duration;

            Debug.Assert(dur.p > 0);
            foreach (var nwd in melodyBase.Notes())
            {
                dur = dur - nwd.duration;
                if (dur.p <= 0)
                {
                    yield return nwd.ChangedDuration(dur + nwd.duration);
                    break;
                }

                yield return nwd;
            }

            foreach (var nwd in end)
                yield return nwd;
        }
    }
}