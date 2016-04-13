using System.Collections.Generic;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2LargeFilePartsIterator : B2BaseIterator<B2LargeFilePart>
    {
        private readonly string _fileId;
        private int _currentStartPartNumber;

        internal B2LargeFilePartsIterator(B2Communicator communicator, string fileId, int startNumber = 1)
            : base(communicator)
        {
            _fileId = fileId;
            _currentStartPartNumber = startNumber;
        }

        protected override List<B2LargeFilePart> GetNextPage(out bool isDone)
        {
            B2LargeFilePartsContainer result = Communicator.ListLargeFileParts(_fileId, _currentStartPartNumber, PageSize).Result;
            _currentStartPartNumber = result.NexPartNumber;

            isDone = _currentStartPartNumber == 0;

            return result.Parts;
        }
    }
}