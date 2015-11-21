using System;
using System.Collections;
using System.Collections.Generic;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2FileVersionsIterator : IEnumerable<B2FileWithSize>
    {
        private readonly Uri _apiUri;
        private readonly string _authToken;
        private readonly string _bucketId;
        private readonly string _startName;
        private readonly string _startId;

        public int PageSize { get; set; } = 1000;

        public B2FileVersionsIterator(Uri apiUri, string authToken, string bucketId, string startName, string startId)
        {
            _apiUri = apiUri;
            _authToken = authToken;
            _bucketId = bucketId;
            _startName = startName;
            _startId = startId;
        }

        public IEnumerator<B2FileWithSize> GetEnumerator()
        {
            string currentStart = _startName;
            string currentStartId = _startId;

            while (true)
            {
                B2FileListContainer result = B2Communicator.ListFileVersions(_apiUri, _authToken, _bucketId, currentStart, currentStartId, PageSize).Result;

                currentStart = result.NextFileName;
                currentStartId = result.NextFileId;

                foreach (B2FileWithSize file in result.Files)
                {
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