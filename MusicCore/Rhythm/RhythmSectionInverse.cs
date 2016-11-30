using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicCore
{
    public class RhythmSectionInverse : RhythmPatternBase
    {
        RhythmPatternBase basePattern;

        public RhythmSectionInverse(RhythmPatternBase pattern)
        {
            basePattern = pattern;
        }

        public override int MeasuresCount
        {
            get
            {
                throw new NotImplementedException();
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
            return basePattern.CountPattern(pattern);
        }

        public override IEnumerable<Fraction> Notes()
        {
            return basePattern.Notes().Reverse();
        }
    }
}
