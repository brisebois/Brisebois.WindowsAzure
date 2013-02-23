using System;
using System.IO;
using System.Net;
using System.Text;

namespace Brisebois.WindowsAzure.REST
{
    public static class WebClientExtentions
    {
        public static void TradeResponse(this HttpWebResponse response, IProgress<string> progress)
        {
            if (progress == null)
                return;

            var headerStringBuilder = new StringBuilder();
            foreach (var header in response.Headers.AllKeys)
            {
                headerStringBuilder.AppendFormat("{0}{1}='{2}'",
                                                 Environment.NewLine,
                                                 header,
                                                 response.Headers[header]);
            }

            progress.Report(string.Format("{0} {1} {2} {3}{6}{4}{6}{6}{5}{6}{6}",
                                          (int)response.StatusCode,
                                          response.StatusCode,
                                          response.Method,
                                          response.ResponseUri,
                                          headerStringBuilder,
                                          response.GetResponseContent(),
                                          Environment.NewLine));
        }

        public static string GetResponseContent(this HttpWebResponse response)
        {
            var responseContent = String.Empty;

            var responseStream = response.GetResponseStream();

            if (responseStream != null)
            {
                responseStream.Position = 0;
                using (var ms = new MemoryStream())
                {
                    responseStream.CopyTo(ms);
                    ms.Position = 0;
                    using (var reader = new StreamReader(ms))
                        responseContent = reader.ReadToEnd();
                }
            }
               
            return responseContent;
        }

        public static string Trace(this WebClient client)
        {
            var builder = new StringBuilder();

            foreach (var header in client.Headers.AllKeys)
            {
                builder.AppendLine(string.Format("{0}{1}='{2}'",
                                                 Environment.NewLine,
                                                 header,
                                                 client.Headers[header]));
            }

            return builder.ToString();
        }
    }
}