using System.Collections.Generic;

namespace B2Lib.Objects
{
    public class B2UnfinishedLargeFile: B2FileBase
    {
        public string AccountId { get; set; }

        public string BucketId { get; set; }

        public string ContentType { get; set; }

        public Dictionary<string, string> FileInfo { get; set; }
    }
}