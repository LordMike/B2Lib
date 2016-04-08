using System.Collections.Generic;
using System.IO;
using B2Lib.Client;

namespace B2Lib.SyncExtensions
{
    public static class B2LargeFileExtensions
    {
        public static void UploadPart(this B2LargeFile file, int partNumber, Stream source, string sha1Hash)
        {
            Utility.AsyncRunHelper(() => file.UploadPartAsync(partNumber, source, sha1Hash));
        }

        public static B2File FinishAcceptHashes(this B2LargeFile file)
        {
            return Utility.AsyncRunHelper(() => file.FinishAcceptHashesAsync());
        }

        public static B2File Finish(this B2LargeFile file, List<string> sha1Hashes)
        {
            return Utility.AsyncRunHelper(() => file.FinishAsync(sha1Hashes));
        }
    }
}