using System.Collections.Generic;

namespace B2Lib.Objects
{
    public class B2LargeFilePartsContainer
    {
        public List<B2LargeFilePart> Parts { get; set; }

        public int NexPartNumber { get; set; }
    }
}