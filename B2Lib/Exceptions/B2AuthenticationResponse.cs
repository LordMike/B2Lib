using System;

namespace B2Lib.Exceptions
{
    public class B2AuthenticationResponse
    {
        public string AccountId { get; set; }

        public string AuthorizationToken { get; set; }

        public Uri ApiUrl { get; set; }

        public Uri DownloadUrl { get; set; }

        public long MinimumPartSize { get; set; }
    }
}