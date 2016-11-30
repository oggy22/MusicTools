using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MusicCore
{
    public enum MusicalModes
    {
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
    public class TwelveToneSet
    {
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

        private bool[] tones;

        public bool this[int pos]
        {
            get
            {
                return tones[pos];
            }
        }

        #region Constructors
        public TwelveToneSet(bool[] tones)
        {
            if (tones.Length > 12)
                Debug.Fail($"Parameter tones.Length = {tones.Length}, but must not be larger than 12");

            tones = new bool[TWELVE];
            for (int i = 0; i < tones.Length; i++)
                this.tones[i] = tones[i];
        }

        public TwelveToneSet(MusicalModes mode) : this(majorScale)
        {
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

        public void MakeSharp(int k)
        {
            int n = FindNth(k);
            tones[n] = false;
            n = (n + 1) % TWELVE;
            Debug.Assert(tones[n] == false);
            tones[n] = true;
        }

        public void MakeFlat(int k)
        {
            int n = FindNth(k);
            tones[n] = false;
            n = (n + TWELVE - 1) % TWELVE;
            Debug.Assert(tones[n] == false);
            tones[n] = true;
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

                tones[pos] = true;
            }
        }

        public TwelveToneSet(Random rand, int ntones, TwelveToneSet scale)
        {
            int pos = 0;

            // Find the first tone the sclae
            while (!scale[pos])
                pos++;

            // Randomly move it forward
            int times = rand.Next(scale.Count);
            pos = scale.Next(pos, times);

            tones = new bool[TWELVE];
            tones[pos] = true;
            for (int i=1; i<ntones; i++)
            {
                pos = scale.Next(pos, 2);
                tones[pos] = true;
            }

            Debug.Assert(ntones == this.Count);
        }

        public TwelveToneSet(Random rand, int ntones, bool inScale=true, bool buildOfThirds=false)
        {
            tones = new bool[TWELVE];
            for (int i = 0; i<ntones; i++)
            {
                again:
                int pos;
                do
                {
                    pos = rand.Next(TWELVE);
                }
                while (tones[pos]);
                tones[pos] = true;
                if (inScale && !CoveredByAnySimilar(majorScale))
                {
                    tones[pos] = false;
                    goto again;
                }

                if (buildOfThirds)
                {
                    //TODO:
                }

            }
        }

        public void IsBuildOfThirds()
        {
            int count = this.Count;

            TwelveToneSet copy = new TwelveToneSet(majorScale);

        }


        public TwelveToneSet MakeThirdChord(int startPos, int count)
        {
            bool[] tones = new bool[TWELVE];
            Debug.Assert(tones[startPos]);
            tones[startPos] = true;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the next note circularly
        /// </summary>
        /// <param name="pos">Current position</param>
        public int Next(int pos, int times=1)
        {
            Debug.Assert(tones[pos]);
            for (int i = 0; i < times; i++)
                do
                {
                    pos++;
                    if (pos == TWELVE)
                        pos = 0;
                } while (!tones[pos]);

            return pos;
        }

        #endregion Constructors

        #region Properties
        public bool IsRooted { get { return tones[0]; } }

        public int Count { get { return tones.Count(f => f); } }

        private int FindNth(int n)
        {
            Debug.Assert(n >= 0);
            int m = n;
            for (int i=0; i<TWELVE; i++)
                if (tones[i] && m-- == 0)
                    return i;

            throw new ArgumentException($"Argument n={n} is to high. There should be at least {n+1} tones");
        }

        public int Calculate(int k)
        {
            if (k < 0) return Calculate(k + Count) - TWELVE;

            int octaves = k / Count;
            int tone = k % Count;
            return TWELVE * octaves + FindNth(tone);
        }


        public int First
        {
            get
            {
                for (int i = 0; i < TWELVE; i++)
                    if (tones[i])
                        return i;

                throw new Exception("Empty!");
            }
        }

        public int Last
        {
            get
            {
                for (int i = TWELVE-1; i >= 0; i--)
                    if (tones[i])
                        return i;

                throw new Exception("Empty!");
            }
        }
        #endregion

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
                if (tones[i] != other.tones[i])
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
                if (tones[i] && !other.tones[i])
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
        /// <param name="other"></param>
        /// <returns></returns>
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
            Debug.Assert(count >= 0 && count < 12);
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
            Debug.Assert(count >= 0 && count < 12);
            bool[] tones = new bool[TWELVE];
            for (int i = count; i < TWELVE; i++)
                tones[i - count] = this.tones[i];
            for (int i = TWELVE - count; i < TWELVE; i++)
                tones[i] = this.tones[i - (TWELVE - count)];
            this.tones = tones;

            return this;
        }
        #endregion

        public override string ToString()
        {
            string st = "";
            bool useSharp = true;

            // Note and Note#
            if (tones[0] && tones[1])
                useSharp = false;
            if (tones[2] && tones[3])
                useSharp = false;
            if (tones[5] && tones[6])
                useSharp = false;
            if (tones[7] && tones[8])
                useSharp = false;
            if (tones[9] && tones[10])
                useSharp = false;

            // C and A# or C and D#
            if (tones[0] && tones[3])
                useSharp = false;
            if (tones[0] && tones[10])
                useSharp = false;

            for (int i=0; i<TWELVE; i++)
            {
                if (!tones[i])
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
    }
}