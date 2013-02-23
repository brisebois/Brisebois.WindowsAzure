using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Brisebois.WindowsAzure.REST
{
    public class StringRestClient
    {
        private readonly RestClient restClient;
        private readonly string data;

        internal StringRestClient(RestClient restClient, string data)
        {
            this.restClient = restClient;
            this.data = data;
        }

        public async Task<string> PostAsync(Action<Uri, HttpStatusCode, string> onError,
                                            IProgress<string> progress = null)
        {
            try
            {
                return await restClient.retryPolicy.ExecuteAsync(() => UploadStringAsync("POST", progress));
            }
            catch (Exception ex)
            {
                return restClient.ExecuteOnError(onError, ex, progress);
            }
        }

        public async Task<string> PutAsync(Action<Uri, HttpStatusCode, string> onError,
                                           IProgress<string> progress = null)
        {
            try
            {
                return await restClient.retryPolicy.ExecuteAsync(() => UploadStringAsync("PUT", progress));
            }
            catch (Exception ex)
            {
                return restClient.ExecuteOnError(onError, ex, progress);
            }
        }

        private Task<string> UploadStringAsync(string method, IProgress<string> progress)
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
                        client.UploadString(address, method, data);
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


        public void ContentType(string contentType)
        {
            restClient.ContentType(contentType);
        }
    }
}