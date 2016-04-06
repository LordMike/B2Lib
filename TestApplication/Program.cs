using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using B2Lib;
using B2Lib.Enums;
using B2Lib.Objects;
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

            string name = "test-123";
            B2Bucket bck = client.GetBucketByName(name);
            if (bck == null)
            {
                bck = client.CreateBucket(name, B2BucketType.AllPrivate);
            }

            List<B2FileInfo> files = client.ListFileVersions(bck).ToList();
            Console.WriteLine(files.Count);

            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 30 }, file =>
            {
                client.DeleteFile(file);
            });

            //client.UploadFile(bck, new FileInfo("state"), "state", new Dictionary<string, string> { { "test", "\"" } }).Wait();

            files = client.ListFileVersions(bck).ToList();
            Console.WriteLine(files.Count);


            client.SaveState("state");

            Console.WriteLine();
        }
    }
}
