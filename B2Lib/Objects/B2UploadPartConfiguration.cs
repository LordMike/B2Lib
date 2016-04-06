using System;

namespace B2Lib.Objects
{
    public class B2UploadPartConfiguration
    {
        public string FileId { get; set; }

        public Uri UploadUrl { get; set; }

        public string AuthorizationToken { get; set; }
    }
}