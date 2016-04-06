using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using B2Lib.Enums;
using B2Lib.Objects;

namespace B2Lib.Client
{
    public class B2File
    {
        private readonly B2Client _b2Client;
        private B2FileInfo _file;

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

        internal B2File(B2Client b2Client, B2FileInfo file)
        {
            _b2Client = b2Client;
            _file = file;

            State = B2FileState.Present;
        }

        internal B2File(B2Client b2Client, string bucketId, string newName)
        {
            _b2Client = b2Client;
            _file = new B2FileInfo
            {
                FileName = newName,
                BucketId = bucketId,
                ContentType = "b2/x-auto"
            };

            State = B2FileState.New;
        }

        private void ThrowIfNot(B2FileState desiredState)
        {
            if (State != desiredState)
                throw new InvalidOperationException($"The B2 File, {_file.FileName}, was {State} and not {desiredState} (id: {_file.FileId})");
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

        public async Task<B2File> UploadFileDataAsync(FileInfo file)
        {
            ThrowIfNot(B2FileState.New);

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

            B2UploadConfiguration config = _b2Client.FetchUploadConfig(_file.BucketId);
            try
            {
                B2FileInfo result = _b2Client.Communicator.UploadFile(config.UploadUrl, config.AuthorizationToken, source, _file.ContentSha1, _file.FileName, _file.ContentType, _file.FileInfo, null).Result;

                _file = result;
                State = B2FileState.Present;
            }
            finally
            {
                _b2Client.ReturnUploadConfig(_file.BucketId, config);
            }

            return this;
        }

        public async Task<Stream> DownloadDataAsync()
        {
            ThrowIfNot(B2FileState.Present);

            if (Action != B2FileAction.Upload)
                throw new InvalidOperationException("This file is not a file - it's most likely a 'hide' placeholder");

            B2DownloadResult res = await _b2Client.Communicator.DownloadFileContent(_b2Client.DownloadUrl, _file.FileId);

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
            B2FileInfo info = _b2Client.Communicator.GetFileInfo(_b2Client.ApiUrl, _file.FileId).Result;

            _file = info;

            return this;
        }

        public async Task<bool> DeleteAsync()
        {
            ThrowIfNot(B2FileState.Present);

            bool res = await _b2Client.Communicator.DeleteFile(_b2Client.ApiUrl, _file.FileName, _file.FileId);
            State = B2FileState.Deleted;

            return res;
        }
    }
}