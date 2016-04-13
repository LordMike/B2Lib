using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace B2Lib.Client
{
    public abstract class B2FileItemBase
    {
        public abstract string FileId { get; }
        public abstract string FileName { get; }
        public abstract DateTime UploadTimestamp { get; }
        public abstract string AccountId { get; }
        public abstract string BucketId { get; }
        public abstract string ContentType { get; }
        public abstract Dictionary<string, string> FileInfo { get; }

        public abstract Task<bool> DeleteAsync();
    }
}