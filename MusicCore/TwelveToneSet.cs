using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

    public struct TwelveToneSet : IEnumerable<tone12>
    {
        #region Constants
        public const int TWELVE = 12;

        private static readonly char SHARP = '#';
        private static readonly char FLAT = '♭';

        static readonly Dictionary<char, int> KEYS = new Dictionary<char, int>
        {
            { 'C', 0}, {'D', 2}, {'E', 4}, {'F', 5}, {'G', 7}, {'A', 9}, {'B', 11}, {'H', 11}
        };

        static public readonly TwelveToneSet majorScale = new TwelveToneSet("CDEFGAH");
        static public readonly TwelveToneSet minorScale = new TwelveToneSet("CDE♭FGA♭B♭");
        static public readonly TwelveToneSet minorHarmonicScale = new TwelveToneSet("CDE♭FGA♭B");
        static public readonly TwelveToneSet minorMelodicScale = new TwelveToneSet("CDE♭FGAB");
        static public readonly TwelveToneSet majorTriad = new TwelveToneSet("CEG");
        static public readonly TwelveToneSet minorTriad = new TwelveToneSet("CE♭G");
        static public readonly TwelveToneSet major7 = new TwelveToneSet("CEGB♭");
        static public readonly TwelveToneSet minor7 = new TwelveToneSet("CE♭GB♭");
        static public readonly TwelveToneSet augmented = new TwelveToneSet("CEG#");
        static public readonly TwelveToneSet halfDiminished = new TwelveToneSet("CDFG#");
        static public readonly TwelveToneSet fullDiminished = new TwelveToneSet("CE♭F#A");

        static public TwelveToneSet chromatic = new TwelveToneSet(MusicalModes.Chromatic);
        #endregion

        #region Implicit operators
        public static implicit operator TwelveToneSet(short s)
        {
            return new TwelveToneSet(s);
        }

        public static implicit operator short(TwelveToneSet tts)
        {
            return tts.tones;
        }
        #endregion

        private readonly short tones;

        #region Properties
        public bool IsRooted { get { return this[0]; } }

        public int Count
        {
            get
            {
                int tones_copy = tones;
                int count = 0;
                while (tones_copy != 0)
                {
                    if ((tones_copy & 1) == 1)
                        count++;

                    tones_copy = tones_copy >> 1;
                }

                return count;
            }
        }

        public bool this[int pos]
        {
            get
            {
                return ((1 << pos) & tones) != 0;
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

            this.tones = (short)(0);
            for (int i = 0; i < tones.Length; i++) ;
            //todo:
        }

        public TwelveToneSet(IEnumerable<int> tones)
        {
            this.tones = (short)(0);
            foreach (int i in tones)
            {
                tone12 t12 = i;
                this.tones = BitOn(this.tones, t12);
            }
        }

        public TwelveToneSet(MusicalModes mode) : this(majorScale)
        {
            if (mode == MusicalModes.Chromatic)
            {
                tones = 1 + 2 + 4 + 8 + 16 + 32 + 64 + 128 + 256 + 512 + 1024 + 2048;

                return;
            }

            if (mode >= MusicalModes.Dorian && mode <= MusicalModes.Locrian)
            {
                int shift = FindNth((int)mode - (int)MusicalModes.Ionian);
                this = ShiftLeft(shift);
                return;
            }

            MusicalModes modeShift = mode;
            if (mode == MusicalModes.MinorHarmonic || mode == MusicalModes.MinorMelodic)
                modeShift = MusicalModes.Minor;

            int tone = FindNth((int)(modeShift) - 1);
            ShiftLeft(tone);

            if (mode == MusicalModes.MinorMelodic)
            {
                MakeSharp(4);
                MakeSharp(5);
            }

            if (mode == MusicalModes.MinorHarmonic)
            {
                MakeSharp(5);
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

        public TwelveToneSet(TwelveToneSet other)
        {
            this.tones = other.tones;
        }

        public TwelveToneSet(string st)
        {
            int pos;
            tones = 0;
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

                tones |= (short)(1 << pos);
            }
        }

        public TwelveToneSet(Random rand, int ntones, TwelveToneSet scale)
        {
            Debug.Assert(ntones <= scale.Count);
            tone12 pos = 0;

            // Find the first tone the scale
            while (!scale[pos])
                pos++;

            // Randomly move it forward
            int times = rand.Next(scale.Count);
            pos = scale.Next(pos, times);

            tones = 0;
            tones &= (short)(1 << pos);
            for (int i=1; i<ntones; i++)
            {
                pos = scale.Next(pos, 2);
                tones &= (short)(1 << pos);
            }

            Debug.Assert(ntones == this.Count);
        }

        public TwelveToneSet(Random rand, int ntones, bool inScale=true, bool buildOfThirds=false)
        {
            //TODO: Test it
            tones = 0;
            for (int i = 0; i<ntones; i++)
            {
                again:
                int pos;
                do
                {
                    pos = rand.Next(TWELVE);
                }
                while (this[pos]);
                tones &= (short)(1 << pos);
                if (inScale && !CoveredByAnySimilar(majorScale))
                {
                    tones &= (short)(1 << pos);
                    goto again;
                }

                if (buildOfThirds)
                {
                    //TODO:
                }
            }
        }

        public TwelveToneSet(short tones)
        {
            this.tones = tones;
        }
        #endregion

        #region Operations
        static bool IsBitOn(short bitmap, tone12 pos)
        {
            return (bitmap & (1 << pos)) != 0;
        }

        static public short BitOn(short bitmap, int pos)
        {
            Debug.Assert(0 <= pos && pos < TWELVE);
            bitmap |= ((short)(1 << pos));
            return bitmap;
        }

        static public short BitOff(short bitmap, int pos)
        {
            Debug.Assert(0 <= pos && pos < TWELVE);
            bitmap &= ((short)(1 << pos));
            return bitmap;
        }

        public TwelveToneSet MakeSharp(int k)
        {
            tone12 n = FindNth(k);
            short copy = tones;
            copy = BitOff(copy, n);
            n++;
            Debug.Assert(this[n] == false);
            copy = BitOn(copy, n);

            return new TwelveToneSet(copy);
        }

        public TwelveToneSet MakeFlat(int k)
        {
            short copy = tones;

            int n = FindNth(k);
            Debug.Assert(IsBitOn(copy, n));
            copy = BitOff(copy, n);
            n--;
            Debug.Assert(!IsBitOn(copy, n));
            copy = BitOn(copy, n);
            return new TwelveToneSet(copy);
        }
        #endregion

        /// <summary>
        /// Returns the next note circularly
        /// </summary>
        /// <param name="pos">Current position</param>
        public tone12 Next(tone12 pos, int times=1)
        {
            Debug.Assert(this[pos]);
            for (int i = 0; i < times; i++)
                while (!this[++pos]);

            return pos;
        }

        /// <summary>
        /// Find the n-th scale member
        /// </summary>
        /// <param name="n">Zero-based</param>
        public tone12 FindNth(int n)
        {
            Debug.Assert(n >= 0);
            int m = n;
            for (int i=0; i<TWELVE; i++)
                if (this[i] && m-- == 0)
                    return i;

            throw new ArgumentException($"Argument n={n} is too high. There should be at least {n+1} tones");
        }

        public tone12 FindOrder(int n)
        {
            Debug.Assert(this[0]);
            Debug.Assert(this[n]);
            int ind = 0;
            tone12 t = 0;
            while (t < n)
            {
                t = Next(t, 1);
                ind++;
            }
            Debug.Assert(t == n);
            return ind;
        }

        public int Calculate(int k)
        {
            if (k < 0) return Calculate(k + Count) - TWELVE;

            int octaves = k / Count;
            int tone = k % Count;
            return TWELVE * octaves + (int)FindNth(tone);
        }

        /// <summary>
        /// For major and minor chords, optionally with some extra notes, get the root note.
        /// </summary>
        public tone12 GetRoot()
        {
            HashSet<int> roots = new HashSet<int>();

            foreach (tone12 t in this)
            {
                if (IsBitOn(tones, t + 7))
                    if (IsBitOn(tones, t + 3) ^ IsBitOn(tones, t + 4))
                        roots.Add(t);
            }

            if (roots.Count == 1)
                return roots.Single();

            throw new Exception("There is no root");
        }

        public int ChromaticToDiatonic(int toneChromatic)
        {
            if (toneChromatic >= 0)
            {
                int octaves = toneChromatic / TWELVE;
                toneChromatic = toneChromatic % TWELVE;
                return octaves * Count + FindOrder(toneChromatic);
            }
            else
            {
                int octaves = Math.Abs(toneChromatic / TWELVE) + 1;
                return ChromaticToDiatonic(toneChromatic + (octaves * TWELVE)) - octaves * Count;
            }
        }

        public int DiatonicToChromatic(int toneDiatonic)
        {
            if (toneDiatonic >= 0)
            {
                int octaves = toneDiatonic / Count;
                toneDiatonic = toneDiatonic % Count;
                int findnth;
                //todo: This is a workaround for apparent compiler bug
                int ret = (findnth = FindNth(toneDiatonic)) + octaves * TWELVE;
                return ret;
            }
            else
            {
                int octaves = Math.Abs(toneDiatonic / Count) + 1;
                return DiatonicToChromatic(toneDiatonic + octaves * Count) - octaves * TWELVE;
            }
        }

        #region Operators == and !=, Eqauls and GetHashCode
        public static bool operator ==(TwelveToneSet tts1, TwelveToneSet tts2)
        {
            return tts1.tones == tts2.tones;
        }

        public static bool operator !=(TwelveToneSet tts1, TwelveToneSet tts2)
        {
            return tts1.tones != tts2.tones;
        }

        public override bool Equals(object obj)
        {
            if (obj is TwelveToneSet tts)
                return tts.tones == tones;

            return false;
        }

        public override int GetHashCode()
        {
            return tones;
        }
        #endregion

        #region Equals, Similar, CoveredBy
        public bool Equals(TwelveToneSet other) => tones == other.tones;

        public bool IsSimilar(TwelveToneSet other)
        {
            if (other.Count != this.Count)
                return false;

            if (this.Equals(other))
                return true;

            TwelveToneSet copy = new TwelveToneSet(this);

            for (int i=1; i<TWELVE; i++)
            {
                copy = copy.ShiftRight(1);
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
                copy = copy.ShiftRight(1);
                if (copy.CoveredBy(other))
                    return true;
            }

            return false;
        }
        #endregion

        #region Union and Intersection
        static TwelveToneSet Union(TwelveToneSet set1, TwelveToneSet set2)
        {
            return new TwelveToneSet((short)(set1.tones | set2.tones));
        }

        static TwelveToneSet Intersection(TwelveToneSet set1, TwelveToneSet set2)
        {
            return new TwelveToneSet((short)(set1.tones & set2.tones));
        }

        static TwelveToneSet Negative(TwelveToneSet set)
        {
            return new TwelveToneSet((short)(chromatic.tones - set.tones));
        }

        static TwelveToneSet Minus(TwelveToneSet set1, TwelveToneSet set2)
        {
            return new TwelveToneSet((short)(set1.tones & (Negative(set2).tones)));
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
            TwelveToneSet common = TwelveToneSet.Intersection(this, other);
            return Math.Max(this.Count, other.Count) - common.Count;
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
        /// Any note -> 0
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
                fifths[i] = IsBitOn(tones, i);
                fifths[i + 1] = IsBitOn(tones, (i+1 + TWELVE / 2) % TWELVE);
            }

            int countStart = fifths.TakeWhile(b => false).Count();
            int count = 0, maxCount = countStart;
            for (int i = countStart + 1; i < TWELVE; i++)
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

            for (int i = 0; i < TWELVE; i++)
            {
                if (IsBitOn(tones, i) && !majorScale[i])
                    count++;
            }

            return count;
        }

        public bool IsInScale(int tone)
        {
            tone12 t12 = tone;
            return IsBitOn(tones, t12);
        }

        #region Modifiers
        public TwelveToneSet ShiftRight(int count)
        {
            Debug.Assert(count >= 0 && count < TWELVE);
            short copy = (short)((tones << count) & chromatic.tones);
            copy |= (short)(tones >> (TWELVE - count));
            return new TwelveToneSet(copy);
        }

        public TwelveToneSet ShiftLeft(int count)
        {
            if (count == 0)
                return this;

            return ShiftRight(TWELVE - count);
        }

        public TwelveToneSet ShiftInScale(int count, TwelveToneSet scale)
        {
            if (count == 0)
                return this;

            Debug.Assert(scale.Count >= Count);
            short set = 0;

            // Transpose in scale notes first
            TwelveToneSet intersect = Intersection(this, scale);
            Dictionary<int, int> mapping = new Dictionary<int, int>();
            foreach (var x in intersect)
            {
                mapping[x] = scale.NextInScale(x, count);
                set = BitOn(set, mapping[x]);
            }

            // If each in-scale note was transposed by the same amount, do it for the rest
            TwelveToneSet rest = Minus(this, scale);
            var nesto = mapping.Select(kpv => new tone12(kpv.Value - kpv.Key));
            if (nesto.Distinct().Count() == 1)
            {
                tone12 offset = mapping.First().Value - mapping.First().Key;

                rest = Minus(this, scale);
                foreach (var x in rest)
                {
                    mapping[x] = (x + offset);
                    set = BitOn(set, x + offset);
                }

                TwelveToneSet ttsInScale = new TwelveToneSet(set);
                Debug.Assert(ttsInScale.Count == this.Count);
                return ttsInScale;
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
                        set = BitOn(set, tone);
                    }
                }
            }

            TwelveToneSet tts = new TwelveToneSet(set);
            Debug.Assert(tts.Count == this.Count);
            return tts;
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

        #region IEnumerator implementation
        public IEnumerator<tone12> GetEnumerator()
        {
            for (int i = 0; i < TWELVE; i++)
                if (IsBitOn(tones, i))
                    yield return i;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    
}