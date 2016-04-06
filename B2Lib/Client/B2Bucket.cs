using System;
using System.Threading.Tasks;
using B2Lib.Enums;
using B2Lib.Objects;
using B2Lib.Utilities;

namespace B2Lib.Client
{
    public class B2Bucket
    {
        private readonly B2Client _b2Client;
        private readonly B2BucketObject _bucket;

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
                throw new InvalidOperationException($"The B2 Bucket, {_bucket.BucketName}, was {State} and not {desiredState} (id: {_bucket.BucketId})");
        }

        public async Task<bool> DeleteAsync()
        {
            ThrowIfNot(B2BucketState.Present);

            B2BucketObject result = await _b2Client.Communicator.DeleteBucket(_b2Client.ApiUrl, _bucket.AccountId, _bucket.BucketId);
            State = B2BucketState.Deleted;

            _b2Client.BucketCache.RemoveBucket(_bucket);

            return true;
        }

        public async Task<bool> UpdateAsync(B2BucketType newType)
        {
            ThrowIfNot(B2BucketState.Present);

            B2BucketObject result = await _b2Client.Communicator.UpdateBucket(_b2Client.ApiUrl, _bucket.AccountId, _bucket.BucketId, newType);

            _bucket.BucketType = result.BucketType;
            return true;
        }

        public B2File CreateFile(string newName)
        {
            ThrowIfNot(B2BucketState.Present);

            return new B2File(_b2Client, _bucket.BucketId, newName);
        }

        public B2FilesIterator GetFiles()
        {
            ThrowIfNot(B2BucketState.Present);

            return new B2FilesIterator(_b2Client, _bucket.BucketId, null);
        }

        public B2FileVersionsIterator GetFileVersions()
        {
            ThrowIfNot(B2BucketState.Present);

            return new B2FileVersionsIterator(_b2Client, _bucket.BucketId, null, null);
        }

        public async Task<bool> HideFileAsync(string fileName)
        {
            ThrowIfNot(B2BucketState.Present);

            B2FileBase result = await _b2Client.Communicator.HideFile(_b2Client.ApiUrl, _bucket.BucketId, fileName);

            return true;
        }
    }
}