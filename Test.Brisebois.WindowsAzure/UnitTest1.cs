using System;
using System.Collections.Generic;
using System.Net;
using Brisebois.WindowsAzure.REST;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

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


        public class Resource
        {
            public Resource()
            {
                Values = new Dictionary<string, string>();
            }
            public string Name { get; set; }
            public Dictionary<string, string> Values { get; set; }
        }

        [TestMethod]
        public void TestPost()
        {
            try
            {

                var r = new Resource
                    {
                        Name = "Alexandre Brisebois",
                        Values = new Dictionary<string, string>
                            {
                                {"specialty", "Windows Azure"}
                            }
                    };

                var progress = new Progress<string>(Console.WriteLine);

                var call = RestClient.Uri("http://localhost:81/testWebApi/api/values")
                                     .Retry(2, true)
                                     .Parameter("_token", Guid.NewGuid().ToString())
                                     .Content(JsonConvert.SerializeObject(r))
                                     .ContentType("application/json; charset=utf-8")
                                     .PostAsync(OnError, progress);


                call.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [TestMethod]
        public void TestPut()
        {
            try
            {

                var r = new Resource
                    {
                        Name = "Alexandre Brisebois",
                        Values = new Dictionary<string, string>
            {
                {"specialty", "Windows Azure"}
            }
                    };

                var progress = new Progress<string>(Console.WriteLine);

                var call = RestClient.Uri("http://localhost:81/testWebApi/api/values")
                                                .Retry(2, true)
                                                .Content(JsonConvert.SerializeObject(r))
                                                .ContentType("application/json; charset=utf-8")
                                                .PutAsync(OnError, progress);


                call.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [TestMethod]
        public void TestDelete()
        {
            try
            {

                var progress = new Progress<string>(Console.WriteLine);

                var call = RestClient.Uri("http://localhost:81/testWebApi/api/values")
                                     .Retry(2, true)
                                     .DeleteAsync(OnError, progress);


                call.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [TestMethod]
        public void TestGet()
        {
            var progress = new Progress<string>(Console.WriteLine);

            Action<Uri, HttpStatusCode, string> onError = (uri, code, message) =>
                {
                    // Console.WriteLine(code.ToString());
                };

            const bool notFoundIsTransient = true;

            var result = RestClient.Uri("http://localhost:81/testWebApi/api/404")
                                   .Retry(3, notFoundIsTransient)
                                   .GetAsync(onError, progress);

            result.Wait();
        }

        [TestMethod]
        public void TestGetWithContentType()
        {
            var progress = new Progress<string>(Console.WriteLine);

            Action<Uri, HttpStatusCode, string> onError = (uri, code, message) => Console.WriteLine(code.ToString());

            var result = RestClient.Uri("http://localhost:81/testWebApi/api/values")
                //.ContentType("application/json; charset=utf-8")
                                   .GetAsync(onError, progress);

            result.Wait();

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(result.Result);
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
                        var message = string.Format("{1}{0}{0}{2}",
                                                    Environment.NewLine,
                                                    uri,
                                                    content);

                        throw new RestClientException(statusCode, message);
                    }
            }
        }
    }
}