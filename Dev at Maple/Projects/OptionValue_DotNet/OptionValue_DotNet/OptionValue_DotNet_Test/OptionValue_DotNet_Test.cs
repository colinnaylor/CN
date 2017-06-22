using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OptionValue_DotNet;

namespace OptionValue_DotNet_Test
{
    /// <summary>
    /// Summary description for OptionValue_DotNet_WithoutDividends
    /// FURTHER TESTS NEEDED: European style, multiple divis, put with divs...
    /// </summary>
    [TestClass]
    public class OptionValue_DotNet_Test
    {
        public OptionValue_DotNet_Test()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CallWithoutDividends()
        {

            Option O = new Option()
            {
                Type = Option.OptionType.Call,
                Style = Option.OptionStyle.American,
                Strike = 0.9900d,
                Maturity = new DateTime(2009, 12, 15),
                UnderlyingPrice = 6.7060d,
                UnderlyingVolatility = 0.33622d,
                Rate = 0.0080000d
            };

            double expected = 5.717967330528360d;
            O.CalculateOptionValue(new DateTime(2009, 9, 15));
            Assert.AreEqual(expected, O.OptionValue, 0.000001);
        }

        [TestMethod]
        public void CallWithDividends()
        {

            Option O = new Option()
            {
                Type = Option.OptionType.Call,
                Style = Option.OptionStyle.American,
                Strike = 0.9900d,
                Maturity = new DateTime(2009, 12, 15),
                UnderlyingPrice = 6.7060d,
                UnderlyingVolatility = 0.33622d,
                Rate = 0.0080000d,                

            };

            O.UnderlyingDividends.Add(new Dividend(new DateTime(2009, 11, 18), 0.048224727));

            double expected = 5.717377542219590d;
            O.CalculateOptionValue(new DateTime(2009, 9, 15));

            Assert.AreEqual(expected, O.OptionValue, 0.000001);
        }

        [TestMethod]
        public void PutWithoutDividends()
        {

            Option O = new Option()
            {
                Type = Option.OptionType.Put,
                Style = Option.OptionStyle.American,
                Strike = 17.7500,
                Maturity = new DateTime(2009, 10, 30),
                UnderlyingPrice = 17.0600d,
                UnderlyingVolatility = 0.24d,
                Rate = 0.0108d
            };

            double expected = 0.877987185;
            O.CalculateOptionValue(new DateTime(2009, 10, 02));
            Assert.AreEqual(expected, O.OptionValue, 0.000001);
        }

    }
}
