using System;
using System.Collections;
using System.Collections.Generic;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2FilesIterator : IEnumerable<B2FileWithSize>
    {
        private readonly Uri _apiUri;
        private readonly string _authToken;
        private readonly string _bucketId;
        private readonly string _startName;

        public int PageSize { get; set; } = 100;

        public B2FilesIterator(Uri apiUri, string authToken, string bucketId, string startName)
        {
            _apiUri = apiUri;
            _authToken = authToken;
            _bucketId = bucketId;
            _startName = startName;
        }

        public IEnumerator<B2FileWithSize> GetEnumerator()
        {
            string currentStart = _startName;

            while (true)
            {
                B2FileListContainer result = B2Communicator.ListFiles(_apiUri, _authToken, _bucketId, currentStart, PageSize).Result;

                currentStart = result.NextFileName;

                foreach (B2FileWithSize file in result.Files)
                {
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