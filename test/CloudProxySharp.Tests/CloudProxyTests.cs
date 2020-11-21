using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CloudProxySharp.Constants;
using CloudProxySharp.Exceptions;
using CloudProxySharp.Solvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudProxySharp.Tests
{
    [TestClass]
    public class CloudProxyTests
    {
        [TestMethod]
        public async Task SolveOk()
        {
            var uri = new Uri("https://www.google.com/");
            var cloudProxy = new CloudProxySolver(Settings.CloudProxyApiUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            var cloudProxyResponse = await cloudProxy.Solve(request);
            Assert.AreEqual("ok", cloudProxyResponse.Status);
            Assert.AreEqual("", cloudProxyResponse.Message);
            Assert.IsTrue(cloudProxyResponse.StartTimestamp > 0);
            Assert.IsTrue(cloudProxyResponse.EndTimestamp > cloudProxyResponse.StartTimestamp);
            Assert.AreEqual("1.0.0", cloudProxyResponse.Version);

            Assert.AreEqual("https://www.google.com/", cloudProxyResponse.Solution.Url);
            Assert.AreEqual(cloudProxyResponse.Solution.Status, HttpStatusCode.OK);
            Assert.IsTrue(cloudProxyResponse.Solution.Response.Contains("<title>Google</title>"));
            Assert.IsTrue(cloudProxyResponse.Solution.Cookies.Any());
            Assert.IsTrue(cloudProxyResponse.Solution.Headers.Any());
            Assert.IsTrue(cloudProxyResponse.Solution.UserAgent.Contains(" Firefox/"));

            var firstCookie = cloudProxyResponse.Solution.Cookies.First();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(firstCookie.Name));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(firstCookie.Value));
        }

        [TestMethod]
        public async Task SolveOkUserAgent()
        {
            const string userAgent = "Mozilla/5.0 (X11; Linux i686; rv:77.0) Gecko/20100101 Firefox/77.0";
            var uri = new Uri("https://www.google.com/");
            var cloudProxy = new CloudProxySolver(Settings.CloudProxyApiUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add(HttpHeaders.UserAgent, userAgent);

            var cloudProxyResponse = await cloudProxy.Solve(request);
            Assert.AreEqual("ok", cloudProxyResponse.Status);
            Assert.AreEqual(userAgent, cloudProxyResponse.Solution.UserAgent);
        }

        [TestMethod]
        public async Task SolveError()
        {
            var uri = new Uri("https://www.google.bad1/");
            var cloudProxy = new CloudProxySolver(Settings.CloudProxyApiUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            try
            {
                await cloudProxy.Solve(request);
                Assert.Fail("Exception not thrown");
            }
            catch (CloudProxyException e)
            {
                Assert.AreEqual("NS_ERROR_UNKNOWN_HOST at https://www.google.bad1/", e.Message);
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception: " + e);
            }
        }

        [TestMethod]
        public async Task SolveErrorConfig()
        {
            var uri = new Uri("https://www.google.com/");
            var cloudProxy = new CloudProxySolver("http://localhost:44445");
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            try
            {
                await cloudProxy.Solve(request);
                Assert.Fail("Exception not thrown");
            }
            catch (CloudProxyException e)
            {
                Assert.IsTrue(e.Message.Contains("Error connecting to CloudProxy server"));
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception: " + e);
            }
        }
    }
}