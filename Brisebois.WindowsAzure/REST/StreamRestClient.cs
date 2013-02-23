using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Brisebois.WindowsAzure.REST
{
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

        public void ContentType(string contentType)
        {
            restClient.ContentType(contentType);
        }

        private Task<string> UploadBytesAsync(string method, IProgress<string> progress)
        {
            return Task.Run(() =>
                {
                    using (var client = new WebClient())
                    {
                        var address = restClient.PrepareUri();

                        restClient.SetHeaders(client);

                        if (progress != null)
                        {
                            progress.Report(address.ToString());
                            progress.Report(client.Trace());
                        }

                        var watch = new Stopwatch();
                        watch.Start();
                        client.UploadData(address, method, content);
                        watch.Stop();

                        if (progress != null)
                            progress.Report(string.Format("{0} {1}\n completed in {2} seconds",
                                                          method,
                                                          address,
                                                          watch.Elapsed.TotalSeconds));
                    }

                    return "Done";
                });
        }
    }
}