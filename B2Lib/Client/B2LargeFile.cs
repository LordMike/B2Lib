using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using B2Lib.Enums;
using B2Lib.Objects;
using B2Lib.Utilities;

namespace B2Lib.Client
{
    public class B2LargeFile : B2FileItemBase
    {
        private readonly B2Client _b2Client;
        private B2UnfinishedLargeFile _file;

        public B2LargeFileState State { get; private set; }

        public override string FileId => _file.FileId;
        public override string FileName => _file.FileName;
        public override DateTime UploadTimestamp => _file.UploadTimestamp;
        public override string AccountId => _file.AccountId;
        public override string BucketId => _file.BucketId;
        public override string ContentType => _file.ContentType;
        public override Dictionary<string, string> FileInfo => _file.FileInfo;

        internal B2LargeFile(B2Client b2Client, B2UnfinishedLargeFile file)
        {
            _file = file;
            _b2Client = b2Client;

            State = B2LargeFileState.New;
        }

        internal B2LargeFile(B2Client b2Client, B2FileInfo file)
        {
            _file = new B2UnfinishedLargeFile
            {
                AccountId = file.AccountId,
                BucketId = file.BucketId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileId = file.FileId,
                FileInfo = file.FileInfo,
                UploadTimestamp = file.UploadTimestamp
            };
            _b2Client = b2Client;

            State = B2LargeFileState.New;
        }

        private void ThrowIfNot(B2LargeFileState desiredState)
        {
            if (State != desiredState)
                throw new InvalidOperationException($"The B2 Large File, {FileName}, was {State} and not {desiredState} (id: {FileId})");
        }

        private B2LargeFilePartsIterator GetParts()
        {
            ThrowIfNot(B2LargeFileState.New);

            return new B2LargeFilePartsIterator(_b2Client.Communicator, FileId);
        }

        public async Task UploadPartAsync(int partNumber, Stream source, string sha1Hash)
        {
            ThrowIfNot(B2LargeFileState.New);

            B2UploadPartConfiguration config = _b2Client.FetchLargeFileUploadConfig(FileId);

            try
            {
                B2LargeFilePart result = await _b2Client.Communicator.UploadPart(config.UploadUrl, config.AuthorizationToken, partNumber, source, sha1Hash);

                if (result.ContentSha1 != sha1Hash)
                    throw new ArgumentException("Bad transfer - hash mismatch");
            }
            finally
            {
                _b2Client.ReturnUploadConfig(config);
            }
        }

        public async Task<B2File> FinishAcceptHashesAsync()
        {
            ThrowIfNot(B2LargeFileState.New);

            // Fetch parts from B2
            List<string> partHashes = GetParts().OrderBy(s => s.PartNumber).Select(s => s.ContentSha1).ToList();

            // Use the parts hashes to complete the file. Note we automatically accept any errors in transfer.
            B2File result = await FinishAsync(partHashes);

            _b2Client.MarkLargeFileDone(FileId);

            return result;
        }

        public async Task<B2File> FinishAsync(List<string> sha1Hashes)
        {
            ThrowIfNot(B2LargeFileState.New);

            B2FileInfo result = await _b2Client.Communicator.FinishLargeFileUpload(FileId, sha1Hashes);

            State = B2LargeFileState.Finished;

            _b2Client.MarkLargeFileDone(FileId);

            return new B2File(_b2Client, result);
        }

        public override async Task<bool> DeleteAsync()
        {
            ThrowIfNot(B2LargeFileState.New);

            bool res = await _b2Client.Communicator.DeleteFile(FileName, FileId);
            State = B2LargeFileState.Deleted;

            _b2Client.MarkLargeFileDone(FileId);

            return res;
        }

    }
}