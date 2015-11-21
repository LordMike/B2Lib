using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        public async Task<B2Bucket> GetBucketById(string bucketId)
        {
            B2BucketCache item = _bucketCache.GetById(bucketId);

            if (item != null)
                return item.CreateBucketClass();

            // Fetch
            List<B2Bucket> buckets = await ListBuckets();

            return buckets.FirstOrDefault(s => s.BucketId == bucketId);
        }

        public async Task<B2Bucket> GetBucketByName(string name)
        {
            B2BucketCache item = _bucketCache.GetByName(name);

            if (item != null)
                return item.CreateBucketClass();

            // Fetch
            List<B2Bucket> buckets = await ListBuckets();

            return buckets.FirstOrDefault(s => s.BucketName == name);
        }

        public async Task Login(string accountId, string applicationKey)
        {
            B2AuthenticationResponse result = await B2Communicator.Login(accountId, applicationKey);

            _accountId = result.AccountId;
            _authorizationToken = result.AuthorizationToken;

            _apiUrl = result.ApiUrl;
            _downloadUrl = result.DownloadUrl;
        }

        public async Task<List<B2Bucket>> ListBuckets()
        {
            return await ListBuckets(_accountId);
        }

        public async Task<List<B2Bucket>> ListBuckets(string accountId)
        {
            ThrowExceptionIfNotAuthorized();

            List<B2Bucket> result = await B2Communicator.ListBuckets(_apiUrl, _authorizationToken, accountId);

            _bucketCache.RecordBucket(result);

            return result;
        }

        public Task<B2Bucket> CreateBucket(string name, B2BucketType bucketType)
        {
            return CreateBucket(_accountId, name, bucketType);
        }

        public async Task<B2Bucket> CreateBucket(string accountId, string name, B2BucketType bucketType)
        {
            ThrowExceptionIfNotAuthorized();

            B2Bucket res = await B2Communicator.CreateBucket(_apiUrl, _authorizationToken, accountId, name, bucketType);

            _bucketCache.RecordBucket(res);

            return res;
        }

        public async Task<B2Bucket> DeleteBucket(string bucketId)
        {
            return await DeleteBucket(_accountId, bucketId);
        }

        public async Task<B2Bucket> DeleteBucket(B2Bucket bucket)
        {
            return await DeleteBucket(bucket.AccountId, bucket.BucketId);
        }

        public async Task<B2Bucket> DeleteBucket(string accountId, string bucketId)
        {
            ThrowExceptionIfNotAuthorized();

            B2Bucket res = await B2Communicator.DeleteBucket(_apiUrl, _authorizationToken, accountId, bucketId);

            _bucketCache.RemoveBucket(res);

            return res;
        }

        public async Task<B2Bucket> UpdateBucket(string bucketId, B2BucketType bucketType)
        {
            return await UpdateBucket(_accountId, bucketId, bucketType);
        }

        public async Task<B2Bucket> UpdateBucket(B2Bucket bucket, B2BucketType bucketType)
        {
            return await UpdateBucket(bucket.AccountId, bucket.BucketId, bucketType);
        }

        public async Task<B2Bucket> UpdateBucket(string accountId, string bucketId, B2BucketType bucketType)
        {
            ThrowExceptionIfNotAuthorized();

            B2Bucket res = await B2Communicator.UpdateBucket(_apiUrl, _authorizationToken, accountId, bucketId, bucketType);

            _bucketCache.RecordBucket(res);

            return res;
        }

        public async Task<B2FileBase> HideFile(string bucketId, B2FileBase file)
        {
            return await HideFile(bucketId, file.FileName);
        }

        public async Task<B2FileBase> HideFile(B2Bucket bucket, B2FileBase file)
        {
            return await HideFile(bucket.BucketId, file.FileName);
        }

        public async Task<B2FileBase> HideFile(B2Bucket bucket, string fileName)
        {
            return await HideFile(bucket.BucketId, fileName);
        }

        public async Task<B2FileBase> HideFile(string bucketId, string fileName)
        {
            ThrowExceptionIfNotAuthorized();

            B2FileBase res = await B2Communicator.HideFile(_apiUrl, _authorizationToken, bucketId, fileName);

            return res;
        }

        public async Task<B2FileInfo> GetFileInfo(B2FileBase file)
        {
            return await GetFileInfo(file.FileId);
        }

        public async Task<B2FileInfo> GetFileInfo(string fileId)
        {
            ThrowExceptionIfNotAuthorized();

            B2FileInfo res = await B2Communicator.GetFileInfo(_apiUrl, _authorizationToken, fileId);

            return res;
        }

        public Task<bool> DeleteFile(B2FileBase file)
        {
            return DeleteFile(file.FileName, file.FileId);
        }

        public Task<bool> DeleteFile(B2FileInfo file)
        {
            return DeleteFile(file.FileName, file.FileId);
        }

        public async Task<bool> DeleteFile(string fileName, string fileId)
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

        public Task<B2FileInfo> UploadFile(B2Bucket bucket, FileInfo file, string fileName, Dictionary<string, string> fileInfo = null, string contentType = null)
        {
            return UploadFile(bucket.BucketId, file, fileName, fileInfo, contentType);
        }

        public async Task<B2FileInfo> UploadFile(string bucketId, FileInfo file, string fileName, Dictionary<string, string> fileInfo = null, string contentType = null)
        {
            B2BucketCache uploadConfig = await FetchUploadUrl(bucketId);

            using (var fs = File.OpenRead(file.FullName))
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                string hash = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", "");

                fs.Seek(0, SeekOrigin.Begin);

                B2FileInfo res = await B2Communicator.UploadFile(uploadConfig.UploadUri, uploadConfig.UploadAuthorizationToken, fs, fileName,
                     hash, fileInfo, contentType);

                return res;
            }
        }

        private async Task<B2BucketCache> FetchUploadUrl(string bucketId)
        {
            B2BucketCache item = _bucketCache.GetById(bucketId);

            if (item != null && item.ReadyForUpload)
                return item;

            B2UploadConfiguration res = await B2Communicator.GetUploadUrl(_apiUrl, _authorizationToken, bucketId);

            return _bucketCache.RecordBucket(res);
        }

        public async Task<B2FileDownloadResult> DownloadFileHead(B2FileBase file)
        {
            return await DownloadFileHead(file.FileId);
        }

        public async Task<B2FileDownloadResult> DownloadFileHead(string fileId)
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