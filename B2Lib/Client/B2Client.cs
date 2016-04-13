using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using B2Lib.Enums;
using B2Lib.Exceptions;
using B2Lib.Objects;
using B2Lib.Utilities;
using Newtonsoft.Json;

namespace B2Lib.Client
{
    public class B2Client
    {
        public string AuthorizationToken
        {
            get { return Communicator.AuthToken; }
            private set { Communicator.AuthToken = value; }
        }

        public string AccountId { get; private set; }
        public long MinimumPartSize { get; set; }

        private readonly ConcurrentDictionary<string, ConcurrentBag<B2UploadConfiguration>> _uploadConfigsByBucket;

        private readonly ConcurrentDictionary<string, ConcurrentBag<B2UploadPartConfiguration>> _uploadConfigsByLargeFile;

        internal B2BucketCacher BucketCache { get; private set; }
        internal B2Communicator Communicator { get; private set; }

        public Uri ApiUri
        {
            get { return Communicator.ApiUri; }
            private set { Communicator.ApiUri = value; }
        }

        public Uri DownloadUri
        {
            get { return Communicator.DownloadUri; }
            private set { Communicator.DownloadUri = value; }
        }

        public TimeSpan TimeoutMeta
        {
            get { return Communicator.TimeoutMeta; }
            set { Communicator.TimeoutMeta = value; }
        }

        public TimeSpan TimeoutData
        {
            get { return Communicator.TimeoutData; }
            set { Communicator.TimeoutData = value; }
        }

        public B2Client()
        {
            _uploadConfigsByBucket = new ConcurrentDictionary<string, ConcurrentBag<B2UploadConfiguration>>();
            _uploadConfigsByLargeFile = new ConcurrentDictionary<string, ConcurrentBag<B2UploadPartConfiguration>>();

            BucketCache = new B2BucketCacher();
            Communicator = new B2Communicator();
        }

        public void LoadState(B2SaveState state)
        {
            AccountId = state.AccountId;
            AuthorizationToken = state.AuthorizationToken;
            MinimumPartSize = state.MinimumPartSize;

            ApiUri = state.ApiUrl;
            DownloadUri = state.DownloadUrl;

            BucketCache.LoadState(state.BucketCache);
        }

        public void LoadState(string file)
        {
            B2SaveState res = JsonConvert.DeserializeObject<B2SaveState>(File.ReadAllText(file));

            LoadState(res);
        }

        public B2SaveState SaveState()
        {
            B2SaveState res = new B2SaveState();

            res.AccountId = AccountId;
            res.AuthorizationToken = AuthorizationToken;
            res.MinimumPartSize = MinimumPartSize;

            res.ApiUrl = ApiUri;
            res.DownloadUrl = DownloadUri;

            res.BucketCache = BucketCache.GetState();

            return res;
        }

        public void SaveState(string file)
        {
            B2SaveState res = SaveState();

            File.WriteAllText(file, JsonConvert.SerializeObject(res));
        }

        private void ThrowExceptionIfNotAuthorized()
        {
            if (String.IsNullOrEmpty(AccountId))
                throw new B2MissingAuthenticationException("You must call Login() or LoadState() first");
        }

        internal void ReturnUploadConfig(B2UploadConfiguration config)
        {
            ConcurrentBag<B2UploadConfiguration> bag = _uploadConfigsByBucket.GetOrAdd(config.BucketId, s => new ConcurrentBag<B2UploadConfiguration>());

            bag.Add(config);
        }

        internal void ReturnUploadConfig(B2UploadPartConfiguration config)
        {
            ConcurrentBag<B2UploadPartConfiguration> bag = _uploadConfigsByLargeFile.GetOrAdd(config.FileId, s => new ConcurrentBag<B2UploadPartConfiguration>());

            bag.Add(config);
        }

        internal B2UploadConfiguration FetchUploadConfig(string bucketId)
        {
            ConcurrentBag<B2UploadConfiguration> bag = _uploadConfigsByBucket.GetOrAdd(bucketId, s => new ConcurrentBag<B2UploadConfiguration>());

            B2UploadConfiguration config;
            if (bag.TryTake(out config))
                return config;

            // Create new config
            B2UploadConfiguration res;
            try
            {
                res = Communicator.GetUploadUrl(bucketId).Result;
            }
            catch (AggregateException ex)
            {
                // Re-throw the inner exception
                throw ex.InnerException;
            }

            return res;
        }

        internal B2UploadPartConfiguration FetchLargeFileUploadConfig(string fileId)
        {
            ConcurrentBag<B2UploadPartConfiguration> bag = _uploadConfigsByLargeFile.GetOrAdd(fileId, s => new ConcurrentBag<B2UploadPartConfiguration>());

            B2UploadPartConfiguration config;
            if (bag.TryTake(out config))
                return config;

            // Create new config
            B2UploadPartConfiguration res;
            try
            {
                res = Communicator.GetPartUploadUrl(fileId).Result;
            }
            catch (AggregateException ex)
            {
                // Re-throw the inner exception
                throw ex.InnerException;
            }

            return res;
        }

        internal void MarkLargeFileDone(string fileId)
        {
            ConcurrentBag<B2UploadPartConfiguration> bag;
            _uploadConfigsByLargeFile.TryRemove(fileId, out bag);
        }

        public async Task LoginAsync(string accountId, string applicationKey)
        {
            B2AuthenticationResponse result = await Communicator.AuthorizeAccount(accountId, applicationKey);

            AccountId = result.AccountId;
            AuthorizationToken = result.AuthorizationToken;
            MinimumPartSize = result.MinimumPartSize;

            ApiUri = result.ApiUrl;
            DownloadUri = result.DownloadUrl;
        }

        public async Task<IEnumerable<B2Bucket>> GetBucketsAsync()
        {
            ThrowExceptionIfNotAuthorized();

            List<B2BucketObject> buckets = await Communicator.ListBuckets(AccountId);

            BucketCache.RecordBucket(buckets);

            return buckets.Select(s => new B2Bucket(this, s));
        }

        public async Task<B2Bucket> CreateBucketAsync(string name, B2BucketType type)
        {
            ThrowExceptionIfNotAuthorized();

            B2BucketObject bucket = await Communicator.CreateBucket(AccountId, name, type);

            BucketCache.RecordBucket(bucket);

            return new B2Bucket(this, bucket);
        }

        public async Task<B2Bucket> GetBucketByNameAsync(string name)
        {
            ThrowExceptionIfNotAuthorized();

            if (BucketCache.GetByName(name) == null)
                await GetBucketsAsync();

            return new B2Bucket(this, BucketCache.GetByName(name));
        }

        public async Task<B2Bucket> GetBucketByIdAsync(string id)
        {
            ThrowExceptionIfNotAuthorized();

            if (BucketCache.GetById(id) == null)
                await GetBucketsAsync();

            return new B2Bucket(this, BucketCache.GetById(id));
        }
    }
}