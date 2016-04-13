using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using B2Lib.Enums;
using B2Lib.Exceptions;
using B2Lib.Objects;
using B2Lib.Utilities;

namespace B2Lib.Client
{
    public class B2Bucket
    {
        private readonly B2Client _b2Client;
        private B2BucketObject _bucket;

        public string BucketId => _bucket.BucketId;
        public string AccountId => _bucket.AccountId;
        public string BucketName => _bucket.BucketName;
        public B2BucketType BucketType => _bucket.BucketType;

        public B2BucketState State { get; private set; }

        internal B2Bucket(B2Client b2Client, B2BucketObject bucket)
        {
            _b2Client = b2Client;
            _bucket = bucket;

            State = B2BucketState.Present;
        }

        private void ThrowIfNot(B2BucketState desiredState)
        {
            if (State != desiredState)
                throw new InvalidOperationException($"The B2 Bucket, {BucketName}, was {State} and not {desiredState} (id: {BucketId})");
        }

        public async Task<bool> DeleteAsync()
        {
            ThrowIfNot(B2BucketState.Present);

            B2BucketObject result = await _b2Client.Communicator.DeleteBucket(AccountId, BucketId);
            State = B2BucketState.Deleted;

            _b2Client.BucketCache.RemoveBucket(_bucket);
            _bucket = result;

            return true;
        }

        public async Task<bool> UpdateAsync(B2BucketType newType)
        {
            ThrowIfNot(B2BucketState.Present);

            B2BucketObject result = await _b2Client.Communicator.UpdateBucket(AccountId, BucketId, newType);
            _bucket = result;

            return true;
        }

        public B2File CreateFile(string newName)
        {
            ThrowIfNot(B2BucketState.Present);

            return new B2File(_b2Client, BucketId, newName);
        }

        public async Task<B2LargeFile> CreateLargeFileAsync(string newName, string contentType = null, Dictionary<string, string> fileInfo = null)
        {
            ThrowIfNot(B2BucketState.Present);

            B2UnfinishedLargeFile result = await _b2Client.Communicator.StartLargeFileUpload(BucketId, newName, contentType, fileInfo);

            return new B2LargeFile(_b2Client, result);
        }

        public async Task<B2File> GetFileAsync(string fileId)
        {
            ThrowIfNot(B2BucketState.Present);

            B2FileInfo info = await _b2Client.Communicator.GetFileInfo(fileId);

            return new B2File(_b2Client, info);
        }

        public async Task<B2LargeFile> GetLargeFileAsync(string fileId)
        {
            ThrowIfNot(B2BucketState.Present);

            B2UnfinishedLargeFilesIterator iterator = new B2UnfinishedLargeFilesIterator(_b2Client, BucketId, fileId)
            {
                PageSize = 1
            };

            B2LargeFile largeFile = await Task.Run(() => iterator.FirstOrDefault());

            if (largeFile == null)
                throw new B2Exception("file_state_none") { ErrorCode = "not_found", HttpStatusCode = HttpStatusCode.NotFound };

            return largeFile;
        }

        public B2FilesIterator GetFiles(string startFileName = null)
        {
            ThrowIfNot(B2BucketState.Present);

            return new B2FilesIterator(_b2Client, BucketId, startFileName);
        }

        public B2FileVersionsIterator GetFileVersions(string startFileName = null, string startFileId = null)
        {
            ThrowIfNot(B2BucketState.Present);

            return new B2FileVersionsIterator(_b2Client, BucketId, startFileName, startFileId);
        }

        public B2UnfinishedLargeFilesIterator GetUnfinishedLargeFiles(string startFileId = null)
        {
            ThrowIfNot(B2BucketState.Present);

            return new B2UnfinishedLargeFilesIterator(_b2Client, BucketId, startFileId);
        }

        public async Task<bool> HideFileAsync(string fileName)
        {
            ThrowIfNot(B2BucketState.Present);

            B2FileBase result = await _b2Client.Communicator.HideFile(BucketId, fileName);

            return true;
        }
    }
}