using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MusicCore
{
    public class MelodyReversed : MelodyBase
    {
        private MelodyBase melodyBase;

        public MelodyReversed(MelodyBase melodyBase)
        {
            Debug.Assert(!melodyBase.Anacrusis.Any());
            Debug.Assert(melodyBase.StartPause.p == 0);
            this.melodyBase = melodyBase;
        }

        public override IEnumerable<NoteWithDuration> Anacrusis => Enumerable.Empty<NoteWithDuration>();

        public override Fraction Duration => melodyBase.Duration;

        public override Fraction StartPause => Fraction.ZERO;

        public override IEnumerable<NoteWithDuration> Notes()
        {
            return melodyBase.Notes().Reverse();
        }
        public override IEnumerable<NoteWithDuration> Notes(int[] coeffs)
        {
            return melodyBase.Notes().Reverse();
        }
    }
}
