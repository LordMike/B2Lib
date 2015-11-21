using System;
using System.Collections.Generic;

namespace B2Lib.Objects
{
    public class B2SaveState
    {
        public string AccountId { get; set; }
        public string AuthorizationToken { get; set; }

        public Uri ApiUrl { get; set; }
        public Uri DownloadUrl { get; set; }

        public List<B2BucketCache> BucketCache { get; set; }
    }
}