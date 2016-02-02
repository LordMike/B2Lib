using System;
using System.Collections;
using System.Collections.Generic;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2FilesIterator : IEnumerable<B2FileWithSize>
    {
        private readonly B2Communicator _communicator;
        private readonly Uri _apiUri;
        private readonly string _bucketId;
        private readonly string _startName;

        public int PageSize { get; set; } = 100;

        public B2FilesIterator(B2Communicator communicator, Uri apiUri, string bucketId, string startName)
        {
            _communicator = communicator;
            _apiUri = apiUri;
            _bucketId = bucketId;
            _startName = startName;
        }

        public IEnumerator<B2FileWithSize> GetEnumerator()
        {
            string currentStart = _startName;

            while (true)
            {
                B2FileListContainer result = _communicator.ListFiles(_apiUri, _bucketId, currentStart, PageSize).Result;

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