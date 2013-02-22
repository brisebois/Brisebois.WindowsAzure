using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Practices.TransientFaultHandling;

namespace Brisebois.WindowsAzure
{
    public class RestClient
    {
        private readonly Uri uri;
        private readonly RetryPolicy retryPolicy;
        private readonly Dictionary<string, string> queryParameters = new Dictionary<string, string>();

        public RestClient(Uri uri)
        {
            retryPolicy = RetryPolicyFactory.MakeHttpRetryPolicy();
            this.uri = uri;
        }

        public RestClient Parameter(string key, string value)
        {
            if (queryParameters.ContainsKey(key))
                queryParameters[key] = value;
            else
                queryParameters.Add(key, value);
            return this;
        }

        private Uri PrepareUri()
        {
            var query = queryParameters
                .Select(kv => string.Format("{0}={1}", kv.Key, kv.Value))
                .Aggregate("", (s, s1) =>
                    {
                        if (string.IsNullOrWhiteSpace(s))
                            return s1;
                        return s1 + "&" + s;
                    });

            var builder = new UriBuilder(uri) { Query = query };

            return builder.Uri;
        }

        public async Task<string> GetAsync(Action<Uri, HttpStatusCode, string> onError)
        {
            try
            {
                return await retryPolicy.ExecuteAsync(DownloadAsStringAsync);
            }
            catch (Exception ex)
            {
                return ExecuteOnError(onError, ex);
            }
        }

        public async Task<Stream> GetStreamAsync(Action<Uri, HttpStatusCode, Stream> onError)
        {
            try
            {
                return await retryPolicy.ExecuteAsync(DownloadAsStreamsync);
            }
            catch (Exception ex)
            {
                return ExecuteOnError(onError, ex);
            }
        }

        protected async Task<string> DownloadAsStringAsync()
        {
            using (var client = new WebClient())
                return await client.DownloadStringTaskAsync(PrepareUri());
        }

        protected async Task<Stream> DownloadAsStreamsync()
        {
            var downloadTask = Task.Run(() =>
                {
                    using (var client = new WebClient())
                    {
                        var bytes = client.DownloadData(PrepareUri());
                        return new MemoryStream(bytes);
                    }
                });

            return await downloadTask;
        }

        private string ExecuteOnError(Action<Uri, HttpStatusCode, string> onError,
                                      Exception ex)
        {
            var response = GetHttpWebResponse(ex);

            string responseContent;

            using (var responseStream = response.GetResponseStream())
                responseContent = GetResponseContent(responseStream);

            onError(uri, response.StatusCode, responseContent);
            return responseContent;
        }

        private Stream ExecuteOnError(Action<Uri, HttpStatusCode, Stream> onError,
                                      Exception ex)
        {
            var response = GetHttpWebResponse(ex);

            var responseStream = response.GetResponseStream();

            onError(uri, response.StatusCode, responseStream);
            return responseStream;
        }

        private static string GetResponseContent(Stream responseStream)
        {
            var responseContent = String.Empty;

            if (responseStream != null)
                using (var reader = new StreamReader(responseStream))
                    responseContent = reader.ReadToEnd();

            return responseContent;
        }

        private HttpWebResponse GetHttpWebResponse(Exception ex)
        {
            var we = ex as WebException;
            if (we == null)
                throw ex;

            var response = we.Response as HttpWebResponse;

            if (response == null)
                throw ex;
            return response;
        }

        public static RestClient Uri(string uri)
        {
            return new RestClient(new Uri(uri));
        }
    }
}