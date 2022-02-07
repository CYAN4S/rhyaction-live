using System;
using UnityEditor;

namespace Core
{
    /// <summary>
    /// Fraction.
    /// TODO
    /// </summary>
    [Serializable]
    public readonly struct Fraction
    {
        private readonly int _num;
        private readonly int _den;

        public Fraction(int numerator, int denominator = 1)
        {
            if (denominator == 0)
            {
                throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));
            }

            _num = numerator;
            _den = denominator;
        }

        public static Fraction operator +(Fraction a) => a;
        public static Fraction operator -(Fraction a) => new(-a._num, a._den);

        public static Fraction operator +(Fraction a, Fraction b)
            => new Fraction(a._num * b._den + b._num * a._den, a._den * b._den);

        public static Fraction operator -(Fraction a, Fraction b)
            => a + (-b);

        public static Fraction operator *(Fraction a, Fraction b)
            => new Fraction(a._num * b._num, a._den * b._den);

        public static Fraction operator /(Fraction a, Fraction b)
        {
            if (b._num == 0)
            {
                throw new DivideByZeroException();
            }

            return new Fraction(a._num * b._den, a._den * b._num);
        }

        public override string ToString() => $"{_num} / {_den}";

        public static explicit operator float(Fraction x) => (float)x._num / x._den;
        public static implicit operator double(Fraction x) => (double)x._num / x._den;
    }
}