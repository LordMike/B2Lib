using System.Collections.Generic;
using B2Lib.Client;
using B2Lib.Enums;

namespace B2Lib.SyncExtensions
{
    public static class B2ClientExtensions
    {
        public static void Login(this B2Client client, string accountId, string applicationKey)
        {
            Utility.AsyncRunHelper(() => client.LoginAsync(accountId, applicationKey));
        }

        public static IEnumerable<B2Bucket> GetBuckets(this B2Client client)
        {
            return Utility.AsyncRunHelper(() => client.GetBucketsAsync());
        }

        public static B2Bucket CreateBucket(this B2Client client, string name, B2BucketType type)
        {
            return Utility.AsyncRunHelper(() => client.CreateBucketAsync(name, type));
        }

        public static B2Bucket GetBucketByName(this B2Client client, string name)
        {
            return Utility.AsyncRunHelper(() => client.GetBucketByNameAsync(name));
        }

        public static B2Bucket GetBucketById(this B2Client client, string id)
        {
            return Utility.AsyncRunHelper(() => client.GetBucketByIdAsync(id));
        }
    }
}