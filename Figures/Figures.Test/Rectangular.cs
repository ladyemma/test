using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Figures.Test
{
    [TestClass]
    public class Rectangular
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FirstArgumentExceptionTest()
        {
            Triangle.Rectangular.Area(0, new Random().Next(1, 100), new Random().Next(1, 100));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SecondArgumentExceptionTest()
        {
            Triangle.Rectangular.Area(new Random().Next(1, 100), 0, new Random().Next(1, 100));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HypotenuseArgumentExceptionTest()
        {
            Triangle.Rectangular.Area(new Random().Next(1, 100), new Random().Next(1, 100), 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PositiveHypotenuseArgumentExceptionTest()
        {
            Triangle.Rectangular.Area(new Random().Next(1, 100), new Random().Next(1, 100), 5);
        }
        [TestMethod]
        public void AreaByCatetusTest()
        {
            var a = new Random().Next(1, 100);
            var b = new Random().Next(1, 100);
            var h = Math.Sqrt(a * a + b * b);

            var s = Triangle.Rectangular.Area(a, b, h);
            var s2 = 0.5 * a * b;

            Assert.AreEqual(Math.Round(s2, 2), Math.Round(s, 2));
        }

        [TestMethod]
        public void AreaBySidesTest()
        {
            var a = new Random().Next(1, 100);
            var b = new Random().Next(1, 100);
            var h = Math.Sqrt(a*a+b*b);

            var p = (a + b + h) / 2;
            var s = (p - a) * (p - b);

            Assert.AreEqual(Triangle.Rectangular.Area(a, b, h), s);
        }
    }
}
