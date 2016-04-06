using System;
using System.Collections.Generic;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2LargeFilePartsIterator : B2BaseIterator<B2LargeFilePart>
    {
        private readonly string _bucketId;
        private int _currentStartPartNumber;

        internal B2LargeFilePartsIterator(B2Communicator communicator, Uri apiUri, string bucketId, int startNumber)
            : base(communicator, apiUri)
        {
            _bucketId = bucketId;
            _currentStartPartNumber = startNumber;
        }

        protected override List<B2LargeFilePart> GetNextPage(out bool isDone)
        {
            B2LargeFilePartsContainer result = Communicator.ListLargeFileParts(ApiUri, _bucketId, _currentStartPartNumber, PageSize).Result;
            _currentStartPartNumber = result.NexPartNumber;

            isDone = _currentStartPartNumber == 0;

            return result.Parts;
        }

        protected override void PreProcessItem(B2LargeFilePart item)
        {

        }
    }
}