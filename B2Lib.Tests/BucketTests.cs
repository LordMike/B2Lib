using System.Collections.Generic;
using System.Linq;
using B2Lib.Client;
using B2Lib.Enums;
using B2Lib.SyncExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace B2Lib.Tests
{
    [TestClass]
    public class BucketTests
    {
        [TestMethod]
        public void TestBucketCreateUpdateDelete()
        {
            const string name = "test-bucket-b2lib";

            B2Client client = new B2Client();
            client.Login(TestConfig.AccountId, TestConfig.ApplicationKey);

            // Create
            B2BucketV2 bucket = client.CreateBucket(name, B2BucketType.AllPrivate);
            TestBucketContents(bucket, B2BucketType.AllPrivate);

            // Verify using a list
            VerifyBucketInList(client, true, name, B2BucketType.AllPrivate);

            // Update
            bucket.Update(B2BucketType.AllPublic);
            TestBucketContents(bucket, B2BucketType.AllPublic);

            // Verify using a list
            VerifyBucketInList(client, true, name, B2BucketType.AllPublic);

            // Delete
            bucket.Delete();
            TestBucketContents(bucket, B2BucketType.AllPublic);

            // Verify using a list
            VerifyBucketInList(client, false, name, B2BucketType.AllPrivate);
        }

        private void VerifyBucketInList(B2Client client, bool shouldExist, string bucketName, B2BucketType expectedType)
        {
            IEnumerable<B2BucketV2> list = client.GetBuckets();

            bool exists = list.Any(s => s.BucketName == bucketName && s.BucketType == expectedType);
            if (shouldExist)
                Assert.IsTrue(exists);
            else
                Assert.IsFalse(exists);
        }

        private void TestBucketContents(B2BucketV2 bucket, B2BucketType expectedType)
        {
            Assert.IsNotNull(bucket);
            Assert.IsFalse(string.IsNullOrWhiteSpace(bucket.AccountId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(bucket.BucketId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(bucket.BucketName));
            Assert.AreEqual(expectedType, bucket.BucketType);
        }
    }
}