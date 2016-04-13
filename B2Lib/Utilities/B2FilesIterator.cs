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
        private string _currentStartFile;

        internal B2FilesIterator(B2Client client, string bucketId, string startFileName)
            : base(client.Communicator)
        {
            _client = client;
            _bucketId = bucketId;
            _currentStartFile = startFileName;
        }

        protected override List<B2File> GetNextPage(out bool isDone)
        {
            B2FileListContainer result = Communicator.ListFiles(_bucketId, _currentStartFile, PageSize).Result;
            _currentStartFile = result.NextFileName;

            isDone = _currentStartFile == null;

            return result.Files.Select(s =>
            {
                s.AccountId = _client.AccountId;
                s.BucketId = _bucketId;

                return new B2File(_client, s);
            }).ToList();
        }
    }
}