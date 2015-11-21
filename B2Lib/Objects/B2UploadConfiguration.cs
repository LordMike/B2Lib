using System;

namespace B2Lib.Objects
{
    public class B2UploadConfiguration
    {
        public string BucketId { get; set; }

        public Uri UploadUrl { get; set; }

        public string AuthorizationToken { get; set; }
    }
}