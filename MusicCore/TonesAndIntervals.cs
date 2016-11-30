
namespace MusicComposer.TonesAndIntervals
{
    public class Scale7Tone
    {
        private static readonly string TONES = "CDEFGAB";

        private int tone7;

        public Scale7Tone(int tone7)
        {
            this.tone7 = tone7;
        }

        public override string ToString()
        {
            int mod7 = tone7 % 7;
            int div7 = tone7 / 7;
            if (mod7 < 0)
            {
                mod7 += 7;
                div7 -= 1;
            }
            return $"{TONES[mod7]}{div7}";
        }
    }

    public class Scale7Interval : Scale7Tone
    {
        public Scale7Interval() : base(0)
        {

        }

        public static Scale7Interval operator+(Scale7Tone a, Scale7Interval b)
        {
            return new Scale7Interval();
        }
    }
}