using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Practices.TransientFaultHandling;

namespace Brisebois.WindowsAzure.REST
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/02/23/asynchronously-calling-rest-services-from-a-windows-azure-role/
    /// </summary>
    public class RestClient
    {
        private readonly Uri uri;
        internal RetryPolicy retryPolicy;

        private readonly Dictionary<string, string> queryParameters = new Dictionary<string, string>();
        private readonly Dictionary<string, string> headers = new Dictionary<string, string>();

        private RestClient(Uri uri)
        {
            retryPolicy = RetryPolicyFactory.MakeHttpRetryPolicy();
            this.uri = uri;
        }
        public async Task<string> GetAsync(Action<Uri, HttpStatusCode, string> onError,
                                           IProgress<string> progress = null)
        {
            try
            {
                return await retryPolicy.ExecuteAsync(() => DownloadStringAsync(progress));
            }
            catch (Exception ex)
            {
                return ExecuteOnError(onError, ex, progress);
            }
        }

        private async Task<string> DownloadStringAsync(IProgress<string> progress)
        {
            using (var client = new WebClient())
            {
                var address = PrepareUri();
                SetHeaders(client);


                client.TraceRequest(address, "GET", progress);

                var responseString = await client.DownloadStringTaskAsync(address);

                client.TraceResponse(address, "GET", responseString, progress);

                return responseString;
            }
        }

        public async Task<Stream> GetStreamAsync(Action<Uri, HttpStatusCode, Stream> onError,
                                                IProgress<string> progress = null)
        {
            try
            {
                return await retryPolicy.ExecuteAsync(() => DownloadStreamAsync(progress));
            }
            catch (Exception ex)
            {
                return ExecuteOnError(onError, ex);
            }
        }

        public async Task<string> DeleteAsync(Action<Uri, HttpStatusCode, string> onError,
                                              IProgress<string> progress = null)
        {
            try
            {
                return await retryPolicy.ExecuteAsync(() => DeleteAsync(progress));
            }
            catch (Exception ex)
            {
                return ExecuteOnError(onError, ex, progress);
            }
        }

        private Task<string> DeleteAsync(IProgress<string> progress)
        {
            var deleteTask = Task.Run(() =>
                {
                    var address = PrepareUri();

                    var request = WebRequest.Create(address);
                    request.Method = "DELETE";

                    SetHeaders(request);

                    request.Trace(progress);

                    var response = (HttpWebResponse)request.GetResponse();
                    
                    response.Trace(progress);

                    return "Done";
                });
            return deleteTask;
        }

        private async Task<Stream> DownloadStreamAsync(IProgress<string> progress)
        {
            var downloadTask = Task.Run(() =>
                {
                    using (var client = new WebClient())
                    {
                        var address = PrepareUri();

                        SetHeaders(client);

                        client.TraceRequest(address, "GET", progress);

                        var bytes = client.DownloadData(address);

                        client.TraceResponse(address, "GET", "Length=" + bytes.Length, progress);

                        return new MemoryStream(bytes);
                    }
                });

            return await downloadTask;
        }

        public StreamRestClient Content(Stream data)
        {
            data.Position = 0;
            using (var ms = new MemoryStream())
            {
                data.CopyTo(ms);
                return new StreamRestClient(this, ms.ToArray());
            }
        }

        public StringRestClient Content(string data)
        {
            return new StringRestClient(this, data);
        }

        public RestClient ContentType(string contentType = "application/x-www-form-urlencoded")
        {
            Header("Content-Type", contentType);
            return this;
        }

        public void Header(string key, string value)
        {
            if (headers.ContainsKey(key))
                headers[key] = value;
            else
                headers.Add(key, value);
        }

        internal void SetHeaders(WebClient client)
        {
            headers
                .ToList()
                .ForEach(kv => client.Headers.Add(kv.Key, kv.Value));
        }

        internal void SetHeaders(WebRequest request)
        {
            headers
                .ToList()
                .ForEach(kv => request.Headers.Add(kv.Key, kv.Value));
        }

        internal Uri PrepareUri()
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

        internal string ExecuteOnError(Action<Uri, HttpStatusCode, string> onError,
                                      Exception ex,
                                      IProgress<string> progress)
        {
            var response = GetHttpWebResponse(ex);

            var responseContent = response.GetResponseContent();

            response.TraceResponse(progress);

            onError(uri, response.StatusCode, responseContent);

            return responseContent;
        }

        internal Stream ExecuteOnError(Action<Uri, HttpStatusCode, Stream> onError,
                                      Exception ex)
        {
            var response = GetHttpWebResponse(ex);
            var responseStream = response.GetResponseStream();

            onError(uri, response.StatusCode, responseStream);

            return responseStream;
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

        public RestClient Parameter(string key, string value)
        {
            if (queryParameters.ContainsKey(key))
                queryParameters[key] = value;
            else
                queryParameters.Add(key, value);
            return this;
        }

        public static RestClient Uri(string uri)
        {
            return new RestClient(new Uri(uri));
        }

        public RestClient Retry(int count, bool notFoundIsTransient = false)
        {
            retryPolicy = RetryPolicyFactory.MakeHttpRetryPolicy(count, notFoundIsTransient);
            return this;
        }
    }
}