using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using B2Lib.Client;
using B2Lib.Enums;
using B2Lib.Exceptions;
using B2Lib.SyncExtensions;
using B2Lib.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace B2Lib.Tests
{
    [TestClass]
    public class FileTests
    {
        private static B2Client _client;
        private static B2Bucket _bucket;
        private static string _tmpPath;

        [ClassInitialize]
        public static void TestSetup(TestContext context)
        {
            _client = new B2Client();
            _client.Login(TestConfig.AccountId, TestConfig.ApplicationKey);

            try
            {
                _bucket = _client.CreateBucket("test-bucket-b2lib", B2BucketType.AllPrivate);
            }
            catch (Exception)
            {
                _bucket = _client.GetBucketByName("test-bucket-b2lib");
            }

            _tmpPath = Path.GetRandomFileName();
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            List<B2FileItemBase> files = _bucket.GetFileVersions().ToList();

            foreach (B2FileItemBase file in files)
                file.Delete();

            _bucket.Delete();

            try
            {
                if (File.Exists(_tmpPath))
                    File.Delete(_tmpPath);
            }
            catch (Exception)
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(B2Exception))]
        public void GetMissingFile()
        {
            B2File res = _bucket.GetFile("4_z4b156694d986f47c54320610_f200866faa871bf5f_d20160408_m223411_c001_v0001022_t0052");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(B2Exception))]
        public void GetMissingLargeFile()
        {
            B2LargeFile res = _bucket.GetLargeFile("4_z4b156694d986f47c54320610_f200866faa871bf5f_d20160408_m223411_c001_v0001022_t0052");

            Assert.Fail();
        }

        [TestMethod]
        public void ListMultiplePages()
        {
            File.WriteAllText(_tmpPath, "Testworld,123");

            List<B2File> files = new List<B2File>();

            // Upload it 
            Parallel.For(1, 10, i =>
            {
                B2File file = _bucket.CreateFile("test-pages-" + i);

                file.UploadFileData(new FileInfo(_tmpPath));

                TestFileContents(file);

                lock (files)
                files.Add(file);
            });

            // Get enumerators
            B2FilesIterator listFiles = _bucket.GetFiles();
            B2FileVersionsIterator listFileVersions = _bucket.GetFileVersions();

            Assert.IsNotNull(listFiles);
            Assert.IsNotNull(listFileVersions);

            listFiles.PageSize = 2;
            listFileVersions.PageSize = 2;

            // Enumerate it
            List<B2File> list = listFiles.ToList();
            List<B2FileItemBase> listVersions = listFileVersions.ToList();

            foreach (B2File expected in files)
            {
                Assert.IsTrue(list.Any(s => s.FileId == expected.FileId));
                Assert.IsTrue(listVersions.Any(s => s.FileId == expected.FileId));
            }

            // Delete files
            Parallel.ForEach(files, info =>
            {
                info.Delete();
            });
        }

        [TestMethod]
        public void UploadFileVersionsListDelete()
        {
            File.WriteAllText(_tmpPath, "Testworld,123");

            // Upload it 
            B2File file1 = _bucket.CreateFile("test").UploadFileData(new FileInfo(_tmpPath));
            TestFileContents(file1);

            // Upload again
            B2File file2 = _bucket.CreateFile("test").UploadFileData(new FileInfo(_tmpPath));
            TestFileContents(file2);

            // Enumerate it
            List<B2FileItemBase> list = _bucket.GetFileVersions().ToList();
            List<B2FileItemBase> listFiles = list.Where(s => s.FileName == "test").ToList();

            Assert.AreEqual(2, listFiles.Count);
            Assert.IsTrue(listFiles.Any(s => s.FileId == file1.FileId));
            Assert.IsTrue(listFiles.Any(s => s.FileId == file2.FileId));

            foreach (B2FileItemBase b2FileItemBase in listFiles)
            {
                B2File listFile = (B2File)b2FileItemBase;
                TestFileContents(listFile, B2FileAction.Upload);
            }

            // Delete file
            file1.Delete();
            file2.Delete();

            // Ensure list is blank
            list = _bucket.GetFileVersions().ToList();
            Assert.IsFalse(list.Any(s => s.FileId == file1.FileId));
            Assert.IsFalse(list.Any(s => s.FileId == file2.FileId));
        }

        [TestMethod]
        public void UploadFileListDelete()
        {
            File.WriteAllText(_tmpPath, "Testworld,123");

            // Upload it 
            B2File file = _bucket.CreateFile("test").UploadFileData(new FileInfo(_tmpPath));
            TestFileContents(file);

            // Enumerate it
            List<B2File> list = _bucket.GetFiles().ToList();
            B2File listFile = list.FirstOrDefault(s => s.FileId == file.FileId);

            TestFileContents(listFile, B2FileAction.Upload);

            // Delete file
            file.Delete();

            // Ensure list is blank
            list = _bucket.GetFiles().ToList();
            Assert.IsFalse(list.Any(s => s.FileId == file.FileId));
        }

        [TestMethod]
        public void UploadFileHideDelete()
        {
            // B2 has a weird way of working with hidden files
            // Read more here: https://www.backblaze.com/b2/docs/file_versions.html

            File.WriteAllText(_tmpPath, "Testworld,123");

            // Upload it 
            B2File originalFile = _bucket.CreateFile("test").UploadFileData(new FileInfo(_tmpPath));
            TestFileContents(originalFile);

            // Enumerate it
            List<B2File> list = _bucket.GetFiles().ToList();
            B2File listFile = list.FirstOrDefault(s => s.FileId == originalFile.FileId);

            TestFileContents(listFile, B2FileAction.Upload);

            // Hide it
            _bucket.HideFile(originalFile.FileName);

            // Enumerate it (it should now be hidden)
            list = _bucket.GetFiles().ToList();
            listFile = list.FirstOrDefault(s => s.FileId == originalFile.FileId);

            Assert.IsNull(listFile);

            // Enumerate versions (it should be visible)
            List<B2FileItemBase> versions = _bucket.GetFileVersions().ToList();
            B2File hideFile = versions.OfType<B2File>().FirstOrDefault(s => s.FileName == originalFile.FileName && s.Action == B2FileAction.Hide);
            listFile = versions.OfType<B2File>().FirstOrDefault(s => s.FileId == originalFile.FileId);

            TestFileContents(hideFile, B2FileAction.Hide);
            TestFileContents(listFile, B2FileAction.Upload);

            // Delete file
            originalFile.Delete();

            // Ensure list is blank
            List<B2FileItemBase> blankFilesList = _bucket.GetFileVersions().ToList();
            Assert.IsFalse(blankFilesList.Any(s => s.FileId == originalFile.FileId));
        }

        [TestMethod]
        public void UploadFileGetInfoDelete()
        {
            File.WriteAllText(_tmpPath, "Testworld,123");

            // Upload it 
            B2File file = _bucket.CreateFile("test").UploadFileData(new FileInfo(_tmpPath));
            TestFileContents(file);

            // Fetch info
            B2File info = _bucket.GetFiles().FirstOrDefault(s => s.FileId == file.FileId);
            TestFileContents(info);

            Assert.AreEqual(file.FileId, info.FileId);
            Assert.AreEqual(file.ContentLength, info.ContentLength);

            // Delete file
            info.Delete();

            // Ensure info is blank
            try
            {
                info = file.Refresh();

                Assert.Fail();
            }
            catch (B2Exception ex)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, ex.HttpStatusCode);
            }
        }

        private void TestFileContents(B2FileItemBase file)
        {
            Assert.IsNotNull(file);
            Assert.IsFalse(string.IsNullOrWhiteSpace(file.FileId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(file.FileName));
            Assert.IsFalse(string.IsNullOrWhiteSpace(file.BucketId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(file.AccountId));
            Assert.IsTrue(file.UploadTimestamp > DateTime.MinValue);

            Assert.IsNotNull(file.FileInfo);
        }

        private void TestFileContents(B2File file, B2FileAction expectedAction)
        {
            TestFileContents(file);
            Assert.IsTrue(Enum.IsDefined(typeof(B2FileAction), file.Action));

            if (file.Action == B2FileAction.Hide)
            {
                Assert.AreEqual(0, file.ContentLength);
                Assert.IsTrue(string.IsNullOrWhiteSpace(file.ContentSha1));
                Assert.IsTrue(string.IsNullOrWhiteSpace(file.ContentType));
            }
            else if (file.Action == B2FileAction.Upload)
            {
                Assert.IsTrue(file.ContentLength > 0);
                Assert.IsFalse(string.IsNullOrWhiteSpace(file.ContentSha1));
                Assert.IsFalse(string.IsNullOrWhiteSpace(file.ContentType));
            }
            else
            {
                Assert.Fail();
            }

            Assert.AreEqual(expectedAction, file.Action);
            if (file.Action == B2FileAction.Upload)
                Assert.IsTrue(file.ContentLength > 0);
        }
    }
}