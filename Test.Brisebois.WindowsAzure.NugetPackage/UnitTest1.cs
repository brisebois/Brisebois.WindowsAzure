using System;
using System.Linq;
using Brisebois.WindowsAzure.TableStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Brisebois.WindowsAzure.NugetPackage
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Enumerable.Range(0,1001).ToList().ForEach(i => Logger.Add("test", i + " event", "these are the details"));
            Logger.Persist(true);
        }
    }
}
