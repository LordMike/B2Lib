using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using B2Lib.Enums;
using B2Lib.Objects;
using B2Lib.Utilities;

namespace B2Lib.Client
{
    public class B2File : B2FileItemBase
    {
        private readonly B2Client _b2Client;
        private B2FileInfo _file;

        private B2Communicator.NotifyProgress _uploadNotifyDelegate;

        public B2FileState State { get; private set; }

        public override string FileId => _file.FileId;
        public override string FileName => _file.FileName;
        public override DateTime UploadTimestamp => _file.UploadTimestamp;
        public override string AccountId => _file.AccountId;
        public override string BucketId => _file.BucketId;
        public override string ContentType => _file.ContentType;
        public override Dictionary<string, string> FileInfo => _file.FileInfo;
        public B2FileAction Action => _file.Action;
        public string ContentSha1 => _file.ContentSha1;
        public long ContentLength => _file.ContentLength;

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
                Action = B2FileAction.Upload,
                FileName = newName,
                BucketId = bucketId,
                ContentType = B2Constants.AutoContenType
            };

            State = B2FileState.New;
        }

        private void ThrowIfNot(B2FileState desiredState)
        {
            if (State != desiredState)
                throw new InvalidOperationException($"The B2 File, {FileName}, was {State} and not {desiredState} (id: {FileId})");
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

        public B2File SetUploadProgressHandler(B2Communicator.NotifyProgress handler)
        {
            _uploadNotifyDelegate = handler;

            return this;
        }

        public async Task<B2File> UploadFileDataAsync(FileInfo file)
        {
            ThrowIfNot(B2FileState.New);

            B2UploadConfiguration config = _b2Client.FetchUploadConfig(BucketId);
            try
            {
                using (var fs = file.OpenRead())
                {
                    string sha1Hash;
                    using (SHA1 sha1 = SHA1.Create())
                        sha1Hash = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", "");

                    fs.Seek(0, SeekOrigin.Begin);

                    B2FileInfo result = await _b2Client.Communicator.UploadFile(config.UploadUrl, config.AuthorizationToken, fs, sha1Hash, FileName, ContentType, FileInfo, _uploadNotifyDelegate);

                    if (result.ContentSha1 != sha1Hash)
                        throw new ArgumentException("Bad transfer - hash mismatch");

                    _file = result;
                }
                State = B2FileState.Present;
            }
            finally
            {
                _b2Client.ReturnUploadConfig(config);
            }

            return this;
        }

        public async Task<B2File> UploadDataAsync(Stream source)
        {
            ThrowIfNot(B2FileState.New);

            B2UploadConfiguration config = _b2Client.FetchUploadConfig(BucketId);
            try
            {
                B2FileInfo result = await _b2Client.Communicator.UploadFile(config.UploadUrl, config.AuthorizationToken, source, ContentSha1, FileName, ContentType, FileInfo, _uploadNotifyDelegate);

                if (result.ContentSha1 != ContentSha1)
                    throw new ArgumentException("Bad transfer - hash mismatch");

                _file = result;
                State = B2FileState.Present;
            }
            finally
            {
                _b2Client.ReturnUploadConfig(config);
            }

            return this;
        }

        public async Task<Stream> DownloadDataAsync(B2Communicator.NotifyProgress progressHandler = null)
        {
            ThrowIfNot(B2FileState.Present);

            if (Action != B2FileAction.Upload)
                throw new InvalidOperationException("This file is not a file - it's most likely a 'hide' placeholder");

            B2DownloadResult res = await _b2Client.Communicator.DownloadFileContent(FileId, notifyProgress: progressHandler);

            return res.Stream;
        }
        
        public async Task<B2File> RefreshAsync()
        {
            B2FileInfo info = await _b2Client.Communicator.GetFileInfo(FileId);

            _file = info;

            return this;
        }

        public override async Task<bool> DeleteAsync()
        {
            ThrowIfNot(B2FileState.Present);

            bool res = await _b2Client.Communicator.DeleteFile(FileName, FileId);
            State = B2FileState.Deleted;

            return res;
        }
    }
}