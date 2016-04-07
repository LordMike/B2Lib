using System.Collections.Generic;
using System.IO;
using B2Lib.Client;

namespace B2Lib.SyncExtensions
{
    public static class B2FileExtensions
    {
        public static B2File StartLargeFile(this B2File file)
        {
            return Utility.AsyncRunHelper(() => file.StartLargeFileAsync());
        }

        public static B2File UploadLargeFilePart(this B2File file, int partNumber, Stream source, string sha1Hash)
        {
            return Utility.AsyncRunHelper(() => file.UploadLargeFilePartAsync(partNumber, source, sha1Hash));
        }

        public static B2File FinalizeLargeFile(this B2File file)
        {
            return Utility.AsyncRunHelper(() => file.FinalizeLargeFileAsync());
        }

        public static B2File FinalizeLargeFile(this B2File file, List<string> sha1Hashes)
        {
            return Utility.AsyncRunHelper(() => file.FinalizeLargeFileAsync(sha1Hashes));
        }
        
        public static B2File UploadData(this B2File file, Stream source)
        {
            return Utility.AsyncRunHelper(() => file.UploadDataAsync(source));
        }

        public static B2File UploadFileData(this B2File file, FileInfo source)
        {
            return Utility.AsyncRunHelper(() => file.UploadFileDataAsync(source));
        }
        
        public static Stream DownloadData(this B2File file)
        {
            return Utility.AsyncRunHelper(() => file.DownloadDataAsync());
        }

        public static Stream DownloadData(this B2File file, long rangeStart, long rangeEnd)
        {
            return Utility.AsyncRunHelper(() => file.DownloadDataAsync(rangeStart, rangeEnd));
        }

        public static B2File Refresh(this B2File file)
        {
            return Utility.AsyncRunHelper(() => file.RefreshAsync());
        }

        public static bool Delete(this B2File file)
        {
            return Utility.AsyncRunHelper(() => file.DeleteAsync());
        }
    }
}
