using System;
using System.IO;
using System.Net;
using System.Text;

namespace Brisebois.WindowsAzure.REST
{
    public static class WebClientExtentions
    {
        public static void Trace(this WebRequest request,
                                 IProgress<string> progress)
        {
            if (progress == null)
                return;

            var headerStringBuilder = new StringBuilder();
            foreach (var header in request.Headers.AllKeys)
            {
                headerStringBuilder.AppendFormat("{0}{1}='{2}'",
                                                 Environment.NewLine,
                                                 header,
                                                 request.Headers[header]);
            }

            var sb = new StringBuilder();
            sb.AppendFormat("[{0}] Request> ", GetTimeString());
            sb.AppendFormat("{0} ", request.Method);
            sb.AppendFormat("{0}", request.RequestUri);
            sb.AppendLine("");
            sb.AppendLine(headerStringBuilder.ToString());
            sb.AppendLine("");
            sb.AppendLine(GetRequestContent(request));
            
            progress.Report(sb.ToString());
        }

        public static void Trace(this HttpWebResponse response,
                                 IProgress<string> progress)
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

            var sb = new StringBuilder();
            sb.AppendFormat("[{0}] Response> ", GetTimeString());
            sb.AppendFormat("({0} {1}) ", (int)response.StatusCode, response.StatusCode);
            sb.AppendFormat("{0} ", response.Method);
            sb.AppendFormat("{0}", response.ResponseUri);
            sb.AppendLine("");
            sb.AppendLine(headerStringBuilder.ToString());
            sb.AppendLine("");
            sb.AppendLine(GetResponseContent(response));

            progress.Report(sb.ToString());
        }

        public static void TraceResponse(this WebClient client,
                                         Uri address,
                                         string httpMethod,
                                         string content,
                                         IProgress<string> progress)
        {
            if (progress == null)
                return;

            var headerStringBuilder = new StringBuilder();
            foreach (var header in client.ResponseHeaders.AllKeys)
            {
                headerStringBuilder.AppendFormat("{0}{1}='{2}'",
                                                 Environment.NewLine,
                                                 header,
                                                 client.ResponseHeaders[header]);
            }

            var sb = new StringBuilder();
            sb.AppendFormat("[{0}] Response> ", GetTimeString());
            sb.AppendFormat("{0} ", httpMethod);
            sb.AppendFormat("{0}", address);
            sb.AppendLine("");
            sb.AppendLine(headerStringBuilder.ToString());
            sb.AppendLine("");
            sb.AppendLine(content);

            progress.Report(sb.ToString());
        }

        public static void TraceResponse(this HttpWebResponse response, 
                                         IProgress<string> progress)
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

            var sb = new StringBuilder();
            sb.AppendFormat("[{0}] Response> ", GetTimeString());
            sb.AppendFormat("({0} {1}) ", (int)response.StatusCode, response.StatusCode);
            sb.AppendFormat("{0} ", response.Method);
            sb.Append(response.ResponseUri);
            sb.AppendLine("");
            sb.AppendLine(headerStringBuilder.ToString());
            sb.AppendLine("");
            sb.AppendLine(response.GetResponseContent());
           
            progress.Report(sb.ToString());
        }

        private static string GetTimeString()
        {
            return DateTime.Now.ToString("yyyy:MM:dd hh:mm:ss");
        }

        public static void TraceRequest(this WebClient client, 
                                        Uri address, 
                                        string httpMethod,
                                        IProgress<string> progress)
        {
            if (progress == null) return;

            TraceRequest(client,address,httpMethod,string.Empty,progress);
        }

        public static void TraceRequest(this WebClient client,
                                       Uri address,
                                       string httpMethod,
                                       string content,
                                       IProgress<string> progress)
        {
            if (progress == null) return;

            var headerStringBuilder = new StringBuilder();
            foreach (var header in client.Headers.AllKeys)
            {
                headerStringBuilder.AppendFormat("{0}{1}='{2}'",
                                                 Environment.NewLine,
                                                 header,
                                                 client.Headers[header]);
            }

            var sb = new StringBuilder();
            sb.AppendFormat("[{0}] Request> ", GetTimeString());
            sb.AppendFormat("{0} ", httpMethod);
            sb.Append(address);
            sb.AppendLine("");
            sb.AppendLine(headerStringBuilder.ToString());
            sb.AppendLine("");
            sb.AppendLine(content);

            progress.Report(sb.ToString());
        }

        public static string GetRequestContent(this WebRequest request)
        {
            var requestContent = String.Empty;

            var requestStream = request.GetRequestStream();

            if (requestStream != null)
            {
                if (!requestStream.CanSeek)
                    return requestContent;

                requestStream.Position = 0;
                using (var ms = new MemoryStream())
                {
                    requestStream.CopyTo(ms);
                    ms.Position = 0;
                    using (var reader = new StreamReader(ms))
                        requestContent = reader.ReadToEnd();
                }
            }

            return requestContent;
        }

        public static string GetResponseContent(this HttpWebResponse response)
        {
            var responseContent = String.Empty;

            var responseStream = response.GetResponseStream();

            if (responseStream != null)
            {
                if (!responseStream.CanSeek)
                    return responseContent;

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
                builder.AppendLine(String.Format("{0}{1}='{2}'",
                                                 Environment.NewLine,
                                                 header,
                                                 client.Headers[header]));
            }

            return builder.ToString();
        }
    }
}