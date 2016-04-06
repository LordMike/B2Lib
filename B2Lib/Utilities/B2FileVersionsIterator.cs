using System;
using System.Collections.Generic;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2FileVersionsIterator : B2BaseIterator<B2FileInfo>
    {
        private readonly string _accountId;
        private readonly string _bucketId;
        private string _currentStartName;
        private string _currentStartId;
        
        internal B2FileVersionsIterator(B2Communicator communicator, Uri apiUri, string accountId, string bucketId, string startName, string startId)
            : base(communicator,apiUri)
        {
            _accountId = accountId;
            _bucketId = bucketId;
            _currentStartName = startName;
            _currentStartId = startId;
        }
        
        protected override List<B2FileInfo> GetNextPage()
        {
            B2FileListContainer result = Communicator.ListFileVersions(ApiUri, _bucketId, _currentStartName, _currentStartId, PageSize).Result;

            _currentStartName = result.NextFileName;
            _currentStartId = result.NextFileId;

            return result.Files;
        }

        protected override void PreProcessItem(B2FileInfo item)
        {
            item.BucketId = _bucketId;
            item.AccountId = _accountId;
        }
    }
}