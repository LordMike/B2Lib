using System;
using System.IO;

namespace B2Lib.Objects
{
    public class B2DownloadResult : IDisposable
    {
        public Stream Stream { get; set; }

        public B2FileDownloadResult Info { get; set; }

        public void Dispose()
        {
            Stream.Close();
        }
    }
}