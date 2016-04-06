using System;
using B2Lib.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace B2Lib.Tests
{
    class TestClass1
    {
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTimeOffset UtcTime { get; set; }
    }

    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void TestUnixDateTimeConverter()
        {
            DateTimeOffset testDate = new DateTimeOffset(DateTime.UtcNow).ToOffset(TimeSpan.FromHours(12));

            TestClass1 c1 = new TestClass1 { UtcTime = testDate };
            TestClass1 c2 = JsonConvert.DeserializeObject<TestClass1>(JsonConvert.SerializeObject(c1));

            Assert.AreEqual(testDate.UtcDateTime, c2.UtcTime);
        }
    }
}