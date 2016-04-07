using System.Collections.Generic;
using System.Linq;
using B2Lib.Client;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2FileVersionsIterator : B2BaseIterator<B2File>
    {
        private readonly B2Client _client;
        private readonly string _bucketId;
        private string _currentStart;
        private string _currentStartId;

        internal B2FileVersionsIterator(B2Client client, string bucketId, string startName, string startId) :
            base(client.Communicator)
        {
            _client = client;
            _bucketId = bucketId;
            _currentStart = startName;
            _currentStartId = startId;
        }

        protected override List<B2File> GetNextPage(out bool isDone)
        {
            B2FileListContainer result = Communicator.ListFileVersions(_bucketId, _currentStart, _currentStartId, PageSize).Result;
            _currentStart = result.NextFileName;
            _currentStartId = result.NextFileId;

            isDone = _currentStart == null && _currentStartId == null;

            return result.Files.Select(s =>
            {
                s.AccountId = _client.AccountId;
                s.BucketId = _bucketId;

                return new B2File(_client, s);
            }).ToList();
        }

        protected override void PreProcessItem(B2File item)
        {
        }
    }
}