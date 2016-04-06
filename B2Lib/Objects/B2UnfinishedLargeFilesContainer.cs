using System.Collections.Generic;

namespace B2Lib.Objects
{
    public class B2UnfinishedLargeFilesContainer
    {
        public List<B2UnfinishedLargeFile> Files { get; set; }

        public string NextFileId { get; set; }
    }
}