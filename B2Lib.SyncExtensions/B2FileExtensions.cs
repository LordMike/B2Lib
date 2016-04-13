using System.IO;
using B2Lib.Client;

namespace B2Lib.SyncExtensions
{
    public static class B2FileExtensions
    {
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
    }
}
