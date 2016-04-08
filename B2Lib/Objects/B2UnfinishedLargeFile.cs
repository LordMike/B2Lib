using System;
using System.Collections.Generic;
using B2Lib.Utilities;
using Newtonsoft.Json;

namespace B2Lib.Objects
{
    public class B2UnfinishedLargeFile
    {
        public string FileId { get; set; }

        public string FileName { get; set; }

        public string AccountId { get; set; }

        public string BucketId { get; set; }

        public string ContentType { get; set; }

        public Dictionary<string, string> FileInfo { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime UploadTimestamp { get; set; }
    }
}