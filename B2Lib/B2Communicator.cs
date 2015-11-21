using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    public static class B2Communicator
    {
        private static readonly Uri UriAuth = new Uri("https://api.backblaze.com/b2api/v1/b2_authorize_account");

        private static async Task HandleErrorResponse(HttpResponseMessage resp)
        {
            B2Error error = JsonConvert.DeserializeObject<B2Error>(await resp.Content.ReadAsStringAsync());

            throw new B2Exception(error.Message)
            {
                HttpStatusCode = resp.StatusCode
            };
        }

        private async static Task<HttpResponseMessage> InternalRequest(Uri apiUri, string authToken, string path, object body)
        {
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, new Uri(apiUri, path));

            msg.Headers.Authorization = new AuthenticationHeaderValue(authToken);

            if (body != null)
            {
                string reqBody = JsonConvert.SerializeObject(body);
                msg.Content = new StringContent(reqBody);
            }

            HttpResponseMessage resp = await new HttpClient().SendAsync(msg);

            if (resp.StatusCode != HttpStatusCode.OK)
                await HandleErrorResponse(resp);

            return resp;
        }

        public static async Task<B2AuthenticationResponse> Login(string accountId, string applicationKey)
        {
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, UriAuth);

            msg.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(accountId + ":" + applicationKey)));

            HttpResponseMessage resp = await new HttpClient().SendAsync(msg);

            if (resp.StatusCode != HttpStatusCode.OK)
                await HandleErrorResponse(resp);

            return JsonConvert.DeserializeObject<B2AuthenticationResponse>(await resp.Content.ReadAsStringAsync());
        }

        public static async Task<List<B2Bucket>> ListBuckets(Uri apiUri, string authToken, string accountId)
        {
            HttpResponseMessage resp = await InternalRequest(apiUri, authToken, "/b2api/v1/b2_list_buckets", new { accountId });

            return JsonConvert.DeserializeObject<B2BucketList>(await resp.Content.ReadAsStringAsync()).Buckets;
        }

        public static async Task<B2Bucket> CreateBucket(Uri apiUri, string authToken, string accountId, string name, B2BucketType bucketType)
        {
            HttpResponseMessage resp = await InternalRequest(apiUri, authToken, "/b2api/v1/b2_create_bucket", new { accountId, bucketName = name, bucketType = bucketType.GetDescription() });

            return JsonConvert.DeserializeObject<B2Bucket>(await resp.Content.ReadAsStringAsync());
        }

        public static async Task<B2Bucket> DeleteBucket(Uri apiUri, string authToken, string accountId, string bucketId)
        {
            HttpResponseMessage resp = await InternalRequest(apiUri, authToken, "/b2api/v1/b2_delete_bucket", new { accountId, bucketId });

            return JsonConvert.DeserializeObject<B2Bucket>(await resp.Content.ReadAsStringAsync());
        }

        public static async Task<B2Bucket> UpdateBucket(Uri apiUri, string authToken, string accountId, string bucketId, B2BucketType bucketType)
        {
            HttpResponseMessage resp = await InternalRequest(apiUri, authToken, "/b2api/v1/b2_update_bucket", new { accountId, bucketId, bucketType = bucketType.GetDescription() });

            return JsonConvert.DeserializeObject<B2Bucket>(await resp.Content.ReadAsStringAsync());
        }

        public static async Task<B2FileBase> HideFile(Uri apiUri, string authToken, string bucketId, string fileName)
        {
            HttpResponseMessage resp = await InternalRequest(apiUri, authToken, "/b2api/v1/b2_hide_file", new { bucketId, fileName });

            return JsonConvert.DeserializeObject<B2FileBase>(await resp.Content.ReadAsStringAsync());
        }

        public static async Task<bool> DeleteFile(Uri apiUri, string authToken, string fileName, string fileId)
        {
            HttpResponseMessage resp = await InternalRequest(apiUri, authToken, "/b2api/v1/b2_delete_file_version", new { fileId, fileName });

            return resp.StatusCode == HttpStatusCode.OK;
        }

        public static async Task<B2FileInfo> GetFileInfo(Uri apiUri, string authToken, string fileId)
        {
            HttpResponseMessage resp = await InternalRequest(apiUri, authToken, "/b2api/v1/b2_get_file_info", new { fileId });

            return JsonConvert.DeserializeObject<B2FileInfo>(await resp.Content.ReadAsStringAsync());
        }

        public static async Task<B2FileListContainer> ListFiles(Uri apiUri, string authToken, string bucketId, string startFileName = null, int maxFileCount = 100)
        {
            HttpResponseMessage resp = await InternalRequest(apiUri, authToken, "/b2api/v1/b2_list_file_names", new { bucketId, startFileName, maxFileCount });

            return JsonConvert.DeserializeObject<B2FileListContainer>(await resp.Content.ReadAsStringAsync());
        }

        public static async Task<B2FileListContainer> ListFileVersions(Uri apiUri, string authToken, string bucketId, string startFileName = null, string startFileId = null, int maxFileCount = 100)
        {
            HttpResponseMessage resp = await InternalRequest(apiUri, authToken, "/b2api/v1/b2_list_file_versions", new { bucketId, startFileName, maxFileCount, startFileId });

            return JsonConvert.DeserializeObject<B2FileListContainer>(await resp.Content.ReadAsStringAsync());
        }

        public static async Task<B2UploadConfiguration> GetUploadUrl(Uri apiUri, string authToken, string bucketId)
        {
            HttpResponseMessage resp = await InternalRequest(apiUri, authToken, "/b2api/v1/b2_get_upload_url", new { bucketId });

            return JsonConvert.DeserializeObject<B2UploadConfiguration>(await resp.Content.ReadAsStringAsync());
        }

        public static async Task<B2FileInfo> UploadFile(Uri uploadUri, string uploadToken, Stream stream, string fileName, string sha1, Dictionary<string, string> fileInfo = null, string contentType = null)
        {
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uploadUri);

            msg.Headers.Authorization = new AuthenticationHeaderValue(uploadToken);
            msg.Headers.Add("X-Bz-File-Name", fileName);
            msg.Headers.Add("X-Bz-Content-Sha1", sha1);

            if (fileInfo != null)
                foreach (KeyValuePair<string, string> pair in fileInfo)
                    msg.Headers.Add("X-Bz-Info-" + pair.Key, WebUtility.UrlEncode(pair.Value));

            msg.Content = new StreamContent(stream);
            msg.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "b2/x-auto");

            HttpResponseMessage resp = await new HttpClient().SendAsync(msg);

            if (resp.StatusCode != HttpStatusCode.OK)
                await HandleErrorResponse(resp);

            return JsonConvert.DeserializeObject<B2FileInfo>(await resp.Content.ReadAsStringAsync());
        }

        public static async Task<B2FileDownloadResult> DownloadFileHead(Uri downloadUri, string fileId, string authToken = null)
        {
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Head, new Uri(downloadUri, "/b2api/v1/b2_download_file_by_id?fileId=" + fileId));

            if (!string.IsNullOrEmpty(authToken))
                msg.Headers.Authorization = new AuthenticationHeaderValue(authToken);

            HttpResponseMessage resp = await new HttpClient().SendAsync(msg);

            if (resp.StatusCode != HttpStatusCode.OK)
                await HandleErrorResponse(resp);

            B2FileDownloadResult res = new B2FileDownloadResult();

            res.FileId = resp.Headers.GetValues("X-Bz-File-Id").First();
            res.FileName = resp.Headers.GetValues("X-Bz-File-Name").First();
            res.ContentSha1 = resp.Headers.GetValues("X-Bz-Content-Sha1").First();

            Debug.Assert(resp.Content.Headers.ContentLength != null, "resp.Content.Headers.ContentLength != null");

            res.ContentLength = resp.Content.Headers.ContentLength.Value;
            res.ContentType = resp.Content.Headers.ContentType.MediaType;

            res.FileInfo = new Dictionary<string, string>();

            foreach (KeyValuePair<string, IEnumerable<string>> pair in resp.Headers)
            {
                if (!pair.Key.StartsWith("X-Bz-Info-"))
                    continue;

                res.FileInfo[pair.Key.Substring("X-Bz-Info-".Length)] = pair.Value.First();
            }

            return res;
        }

        public static Stream DownloadFileContent(Uri downloadUri, string fileId, out B2FileDownloadResult info, string authToken = null)
        {
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, new Uri(downloadUri, "/b2api/v1/b2_download_file_by_id?fileId=" + fileId));

            if (!string.IsNullOrEmpty(authToken))
                msg.Headers.Authorization = new AuthenticationHeaderValue(authToken);

            HttpResponseMessage resp = new HttpClient().SendAsync(msg, HttpCompletionOption.ResponseHeadersRead).Result;

            if (resp.StatusCode != HttpStatusCode.OK)
                HandleErrorResponse(resp).Wait();

            info = new B2FileDownloadResult();

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

            return resp.Content.ReadAsStreamAsync().Result;
        }
    }
}
