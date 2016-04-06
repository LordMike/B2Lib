using System;
using System.Collections;
using System.Collections.Generic;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2FileVersionsIterator : IEnumerable<B2FileInfo>
    {
        private readonly B2Communicator _communicator;
        private readonly Uri _apiUri;
        private readonly string _accountId;
        private readonly string _bucketId;
        private readonly string _startName;
        private readonly string _startId;

        public int PageSize { get; set; } = 1000;

        internal B2FileVersionsIterator(B2Communicator communicator, Uri apiUri, string accountId, string bucketId, string startName, string startId)
        {
            _communicator = communicator;
            _apiUri = apiUri;
            _accountId = accountId;
            _bucketId = bucketId;
            _startName = startName;
            _startId = startId;
        }

        public IEnumerator<B2FileInfo> GetEnumerator()
        {
            string currentStart = _startName;
            string currentStartId = _startId;

            while (true)
            {
                B2FileListContainer result = _communicator.ListFileVersions(_apiUri, _bucketId, currentStart, currentStartId, PageSize).Result;

                currentStart = result.NextFileName;
                currentStartId = result.NextFileId;

                foreach (B2FileInfo file in result.Files)
                {
                    file.BucketId = _bucketId;
                    file.AccountId = _accountId;

                    yield return file;
                }

                if (string.IsNullOrEmpty(currentStart) && string.IsNullOrEmpty(currentStartId))
                    yield break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}