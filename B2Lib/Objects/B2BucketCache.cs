using B2Lib.Enums;

namespace B2Lib.Objects
{
    public class B2BucketCache
    {
        public string BucketId { get; set; }

        public string BucketName { get; set; }

        public B2BucketType BucketType { get; set; }

        public string AccountId { get; set; }

        internal B2BucketCache CreateCopy()
        {
            return new B2BucketCache
            {
                BucketId = BucketId,
                BucketName = BucketName,
                BucketType =  BucketType,
                AccountId = AccountId
            };
        }

        internal B2Bucket CreateBucketClass()
        {
            return new B2Bucket
            {
                AccountId = AccountId,
                BucketId = BucketId,
                BucketName = BucketName,
                BucketType = BucketType
            };
        }

        public override string ToString()
        {
            return BucketName;
        }
    }
}