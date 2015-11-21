using System;
using B2Lib.Enums;

namespace B2Lib.Objects
{
    public class B2FileBase
    {
        public B2FileAction Action { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
        public long UploadTimestamp { get; set; }

        public DateTime UploadTimestampDate => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(UploadTimestamp);

        public override string ToString()
        {
            return FileName;
        }
    }
}