using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using B2Lib.Enums;
using B2Lib.Exceptions;
using B2Lib.Objects;
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
            List<B2FileInfo> files = _client.ListFileVersions(_bucket).ToList();

            foreach (B2FileBase file in files)
                _client.DeleteFile(file);

            _client.DeleteBucket(_bucket);

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
        public void ListMultiplePages()
        {
            File.WriteAllText(_tmpPath, "Testworld,123");

            List<B2FileInfo> files = new List<B2FileInfo>();

            // Upload it 
            Parallel.For(1, 10, i =>
            {
                B2FileInfo file = _client.UploadFile(_bucket, new FileInfo(_tmpPath), "test-pages-" + i);
                TestFileContents(file);

                lock (files)
                files.Add(file);
            });

            // Get enumerators
            B2FilesIterator listFiles = _client.ListFiles(_bucket) as B2FilesIterator;
            B2FileVersionsIterator listFileVersions = _client.ListFileVersions(_bucket) as B2FileVersionsIterator;

            Assert.IsNotNull(listFiles);
            Assert.IsNotNull(listFileVersions);

            listFiles.PageSize = 2;
            listFileVersions.PageSize = 2;

            // Enumerate it
            List<B2FileInfo> list = listFiles.ToList();
            List<B2FileInfo> listVersions = listFileVersions.ToList();

            foreach (B2FileInfo expected in files)
            {
                Assert.IsTrue(list.Any(s => s.FileId == expected.FileId));
                Assert.IsTrue(listVersions.Any(s => s.FileId == expected.FileId));
            }

            // Delete files
            Parallel.ForEach(files, info =>
            {
                _client.DeleteFile(info);
            });
        }

        [TestMethod]
        public void UploadFileVersionsListDelete()
        {
            File.WriteAllText(_tmpPath, "Testworld,123");

            // Upload it 
            B2FileInfo file1 = _client.UploadFile(_bucket, new FileInfo(_tmpPath), "test");
            TestFileContents(file1);

            // Upload again
            B2FileInfo file2 = _client.UploadFile(_bucket, new FileInfo(_tmpPath), "test");
            TestFileContents(file2);

            // Enumerate it
            List<B2FileInfo> list = _client.ListFileVersions(_bucket).ToList();
            List<B2FileInfo> listFiles = list.Where(s => s.FileName == "test").ToList();

            Assert.AreEqual(2, listFiles.Count);
            Assert.IsTrue(listFiles.Any(s => s.FileId == file1.FileId));
            Assert.IsTrue(listFiles.Any(s => s.FileId == file2.FileId));

            foreach (B2FileInfo listFile in listFiles)
            {
                TestFileContents(listFile, B2FileAction.Upload);
            }

            // Delete file
            _client.DeleteFile(file1);
            _client.DeleteFile(file2);

            // Ensure list is blank
            list = _client.ListFileVersions(_bucket).ToList();
            Assert.IsFalse(list.Any(s => s.FileId == file1.FileId));
            Assert.IsFalse(list.Any(s => s.FileId == file2.FileId));
        }

        [TestMethod]
        public void UploadFileListDelete()
        {
            File.WriteAllText(_tmpPath, "Testworld,123");

            // Upload it 
            B2FileInfo file = _client.UploadFile(_bucket, new FileInfo(_tmpPath), "test");
            TestFileContents(file);

            // Enumerate it
            List<B2FileInfo> list = _client.ListFiles(_bucket).ToList();
            B2FileInfo listFile = list.FirstOrDefault(s => s.FileId == file.FileId);

            TestFileContents(listFile, B2FileAction.Upload);

            // Delete file
            _client.DeleteFile(file);

            // Ensure list is blank
            list = _client.ListFiles(_bucket).ToList();
            Assert.IsFalse(list.Any(s => s.FileId == file.FileId));
        }

        [TestMethod]
        public void UploadFileHideDelete()
        {
            // B2 has a weird way of working with hidden files
            // Read more here: https://www.backblaze.com/b2/docs/file_versions.html

            File.WriteAllText(_tmpPath, "Testworld,123");

            // Upload it 
            B2FileInfo file = _client.UploadFile(_bucket, new FileInfo(_tmpPath), "test");
            TestFileContents(file);

            // Enumerate it
            List<B2FileInfo> list = _client.ListFiles(_bucket).ToList();
            B2FileInfo listFile = list.FirstOrDefault(s => s.FileId == file.FileId);

            TestFileContents(listFile, B2FileAction.Upload);

            // Hide it
            B2FileBase hideFileItem = _client.HideFile(_bucket, listFile);

            // Enumerate it (it should now be hidden)
            list = _client.ListFiles(_bucket).ToList();
            listFile = list.FirstOrDefault(s => s.FileId == file.FileId);

            Assert.IsNull(listFile);

            // Enumerate versions (it should be visible)
            list = _client.ListFileVersions(_bucket).ToList();
            B2FileInfo hideFile = list.FirstOrDefault(s => s.FileId == hideFileItem.FileId);
            listFile = list.FirstOrDefault(s => s.FileId == file.FileId);

            TestFileContents(hideFile, B2FileAction.Hide);
            TestFileContents(listFile, B2FileAction.Upload);

            // Delete file
            _client.DeleteFile(file);

            // Ensure list is blank
            list = _client.ListFiles(_bucket).ToList();
            Assert.IsFalse(list.Any(s => s.FileId == file.FileId));
        }

        [TestMethod]
        public void UploadFileGetInfoDelete()
        {
            File.WriteAllText(_tmpPath, "Testworld,123");

            // Upload it 
            B2FileInfo file = _client.UploadFile(_bucket, new FileInfo(_tmpPath), "test");
            TestFileContents(file);

            // Fetch info
            B2FileInfo info = _client.GetFileInfo(file.FileId);
            TestFileContents(info);

            Assert.AreEqual(file.FileId, info.FileId);
            Assert.AreEqual(file.ContentLength, info.ContentLength);

            // Delete file
            _client.DeleteFile(info);

            // Ensure info is blank
            try
            {
                info = _client.GetFileInfo(file.FileId);

                Assert.Fail();
            }
            catch (B2Exception ex)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, ex.HttpStatusCode);
            }
        }

        [TestMethod]
        public void FileUploader()
        {
            File.WriteAllText(_tmpPath, "Testworld,123");

            // Upload it 
            B2FileInfo file;
            using (FileStream fs = File.OpenRead(_tmpPath))
            {
                B2Uploader uploader = _client.GetUploader(_bucket);

                uploader.SetFileName("test");
                uploader.SetInput(fs);
                
                uploader.CalculateSha1FromInput();

                file = _client.UploadFile(uploader);
                TestFileContents(file);
            }
            
            // Fetch info
            B2FileInfo info = _client.GetFileInfo(file.FileId);
            TestFileContents(info);

            Assert.AreEqual(file.FileId, info.FileId);
            Assert.AreEqual(file.ContentLength, info.ContentLength);

            // Delete file
            _client.DeleteFile(info);

            // Ensure info is blank
            try
            {
                info = _client.GetFileInfo(file.FileId);

                Assert.Fail();
            }
            catch (B2Exception ex)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, ex.HttpStatusCode);
            }
        }

        private void TestFileContents(B2FileInfo file)
        {
            Assert.IsNotNull(file);
            Assert.IsFalse(string.IsNullOrWhiteSpace(file.FileId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(file.FileName));
            Assert.IsFalse(string.IsNullOrWhiteSpace(file.BucketId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(file.ContentSha1));
            Assert.IsFalse(string.IsNullOrWhiteSpace(file.AccountId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(file.ContentType));
            Assert.IsTrue(file.ContentLength > 0);

            Assert.IsNotNull(file.FileInfo);
        }

        private void TestFileContents(B2FileInfo file, B2FileAction expectedAction)
        {
            Assert.IsNotNull(file);
            Assert.IsFalse(string.IsNullOrWhiteSpace(file.FileId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(file.FileName));
            Assert.IsTrue(file.UploadTimestamp > 0);

            Assert.AreEqual(expectedAction, file.Action);
            if (file.Action == B2FileAction.Upload)
                Assert.IsTrue(file.ContentLength > 0);
        }
    }
}