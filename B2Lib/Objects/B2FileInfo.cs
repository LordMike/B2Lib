using System.Collections.Generic;

namespace B2Lib.Objects
{
    public class B2FileInfo : B2FileBase
    {
        public string AccountId { get; set; }

        public string ContentSha1 { get; set; }

        public string BucketId { get; set; }

        public long ContentLength { get; set; }

        public string ContentType { get; set; }

        public Dictionary<string, string> FileInfo { get; set; }

        // Hack to cover B2's different interpretations of a File.
        public long Size
        {
            get { return ContentLength; }
            set { ContentLength = value; }
        }
    }
}