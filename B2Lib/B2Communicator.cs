using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using B2Lib.Enums;
using B2Lib.Exceptions;
using B2Lib.Objects;
using B2Lib.Utilities;
using Newtonsoft.Json;

namespace B2Lib
{
    public class B2Communicator
    {
        public delegate void NotifyProgress(long position, long length);

        private static readonly Uri UriAuth = new Uri("https://api.backblaze.com/b2api/v1/b2_authorize_account");

        public TimeSpan TimeoutMeta { get; set; }
        public TimeSpan TimeoutData { get; set; }

        public string AuthToken { get; set; }
        public Uri ApiUri { get; set; }
        public Uri DownloadUri { get; set; }

        public B2Communicator()
        {
            TimeoutMeta = TimeSpan.FromMinutes(1);
            TimeoutData = TimeSpan.FromMinutes(30);
        }

        private HttpClient GetHttpClient(bool isDataRequest, HttpMessageHandler handler = null)
        {
            HttpClient res = handler != null ? new HttpClient(handler) : new HttpClient();

            res.Timeout = isDataRequest ? TimeoutData : TimeoutMeta;

            return res;
        }

        private async Task HandleErrorResponse(HttpResponseMessage resp)
        {
            B2Error error = JsonConvert.DeserializeObject<B2Error>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));

            throw new B2Exception(error.Message)
            {
                HttpStatusCode = resp.StatusCode,
                ErrorCode = error.Code
            };
        }

        private async Task<HttpResponseMessage> InternalRequest(Uri apiUri, string path, object body)
        {
            if (string.IsNullOrEmpty(AuthToken))
                throw new ArgumentException("Value must be set", nameof(AuthToken));

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, new Uri(ApiUri, path));

            msg.Headers.Authorization = new AuthenticationHeaderValue(AuthToken);

            if (body != null)
            {
                string reqBody = JsonConvert.SerializeObject(body);
                msg.Content = new StringContent(reqBody);
            }

            HttpResponseMessage resp = await GetHttpClient(false).SendAsync(msg).ConfigureAwait(false);

            if (resp.StatusCode != HttpStatusCode.OK)
                await HandleErrorResponse(resp);

            return resp;
        }

        /// <summary>
        /// Calls b2_authorize_account
        /// 
        /// https://www.backblaze.com/b2/docs/b2_authorize_account.html
        /// </summary>
        public async Task<B2AuthenticationResponse> AuthorizeAccount(string accountId, string applicationKey)
        {
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, UriAuth);

            msg.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(accountId + ":" + applicationKey)));

            HttpResponseMessage resp = await GetHttpClient(false).SendAsync(msg).ConfigureAwait(false);

            if (resp.StatusCode != HttpStatusCode.OK)
                await HandleErrorResponse(resp);

            return JsonConvert.DeserializeObject<B2AuthenticationResponse>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Calls b2_list_buckets
        /// 
        /// https://www.backblaze.com/b2/docs/b2_list_buckets.html
        /// </summary>
        public async Task<List<B2BucketObject>> ListBuckets(string accountId)
        {
            HttpResponseMessage resp = await InternalRequest(ApiUri, "/b2api/v1/b2_list_buckets", new { accountId });

            return JsonConvert.DeserializeObject<B2BucketList>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false)).Buckets;
        }

        /// <summary>
        /// Calls b2_create_bucket
        /// 
        /// https://www.backblaze.com/b2/docs/b2_create_bucket.html
        /// </summary>
        public async Task<B2BucketObject> CreateBucket(string accountId, string name, B2BucketType bucketType)
        {
            HttpResponseMessage resp = await InternalRequest(ApiUri, "/b2api/v1/b2_create_bucket", new { accountId, bucketName = name, bucketType = bucketType.GetDescription() });

            return JsonConvert.DeserializeObject<B2BucketObject>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Calls b2_delete_bucket
        /// 
        /// https://www.backblaze.com/b2/docs/b2_delete_bucket.html
        /// </summary>
        public async Task<B2BucketObject> DeleteBucket(string accountId, string bucketId)
        {
            HttpResponseMessage resp = await InternalRequest(ApiUri, "/b2api/v1/b2_delete_bucket", new { accountId, bucketId });

            return JsonConvert.DeserializeObject<B2BucketObject>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Calls b2_update_bucket
        /// 
        /// https://www.backblaze.com/b2/docs/b2_update_bucket.html
        /// </summary>
        public async Task<B2BucketObject> UpdateBucket(string accountId, string bucketId, B2BucketType bucketType)
        {
            HttpResponseMessage resp = await InternalRequest(ApiUri, "/b2api/v1/b2_update_bucket", new { accountId, bucketId, bucketType = bucketType.GetDescription() });

            return JsonConvert.DeserializeObject<B2BucketObject>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Calls b2_hide_file
        /// 
        /// https://www.backblaze.com/b2/docs/b2_hide_file.html
        /// </summary>
        public async Task<B2FileBase> HideFile(string bucketId, string fileName)
        {
            HttpResponseMessage resp = await InternalRequest(ApiUri, "/b2api/v1/b2_hide_file", new { bucketId, fileName });

            return JsonConvert.DeserializeObject<B2FileBase>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Calls b2_delete_file_version
        /// 
        /// https://www.backblaze.com/b2/docs/b2_delete_file_version.html
        /// </summary>
        public async Task<bool> DeleteFile(string fileName, string fileId)
        {
            HttpResponseMessage resp = await InternalRequest(ApiUri, "/b2api/v1/b2_delete_file_version", new { fileId, fileName });

            return resp.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Calls b2_get_file_info
        /// 
        /// https://www.backblaze.com/b2/docs/b2_get_file_info.html
        /// </summary>
        public async Task<B2FileInfo> GetFileInfo(string fileId)
        {
            HttpResponseMessage resp = await InternalRequest(ApiUri, "/b2api/v1/b2_get_file_info", new { fileId });

            return JsonConvert.DeserializeObject<B2FileInfo>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Calls b2_list_file_names
        /// 
        /// https://www.backblaze.com/b2/docs/b2_list_file_names.html
        /// </summary>
        public async Task<B2FileListContainer> ListFiles(string bucketId, string startFileName = null, int maxFileCount = 100)
        {
            HttpResponseMessage resp = await InternalRequest(ApiUri, "/b2api/v1/b2_list_file_names", new { bucketId, startFileName, maxFileCount });

            return JsonConvert.DeserializeObject<B2FileListContainer>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Calls b2_list_file_versions
        /// 
        /// https://www.backblaze.com/b2/docs/b2_list_file_versions.html
        /// </summary>
        public async Task<B2FileListContainer> ListFileVersions(string bucketId, string startFileName = null, string startFileId = null, int maxFileCount = 100)
        {
            HttpResponseMessage resp = await InternalRequest(ApiUri, "/b2api/v1/b2_list_file_versions", new { bucketId, startFileName, maxFileCount, startFileId });

            return JsonConvert.DeserializeObject<B2FileListContainer>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Calls b2_get_upload_url
        /// 
        /// https://www.backblaze.com/b2/docs/b2_get_upload_url.html
        /// </summary>
        public async Task<B2UploadConfiguration> GetUploadUrl(string bucketId)
        {
            HttpResponseMessage resp = await InternalRequest(ApiUri, "/b2api/v1/b2_get_upload_url", new { bucketId });

            return JsonConvert.DeserializeObject<B2UploadConfiguration>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Calls b2_upload_file
        /// Needs an Upload Url
        /// 
        /// https://www.backblaze.com/b2/docs/b2_upload_file.html
        /// </summary>
        public async Task<B2FileInfo> UploadFile(Uri uploadUri, string uploadToken, Stream inputStream, string sha1, string fileName, string contentType, Dictionary<string, string> fileInfo, NotifyProgress notifyDelegate)
        {
            // Pre-checks
            if (inputStream == null)
                throw new ArgumentNullException("Stream must be set");

            if (string.IsNullOrEmpty(sha1) || sha1.Length != 40)
                throw new ArgumentException("SHA1 must be set or computed");

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Filename must be set");

            if (string.IsNullOrEmpty(contentType))
                contentType = B2Constants.AutoContenType;

            // Prepare
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uploadUri);

            msg.Headers.Authorization = new AuthenticationHeaderValue(uploadToken);
            msg.Headers.Add("X-Bz-File-Name", WebUtility.UrlEncode(fileName));
            msg.Headers.Add("X-Bz-Content-Sha1", sha1);

            // Upload
            if (fileInfo != null)
            {
                foreach (KeyValuePair<string, string> info in fileInfo)
                    msg.Headers.Add("X-Bz-Info-" + info.Key, WebUtility.UrlEncode(info.Value));
            }

            msg.Content = new StreamContent(inputStream);
            msg.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            ProgressMessageHandler progressMessageHandler = new ProgressMessageHandler(new HttpClientHandler());

            EventHandler<HttpProgressEventArgs> progress = null;
            if (notifyDelegate != null)
            {
                progress = (sender, args) =>
                {
                    notifyDelegate(args.TotalBytes ?? 0, args.TotalBytes ?? 0);
                };

                progressMessageHandler.HttpSendProgress += progress;
            }

            try
            {
                using (HttpClient http = GetHttpClient(true, progressMessageHandler))
                {
                    HttpResponseMessage resp = await http.SendAsync(msg).ConfigureAwait(false);

                    if (resp.StatusCode != HttpStatusCode.OK)
                        await HandleErrorResponse(resp);

                    return JsonConvert.DeserializeObject<B2FileInfo>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
            }
            finally
            {
                if (progress != null)
                    progressMessageHandler.HttpSendProgress -= progress;
            }
        }

        /// <summary>
        /// Calls b2_upload_file
        /// Needs an Upload Url
        /// 
        /// https://www.backblaze.com/b2/docs/b2_upload_file.html
        /// </summary>
        public async Task<B2FileInfo> UploadFile(Uri uploadUri, string uploadToken, Stream stream, string fileName, string sha1, string contentType = null)
        {
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uploadUri);

            msg.Headers.Authorization = new AuthenticationHeaderValue(uploadToken);
            msg.Headers.Add("X-Bz-File-Name", WebUtility.UrlEncode(fileName));
            msg.Headers.Add("X-Bz-Content-Sha1", sha1);

            msg.Content = new StreamContent(stream);
            msg.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? B2Constants.AutoContenType);

            HttpResponseMessage resp = await GetHttpClient(true).SendAsync(msg).ConfigureAwait(false);

            if (resp.StatusCode != HttpStatusCode.OK)
                await HandleErrorResponse(resp);

            return JsonConvert.DeserializeObject<B2FileInfo>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Calls b2_download_file_by_id
        /// 
        /// https://www.backblaze.com/b2/docs/b2_download_file_by_id.html
        /// </summary>
        public async Task<B2DownloadResult> DownloadFileContent(string fileId, string overrideAuthToken = null, NotifyProgress notifyProgress = null)
        {
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, new Uri(DownloadUri, "/b2api/v1/b2_download_file_by_id?fileId=" + fileId));

            if (!string.IsNullOrEmpty(overrideAuthToken) || !string.IsNullOrEmpty(AuthToken))
                msg.Headers.Authorization = new AuthenticationHeaderValue(overrideAuthToken ?? AuthToken);

            ProgressMessageHandler progressMessageHandler = new ProgressMessageHandler(new HttpClientHandler());

            EventHandler<HttpProgressEventArgs> progress = null;
            if (notifyProgress != null)
            {
                progress = (sender, args) =>
                {
                    notifyProgress(args.TotalBytes ?? 0, args.TotalBytes ?? 0);
                };

                progressMessageHandler.HttpSendProgress += progress;
            }

            HttpResponseMessage resp = GetHttpClient(true, progressMessageHandler).SendAsync(msg, HttpCompletionOption.ResponseHeadersRead).Result;

            if (resp.StatusCode != HttpStatusCode.OK)
                HandleErrorResponse(resp).Wait();

            B2FileDownloadResult info = new B2FileDownloadResult();

            info.FileId = resp.Headers.GetValues("X-Bz-File-Id").First();
            info.FileName = resp.Headers.GetValues("X-Bz-File-Name").First();
            info.ContentSha1 = resp.Headers.GetValues("X-Bz-Content-Sha1").First();

            Debug.Assert(resp.Content.Headers.ContentLength != null, "resp.Content.Headers.ContentLength != null");

            info.ContentLength = resp.Content.Headers.ContentLength.Value;
            info.ContentType = resp.Content.Headers.ContentType.MediaType;

            info.FileInfo = new Dictionary<string, string>();

            foreach (KeyValuePair<string, IEnumerable<string>> pair in resp.Headers)
            {
                if (!pair.Key.StartsWith("X-Bz-Info-"))
                    continue;

                info.FileInfo[pair.Key.Substring("X-Bz-Info-".Length)] = pair.Value.First();
            }

            B2DownloadResult result = new B2DownloadResult();

            result.Info = info;
            result.Stream = await resp.Content.ReadAsStreamAsync();

            return result;
        }


        /// <summary>
        /// Calls b2_get_upload_part_url
        /// 
        /// https://www.backblaze.com/b2/docs/b2_get_upload_part_url.html
        /// </summary>
        public async Task<B2UploadPartConfiguration> GetPartUploadUrl(string fileId)
        {
            HttpResponseMessage resp = await InternalRequest(ApiUri, "/b2api/v1/b2_get_upload_part_url", new { fileId });

            return JsonConvert.DeserializeObject<B2UploadPartConfiguration>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Calls b2_cancel_large_file
        /// 
        /// https://www.backblaze.com/b2/docs/b2_cancel_large_file.html
        /// </summary>
        public async Task<B2CanceledLargeFile> CancelLargeFileUpload(string fileId)
        {
            HttpResponseMessage resp = await InternalRequest(ApiUri, "/b2api/v1/b2_cancel_large_file", new { fileId });

            return JsonConvert.DeserializeObject<B2CanceledLargeFile>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Calls b2_finish_large_file
        /// 
        /// https://www.backblaze.com/b2/docs/b2_finish_large_file.html
        /// </summary>
        public async Task<B2FileInfo> FinishLargeFileUpload(string fileId, List<string> partSha1Array)
        {
            HttpResponseMessage resp = await InternalRequest(ApiUri, "/b2api/v1/b2_finish_large_file", new { fileId, partSha1Array });

            return JsonConvert.DeserializeObject<B2FileInfo>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Calls b2_start_large_file
        /// 
        /// https://www.backblaze.com/b2/docs/b2_start_large_file.html
        /// </summary>
        public async Task<B2UnfinishedLargeFile> StartLargeFileUpload(string bucketId, string fileName, string contentType, Dictionary<string, string> fileInfo)
        {
            // Pre-checks
            if (string.IsNullOrEmpty(bucketId))
                throw new ArgumentException("BucketId must be set");

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Filename must be set");

            if (string.IsNullOrEmpty(contentType))
                contentType = B2Constants.AutoContenType;

            // Prepare
            HttpResponseMessage resp = await InternalRequest(ApiUri, "/b2api/v1/b2_start_large_file", new
            {
                bucketId,
                fileName,
                contentType,
                fileInfo
            });

            B2UnfinishedLargeFile file = JsonConvert.DeserializeObject<B2UnfinishedLargeFile>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));

            return file;
        }

        /// <summary>
        /// Calls b2_list_parts
        /// 
        /// https://www.backblaze.com/b2/docs/b2_list_parts.html
        /// </summary>
        public async Task<B2LargeFilePartsContainer> ListLargeFileParts(string fileId, int startPartNumber = 0, int maxPartCount = 100)
        {
            HttpResponseMessage resp = await InternalRequest(ApiUri, "/b2api/v1/b2_list_parts", new { fileId, startPartNumber, maxPartCount });

            return JsonConvert.DeserializeObject<B2LargeFilePartsContainer>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Calls b2_list_unfinished_large_files
        /// 
        /// https://www.backblaze.com/b2/docs/b2_list_unfinished_large_files.html
        /// </summary>
        public async Task<B2UnfinishedLargeFilesContainer> ListUnfinishedLargeFiles(string bucketId, string startFileId = null, int maxFileCount = 100)
        {
            HttpResponseMessage resp = await InternalRequest(ApiUri, "/b2api/v1/b2_list_unfinished_large_files", new { bucketId, startFileId, maxFileCount });

            return JsonConvert.DeserializeObject<B2UnfinishedLargeFilesContainer>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Calls b2_upload_part
        /// 
        /// https://www.backblaze.com/b2/docs/b2_upload_part.html
        /// </summary>
        public async Task<B2LargeFilePart> UploadPart(Uri uploadUri, string uploadToken, int partNumber, Stream stream, string sha1)
        {
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uploadUri);

            msg.Headers.Authorization = new AuthenticationHeaderValue(uploadToken);
            msg.Headers.Add("X-Bz-Content-Sha1", sha1);
            msg.Headers.Add("X-Bz-Part-Number", partNumber.ToString());

            msg.Content = new StreamContent(stream);

            HttpResponseMessage resp = await GetHttpClient(true).SendAsync(msg).ConfigureAwait(false);

            if (resp.StatusCode != HttpStatusCode.OK)
                await HandleErrorResponse(resp);

            B2LargeFilePart result = JsonConvert.DeserializeObject<B2LargeFilePart>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));

            result.UploadTimestamp = DateTime.UtcNow;

            return result;
        }
    }
}
