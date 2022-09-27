using System;
using UnityEngine;

namespace Core
{
    [Serializable]
    public class Fraction
    {
        [SerializeField] private int num;
        [SerializeField] private int den;

        public Fraction(int numerator, int denominator = 1)
        {
            if (denominator == 0)
                throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));

            var gcd = Gcd(numerator, denominator);

            num = numerator / gcd;
            den = denominator / gcd;
        }

        public Fraction(decimal number)
        {
        }

        public static int Gcd(int a, int b)
        {
            while (a != 0 && b != 0)
                if (a > b) a %= b;
                else b %= a;

            return a | b;
        }

        public static Fraction operator +(Fraction a)
        {
            return a;
        }

        public static Fraction operator -(Fraction a)
        {
            return new Fraction(-a.num, a.den);
        }

        public static Fraction operator +(Fraction a, Fraction b)
        {
            var newNum = a.num * b.den + b.num * a.den;
            var newDen = a.den * b.den;
            var gcd = Gcd(newNum, newDen);

            return new Fraction(newNum / gcd, newDen / gcd);
        }

        public static Fraction operator -(Fraction a, Fraction b)
        {
            return a + -b;
        }

        public static Fraction operator *(Fraction a, Fraction b)
        {
            var newNum = a.num * b.num;
            var newDen = a.den * b.den;
            var gcd = Gcd(newNum, newDen);

            return new Fraction(newNum / gcd, newDen / gcd);
        }

        public static Fraction operator /(Fraction a, Fraction b)
        {
            if (b.num == 0) throw new DivideByZeroException();

            var newNum = a.num * b.den;
            var newDen = a.den * b.num;
            var gcd = Gcd(newNum, newDen);

            return new Fraction(newNum / gcd, newDen / gcd);
        }

        public override string ToString()
        {
            return $"{num} / {den}";
        }

        public static explicit operator float(Fraction x)
        {
            return (float) x.num / x.den;
        }

        public static implicit operator double(Fraction x)
        {
            return (double) x.num / x.den;
        }
    }
}