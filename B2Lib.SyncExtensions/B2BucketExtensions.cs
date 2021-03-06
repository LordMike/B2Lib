﻿using System.Collections.Generic;
using B2Lib.Client;
using B2Lib.Enums;

namespace B2Lib.SyncExtensions
{
    public static class B2BucketExtensions
    {
        public static bool Delete(this B2Bucket bucket)
        {
            return Utility.AsyncRunHelper(bucket.DeleteAsync);
        }

        public static bool Update(this B2Bucket bucket, B2BucketType newType)
        {
            return Utility.AsyncRunHelper(() => bucket.UpdateAsync(newType));
        }

        public static bool HideFile(this B2Bucket bucket, string fileName)
        {
            return Utility.AsyncRunHelper(() => bucket.HideFileAsync(fileName));
        }

        public static B2LargeFile CreateLargeFile(this B2Bucket bucket, string newName, string contentType = null, Dictionary<string, string> fileInfo = null)
        {
            return Utility.AsyncRunHelper(() => bucket.CreateLargeFileAsync(newName, contentType, fileInfo));
        }

        public static B2File GetFile(this B2Bucket bucket, string fileId)
        {
            return Utility.AsyncRunHelper(() => bucket.GetFileAsync(fileId));
        }

        public static B2LargeFile GetLargeFile(this B2Bucket bucket, string fileId)
        {
            return Utility.AsyncRunHelper(() => bucket.GetLargeFileAsync(fileId));
        }
    }
}