using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using B2Lib.Client;
using B2Lib.Enums;
using B2Lib.SyncExtensions;

namespace TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            B2Client client = new B2Client();

            if (File.Exists("state"))
                client.LoadState("state");
            else
            {
                // To protect credentials from being committed, they've been put in a text file at: MyDocuments\B2Auth.txt
                // The text goes in two lines, with AccountId being the first, and ApplicationKey the second.
                string[] lines = File.ReadAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "B2Auth.txt"));

                client.Login(lines[0], lines[1]);
                client.SaveState("state");
            }

            string name = "testbucket-lordmike-123";
            B2Bucket bck = client.GetBucketByName(name);
            if (bck == null)
            {
                bck = client.CreateBucket(name, B2BucketType.AllPrivate);
            }

            client.SaveState("state");

            string path = Path.GetTempFileName();
            File.WriteAllText(path, "Testworld".PadRight(2000, 'A'));

            //var res = bck.CreateFile("Testfile.txt");
            //res.UploadFileData(new FileInfo(path));

            List<B2FileItemBase> files = bck.GetFileVersions().ToList();
            Task.WaitAll(files.Select(s => s.DeleteAsync()).ToArray());
            
            B2LargeFile largeFile = bck.CreateLargeFile("large-file.txt");

            List<B2File> files1 = bck.GetFiles().ToList();
            List<B2FileItemBase> files2 = bck.GetFileVersions().ToList();
            List<B2LargeFile> files3 = bck.GetUnfinishedLargeFiles().ToList();

            Console.WriteLine();
        }
    }
}
