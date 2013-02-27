using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Brisebois.WindowsAzure.REST
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/02/23/asynchronously-calling-rest-services-from-a-windows-azure-role/
    /// </summary>
    public class StreamRestClient
    {
        private readonly RestClient restClient;
        private readonly byte[] content;

        internal StreamRestClient(RestClient client, byte[] bytes)
        {
            restClient = client;
            content = bytes;
        }

        public async Task<string> PutAsync(Action<Uri, HttpStatusCode, string> onError,
                                           IProgress<string> progress = null)
        {
            try
            {
                return await restClient.retryPolicy.ExecuteAsync(() => UploadBytesAsync("PUT", progress));
            }
            catch (Exception ex)
            {
                return restClient.ExecuteOnError(onError, ex, progress);
            }
        }

        public async Task<string> PostAsync(Action<Uri, HttpStatusCode, string> onError,
                                            IProgress<string> progress = null)
        {
            try
            {
                return await restClient.retryPolicy.ExecuteAsync(() => UploadBytesAsync("POST", progress));
            }
            catch (Exception ex)
            {
                return restClient.ExecuteOnError(onError, ex, progress);
            }
        }

        public StreamRestClient ContentType(string contentType)
        {
            restClient.ContentType(contentType);
            return this;
        }

        private Task<string> UploadBytesAsync(string method, IProgress<string> progress)
        {
            return Task.Run(() =>
                {
                    using (var client = new WebClient())
                    {
                        var address = restClient.PrepareUri();

                        restClient.SetHeaders(client);
                        
                        client.TraceRequest(address, method, progress);

                        var watch = new Stopwatch();
                        watch.Start();
                        client.UploadData(address, method, content);
                        watch.Stop();

                        client.TraceResponse(address,
                                                  method,
                                                  string.Format("Completed in {0} seconds",
                                                                watch.Elapsed.TotalSeconds),
                                                  progress);
                    }

                    return "Done";
                });
        }
    }
}