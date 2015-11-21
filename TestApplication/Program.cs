using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using B2Lib;
using B2Lib.Enums;
using B2Lib.Objects;

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
                client.Login("", "").Wait();
                client.SaveState("state");
            }

            string name = "test-123";
            B2Bucket bck = client.GetBucketByName(name).Result;
            if (bck == null)
            {
                bck = client.CreateBucket(name, B2BucketType.AllPrivate).Result;
            }

            List<B2FileWithSize> files = client.ListFileVersions(bck).ToList();
            Console.WriteLine(files.Count);

            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 30 }, file =>
              {
                  client.DeleteFile(file).Wait();
              });

            //client.UploadFile(bck, new FileInfo("state"), "state", new Dictionary<string, string> { { "test", "\"" } }).Wait();

            files = client.ListFileVersions(bck).ToList();
            Console.WriteLine(files.Count);


            client.SaveState("state");

            Console.WriteLine();
        }
    }
}
