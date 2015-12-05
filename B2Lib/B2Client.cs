using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading;
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
        private string _accountId;
        private string _authorizationToken;

        private Uri _apiUrl;
        private Uri _downloadUrl;

        private B2BucketCacher _bucketCache;

        public B2Client()
        {
            _bucketCache = new B2BucketCacher();
        }

        public void LoadState(B2SaveState state)
        {
            _accountId = state.AccountId;
            _authorizationToken = state.AuthorizationToken;

            _apiUrl = state.ApiUrl;
            _downloadUrl = state.DownloadUrl;

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

            res.AccountId = _accountId;
            res.AuthorizationToken = _authorizationToken;

            res.ApiUrl = _apiUrl;
            res.DownloadUrl = _downloadUrl;

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
            if (string.IsNullOrEmpty(_accountId))
                throw new B2MissingAuthenticationException("You must call Login() or LoadState() first");
        }

        public B2Bucket GetBucketById(string bucketId)
        {
            try
            {
                return GetBucketByIdAsync(bucketId).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
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

        public B2Bucket GetBucketByName(string name)
        {
            try
            {
                return GetBucketByNameAsync(name).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
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

        public void Login(string accountId, string applicationKey)
        {
            try
            {
                LoginAsync(accountId, applicationKey).Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task LoginAsync(string accountId, string applicationKey)
        {
            B2AuthenticationResponse result = await B2Communicator.Login(accountId, applicationKey);

            _accountId = result.AccountId;
            _authorizationToken = result.AuthorizationToken;

            _apiUrl = result.ApiUrl;
            _downloadUrl = result.DownloadUrl;
        }

        public List<B2Bucket> ListBuckets()
        {
            try
            {
                return ListBucketsAsync().Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<List<B2Bucket>> ListBucketsAsync()
        {
            return await ListBucketsAsync(_accountId);
        }

        public List<B2Bucket> ListBuckets(string accountId)
        {
            try
            {
                return ListBucketsAsync(accountId).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<List<B2Bucket>> ListBucketsAsync(string accountId)
        {
            ThrowExceptionIfNotAuthorized();

            List<B2Bucket> result = await B2Communicator.ListBuckets(_apiUrl, _authorizationToken, accountId);

            _bucketCache.RecordBucket(result);

            return result;
        }

        public B2Bucket CreateBucket(string name, B2BucketType bucketType)
        {
            try
            {
                return CreateBucketAsync(name, bucketType).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public Task<B2Bucket> CreateBucketAsync(string name, B2BucketType bucketType)
        {
            return CreateBucketAsync(_accountId, name, bucketType);
        }

        public B2Bucket CreateBucket(string accountId, string name, B2BucketType bucketType)
        {
            try
            {
                return CreateBucketAsync(accountId, name, bucketType).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<B2Bucket> CreateBucketAsync(string accountId, string name, B2BucketType bucketType)
        {
            ThrowExceptionIfNotAuthorized();

            B2Bucket res = await B2Communicator.CreateBucket(_apiUrl, _authorizationToken, accountId, name, bucketType);

            _bucketCache.RecordBucket(res);

            return res;
        }

        public B2Bucket DeleteBucket(string bucketId)
        {
            try
            {
                return DeleteBucketAsync(_accountId, bucketId).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<B2Bucket> DeleteBucketAsync(string bucketId)
        {
            return await DeleteBucketAsync(_accountId, bucketId);
        }

        public B2Bucket DeleteBucket(B2Bucket bucket)
        {
            try
            {
                return DeleteBucketAsync(bucket.AccountId, bucket.BucketId).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<B2Bucket> DeleteBucketAsync(B2Bucket bucket)
        {
            return await DeleteBucketAsync(bucket.AccountId, bucket.BucketId);
        }

        public B2Bucket DeleteBucket(string accountId, string bucketId)
        {
            try
            {
                return DeleteBucketAsync(accountId, bucketId).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<B2Bucket> DeleteBucketAsync(string accountId, string bucketId)
        {
            ThrowExceptionIfNotAuthorized();

            B2Bucket res = await B2Communicator.DeleteBucket(_apiUrl, _authorizationToken, accountId, bucketId);

            _bucketCache.RemoveBucket(res);

            return res;
        }

        public B2Bucket UpdateBucket(string bucketId, B2BucketType bucketType)
        {
            try
            {
                return UpdateBucketAsync(_accountId, bucketId, bucketType).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<B2Bucket> UpdateBucketAsync(string bucketId, B2BucketType bucketType)
        {
            return await UpdateBucketAsync(_accountId, bucketId, bucketType);
        }

        public B2Bucket UpdateBucket(B2Bucket bucket, B2BucketType bucketType)
        {
            try
            {
                return UpdateBucketAsync(bucket.AccountId, bucket.BucketId, bucketType).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<B2Bucket> UpdateBucketAsync(B2Bucket bucket, B2BucketType bucketType)
        {
            return await UpdateBucketAsync(bucket.AccountId, bucket.BucketId, bucketType);
        }

        public B2Bucket UpdateBucket(string accountId, string bucketId, B2BucketType bucketType)
        {
            try
            {
                return UpdateBucketAsync(accountId, bucketId, bucketType).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<B2Bucket> UpdateBucketAsync(string accountId, string bucketId, B2BucketType bucketType)
        {
            ThrowExceptionIfNotAuthorized();

            B2Bucket res = await B2Communicator.UpdateBucket(_apiUrl, _authorizationToken, accountId, bucketId, bucketType);

            _bucketCache.RecordBucket(res);

            return res;
        }

        public B2FileBase HideFile(string bucketId, B2FileBase file)
        {
            try
            {
                return HideFileAsync(bucketId, file.FileName).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<B2FileBase> HideFileAsync(string bucketId, B2FileBase file)
        {
            return await HideFileAsync(bucketId, file.FileName);
        }

        public B2FileBase HideFile(B2Bucket bucket, B2FileBase file)
        {
            try
            {
                return HideFileAsync(bucket.BucketId, file.FileName).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<B2FileBase> HideFileAsync(B2Bucket bucket, B2FileBase file)
        {
            return await HideFileAsync(bucket.BucketId, file.FileName);
        }

        public B2FileBase HideFile(B2Bucket bucket, string fileName)
        {
            try
            {
                return HideFileAsync(bucket.BucketId, fileName).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<B2FileBase> HideFileAsync(B2Bucket bucket, string fileName)
        {
            return await HideFileAsync(bucket.BucketId, fileName);
        }

        public B2FileBase HideFile(string bucketId, string fileName)
        {
            try
            {
                return HideFileAsync(bucketId, fileName).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<B2FileBase> HideFileAsync(string bucketId, string fileName)
        {
            ThrowExceptionIfNotAuthorized();

            B2FileBase res = await B2Communicator.HideFile(_apiUrl, _authorizationToken, bucketId, fileName);

            return res;
        }

        public B2FileInfo GetFileInfo(B2FileBase file)
        {
            try
            {
                return GetFileInfoAsync(file.FileId).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<B2FileInfo> GetFileInfoAsync(B2FileBase file)
        {
            return await GetFileInfoAsync(file.FileId);
        }

        public B2FileInfo GetFileInfo(string fileId)
        {
            try
            {
                return GetFileInfoAsync(fileId).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<B2FileInfo> GetFileInfoAsync(string fileId)
        {
            ThrowExceptionIfNotAuthorized();

            B2FileInfo res = await B2Communicator.GetFileInfo(_apiUrl, _authorizationToken, fileId);

            return res;
        }

        public bool DeleteFile(B2FileBase file)
        {
            try
            {
                return DeleteFileAsync(file.FileName, file.FileId).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public Task<bool> DeleteFileAsync(B2FileBase file)
        {
            return DeleteFileAsync(file.FileName, file.FileId);
        }

        public bool DeleteFile(B2FileInfo file)
        {
            try
            {
                return DeleteFileAsync(file.FileName, file.FileId).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public Task<bool> DeleteFileAsync(B2FileInfo file)
        {
            return DeleteFileAsync(file.FileName, file.FileId);
        }

        public bool DeleteFile(string fileName, string fileId)
        {
            try
            {
                return DeleteFileAsync(fileName, fileId).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName, string fileId)
        {
            ThrowExceptionIfNotAuthorized();

            bool res = await B2Communicator.DeleteFile(_apiUrl, _authorizationToken, fileName, fileId);

            return res;
        }

        public IEnumerable<B2FileWithSize> ListFiles(B2Bucket bucket, string startFileName = null)
        {
            return ListFiles(bucket.BucketId, startFileName);
        }

        public IEnumerable<B2FileWithSize> ListFiles(string bucketId, string startFileName = null)
        {
            return new B2FilesIterator(_apiUrl, _authorizationToken, bucketId, startFileName);
        }

        public IEnumerable<B2FileWithSize> ListFileVersions(B2Bucket bucket, string startFileName = null, string startFileId = null)
        {
            return ListFileVersions(bucket.BucketId, startFileName, startFileId);
        }

        public IEnumerable<B2FileWithSize> ListFileVersions(string bucketId, string startFileName = null, string startFileId = null)
        {
            return new B2FileVersionsIterator(_apiUrl, _authorizationToken, bucketId, startFileName, startFileId);
        }

        public B2FileInfo UploadFile(B2Bucket bucket, FileInfo file, string fileName, Dictionary<string, string> fileInfo = null, string contentType = null)
        {
            try
            {
                return UploadFileAsync(bucket.BucketId, file, fileName, fileInfo, contentType).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public Task<B2FileInfo> UploadFileAsync(B2Bucket bucket, FileInfo file, string fileName, Dictionary<string, string> fileInfo = null, string contentType = null)
        {
            return UploadFileAsync(bucket.BucketId, file, fileName, fileInfo, contentType);
        }

        public B2FileInfo UploadFile(string bucketId, FileInfo file, string fileName, Dictionary<string, string> fileInfo = null, string contentType = null)
        {
            try
            {
                return UploadFileAsync(bucketId, file, fileName, fileInfo, contentType).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
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
                    res = B2Communicator.GetUploadUrl(_apiUrl, _authorizationToken, bucketId).Result;
                }
                catch (AggregateException ex)
                {
                    // Re-throw the inner exception
                    throw ex.InnerException;
                }

                return _bucketCache.RecordBucket(res);
            }
        }

        public B2FileDownloadResult DownloadFileHead(B2FileBase file)
        {
            try
            {
                return DownloadFileHeadAsync(file.FileId).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<B2FileDownloadResult> DownloadFileHeadAsync(B2FileBase file)
        {
            return await DownloadFileHeadAsync(file.FileId);
        }

        public B2FileDownloadResult DownloadFileHead(string fileId)
        {
            try
            {
                return DownloadFileHeadAsync(fileId).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<B2FileDownloadResult> DownloadFileHeadAsync(string fileId)
        {
            ThrowExceptionIfNotAuthorized();

            return await B2Communicator.DownloadFileHead(_downloadUrl, fileId, _authorizationToken);
        }

        public Stream DownloadFileContent(B2FileBase file, out B2FileDownloadResult info)
        {
            return DownloadFileContent(file.FileId, out info);
        }

        public Stream DownloadFileContent(string fileId, out B2FileDownloadResult info)
        {
            ThrowExceptionIfNotAuthorized();

            return B2Communicator.DownloadFileContent(_downloadUrl, fileId, out info, _authorizationToken);
        }
    }
}