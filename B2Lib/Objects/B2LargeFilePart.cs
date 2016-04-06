using System;
using B2Lib.Utilities;
using Newtonsoft.Json;

namespace B2Lib.Objects
{
    public class B2LargeFilePart
    {
        public string FileId { get; set; }

        public int PartNumber { get; set; }

        public long ContentLength { get; set; }

        public string ContentSha1 { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime UploadTimestamp { get; set; }
    }
}