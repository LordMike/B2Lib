﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace B2Lib.Objects
{
    public class B2SaveState
    {
        public string AccountId { get; set; }
        public string AuthorizationToken { get; set; }
        public long MinimumPartSize { get; set; }

        public Uri ApiUrl { get; set; }
        public Uri DownloadUrl { get; set; }

        public List<B2BucketObject> BucketCache { get; set; }

        public void CopyTo(B2SaveState other)
        {
            other.AccountId = AccountId;
            other.AuthorizationToken = AuthorizationToken;
            other.MinimumPartSize = MinimumPartSize;

            other.ApiUrl = ApiUrl;
            other.DownloadUrl = DownloadUrl;

            other.BucketCache = BucketCache.ToList();
        }

        public override string ToString()
        {
            return $"AccountId: {AccountId}, ApiUrl: {ApiUrl}, BucketCache: {BucketCache?.Count}";
        }
    }
}