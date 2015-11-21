using B2Lib.Enums;
using B2Lib.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace B2Lib.Tests
{
    [TestClass]
    public class FileTests
    {
        private static B2Client _client;
        private static B2Bucket _bucket;

        [ClassInitialize]
        public static void TestSetup(TestContext context)
        {
            _client = new B2Client();
            _client.Login(TestConfig.AccountId, TestConfig.ApplicationKey).Wait();

            _bucket = _client.CreateBucket("test-bucket-b2lib", B2BucketType.AllPrivate).Result;
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            // TODO: Delete all files
            _client.DeleteBucket(_bucket).Wait();
        }

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}