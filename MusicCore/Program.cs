using System.Diagnostics;

namespace MusicCore
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    public abstract class Scale
    {
        public abstract int this[int k] { get; }
    }

    public class MajorScale : Scale
    {
        public override int this[int k]
        {
            get
            {
                switch (k)
                {
                    case 0: return 0;
                    case 1: return 2;
                    case 2: return 4;
                    case 3: return 5;
                    case 4: return 7;
                    case 5: return 9;
                    case 6: return 11;
                    case 7: return 12;
                }
                Debug.Fail("wrong number");
                return -1;
            }
        }
    }

    public class MinorScale : Scale
    {
        public override int this[int k]
        {
            get
            {
                switch (k)
                {
                    case 0: return 0;
                    case 1: return 2;
                    case 2: return 3;
                    case 3: return 5;
                    case 4: return 7;
                    case 5: return 8;
                    case 6: return 10;
                    case 7: return 12;
                }
                Debug.Fail("wrong number");
                return -1;
            }
        }
    }

    public struct Measure
    {
        private bool IsPowerOfTwo(int n)
        {
            var x = n & (n - 1);
            return n > 0 && x == 2 * n - 1;
        }

        public readonly int Counts;
        public readonly int Divider;
        public Measure(int counts, int divider)
        {
            Counts = counts;
            Divider = divider;
            Debug.Assert(counts > 0 && IsPowerOfTwo(divider));
        }
    }

    public struct Duration
    {
        public int Counts, Divider;

        public static Duration operator+(Duration dur, Duration dur2)
        {
            return new Duration();
        }
    }
}