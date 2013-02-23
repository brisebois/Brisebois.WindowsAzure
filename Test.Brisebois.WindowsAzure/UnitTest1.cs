using System;
using System.Net;
using Brisebois.WindowsAzure.REST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Brisebois.WindowsAzure
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod()
        {
            try
            {
                var notFoundIsTransient = true;
                var progress = new Progress<string>(Console.WriteLine);

                var call = RestClient.Uri("http://brisebois.com/api/configurations")
                                        .Retry(6, notFoundIsTransient)
                                        .Parameter("_token", Guid.NewGuid().ToString())
                                        .GetAsync(OnError, progress);

                call.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        
        [TestMethod]
        public void TestMethodNotTransient()
        {
            try
            {
                var notFoundIsTransient = true;
                var progress = new Progress<string>(Console.WriteLine);

                var call = RestClient.Uri("http://brisebois.com/api/configurations")
                                        .Retry(2)
                                        .Parameter("_token", Guid.NewGuid().ToString())
                                        .GetAsync(OnError, progress);

                call.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [TestMethod]
        public void TestMethodGetGoogleHomePage()
        {
            try
            {
                var progress = new Progress<string>(Console.WriteLine);

                var call = RestClient.Uri("http://www.google.com")
                                        .Retry(2)
                                        .GetAsync(OnError, progress);

                call.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnError(Uri uri, HttpStatusCode statusCode, string content)
        {
            switch (statusCode)
            {
                case HttpStatusCode.NotFound:
                    {
                        // TODO: do something
                    }
                    break;
                case HttpStatusCode.Unauthorized:
                    {
                        // TODO: log user out
                    }
                    break;
                default:
                    {
                        var message = string.Format("{1}{0}{0}{2}", Environment.NewLine, uri, content);
                        throw new RestClientException(statusCode, message);    
                    }
            }
        }
    }
}