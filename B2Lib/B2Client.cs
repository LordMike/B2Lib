using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using B2Lib.Enums;
using B2Lib.Exceptions;
using B2Lib.Objects;
using B2Lib.Utilities;
using Newtonsoft.Json;

namespace B2Lib
{
    public class B2Client
    {
        public string AuthorizationToken { get; private set; }
        public string AccountId { get; private set; }

        private B2BucketCacher _bucketCache;

        public Uri ApiUrl { get; private set; }
        public Uri DownloadUrl { get; private set; }

        public B2Client()
        {
            _bucketCache = new B2BucketCacher();
        }

        public void LoadState(B2SaveState state)
        {
            AccountId = state.AccountId;
            AuthorizationToken = state.AuthorizationToken;

            ApiUrl = state.ApiUrl;
            DownloadUrl = state.DownloadUrl;

            _bucketCache.LoadState(state.BucketCache);
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

            res.BucketCache = _bucketCache.GetState();

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

        public async Task<B2Bucket> GetBucketByIdAsync(string bucketId)
        {
            B2BucketCache item = _bucketCache.GetById(bucketId);

            if (item != null)
                return item.CreateBucketClass();

            // Fetch
            List<B2Bucket> buckets = await ListBucketsAsync();

            return buckets.FirstOrDefault(s => s.BucketId == bucketId);
        }

        public async Task<B2Bucket> GetBucketByNameAsync(string name)
        {
            B2BucketCache item = _bucketCache.GetByName(name);

            if (item != null)
                return item.CreateBucketClass();

            // Fetch
            List<B2Bucket> buckets = await ListBucketsAsync();

            return buckets.FirstOrDefault(s => s.BucketName == name);
        }

        public async Task LoginAsync(string accountId, string applicationKey)
        {
            B2AuthenticationResponse result = await B2Communicator.Login(accountId, applicationKey);

            AccountId = result.AccountId;
            AuthorizationToken = result.AuthorizationToken;

            ApiUrl = result.ApiUrl;
            DownloadUrl = result.DownloadUrl;
        }

        public async Task<List<B2Bucket>> ListBucketsAsync()
        {
            return await ListBucketsAsync(AccountId);
        }

        public async Task<List<B2Bucket>> ListBucketsAsync(string accountId)
        {
            ThrowExceptionIfNotAuthorized();

            List<B2Bucket> result = await B2Communicator.ListBuckets(ApiUrl, AuthorizationToken, accountId);

            _bucketCache.RecordBucket(result);

            return result;
        }

        public async Task<B2Bucket> CreateBucketAsync(string name, B2BucketType bucketType)
        {
            return await CreateBucketAsync(AccountId, name, bucketType);
        }

        public async Task<B2Bucket> CreateBucketAsync(string accountId, string name, B2BucketType bucketType)
        {
            ThrowExceptionIfNotAuthorized();

            B2Bucket res = await B2Communicator.CreateBucket(ApiUrl, AuthorizationToken, accountId, name, bucketType);

            _bucketCache.RecordBucket(res);

            return res;
        }

        public async Task<B2Bucket> DeleteBucketAsync(string bucketId)
        {
            return await DeleteBucketAsync(AccountId, bucketId);
        }

        public async Task<B2Bucket> DeleteBucketAsync(B2Bucket bucket)
        {
            return await DeleteBucketAsync(bucket.AccountId, bucket.BucketId);
        }

        public async Task<B2Bucket> DeleteBucketAsync(string accountId, string bucketId)
        {
            ThrowExceptionIfNotAuthorized();

            B2Bucket res = await B2Communicator.DeleteBucket(ApiUrl, AuthorizationToken, accountId, bucketId);

            _bucketCache.RemoveBucket(res);

            return res;
        }

        public async Task<B2Bucket> UpdateBucketAsync(string bucketId, B2BucketType bucketType)
        {
            return await UpdateBucketAsync(AccountId, bucketId, bucketType);
        }

        public async Task<B2Bucket> UpdateBucketAsync(B2Bucket bucket, B2BucketType bucketType)
        {
            return await UpdateBucketAsync(bucket.AccountId, bucket.BucketId, bucketType);
        }

        public async Task<B2Bucket> UpdateBucketAsync(string accountId, string bucketId, B2BucketType bucketType)
        {
            ThrowExceptionIfNotAuthorized();

            B2Bucket res = await B2Communicator.UpdateBucket(ApiUrl, AuthorizationToken, accountId, bucketId, bucketType);

            _bucketCache.RecordBucket(res);

            return res;
        }

        public async Task<B2FileBase> HideFileAsync(string bucketId, B2FileBase file)
        {
            return await HideFileAsync(bucketId, file.FileName);
        }

        public async Task<B2FileBase> HideFileAsync(B2Bucket bucket, B2FileBase file)
        {
            return await HideFileAsync(bucket.BucketId, file.FileName);
        }

        public async Task<B2FileBase> HideFileAsync(B2Bucket bucket, string fileName)
        {
            return await HideFileAsync(bucket.BucketId, fileName);
        }

        public async Task<B2FileBase> HideFileAsync(string bucketId, string fileName)
        {
            ThrowExceptionIfNotAuthorized();

            B2FileBase res = await B2Communicator.HideFile(ApiUrl, AuthorizationToken, bucketId, fileName);

            return res;
        }

        public async Task<B2FileInfo> GetFileInfoAsync(B2FileBase file)
        {
            return await GetFileInfoAsync(file.FileId);
        }

        public async Task<B2FileInfo> GetFileInfoAsync(string fileId)
        {
            ThrowExceptionIfNotAuthorized();

            B2FileInfo res = await B2Communicator.GetFileInfo(ApiUrl, AuthorizationToken, fileId);

            return res;
        }

        public async Task<bool> DeleteFileAsync(B2FileBase file)
        {
            return await DeleteFileAsync(file.FileName, file.FileId);
        }

        public async Task<bool> DeleteFileAsync(B2FileInfo file)
        {
            return await DeleteFileAsync(file.FileName, file.FileId);
        }

        public async Task<bool> DeleteFileAsync(string fileName, string fileId)
        {
            ThrowExceptionIfNotAuthorized();

            bool res = await B2Communicator.DeleteFile(ApiUrl, AuthorizationToken, fileName, fileId);

            return res;
        }

        public IEnumerable<B2FileWithSize> ListFiles(B2Bucket bucket, string startFileName = null)
        {
            return ListFiles(bucket.BucketId, startFileName);
        }

        public IEnumerable<B2FileWithSize> ListFiles(string bucketId, string startFileName = null)
        {
            return new B2FilesIterator(ApiUrl, AuthorizationToken, bucketId, startFileName);
        }

        public IEnumerable<B2FileWithSize> ListFileVersions(B2Bucket bucket, string startFileName = null, string startFileId = null)
        {
            return ListFileVersions(bucket.BucketId, startFileName, startFileId);
        }

        public IEnumerable<B2FileWithSize> ListFileVersions(string bucketId, string startFileName = null, string startFileId = null)
        {
            return new B2FileVersionsIterator(ApiUrl, AuthorizationToken, bucketId, startFileName, startFileId);
        }

        public async Task<B2FileInfo> UploadFileAsync(B2Bucket bucket, FileInfo file, string fileName, Dictionary<string, string> fileInfo = null, string contentType = null)
        {
            return await UploadFileAsync(bucket.BucketId, file, fileName, fileInfo, contentType);
        }

        public async Task<B2FileInfo> UploadFileAsync(string bucketId, FileInfo file, string fileName, Dictionary<string, string> fileInfo = null, string contentType = null)
        {
            B2BucketCache uploadConfig = await Task.Run(() => FetchUploadUrl(bucketId));

            using (FileStream fs = File.OpenRead(file.FullName))
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                string hash = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", "");

                fs.Seek(0, SeekOrigin.Begin);

                B2FileInfo res = await B2Communicator.UploadFile(uploadConfig.UploadUri, uploadConfig.UploadAuthorizationToken, fs, fileName, hash, fileInfo, contentType);

                return res;
            }
        }

        private B2BucketCache FetchUploadUrl(string bucketId)
        {
            object lockObj = KeyLocker.GetLockObject(bucketId);

            lock (lockObj)
            {
                B2BucketCache item = _bucketCache.GetById(bucketId);

                if (item != null && item.ReadyForUpload)
                    return item;

                B2UploadConfiguration res;
                try
                {
                    res = B2Communicator.GetUploadUrl(ApiUrl, AuthorizationToken, bucketId).Result;
                }
                catch (AggregateException ex)
                {
                    // Re-throw the inner exception
                    throw ex.InnerException;
                }

                return _bucketCache.RecordBucket(res);
            }
        }

        public async Task<B2FileDownloadResult> DownloadFileHeadAsync(B2FileBase file)
        {
            return await DownloadFileHeadAsync(file.FileId);
        }

        public async Task<B2FileDownloadResult> DownloadFileHeadAsync(string fileId)
        {
            ThrowExceptionIfNotAuthorized();

            return await B2Communicator.DownloadFileHead(DownloadUrl, fileId, AuthorizationToken);
        }

        public async Task<B2DownloadResult> DownloadFileContent(B2FileBase file)
        {
            return await DownloadFileContent(file.FileId);
        }

        public async Task<B2DownloadResult> DownloadFileContent(string fileId)
        {
            ThrowExceptionIfNotAuthorized();

            return await B2Communicator.DownloadFileContent(DownloadUrl, fileId, AuthorizationToken);
        }
    }
}