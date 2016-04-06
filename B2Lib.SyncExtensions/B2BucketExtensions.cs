using B2Lib.Client;
using B2Lib.Enums;

namespace B2Lib.SyncExtensions
{
    public static class B2BucketExtensions
    {
        public static bool Delete(this B2BucketV2 bucket)
        {
            return Utility.AsyncRunHelper(bucket.DeleteAsync);
        }

        public static bool Update(this B2BucketV2 bucket, B2BucketType newType)
        {
            return Utility.AsyncRunHelper(() => bucket.UpdateAsync(newType));
        }


        public static bool HideFile(this B2BucketV2 bucket, string fileName)
        {
            return Utility.AsyncRunHelper(() => bucket.HideFileAsync(fileName));
        }
    }
}