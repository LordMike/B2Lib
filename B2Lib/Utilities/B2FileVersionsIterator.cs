using System;
using System.Collections;
using System.Collections.Generic;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2FileVersionsIterator : IEnumerable<B2FileWithSize>
    {
        private readonly B2Communicator _communicator;
        private readonly Uri _apiUri;
        private readonly string _bucketId;
        private readonly string _startName;
        private readonly string _startId;

        public int PageSize { get; set; } = 1000;

        public B2FileVersionsIterator(B2Communicator communicator, Uri apiUri, string bucketId, string startName, string startId)
        {
            _communicator = communicator;
            _apiUri = apiUri;
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
                B2FileListContainer result = _communicator.ListFileVersions(_apiUri, _bucketId, currentStart, currentStartId, PageSize).Result;

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