using System;
using System.Collections.Generic;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2FilesIterator : B2BaseIterator<B2FileInfo>
    {
        private readonly string _accountId;
        private readonly string _bucketId;
        private string _currentStart;

        internal B2FilesIterator(B2Communicator communicator, Uri apiUri, string accountId, string bucketId, string startName) :
            base(communicator, apiUri)
        {
            _accountId = accountId;
            _bucketId = bucketId;
            _currentStart = startName;
        }

        protected override List<B2FileInfo> GetNextPage()
        {
            B2FileListContainer result = Communicator.ListFiles(ApiUri, _bucketId, _currentStart, PageSize).Result;
            _currentStart = result.NextFileName;

            return result.Files;
        }

        protected override void PreProcessItem(B2FileInfo item)
        {
            item.BucketId = _bucketId;
            item.AccountId = _accountId;
        }
    }
}