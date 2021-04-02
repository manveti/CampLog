using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestCalendarSpecs {
        [TestMethod]
        public void test_specs_none() {
            Assert.IsNotNull(CalendarSpecs.specs);
            Assert.IsTrue(CalendarSpecs.specs.ContainsKey("None"));
        }
    }


    [TestClass]
    public class TestCalendar {
        [TestMethod]
        public void test_format_timestamp() {
            Calendar cal = new Calendar();
            Assert.AreEqual(cal.format_timestamp(0), "0");
            Assert.AreEqual(cal.format_timestamp(42), "42");
            Assert.AreEqual(cal.format_timestamp(-3), "-3");
        }

        [TestMethod]
        public void test_format_interval() {
            Calendar cal = new Calendar();
            Assert.AreEqual(cal.format_interval(0), "0");
            Assert.AreEqual(cal.format_interval(86400), "86400");
            Assert.AreEqual(cal.format_interval(-25), "-25");
        }
    }


    [TestClass]
    public class TestCalendarFactory {
        [TestMethod]
        public void test_default_parameters() {
            CalendarFactory fact = new CalendarFactory();
            Assert.IsNull(fact.default_parameters());
        }

        [TestMethod]
        public void test_get_calendar() {
            CalendarFactory fact = new CalendarFactory();
            Calendar cal = fact.get_calendar(null);
            Assert.IsNotNull(cal);
            Assert.AreEqual(cal.format_timestamp(123456789), "123456789");
            Assert.AreEqual(cal.format_interval(86400), "86400");
        }
    }
}