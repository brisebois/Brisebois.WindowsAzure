using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

namespace Brisebois.WindowsAzure.Rest
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/02/23/asynchronously-calling-rest-services-from-a-windows-azure-role/
    /// </summary>
    public class StringRestClient
    {
        private readonly RestClient restClient;
        private readonly string data;

        internal StringRestClient(RestClient restClient, string data)
        {
            this.restClient = restClient;
            this.data = data;
        }

        public Task<string> PostAsync(Action<Uri, HttpStatusCode, string> onError)
        {
            return PostAsync(onError, null);
        }

        public async Task<string> PostAsync(Action<Uri, HttpStatusCode, string> onError,
                                            IProgress<string> progress)
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

        public Task<string> PutAsync(Action<Uri, HttpStatusCode, string> onError)
        {
            return PutAsync(onError, null);
        }

        public async Task<string> PutAsync(Action<Uri, HttpStatusCode, string> onError,
                                           IProgress<string> progress)
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
                        
                        client.TraceRequest(address, method, data, progress);

                        var watch = new Stopwatch();
                        watch.Start();
                        client.UploadString(address, method, data);
                        watch.Stop();
                        
                        client.TraceResponse(address,
                                             method,
                                             string.Format(CultureInfo.InvariantCulture,
                                                           "Completed in {0} seconds",
                                                           watch.Elapsed.TotalSeconds),
                                             progress);
                    }

                    return "Done";
                });
        }


        public StringRestClient ContentType(string value)
        {
            restClient.ContentType(value);
            return this;
        }
    }
}