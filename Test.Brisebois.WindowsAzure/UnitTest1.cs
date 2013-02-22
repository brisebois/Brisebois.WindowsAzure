using System;
using System.Net;
using Brisebois.WindowsAzure;
using Brisebois.WindowsAzure.REST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Brisebois.WindowsAzure
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            try
            {
                var call = RestClient.Uri("http://brisebois.com/api/configurations")
                                     .Parameter("_token", Guid.NewGuid().ToString())
                                     .GetAsync(OnError());

                call.Wait();

                Console.WriteLine(call.Result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        
        private static Action<Uri, HttpStatusCode, string> OnError()
        {
            return (uri, code, arg3) =>
                {
                    switch (code)
                    {
                        case HttpStatusCode.Found:
                            Console.WriteLine("Not Found");
                            break;
                        case HttpStatusCode.Unauthorized:
                            Console.WriteLine("You were logged out");
                            break;
                    }
                };
        }
    }
}