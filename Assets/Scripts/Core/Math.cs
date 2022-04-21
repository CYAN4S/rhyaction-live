using System;
using UnityEditor;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Fraction.
    /// TODO
    /// </summary>
    [Serializable]
    public class Fraction
    {
        [SerializeField] private int num;
        [SerializeField] private int den;

        public Fraction(int numerator, int denominator = 1)
        {
            if (denominator == 0)
            {
                throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));
            }

            num = numerator;
            den = denominator;
        }

        public static Fraction operator +(Fraction a) => a;
        public static Fraction operator -(Fraction a) => new(-a.num, a.den);

        public static Fraction operator +(Fraction a, Fraction b)
            => new Fraction(a.num * b.den + b.num * a.den, a.den * b.den);

        public static Fraction operator -(Fraction a, Fraction b)
            => a + (-b);

        public static Fraction operator *(Fraction a, Fraction b)
            => new Fraction(a.num * b.num, a.den * b.den);

        public static Fraction operator /(Fraction a, Fraction b)
        {
            if (b.num == 0)
            {
                throw new DivideByZeroException();
            }

            return new Fraction(a.num * b.den, a.den * b.num);
        }

        public override string ToString() => $"{num} / {den}";

        public static explicit operator float(Fraction x) => (float) x.num / x.den;
        public static implicit operator double(Fraction x) => (double) x.num / x.den;
    }
}