using System.Diagnostics;

namespace MusicCore
{
    public struct Fraction
    {
        public int p { get; }
        public int q { get; }

        public Fraction(int p, int q)
        {
            Debug.Assert(q > 0);
            int gcd = GCD(p, q);
            this.p = p / gcd;
            this.q = q / gcd;
        }

        static public int GCD(int a, int b)
        {
            if (b == 0)
                return a;
            else
                return GCD(b, a % b);
        }

        static public int LCD(int a, int b)
        {
            int gcd = GCD(a, b);
            return (a / gcd) * b;
        }

        public static Fraction operator+(Fraction f, int n)
        {
            return new Fraction(f.p + n * f.q, f.q);
        }

        public static Fraction operator-(Fraction f, int n)
        {
            return new Fraction(f.p - n * f.q, f.q);
        }

        public static Fraction operator +(Fraction f1, Fraction f2)
        {
            return new Fraction(f1.p * f2.q + f2.p * f1.q, f1.q * f2.q);
        }

        public static Fraction operator -(Fraction f1, Fraction f2)
        {
            return new Fraction(f1.p * f2.q - f2.p * f1.q, f1.q * f2.q);
        }

        public static Fraction operator*(Fraction f, int n)
        {
            return new Fraction(f.p * n, f.q);
        }

        #region Equals and GetHashCode
        public override bool Equals(object obj)
        {
            if (!(obj is Fraction))
                return false;

            Fraction fract = (Fraction)(obj);

            return fract.p == p && fract.q == q;
        }

        public override int GetHashCode()
        {
            return p.GetHashCode() + q.GetHashCode();
        }
        #endregion

        static public explicit operator int(Fraction fract)
        {
            Debug.Assert(fract.q == 1);
            return fract.p;
        }

        public override string ToString()
        {
            if (q == 1)
                return $"{p}";
            return $"{p}/{q}";
        }
    }
}