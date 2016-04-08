using System.Collections.Generic;
using System.Linq;
using B2Lib.Client;
using B2Lib.Enums;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2FileVersionsIterator : B2BaseIterator<B2FileItemBase>
    {
        private readonly B2Client _client;
        private readonly string _bucketId;
        private string _currentStartFile;
        private string _currentStartFileId;

        internal B2FileVersionsIterator(B2Client client, string bucketId, string startFileName, string startFileId)
            : base(client.Communicator)
        {
            _client = client;
            _bucketId = bucketId;
            _currentStartFile = startFileName;
            _currentStartFileId = startFileId;
        }

        protected override List<B2FileItemBase> GetNextPage(out bool isDone)
        {
            B2FileListContainer result = Communicator.ListFileVersions(_bucketId, _currentStartFile, _currentStartFileId, PageSize).Result;
            _currentStartFile = result.NextFileName;
            _currentStartFileId = result.NextFileId;

            isDone = _currentStartFile == null && _currentStartFileId == null;

            return result.Files.Select(s =>
            {
                s.AccountId = _client.AccountId;
                s.BucketId = _bucketId;

                if (s.Action == B2FileAction.Start)
                    return new B2LargeFile(_client, s);

                return (B2FileItemBase)new B2File(_client, s);
            }).ToList();
        }
    }
}