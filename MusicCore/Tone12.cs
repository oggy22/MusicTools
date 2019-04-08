using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MusicCore
{
    /// <summary>
    /// Represents a midi tone (from
    /// </summary>
    public class Tone
    {
        public const int OCTAVE = 12;
        public const int FIFTH = 7;
        public const int MAJOR3 = 4;
        public const int MINOR3 = 3;
        public const int SECOND = 2;

        int tone;

#region Constructors
        public Tone(int tone)
        {
            this.tone = tone;
        }

        public Tone(string st)
        {
            tone = FromString(st);
        }

        static Tone()
        {
            // Calculate frequencies
            frequency = new double[256];
            int initialTone = new Tone("A3");
            frequency[initialTone] = 110;
            double SemiToneRatio = Math.Pow(2.0, 1.0 / 12);
            for (int i = initialTone + 1; i < frequency.Length; i++)
                frequency[i] = frequency[i - 1] * SemiToneRatio;
            for (int i = initialTone - 1; i >= 0; i--)
                frequency[i] = frequency[i + 1] / SemiToneRatio;

            // Calculate curve A
            //https://en.wikipedia.org/wiki/A-weighting#Function_realisation_of_some_common_weightings
            curveA = new double[256];
            Func<double, double> sq = (double x) => x * x;
            for (int i = 0; i < curveA.Length; i++)
            {
                double f = frequency[i];
                double f2 = sq(f);
                double f4 = sq(f2);
                double Top = sq(12194) * f4;
                double Bottom = (f2 + sq(20.6)) * Math.Sqrt((f2 + sq(107.7)) * (f2 + sq(737.9))) * (f2 + sq(12194));
                double Ra = Top / Bottom;
                double A = 20 * Math.Log10(Ra) + 2.0;
                curveA[i] = A;
            }
        }
#endregion

        private static double[] frequency, curveA;

        public double Frequency
        {
            get => frequency[tone];
        }

        public double CurveA
        {
            get => curveA[tone];
        }

        #region Static methods and static converters
        public static int FromString(string st)
        {
            int i = st.IndexOfAny(new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
            if (i == -1)
            {
                return tone12.FromString(st);
            }
            else
            {
                int octave = int.Parse(st.Substring(i));
                return 12 * octave + tone12.FromString(st.Substring(0, i));
            }
        }

        public static implicit operator int(Tone tone)
        {
            return tone.tone;
        }

        public static implicit operator Tone(int tone)
        {
            return new Tone(tone);
        }

        public static implicit operator Tone(string tone)
        {
            return new Tone(tone);
        }

        public static IEnumerable<int> GetHarmonics()
        {
            yield return 0;
            yield return OCTAVE;
            yield return OCTAVE + FIFTH;

            // Second octave
            yield return OCTAVE + OCTAVE;
            yield return OCTAVE + OCTAVE + MAJOR3;
            yield return OCTAVE + OCTAVE + FIFTH;
            yield return OCTAVE + OCTAVE + FIFTH + MINOR3;

            // Third octave
            yield return OCTAVE + OCTAVE + OCTAVE;
            yield return OCTAVE + OCTAVE + OCTAVE + SECOND;
            yield return OCTAVE + OCTAVE + OCTAVE + MAJOR3;
            yield return OCTAVE + OCTAVE + OCTAVE + FIFTH;
            yield return OCTAVE + OCTAVE + OCTAVE + FIFTH + MINOR3;
            yield return OCTAVE + OCTAVE + OCTAVE + FIFTH + MINOR3 + 1;

            // The rest
            for (int i = 4 * OCTAVE; ; i++)
                yield return i;
        }
        #endregion

        public IEnumerable<Tone> GetHarmonicsUp()
        {
            foreach (int d in GetHarmonics())
                yield return new Tone(tone + d);
        }

        public IEnumerable<Tone> GetHarmonicsDown()
        {
            foreach (int d in GetHarmonics())
                yield return new Tone(tone - d);
        }

        public override string ToString()
        {
            return $"{new tone12(tone % 12)}{tone / 12}";
        }
    }

    public struct tone12
    {
        public override int GetHashCode()
        {
            return tone;
        }

        const int TWELVE = 12;

        int tone;

        public static int correct(int tone)
        {
            if (tone >= 0)
                return tone % TWELVE;
            else
            {
                int mod = tone % TWELVE;
                return mod == 0 ? 0 : TWELVE + mod;
            }
        }

        public static tone12 operator++(tone12 tone)
        {
            return new tone12(correct(tone.tone + 1));
        }

        public static tone12 operator--(tone12 tone)
        {
            return new tone12(correct(tone.tone - 1));
        }

        public tone12(int tone)
        {
            this.tone = correct(tone);
        }

        public tone12(string st)
        {
            tone = FromString(st);
        }

        public static implicit operator tone12(int tone)
        {
            return new tone12(tone);
        }

        public static implicit operator int(tone12 tone)
        {
            return tone.tone;
        }

        public static tone12 operator+(tone12 tone, int x)
        {
            return new tone12(correct(tone.tone+x));
        }

        public static int FromString(string st)
        {
            Debug.Assert(st.Length >= 1 && st.Length <= 2);
            st = st.ToUpper();
            Debug.Assert("CDEFGAB".IndexOf(st[0]) != -1);
            int num;
            switch(st[0])
            {
                case 'C': num = 0; break;
                case 'D': num = 2; break;
                case 'E': num = 4; break;
                case 'F': num = 5; break;
                case 'G': num = 7; break;
                case 'A': num = 9; break;
                case 'B': num = 11; break;
                default: Debug.Fail($"{st[0]} is not a note!"); num = 0; break;
            }
            if (st.Length == 2)
            {
                if (st[1] == '#')
                    num++;
                else if (st[1] == 'b' || st[1] == '♭')
                    num--;
                else Debug.Fail($"{st[1]} is wrong modifier!");
            }
                
            return num;
        }

        public override string ToString()
        {
            switch (tone)
            {
                case 0: return "C";
                case 1: return "C#";
                case 2: return "D";
                case 3: return "D#";
                case 4: return "E";
                case 5: return "F";
                case 6: return "F#";
                case 7: return "G"; ;
                case 8: return "G#";
                case 9: return "A"; ;
                case 10: return "B♭";
                case 11: return "B";
            }
            Debug.Fail("invalid tone");
            return "";
        }
    }
}
