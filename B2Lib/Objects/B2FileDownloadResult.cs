using System.Collections.Generic;

namespace B2Lib.Objects
{
    public class B2FileDownloadResult
    {
        public long ContentLength { get; set; }
        public string ContentType { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
        public string ContentSha1 { get; set; }

        public Dictionary<string, string> FileInfo { get; set; }
    }
}