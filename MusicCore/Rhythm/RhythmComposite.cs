using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MusicCore
{
    public class RhytmPatternComposite : RhythmPatternBase
    {
        public static readonly string[] sts = { "AB", "AA", "AABB", "AABA", "AAAB" };

        RhythmPatternBase[] patterns;

        public RhytmPatternComposite(string st, params RhythmPatternBase[] patterns)
        {
            Debug.Assert(st.Length >= 2, "At least two-fold");
            char min = st.Min();
            this.patterns = new RhythmPatternBase[st.Length];

            measureCount = 0;
            for (int i = 0; i < st.Length; i++)
            {
                this.patterns[i] = patterns[st[i] - min];
                measureCount += patterns[st[i] - min].MeasuresCount;
            }
        }

        private readonly int measureCount;

        public override int MeasuresCount
        {
            get
            {
                return measureCount;
            }
        }

        public override Fraction StartPause
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int CountPattern(RhythmPatternBase pattern)
        {
            return patterns.Sum(p => p.CountPattern(pattern));
        }

        public override IEnumerable<Fraction> Notes()
        {
            foreach (var pattern in patterns)
            {
                foreach (var fract in pattern.Notes())
                    yield return fract;
            }
        }
    }
}
