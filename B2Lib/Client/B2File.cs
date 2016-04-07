using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using B2Lib.Enums;
using B2Lib.Objects;
using B2Lib.Utilities;

namespace B2Lib.Client
{
    public class B2File
    {
        private readonly B2Client _b2Client;
        private B2FileInfo _file;

        private ConcurrentBag<B2UploadPartConfiguration> _partUploadConfigs;

        public B2FileState State { get; private set; }

        public B2FileAction Action => _file.Action;
        public string FileId => _file.FileId;
        public string FileName => _file.FileName;
        public DateTime UploadTimestamp => _file.UploadTimestamp;
        public string AccountId => _file.AccountId;
        public string ContentSha1 => _file.ContentSha1;
        public string BucketId => _file.BucketId;
        public long ContentLength => _file.ContentLength;
        public string ContentType => _file.ContentType;
        public Dictionary<string, string> FileInfo => _file.FileInfo;

        internal B2File(B2Client b2Client, B2UnfinishedLargeFile file)
        {
            _b2Client = b2Client;
            _file = new B2FileInfo
            {
                BucketId = file.BucketId,
                AccountId = file.AccountId,
                FileId = file.FileId,
                FileName = file.FileName,
                FileInfo = file.FileInfo,
                ContentType = file.ContentType,
                UploadTimestamp = file.UploadTimestamp
            };

            State = B2FileState.Present;
            InitLargeFile();
        }

        internal B2File(B2Client b2Client, B2FileInfo file)
        {
            _b2Client = b2Client;
            _file = file;

            State = B2FileState.Present;

            if (file.Action == B2FileAction.Start)
                InitLargeFile();
        }

        internal B2File(B2Client b2Client, string bucketId, string newName, bool isLargeFile)
        {
            _b2Client = b2Client;
            _file = new B2FileInfo
            {
                FileName = newName,
                BucketId = bucketId,
                ContentType = "b2/x-auto"
            };

            State = B2FileState.New;
            if (isLargeFile)
                InitLargeFile();
        }

        private void InitLargeFile()
        {
            _file.Action = B2FileAction.Start;
            _partUploadConfigs = new ConcurrentBag<B2UploadPartConfiguration>();
        }

        internal void ReturnUploadConfig(B2UploadPartConfiguration config)
        {
            _partUploadConfigs.Add(config);
        }

        internal B2UploadPartConfiguration FetchUploadConfig()
        {
            B2UploadPartConfiguration config;
            if (_partUploadConfigs.TryTake(out config))
                return config;

            // Create new config
            B2UploadPartConfiguration res;
            try
            {
                res = _b2Client.Communicator.GetPartUploadUrl(_file.FileId).Result;
            }
            catch (AggregateException ex)
            {
                // Re-throw the inner exception
                throw ex.InnerException;
            }

            return res;
        }

        private void ThrowIfNot(B2FileState desiredState)
        {
            if (State != desiredState)
                throw new InvalidOperationException($"The B2 File, {_file.FileName}, was {State} and not {desiredState} (id: {_file.FileId})");
        }

        private void ThrowIfLargeFile(bool requireLarge)
        {
            if (Action == B2FileAction.Start && !requireLarge)
                throw new InvalidOperationException($"The B2 File, {_file.FileName}, is a large file, and it can't be for this operation (id: {_file.FileId})");
            if (!(Action == B2FileAction.Start) && requireLarge)
                throw new InvalidOperationException($"The B2 File, {_file.FileName}, is not a large file, and it must be for this operation (id: {_file.FileId})");
        }

        public B2File SetUploadContentType(string contentType)
        {
            ThrowIfNot(B2FileState.New);

            _file.ContentType = contentType;

            return this;
        }

        public B2File SetUploadSha1(string sha1)
        {
            ThrowIfNot(B2FileState.New);

            _file.ContentSha1 = sha1;

            return this;
        }

        public B2File SetUploadFileInfo(string key, string value)
        {
            ThrowIfNot(B2FileState.New);

            if (_file.FileInfo == null)
                _file.FileInfo = new Dictionary<string, string>();

            _file.FileInfo[key] = value;

            return this;
        }

        private B2LargeFilePartsIterator GetLargeFileParts()
        {
            return new B2LargeFilePartsIterator(_b2Client.Communicator, _file.FileId, 1);
        }

        public async Task<B2File> UploadFileDataAsync(FileInfo file)
        {
            ThrowIfNot(B2FileState.New);
            ThrowIfLargeFile(false);

            B2UploadConfiguration config = _b2Client.FetchUploadConfig(_file.BucketId);
            try
            {
                using (var fs = file.OpenRead())
                {
                    string sha1Hash;
                    using (SHA1 sha1 = SHA1.Create())
                        sha1Hash = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", "");

                    fs.Seek(0, SeekOrigin.Begin);

                    B2FileInfo result = _b2Client.Communicator.UploadFile(config.UploadUrl, config.AuthorizationToken, fs, sha1Hash, _file.FileName, _file.ContentType, _file.FileInfo, null).Result;

                    if (result.ContentSha1 != sha1Hash)
                        throw new ArgumentException("Bad transfer - hash mismatch");

                    _file = result;
                }
                State = B2FileState.Present;
            }
            finally
            {
                _b2Client.ReturnUploadConfig(_file.BucketId, config);
            }

            return this;
        }

        public async Task<B2File> UploadDataAsync(Stream source)
        {
            ThrowIfNot(B2FileState.New);
            ThrowIfLargeFile(false);

            B2UploadConfiguration config = _b2Client.FetchUploadConfig(_file.BucketId);
            try
            {
                B2FileInfo result = _b2Client.Communicator.UploadFile(config.UploadUrl, config.AuthorizationToken, source, _file.ContentSha1, _file.FileName, _file.ContentType, _file.FileInfo, null).Result;

                if (result.ContentSha1 != _file.ContentSha1)
                    throw new ArgumentException("Bad transfer - hash mismatch");

                _file = result;
                State = B2FileState.Present;
            }
            finally
            {
                _b2Client.ReturnUploadConfig(_file.BucketId, config);
            }

            return this;
        }

        public async Task<B2File> StartLargeFileAsync()
        {
            ThrowIfNot(B2FileState.New);
            ThrowIfLargeFile(true);

            B2FileInfo result = await _b2Client.Communicator.StartLargeFileUpload(_file.BucketId, _file.FileName, _file.ContentType, _file.FileInfo);

            _file = result;
            State = B2FileState.Present;

            return this;
        }

        public async Task<B2File> UploadLargeFilePartAsync(int partNumber, Stream source, string sha1Hash)
        {
            // TODO: Determine if the file is finished already?

            ThrowIfLargeFile(true);

            B2UploadPartConfiguration config = FetchUploadConfig();

            try
            {
                B2LargeFilePart result = await _b2Client.Communicator.UploadPart(config.UploadUrl, config.AuthorizationToken, partNumber, source, sha1Hash);

                if (result.ContentSha1 != sha1Hash)
                    throw new ArgumentException("Bad transfer - hash mismatch");
            }
            finally
            {
                ReturnUploadConfig(config);
            }

            return this;
        }

        public async Task<B2File> FinalizeLargeFileAsync()
        {
            IOrderedEnumerable<B2LargeFilePart> parts = GetLargeFileParts().OrderBy(s => s.PartNumber);

            return await FinalizeLargeFileAsync(parts.Select(s => s.ContentSha1).ToList());
        }

        public async Task<B2File> FinalizeLargeFileAsync(List<string> sha1Hashes)
        {
            // TODO: Determine if the file is finished already?

            ThrowIfLargeFile(true);

            B2FileInfo result = await _b2Client.Communicator.FinishLargeFileUpload(_file.FileId, sha1Hashes);

            _file = result;

            return this;
        }

        public async Task<Stream> DownloadDataAsync()
        {
            ThrowIfNot(B2FileState.Present);

            if (Action != B2FileAction.Upload)
                throw new InvalidOperationException("This file is not a file - it's most likely a 'hide' placeholder");

            B2DownloadResult res = await _b2Client.Communicator.DownloadFileContent(_file.FileId);

            return res.Stream;
        }

        public async Task<Stream> DownloadDataAsync(long rangeStart, long rangeEnd)
        {
            throw new NotImplementedException();
            //ThrowIfNot(B2FileState.Present);

            //if (Action != B2FileAction.Upload)
            //    throw new InvalidOperationException("This file is not a file - it's most likely a 'hide' placeholder");

            //B2DownloadResult res = await _b2Client.Communicator.DownloadFileContent(_b2Client.DownloadUrl, _file.FileId);

            //return res.Stream;
        }

        public async Task<B2File> RefreshAsync()
        {
            B2FileInfo info = _b2Client.Communicator.GetFileInfo(_file.FileId).Result;

            _file = info;

            return this;
        }

        public async Task<bool> DeleteAsync()
        {
            ThrowIfNot(B2FileState.Present);

            bool res = await _b2Client.Communicator.DeleteFile(_file.FileName, _file.FileId);
            State = B2FileState.Deleted;

            return res;
        }
    }
}