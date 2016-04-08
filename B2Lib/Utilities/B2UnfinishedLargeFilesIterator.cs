using System.Collections.Generic;
using System.Linq;
using B2Lib.Client;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2UnfinishedLargeFilesIterator : B2BaseIterator<B2LargeFile>
    {
        private readonly B2Client _client;
        private readonly string _bucketId;
        private string _currentStartFileId;

        internal B2UnfinishedLargeFilesIterator(B2Client client, string bucketId, string startFileId)
            : base(client.Communicator)
        {
            _client = client;
            _bucketId = bucketId;
            _currentStartFileId = startFileId;
        }

        protected override List<B2LargeFile> GetNextPage(out bool isDone)
        {
            B2UnfinishedLargeFilesContainer result = Communicator.ListUnfinishedLargeFiles(_bucketId, _currentStartFileId, PageSize).Result;
            _currentStartFileId = result.NextFileId;

            isDone = _currentStartFileId == null;

            return result.Files.Select(s => new B2LargeFile(_client, s)).ToList();
        }
    }
}