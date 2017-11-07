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
        public Tone(int tone)
        {
            this.tone = tone;
        }

        public static implicit operator int(Tone tone)
        {
            return tone.tone;
        }

        public static implicit operator Tone(int tone)
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
            return $"{tone} = {tone / 12}";
        }
    }

    public class tone12
    {
        public override int GetHashCode()
        {
            return tone;
        }

        public override bool Equals(object obj)
        {
            if (obj is tone12)
                return (obj as tone12).tone == tone;

            return false;
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
