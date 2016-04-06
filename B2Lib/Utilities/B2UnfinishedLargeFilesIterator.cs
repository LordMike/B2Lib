using System;
using System.Collections.Generic;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2UnfinishedLargeFilesIterator : B2BaseIterator<B2UnfinishedLargeFile>
    {
        private readonly string _bucketId;
        private string _currentStartFileId;

        internal B2UnfinishedLargeFilesIterator(B2Communicator communicator, Uri apiUri, string bucketId, string startFileId)
            : base(communicator, apiUri)
        {
            _bucketId = bucketId;
            _currentStartFileId = startFileId;
        }

        protected override List<B2UnfinishedLargeFile> GetNextPage()
        {
            B2UnfinishedLargeFilesContainer result = Communicator.ListUnfinishedLargeFiles(ApiUri, _bucketId, _currentStartFileId, PageSize).Result;
            _currentStartFileId = result.NextFileId;

            return result.Files;
        }

        protected override void PreProcessItem(B2UnfinishedLargeFile item)
        {

        }
    }
}