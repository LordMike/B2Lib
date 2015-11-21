using System.Collections.Generic;

namespace B2Lib.Objects
{
    public class B2FileListContainer
    {
        public List<B2FileWithSize> Files { get; set; }

        public string NextFileName { get; set; }

        public string NextFileId { get; set; }
    }
}