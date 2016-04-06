using B2Lib.Enums;

namespace B2Lib.Objects
{
    public class B2BucketObject
    {
        public string BucketId { get; set; }
        public string AccountId { get; set; }
        public string BucketName { get; set; }
        public B2BucketType BucketType { get; set; }

        public override string ToString()
        {
            return BucketName + " (" + BucketId + " / " + BucketType + ")";
        }
    }
}