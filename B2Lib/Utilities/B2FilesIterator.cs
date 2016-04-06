using System.Collections.Generic;
using System.Linq;
using B2Lib.Client;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2FilesIterator : B2BaseIterator<B2File>
    {
        private readonly B2Client _client;
        private readonly string _bucketId;
        private string _currentStart;

        internal B2FilesIterator(B2Client client, string bucketId, string startName) :
            base(client.Communicator, client.ApiUrl)
        {
            _client = client;
            _bucketId = bucketId;
            _currentStart = startName;
        }

        protected override List<B2File> GetNextPage(out bool isDone)
        {
            B2FileListContainer result = Communicator.ListFiles(ApiUri, _bucketId, _currentStart, PageSize).Result;
            _currentStart = result.NextFileName;

            isDone = _currentStart == null;

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