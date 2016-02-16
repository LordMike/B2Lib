using System;
using System.Collections.Generic;

namespace B2Lib.Objects
{
    public class B2LargeFileResponse
    {
        public string FileId { get; set; }
        
        public string FileName { get; set; }

        public string AccountId { get; set; }

        public string BucketId { get; set; }

        public string ContentType { get; set; }

        public Dictionary<string,string> FileInfo { get; set; }

        public string UploadAuthToken { get; set; }

        public List<Uri> UploadUrls { get; set; }

    }
}