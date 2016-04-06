using System;
using B2Lib.Enums;
using B2Lib.Utilities;
using Newtonsoft.Json;

namespace B2Lib.Objects
{
    public class B2FileBase
    {
        public B2FileAction Action { get; set; }

        public string FileId { get; set; }

        public string FileName { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime UploadTimestamp { get; set; }

        public override string ToString()
        {
            return FileName;
        }
    }
}