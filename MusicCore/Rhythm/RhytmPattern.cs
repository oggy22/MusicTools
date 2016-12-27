using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MusicCore
{
    public abstract class RhythmPatternBase
    {
        public virtual int BeatsPerMeasure { get; }

        public abstract int MeasuresCount { get; }

        public int Length => BeatsPerMeasure * MeasuresCount;

        public abstract IEnumerable<Fraction> Notes();

        public int CountNotes()
        {
            return Notes().Count();
        }

        public abstract Fraction StartPause { get; }

        public abstract int CountPattern(RhythmPatternBase pattern);
    }

    public class RhythmPattern : RhythmPatternBase
    {
        static readonly Dictionary<char, Fraction> dict = new Dictionary<char, Fraction>
        {
            {'1', new Fraction(1, 1) },
            {'2', new Fraction(2, 1) },
            {'3', new Fraction(3, 1) },
            {'4', new Fraction(4, 1) },
            {'h', new Fraction(1, 2) },
            {'q', new Fraction(1, 4) },
        };

        private bool[] b;
        public readonly int beatsPerMeasure, BeatsPerUnit;

        public override int BeatsPerMeasure
        {
            get
            {
                return beatsPerMeasure;
            }
        }

        public override int MeasuresCount
        {
            get
            {
                return b.Length / (BeatsPerUnit * BeatsPerMeasure);
            }
        }

        public override Fraction StartPause
        {
            get
            {
                for (int i = 0; i < b.Length; i++)
                    if (b[i])
                        return new Fraction(i, BeatsPerUnit);
                throw new ArgumentException("Empty seciton");
            }
        }

        public RhythmPattern(int bpm, int bpu, Random rand)
        {
            b = new bool[bpm * bpu];
            BeatsPerUnit = bpu;
            beatsPerMeasure = bpm;
            b[0] = true;
            int count = rand.Next(bpm + bpu); // This is very arbitrary number
            while (count-- >= 0)
            {
                int pos;
                do
                {
                    pos = rand.Next(bpm * bpu);

                    // Make the first beat very probable
                    if (pos == 0)
                        break;

                    // Make on beat more probable
                    if (pos % bpu == 0)
                    {
                        if (rand.Next(bpu) == 0)
                            break;
                        continue;
                    }

                    // Off-beat are rear
                    if (rand.Next(bpu + bpm) == 0)
                        break;

                } while (true);

                b[pos] = true;
            }
        }

        public RhythmPattern(int bpm, int bpu, string st="")
        {
            Debug.Assert(bpm >= 2 && bpm <= 4);
            Debug.Assert(bpu == 1 || bpu == 2 || bpu == 4);
            beatsPerMeasure = bpm;
            BeatsPerUnit = bpu;

            int i = 0;
            bool[] b_temp = new bool[4 * st.Length * bpu];
            int add = 0;
            foreach (char c in st)
            {
                if (c == ' ')
                    continue;

                if (c == '.')
                {
                    Debug.Assert(add != 0);
                    Debug.Assert(add % 2 == 0);
                    add += add / 2;
                    continue;
                }
                i += add;
                b_temp[i] = true;
                add = (int)(dict[c] * bpu);
            }
            i += add;

            b = new bool[i];
            for (int j = 0; j < i; j++)
                b[j] = b_temp[j];

            Debug.Assert(i % (BeatsPerMeasure * BeatsPerUnit) == 0);
        }

        public RhythmPattern(int bpm, bool[] b)
        {
            Debug.Assert(b.Length % bpm == 0);
            beatsPerMeasure = bpm;
            BeatsPerUnit = b.Length / bpm;
            this.b = b;
        }

        public void Reverse()
        {
            RhythmPattern rp = new RhythmPattern(BeatsPerMeasure, BeatsPerUnit);
            //todo
        }

        int GCD(int a, int b)
        {
            return b == 0 ? a : GCD(b, a % b);
        }

        public void Simplify()
        {
            int number = b.Length;
            int gcd = 0;
            for (int i = 1; i < b.Length; i++)
            {
                if (b[i])
                {
                    gcd = GCD(gcd, i);
                    if (gcd == 1)
                        return;
                }
            }
        }

        public override IEnumerable<Fraction> Notes()
        {
            for (int i = 0; i<b.Length; i++)
            {
                if (b[i])
                {
                    int j;
                    for (j = i + 1; j < b.Length && !b[j]; j++) ;
                    yield return new Fraction(j - i, BeatsPerUnit);
                }
            }
        }

        public override int CountPattern(RhythmPatternBase pattern)
        {
            return pattern == this ? 1 : 0;
        }
    }

    public class RhythmPatternModEnd : RhythmPatternBase
    {
        public readonly RhythmPatternBase pattern, patternEnd;

        public RhythmPatternModEnd(RhythmPatternBase pattern, RhythmPatternBase patternEnd)
        {
            Debug.Assert(pattern.BeatsPerMeasure == patternEnd.BeatsPerMeasure);
            Debug.Assert(patternEnd.MeasuresCount < pattern.MeasuresCount);
            this.pattern = pattern;
            this.patternEnd = patternEnd;
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
            throw new NotImplementedException();
        }

        public override IEnumerable<Fraction> Notes()
        {
            Fraction pos = new Fraction(0, 0);
            int lenght = pattern.Length - patternEnd.Length;
            foreach (var fract in pattern.Notes())
            {
                pos = pos + fract;

                if (((pos - lenght).p <= 0))
                {
                    yield return fract;
                }
                else
                {
                    foreach (var f in patternEnd.Notes())
                        yield return f;

                    yield break;
                }
            }
        }
    }
}