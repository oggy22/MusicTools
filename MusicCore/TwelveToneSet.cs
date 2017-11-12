using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MusicCore
{
    public enum MusicalModes
    {
        Chromatic = 0,
        Ionian = 1, Major = Ionian,
        Dorian = 2,
        Phrygian = 3,
        Lydian = 4,
        Mixolydian = 5,
        Aoelian = 6, Minor = Aoelian,
        Locrian = 7,
        MinorHarmonic,
        MinorMelodic
    }

    public class ToneSet : IEnumerable<Tone>
    {
        public ToneSet()
        {
            tones = new bool[128];
        }

        public ToneSet(params int[] array) : this()
        {
            foreach (var num in array)
                tones[num] = true;
        }

        public ToneSet(Tone tone, TwelveToneSet twelveToneSet) : this()
        {
            foreach (var t in twelveToneSet)
                tones[tone + t] = true;
        }

        bool[] tones;

        public Tone GetLowestCommonHarmonic()
        {
            List<IEnumerator<Tone>> iters = new List<IEnumerator<Tone>>();

            foreach (var iter in this)
            {
                iters.Add(iter.GetHarmonicsUp().GetEnumerator());
                iters.Last().MoveNext();
            }

            int min;
            while ((min = iters.Min(it => (int)it.Current)) < iters.Max(it => (int)it.Current))
            {
                var iter = iters.Find(it => it.Current == min);
                bool moved = iter.MoveNext();
                Debug.Assert(moved);
            }

            return iters[0].Current;
        }

        public Tone GetHighestCommonSubHarmonic()
        {
            List<IEnumerator<Tone>> iters = new List<IEnumerator<Tone>>();

            foreach (var tone in this)
            {
                iters.Add(tone.GetHarmonicsDown().GetEnumerator());
                iters.Last().MoveNext();
            }

            int max;
            while (iters.Min(it => (int)it.Current) < (max=iters.Max(it => (int)it.Current)))
            {
                var iter = iters.Find(it => it.Current == max);
                bool moved = iter.MoveNext();
                Debug.Assert(moved);
            }

            return iters[0].Current;
        }

        public Tone GetDisharmony(int A = 1, int B = 2)
        {
            return A * GetLowestCommonHarmonic() - B * GetHighestCommonSubHarmonic();
        }

        public TwelveToneSet ToTwelveToneSet()
        {
            TwelveToneSet set = new TwelveToneSet();
            foreach (var tone in this)
                set.Add(tone % 12);

            return set;
        }

        public IEnumerator<Tone> GetEnumerator()
        {
            return new Iterator(this);
        }

        class Iterator : IEnumerator<Tone>
        {
            public Iterator(ToneSet toneset)
            {
                this.toneset = toneset;
                Reset();
            }

            int index;
            ToneSet toneset;

            public Tone Current => new Tone(index);

            object IEnumerator.Current => new Tone(index);

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                do
                {
                    index++;
                    if (index >= toneset.tones.Length)
                        return false;
                }
                while (!toneset.tones[index]);
                return true;
            }

            public void Reset()
            {
                index = -1;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Iterator(this);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i<tones.Length; i++)
                if (tones[i])
                    sb.Append($"{new Tone(i).ToString()} ");

            return sb.ToString();
        }
    }

    public class TwelveToneSet : IEnumerable<tone12>
    {
        #region Constants
        public const int TWELVE = 12;

        private static readonly char SHARP = '#';
        private static readonly char FLAT = '♭';

        static readonly Dictionary<char, int> KEYS = new Dictionary<char, int>
            {
                {'C', 0}, {'D', 2}, {'E', 4}, {'F', 5}, {'G', 7}, {'A', 9}, {'B', 11}, {'H', 11}
            };

        static public TwelveToneSet majorScale = new TwelveToneSet("CDEFGAH");
        static public TwelveToneSet minorScale = new TwelveToneSet("CDE♭FGA♭B♭");
        static public TwelveToneSet minorHarmonicScale = new TwelveToneSet("CDE♭FGA♭B");
        static public TwelveToneSet minorMelodicScale = new TwelveToneSet("CDE♭FGAB");

        static public TwelveToneSet majorTriad = new TwelveToneSet("CEG");
        static public TwelveToneSet minorTriad = new TwelveToneSet("CE♭G");
        static public TwelveToneSet major7 = new TwelveToneSet("CEGB♭");
        static public TwelveToneSet minor7 = new TwelveToneSet("CE♭GB♭");
        static public TwelveToneSet augmented = new TwelveToneSet("CEG#");
        static public TwelveToneSet halfDiminished = new TwelveToneSet("CDFG#");
        static public TwelveToneSet fullDiminished = new TwelveToneSet("CE♭F#A");

        static public TwelveToneSet chromatic = new TwelveToneSet(MusicalModes.Chromatic);
        #endregion

        private bool[] tones;

        #region Properties
        public bool IsRooted { get { return this[0]; } }

        public int Count { get { return tones.Count(f => f); } }

        public bool IsConst
        {
            get; private set;
        }

        public bool this[int pos]
        {
            get
            {
                return tones[pos];
            }
            set
            {
                Debug.Assert(!IsConst);
                tones[pos] = value;
            }
        }

        public tone12 First
        {
            get
            {
                for (int i = 0; i < TWELVE; i++)
                    if (this[i])
                        return i;

                throw new Exception("Empty!");
            }
        }

        public tone12 Last
        {
            get
            {
                for (int i = TWELVE - 1; i >= 0; i--)
                    if (this[i])
                        return i;

                throw new Exception("Empty!");
            }
        }
        #endregion

        #region Constructors
        public TwelveToneSet(bool[] tones)
        {
            if (tones.Length > TWELVE)
                Debug.Fail($"Parameter tones.Length = {tones.Length} must not be larger than {TWELVE}");

            tones = new bool[TWELVE];
            for (int i = 0; i < tones.Length; i++)
                this[i] = tones[i];
        }

        public TwelveToneSet(MusicalModes mode) : this(majorScale)
        {
            if (mode == MusicalModes.Chromatic)
            {
                for (int i = 0; i < TWELVE; i++)
                    this[i] = true;
                return;
            }

            if (mode >= MusicalModes.Dorian && mode <= MusicalModes.Locrian)
            {
                int shift = FindNth((int)mode - (int)MusicalModes.Ionian);
                this.ShiftLeft(shift);
                return;
            }

            MusicalModes modeShift = mode;
            if (mode == MusicalModes.MinorHarmonic || mode == MusicalModes.MinorMelodic)
                modeShift = MusicalModes.Minor;

            int tone = FindNth((int)(modeShift) - 1);
            ShiftLeft(tone);

            if (mode == MusicalModes.MinorMelodic)
            {
                MakeSharp(5);
                MakeSharp(6);
            }

            if (mode == MusicalModes.MinorHarmonic)
            {
                MakeSharp(6);
            }
        }

        private tone12 NextInScale(tone12 k, int steps=1)
        {
            if (steps < 0)
                return PreviousInScale(k, 0 - steps);

            Debug.Assert(this[k]);
            Debug.Assert(steps > 0);
            do
            {
                k++;
            } while (!this[k]);
            return steps == 1 ? k : NextInScale(k, steps - 1);
        }

        private tone12 PreviousInScale(tone12 k, int steps=1)
        {
            Debug.Assert(this[k]);
            Debug.Assert(steps > 0);
            do
            {
                k--;
            } while (!this[k]);

            return steps == 1 ? k : PreviousInScale(k, steps - 1);
        }

        public TwelveToneSet()
        {
            tones = new bool[TWELVE];
        }

        public TwelveToneSet(int[] tones)
        {
            Debug.Assert(tones[0] >= 0);
            Debug.Assert(tones[tones.Length-1] < TWELVE);
            this.tones = new bool[TWELVE];
            this.tones[tones[0]] = true;
            for (int i=1; i<tones.Length; i++)
            {
                Debug.Assert(tones[i] > tones[i - 1]);
                this.tones[tones[i]] = true;
            }
            IsConst = true;
        }

        public TwelveToneSet(TwelveToneSet other)
        {
            tones = new bool[TWELVE];
            for (int i = 0; i < TWELVE; i++)
                tones[i] = other.tones[i];
        }

        public TwelveToneSet(string st)
        {
            tones = new bool[TWELVE];
            int pos;
            for (int i = 0; i < st.Length; i++)
            {
                pos = KEYS[st[i]];
                int j = i + 1;
                if (j < st.Length && (st[j]== SHARP || st[j]== '♭'))
                {
                    if (st[j] == SHARP)
                        pos++;
                    if (st[j] == FLAT)
                        pos--;
                    i++;
                }

                this[pos] = true;
            }
            IsConst = true;
        }

        public TwelveToneSet(Random rand, int ntones, TwelveToneSet scale = null)
        {
            if (scale == null)
                scale = chromatic;

            Debug.Assert(ntones <= scale.Count);
            tone12 pos = 0;

            // Find the first tone the scale
            while (!scale[pos])
                pos++;

            // Randomly move it forward
            int times = rand.Next(scale.Count);
            pos = scale.Next(pos, times);

            tones = new bool[TWELVE];
            this[pos] = true;
            for (int i=1; i<ntones; i++)
            {
                pos = scale.Next(pos, 2);
                this[pos] = true;
            }

            Debug.Assert(ntones == this.Count);
            IsConst = true;
        }

        public TwelveToneSet(Random rand, int ntones, bool inScale=true, bool buildOfThirds=false)
        {
            //TODO: Test it
            tones = new bool[TWELVE];
            for (int i = 0; i<ntones; i++)
            {
                again:
                int pos;
                do
                {
                    pos = rand.Next(TWELVE);
                }
                while (this[pos]);
                this[pos] = true;
                if (inScale && !CoveredByAnySimilar(majorScale))
                {
                    this[pos] = false;
                    goto again;
                }

                if (buildOfThirds)
                {
                    //TODO:
                }
            }
        }
        #endregion

        public void Add(int k)
        {
            Debug.Assert(!IsConst);
            tones[k] = true;
        }

        public void MakeSharp(int k)
        {
            tone12 n = FindNth(k);
            this[n] = false;
            n++;
            Debug.Assert(this[n] == false);
            this[n] = true;
        }

        public void MakeFlat(int k)
        {
            int n = FindNth(k);
            this[n] = false;
            n--;
            Debug.Assert(this[n] == false);
            this[n] = true;
        }

        public TwelveToneSet MakeThirdChord(int startPos, int count)
        {
            bool[] tones = new bool[TWELVE];
            Debug.Assert(this[startPos]);
            this[startPos] = true;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the next note circularly
        /// </summary>
        /// <param name="pos">Current position</param>
        public tone12 Next(tone12 pos, int times=1)
        {
            Debug.Assert(this[pos]);
            for (int i = 0; i < times; i++)
                while (!this[pos++]);

            return pos;
        }

        /// <summary>
        /// Find the n-th scale member
        /// </summary>
        /// <param name="n">Zero-based</param>
        private tone12 FindNth(int n)
        {
            Debug.Assert(n >= 0);
            int m = n;
            for (int i=0; i<TWELVE; i++)
                if (this[i] && m-- == 0)
                    return i;

            throw new ArgumentException($"Argument n={n} is too high. There should be at least {n+1} tones");
        }

        public int Calculate(int k)
        {
            if (k < 0) return Calculate(k + Count) - TWELVE;

            int octaves = k / Count;
            int tone = k % Count;
            return TWELVE * octaves + (int)FindNth(tone);
        }

        #region Equals, Similar, CoveredBy
        public override bool Equals(object obj)
        {
            if (obj is TwelveToneSet)
                return Equals(obj as TwelveToneSet);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return tones.Aggregate(0, (c, b) => (c>>1) + (b ? 1 : 0));
        }

        public bool Equals(TwelveToneSet other)
        {
            for (int i = 0; i < TWELVE; i++)
                if (this[i] != other[i])
                    return false;

            return true;
        }

        public bool Similar(TwelveToneSet other)
        {
            if (other.Count != this.Count)
                return false;

            if (this.Equals(other))
                return true;

            TwelveToneSet copy = new TwelveToneSet(this);

            for (int i=1; i<TWELVE; i++)
            {
                copy.ShiftRight(1);
                if (copy.Equals(other))
                    return true;
            }

            return false;
        }

        public bool CoveredBy(TwelveToneSet other)
        {
            for (int i = 0; i < TWELVE; i++)
                if (this[i] && !other[i])
                    return false;

            return true;
        }

        public bool CoveredByAnySimilar(TwelveToneSet other)
        {
            if (this.CoveredBy(other))
                return true;

            TwelveToneSet copy = new TwelveToneSet(this);

            for (int i = 1; i < TWELVE; i++)
            {
                copy.ShiftRight(1);
                if (copy.CoveredBy(other))
                    return true;
            }

            return false;
        }
        #endregion

        #region Union and Intersection
        static TwelveToneSet Union(TwelveToneSet set1, TwelveToneSet set2)
        {
            TwelveToneSet set = new TwelveToneSet();
            for (int i = 0; i < TWELVE; i++)
                set[i] = set1[i] || set2[i];
            return set;
        }

        static TwelveToneSet Intersection(TwelveToneSet set1, TwelveToneSet set2)
        {
            TwelveToneSet set = new TwelveToneSet();
            for (int i = 0; i < TWELVE; i++)
                set[i] = set1[i] && set2[i];
            return set;
        }

        static TwelveToneSet Minus(TwelveToneSet set1, TwelveToneSet set2)
        {
            TwelveToneSet set = new TwelveToneSet();
            for (int i = 0; i < TWELVE; i++)
                set[i] = set1[i] && !set2[i];
            return set;
        }
        #endregion

        #region Distances
        int CountMajorScales()
        {
            int count = 0;

            TwelveToneSet copy = new TwelveToneSet(majorScale);
            if (this.CoveredBy(copy))
                count++;

            for (int i = 1; i < TWELVE; i++)
            {
                copy.ShiftRight(1);
                if (this.CoveredBy(copy))
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Distance based on common tones between the two chords
        /// </summary>
        /// <param name="other">The other TwelveToneSet</param>
        /// <returns>Greater count minus the number of common tones</returns>
        public int DistanceCommonTones(TwelveToneSet other)
        {
            int max = Math.Max(this.Count, other.Count);
            int count = 0;
            for (int i=0; i<TWELVE; i++)
            {
                if (tones[i] && other.tones[i])
                    count++;
            }

            return max - count;
        }

        /// <summary>
        /// Distance based on the number of common underlying Scales
        /// </summary>
        public int DistanceScales(TwelveToneSet other)
        {
            int count = 0;
            TwelveToneSet copy = new TwelveToneSet(majorScale);
            for (int i =0; i<TWELVE; i++)
            {
                if (this.CoveredBy(copy) && other.CoveredBy(copy))
                    count++;
                copy.ShiftRight(1);
            }

            int ret = Math.Min(CountMajorScales(), other.CountMajorScales()) - count;
            return ret;
        }

        public int TotalDistance(TwelveToneSet other)
        {
            return DistanceScales(other) + DistanceCommonTones(other);
        }
        #endregion

        /// <summary>
        /// E.g:
        /// C-E-G -> 4
        /// A-C-E -> 4
        /// C-E-G-H -> 5
        /// D-F#-A-C -> 6
        /// F#-A-C -> 6
        /// D#-F#-A-C -> 9
        /// C-E-G# -> 8
        /// </summary>
        public int MeasureHarmonyByFifths()
        {
            bool[] fifths = new bool[TWELVE];
            for (int i = 0; i < TWELVE; i += 2)
            {
                fifths[i] = tones[i];
                fifths[i + 1] = tones[(i+1 + TWELVE / 2) % TWELVE];
            }

            int countStart = fifths.TakeWhile(b => false).Count();
            int count = 0, maxCount = countStart;
            for (int i = countStart+1; i<TWELVE; i++)
            {
                if (!fifths[i])
                {
                    count++;
                }
                else
                {
                    if (count > maxCount)
                        maxCount = count;
                    count = 0;
                }
            }

            if (count + countStart > maxCount)
                maxCount = count + countStart;

            return TWELVE - maxCount - 1;
        }

        public int InScale()
        {
            int count = 0;

            for (int i=0; i<TWELVE; i++)
            {
                if (tones[i] && !majorScale[i])
                    count++;
            }

            return count;
        }

        #region Modifiers
        public TwelveToneSet ShiftRight(int count)
        {
            Debug.Assert(!IsConst);
            Debug.Assert(count >= 0 && count < TWELVE);
            bool[] tones = new bool[TWELVE];
            for (int i = count; i < TWELVE; i++)
                tones[i] = this.tones[i - count];
            for (int i = 0; i < count; i++)
                tones[i] = this.tones[i + TWELVE - count];
            this.tones = tones;

            return this;
        }

        public TwelveToneSet ShiftLeft(int count)
        {
            Debug.Assert(!IsConst);
            Debug.Assert(count >= 0 && count < 12);
            bool[] tones = new bool[TWELVE];
            for (int i = count; i < TWELVE; i++)
                tones[i - count] = this.tones[i];
            for (int i = TWELVE - count; i < TWELVE; i++)
                tones[i] = this.tones[i - (TWELVE - count)];
            this.tones = tones;

            return this;
        }

        public TwelveToneSet ShiftInScale(int count, TwelveToneSet scale)
        {
            if (count == 0)
                return this;

            Debug.Assert(scale.IsConst);
            Debug.Assert(scale.Count >= Count);
            TwelveToneSet set = new TwelveToneSet();

            // Transpose in scale notes first
            TwelveToneSet intersect = Intersection(this, scale);
            Dictionary<int, int> mapping = new Dictionary<int, int>();
            foreach (var x in intersect)
            {
                mapping[x] = scale.NextInScale(x, count);
                set[mapping[x]] = true;
            }

            // If each in-scale note was transposed by the same amount, do it for the rest
            TwelveToneSet rest = Minus(this, scale);
            if (mapping.Select(kpv => new tone12(kpv.Value - kpv.Key)).Distinct().Count() == 1)
            {
                tone12 offset = mapping.First().Value - mapping.First().Key;

                rest = Minus(this, scale);
                foreach (var x in rest)
                {
                    mapping[x] = (x + offset);
                    set[mapping[x]] = true;
                }
            }

            // If the choice is between in-scale and out-scale, choose in scale
            rest = Minus(this, scale);
            foreach (var x in rest)
            {
                var list = mapping.Select(kpv => x + (kpv.Value - kpv.Key)).Distinct().ToList();
                if (list.Count == 2)
                {
                    if (scale[list.First()] ^ scale[list.Last()])
                    {
                        tone12 tone = scale[list.First()] ? list.First() : list.Last();
                        mapping[x] = tone;
                        set[tone] = true;
                    }
                }
            }

            Debug.Assert(set.Count == this.Count);
            return set;
        }
        #endregion

        public override string ToString()
        {
            string st = "";
            bool useSharp = true;

            // Note and Note#
            if (this[0] && this[1])
                useSharp = false;
            if (this[2] && this[3])
                useSharp = false;
            if (this[5] && this[6])
                useSharp = false;
            if (this[7] && this[8])
                useSharp = false;
            if (this[9] && this[10])
                useSharp = false;

            // C and A# or C and D#
            if (this[0] && this[3])
                useSharp = false;
            if (this[0] && this[10])
                useSharp = false;

            for (int i=0; i<TWELVE; i++)
            {
                if (!this[i])
                    continue;

                switch (i)
                {
                    case 0: st += "C"; break;
                    case 1: st += useSharp ? "C#" : "D♭"; break;
                    case 2: st += "D"; break;
                    case 3: st += useSharp ? "D#" : "E♭"; break;
                    case 4: st += "E"; break;
                    case 5: st += "F"; break;
                    case 6: st += useSharp ? "F#" : "G♭"; break;
                    case 7: st += "G"; break;
                    case 8: st += useSharp ? "G#" : "A♭"; break;
                    case 9: st += "A"; break;
                    case 10: st += useSharp ? "A#" : "B♭"; break;
                    case 11: st += "B"; break;
                }
                st += ' ';
            }

            return st;
        }

        public IEnumerator<tone12> GetEnumerator()
        {
            for (int i = 0; i < TWELVE; i++)
                if (tones[i])
                    yield return i;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    
}