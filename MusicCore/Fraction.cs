﻿using System;
using System.Diagnostics;

namespace MusicCore
{
    public struct Fraction
    {
        static public readonly Fraction ZERO = new Fraction(0, 1);
        //Invariant: p and q are always co-prime
        public readonly int p, q;

        public Fraction(int p, int q)
        {
            Debug.Assert(q > 0);
            int gcd = GCD(Math.Abs(p), q);
            this.p = p / gcd;
            this.q = q / gcd;
        }

        static public Fraction Parse(string s)
        {
            int slashIndex = s.IndexOf('/');
            if (slashIndex == -1)
                return new Fraction(int.Parse(s), 1);

            int p = int.Parse(s.Substring(0, slashIndex));
            int q = int.Parse(s.Substring(slashIndex + 1));
            return new Fraction(p, q);
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

        public static Fraction Min(Fraction a, Fraction b)
        {
            // a = a.p / a.q, b = b.p / b.q;
            if (((long)a.p) * b.q < ((long)b.p * a.q))
                return a;
            else
                return b;
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

        public static Fraction operator*(int n, Fraction f)
        {
            return new Fraction(f.p * n, f.q);
        }

        public static Fraction operator/(Fraction f1, Fraction f2)
        {
            return new Fraction(f1.p * f2.q, f1.q * f2.p);
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

        static public implicit operator int(Fraction fract)
        {
            return fract.p / fract.q;
        }

        static public implicit operator double(Fraction fract)
        {
            return ((double)fract.p) / fract.q;
        }

        static public implicit operator Fraction(int n)
        {
            return new Fraction(n, 1);
        }

        public bool IsWhole()
        {
            return p % q == 0;
        }

        public override string ToString()
        {
            if (q == 1)
                return $"{p}";
            return $"{p}/{q}";
        }

        public static bool operator ==(Fraction f1, Fraction f2)
        {
            return f1.p == f2.p && f1.q == f2.q;
        }

        public static bool operator !=(Fraction f1, Fraction f2)
        {
            return f1.p != f2.p || f1.q != f2.q;
        }
    }
}