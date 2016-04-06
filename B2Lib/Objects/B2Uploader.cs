using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace B2Lib.Objects
{
    public class B2Uploader
    {
        public delegate void NotifyProgress(long position, long length);

        internal string BucketId { get; }

        internal string FileName { get; private set; }
        internal Stream InputStream { get; private set; }
        internal string ContentType { get; private set; }
        internal string Sha1 { get; private set; }
        internal NotifyProgress NotifyDelegate { get; private set; }

        internal Dictionary<string, string> Infoes { get; }

        public B2Uploader(string bucketId)
        {
            BucketId = bucketId;
            ContentType = "b2/x-auto";
            Infoes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public B2Uploader SetInfo(string key, string value)
        {
            Infoes[key] = value;

            return this;
        }

        public B2Uploader SetExpectedSha1(string sha1)
        {
            Sha1 = sha1;

            return this;
        }

        public B2Uploader SetExpectedSha1(byte[] sha1)
        {
            return SetExpectedSha1(BitConverter.ToString(sha1).Replace("-", ""));
        }

        public B2Uploader SetInput(Stream stream)
        {
            InputStream = stream;

            return this;
        }

        public B2Uploader CalculateSha1FromInput()
        {
            byte[] hash;
            using (SHA1 sha1 = SHA1.Create())
            {
                InputStream.Seek(0, SeekOrigin.Begin);
                hash = sha1.ComputeHash(InputStream);
                InputStream.Seek(0, SeekOrigin.Begin);
            }

            return SetExpectedSha1(hash);
        }

        public B2Uploader SetFileName(string fileName)
        {
            FileName = fileName;

            return this;
        }

        public B2Uploader SetNotificationAction(NotifyProgress method)
        {
            NotifyDelegate = method;

            return this;
        }

        public B2Uploader SetContentType(string contentType)
        {
            ContentType = contentType;

            return this;
        }
    }
}