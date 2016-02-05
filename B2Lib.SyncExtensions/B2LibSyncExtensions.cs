using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using B2Lib.Enums;
using B2Lib.Objects;

namespace B2Lib.SyncExtensions
{
    public static class B2LibSyncExtensions
    {
        private static T AsyncRunHelper<T>(Func<Task<T>> action)
        {
            try
            {
                return action().Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        private static void AsyncRunHelper(Func<Task> action)
        {
            try
            {
                action().Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public static B2Bucket GetBucketById(this B2Client client, string bucketId)
        {
            return AsyncRunHelper(() => client.GetBucketByIdAsync(bucketId));
        }

        public static B2Bucket GetBucketByName(this B2Client client, string name)
        {
            return AsyncRunHelper(() => client.GetBucketByNameAsync(name));
        }

        public static void Login(this B2Client client, string accountId, string applicationKey)
        {
            AsyncRunHelper(() => client.LoginAsync(accountId, applicationKey));
        }

        public static List<B2Bucket> ListBuckets(this B2Client client)
        {
            return AsyncRunHelper(() => client.ListBucketsAsync());
        }

        public static List<B2Bucket> ListBuckets(this B2Client client, string accountId)
        {
            return AsyncRunHelper(() => client.ListBucketsAsync(accountId));
        }

        public static B2Bucket CreateBucket(this B2Client client, string name, B2BucketType bucketType)
        {
            return AsyncRunHelper(() => client.CreateBucketAsync(name, bucketType));
        }

        public static B2Bucket CreateBucket(this B2Client client, string accountId, string name, B2BucketType bucketType)
        {
            return AsyncRunHelper(() => client.CreateBucketAsync(accountId, name, bucketType));
        }

        public static B2Bucket DeleteBucket(this B2Client client, string bucketId)
        {
            return AsyncRunHelper(() => client.DeleteBucketAsync(client.AccountId, bucketId));
        }

        public static B2Bucket DeleteBucket(this B2Client client, B2Bucket bucket)
        {
            return AsyncRunHelper(() => client.DeleteBucketAsync(bucket));
        }

        public static B2Bucket DeleteBucket(this B2Client client, string accountId, string bucketId)
        {
            return AsyncRunHelper(() => client.DeleteBucketAsync(accountId, bucketId));
        }

        public static B2Bucket UpdateBucket(this B2Client client, string bucketId, B2BucketType bucketType)
        {
            return AsyncRunHelper(() => client.UpdateBucketAsync( bucketId, bucketType));
        }

        public static B2Bucket UpdateBucket(this B2Client client, B2Bucket bucket, B2BucketType bucketType)
        {
            return AsyncRunHelper(() => client.UpdateBucketAsync(bucket, bucketType));
        }

        public static B2Bucket UpdateBucket(this B2Client client, string accountId, string bucketId, B2BucketType bucketType)
        {
            return AsyncRunHelper(() => client.UpdateBucketAsync(accountId, bucketId, bucketType));
        }

        public static B2FileBase HideFile(this B2Client client, string bucketId, B2FileBase file)
        {
            return AsyncRunHelper(() => client.HideFileAsync(bucketId, file));
        }

        public static B2FileBase HideFile(this B2Client client, B2Bucket bucket, B2FileBase file)
        {
            return AsyncRunHelper(() => client.HideFileAsync(bucket, file));
        }

        public static B2FileBase HideFile(this B2Client client, B2Bucket bucket, string fileName)
        {
            return AsyncRunHelper(() => client.HideFileAsync(bucket, fileName));
        }

        public static B2FileBase HideFile(this B2Client client, string bucketId, string fileName)
        {
            return AsyncRunHelper(() => client.HideFileAsync(bucketId, fileName));
        }

        public static B2FileInfo GetFileInfo(this B2Client client, B2FileBase file)
        {
            return AsyncRunHelper(() => client.GetFileInfoAsync(file));
        }

        public static B2FileInfo GetFileInfo(this B2Client client, string fileId)
        {
            return AsyncRunHelper(() => client.GetFileInfoAsync(fileId));
        }

        public static bool DeleteFile(this B2Client client, B2FileBase file)
        {
            return AsyncRunHelper(() => client.DeleteFileAsync(file));
        }

        public static bool DeleteFile(this B2Client client, B2FileInfo file)
        {
            return AsyncRunHelper(() => client.DeleteFileAsync(file));
        }

        public static bool DeleteFile(this B2Client client, string fileName, string fileId)
        {
            return AsyncRunHelper(() => client.DeleteFileAsync(fileName, fileId));
        }

        public static B2FileInfo UploadFile(this B2Client client, B2Bucket bucket, FileInfo file, string fileName,string contentType = null)
        {
            return AsyncRunHelper(() => client.UploadFileAsync(bucket, file, fileName, contentType));
        }

        public static B2FileInfo UploadFile(this B2Client client, string bucketId, FileInfo file, string fileName, string contentType = null)
        {
            return AsyncRunHelper(() => client.UploadFileAsync(bucketId, file, fileName, contentType));
        }

        public static B2FileInfo UploadFile(this B2Client client, B2Uploader uploader)
        {
            return AsyncRunHelper(() => client.UploadFileAsync(uploader));
        }

        public static B2FileDownloadResult DownloadFileHead(this B2Client client, B2FileBase file)
        {
            return AsyncRunHelper(() => client.DownloadFileHeadAsync(file));
        }

        public static B2FileDownloadResult DownloadFileHead(this B2Client client, string fileId)
        {
            return AsyncRunHelper(() => client.DownloadFileHeadAsync(fileId));
        }
        
        public static B2DownloadResult DownloadFileContent(this B2Client client, B2FileBase file)
        {
            return AsyncRunHelper(() => client.DownloadFileContentAsync(file));
        }

        public static B2DownloadResult DownloadFileContent(this B2Client client, string fileId)
        {
            return AsyncRunHelper(() => client.DownloadFileContentAsync(fileId));
        }
    }
}
