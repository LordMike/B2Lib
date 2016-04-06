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

        private readonly ConcurrentDictionary<string, ConcurrentBag<B2UploadConfiguration>> _bucketUploadUrls;
        internal B2BucketCacher BucketCache { get; private set; }

        internal B2Communicator Communicator { get; private set; }

        public Uri ApiUrl { get; private set; }
        public Uri DownloadUrl { get; private set; }

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
            _bucketUploadUrls = new ConcurrentDictionary<string, ConcurrentBag<B2UploadConfiguration>>();
            BucketCache = new B2BucketCacher();
            Communicator = new B2Communicator();
        }

        public void LoadState(B2SaveState state)
        {
            AccountId = state.AccountId;
            AuthorizationToken = state.AuthorizationToken;

            ApiUrl = state.ApiUrl;
            DownloadUrl = state.DownloadUrl;

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

            res.ApiUrl = ApiUrl;
            res.DownloadUrl = DownloadUrl;

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

        internal void ReturnUploadConfig(string bucketId, B2UploadConfiguration config)
        {
            ConcurrentBag<B2UploadConfiguration> bag = _bucketUploadUrls.GetOrAdd(bucketId, s => new ConcurrentBag<B2UploadConfiguration>());

            bag.Add(config);
        }

        internal B2UploadConfiguration FetchUploadConfig(string bucketId)
        {
            ConcurrentBag<B2UploadConfiguration> bag = _bucketUploadUrls.GetOrAdd(bucketId, s => new ConcurrentBag<B2UploadConfiguration>());

            B2UploadConfiguration config;
            if (bag.TryTake(out config))
                return config;

            // Create new config
            B2UploadConfiguration res;
            try
            {
                res = Communicator.GetUploadUrl(ApiUrl, bucketId).Result;
            }
            catch (AggregateException ex)
            {
                // Re-throw the inner exception
                throw ex.InnerException;
            }

            return res;
        }

        public async Task LoginAsync(string accountId, string applicationKey)
        {
            B2AuthenticationResponse result = await Communicator.AuthorizeAccount(accountId, applicationKey);

            AccountId = result.AccountId;
            AuthorizationToken = result.AuthorizationToken;

            ApiUrl = result.ApiUrl;
            DownloadUrl = result.DownloadUrl;
        }

        public async Task<IEnumerable<B2BucketV2>> GetBucketsAsync()
        {
            ThrowExceptionIfNotAuthorized();

            List<B2Bucket> buckets = await Communicator.ListBuckets(ApiUrl, AccountId);

            BucketCache.RecordBucket(buckets);

            return buckets.Select(s => new B2BucketV2(this, s));
        }

        public async Task<B2BucketV2> CreateBucketAsync(string name, B2BucketType type)
        {
            ThrowExceptionIfNotAuthorized();

            B2Bucket bucket = await Communicator.CreateBucket(ApiUrl, AccountId, name, type);

            BucketCache.RecordBucket(bucket);

            return new B2BucketV2(this, bucket);
        }

        public async Task<B2BucketV2> GetBucketByNameAsync(string name)
        {
            ThrowExceptionIfNotAuthorized();

            IEnumerable<B2BucketV2> buckets = await GetBucketsAsync();

            return buckets.FirstOrDefault(s => s.BucketName.Equals(name));
        }

        public async Task<B2BucketV2> GetBucketByIdAsync(string id)
        {
            ThrowExceptionIfNotAuthorized();

            IEnumerable<B2BucketV2> buckets = await GetBucketsAsync();

            return buckets.FirstOrDefault(s => s.BucketId.Equals(id));
        }
    }
}