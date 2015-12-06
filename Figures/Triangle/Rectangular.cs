using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Triangle
{
    public static class Rectangular
    {
        public static double Area(double cathetusA, double cathetusB, double hypotenuse)
        {
            if (cathetusA <= 0)
                throw new ArgumentException("cathetusA");

            if (cathetusB <= 0)
                throw new ArgumentException("cathetusB");

            if (hypotenuse <= 0)
                throw new ArgumentException("hypotenuse");

            if (hypotenuse.CompareTo(Math.Sqrt(cathetusA * cathetusA + cathetusB * cathetusB)) != 0)
                throw new ArgumentException("square of the hypotenuse equals the sum of the squares of the legs");

            var p = (cathetusA + cathetusB + hypotenuse) / 2;

            return (p - cathetusA) * (p - cathetusB);
        }
    }
}
