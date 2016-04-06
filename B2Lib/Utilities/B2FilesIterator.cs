using System;
using System.Collections;
using System.Collections.Generic;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2FilesIterator : IEnumerable<B2FileInfo>
    {
        private readonly B2Communicator _communicator;
        private readonly Uri _apiUri;
        private readonly string _accountId;
        private readonly string _bucketId;
        private readonly string _startName;

        public int PageSize { get; set; } = 1000;

        internal B2FilesIterator(B2Communicator communicator, Uri apiUri, string accountId, string bucketId, string startName)
        {
            _communicator = communicator;
            _apiUri = apiUri;
            _accountId = accountId;
            _bucketId = bucketId;
            _startName = startName;
        }

        public IEnumerator<B2FileInfo> GetEnumerator()
        {
            string currentStart = _startName;

            while (true)
            {
                B2FileListContainer result = _communicator.ListFiles(_apiUri, _bucketId, currentStart, PageSize).Result;

                currentStart = result.NextFileName;

                foreach (B2FileInfo file in result.Files)
                {
                    file.BucketId = _bucketId;
                    file.AccountId = _accountId;

                    yield return file;
                }

                if (string.IsNullOrEmpty(currentStart))
                    yield break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}