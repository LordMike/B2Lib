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
        private readonly ConcurrentBag<B2UploadPartConfiguration> _partUploadConfigs;
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

            _partUploadConfigs = new ConcurrentBag<B2UploadPartConfiguration>();
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

            _partUploadConfigs = new ConcurrentBag<B2UploadPartConfiguration>();
            State = B2LargeFileState.New;
        }

        private void ThrowIfNot(B2LargeFileState desiredState)
        {
            if (State != desiredState)
                throw new InvalidOperationException($"The B2 Large File, {_file.FileName}, was {State} and not {desiredState} (id: {_file.FileId})");
        }

        private void ReturnUploadConfig(B2UploadPartConfiguration config)
        {
            _partUploadConfigs.Add(config);
        }

        private B2UploadPartConfiguration FetchUploadConfig()
        {
            B2UploadPartConfiguration config;
            if (_partUploadConfigs.TryTake(out config))
                return config;

            // Create new config
            B2UploadPartConfiguration res;
            try
            {
                res = _b2Client.Communicator.GetPartUploadUrl(_file.FileId).Result;
            }
            catch (AggregateException ex)
            {
                // Re-throw the inner exception
                throw ex.InnerException;
            }

            return res;
        }

        private B2LargeFilePartsIterator GetParts()
        {
            ThrowIfNot(B2LargeFileState.New);

            return new B2LargeFilePartsIterator(_b2Client.Communicator, _file.FileId);
        }

        public async Task UploadPartAsync(int partNumber, Stream source, string sha1Hash)
        {
            ThrowIfNot(B2LargeFileState.New);

            B2UploadPartConfiguration config = FetchUploadConfig();

            try
            {
                B2LargeFilePart result = await _b2Client.Communicator.UploadPart(config.UploadUrl, config.AuthorizationToken, partNumber, source, sha1Hash);

                if (result.ContentSha1 != sha1Hash)
                    throw new ArgumentException("Bad transfer - hash mismatch");
            }
            finally
            {
                ReturnUploadConfig(config);
            }
        }

        public async Task<B2File> FinishAcceptHashesAsync()
        {
            ThrowIfNot(B2LargeFileState.New);

            // Fetch parts from B2
            List<string> partHashes = GetParts().OrderBy(s => s.PartNumber).Select(s => s.ContentSha1).ToList();

            // Use the parts hashes to complete the file. Note we automatically accept any errors in transfer.
            return await FinishAsync(partHashes);
        }

        public async Task<B2File> FinishAsync(List<string> sha1Hashes)
        {
            // TODO: Determine if the file is finished already?
            B2FileInfo result = await _b2Client.Communicator.FinishLargeFileUpload(_file.FileId, sha1Hashes);

            State = B2LargeFileState.Finished;

            return new B2File(_b2Client, result);
        }
        
        public override async Task<bool> DeleteAsync()
        {
            ThrowIfNot(B2LargeFileState.New);

            bool res = await _b2Client.Communicator.DeleteFile(_file.FileName, _file.FileId);
            State = B2LargeFileState.Deleted;

            return res;
        }

    }
}