using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading.Tasks;
using CloudProxySharp.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudProxySharp.Tests
{
    [TestClass]
    public class ClearanceHandlerTests
    {
        private readonly Uri _protectedUri = new Uri("https://iptvm3ulist.com/");
        private readonly Uri _protectedDownloadUri = new Uri("https://iptvm3ulist.com/m3u/de01_iptvm3ulist_com_211120.m3u");
        private readonly Uri _protectedDownloadUri2 = new Uri("https://www.spigotmc.org/resources/hubkick.2/download?version=203285");

        [TestMethod]
        public async Task SolveOk()
        {
            var uri = new Uri("https://www.google.com/");
            var handler = new ClearanceHandler(Settings.CloudProxyApiUrl)
            {
                UserAgent = null,
                MaxTimeout = 60000
            };

            var client = new HttpClient(handler);
            var response = await client.GetAsync(uri);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task SolveOkCloudflare()
        {
            var handler = new ClearanceHandler(Settings.CloudProxyApiUrl)
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.75 Safari/537.36",
                MaxTimeout = 60000
            };

            var client = new HttpClient(handler);
            var response = await client.GetAsync(_protectedUri);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task SolveOkCloudflareDownload()
        {
            var handler = new ClearanceHandler(Settings.CloudProxyApiUrl)
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.75 Safari/537.36",
                MaxTimeout = 60000
            };

            var client = new HttpClient(handler);
            var response = await client.GetAsync(_protectedDownloadUri);
            Assert.AreEqual(MediaTypeHeaderValue.Parse("audio/x-mpegurl"), response.Content.Headers.ContentType);
        }

        [TestMethod]
        public async Task SolveOkCloudflareDownload2()
        {
            var handler = new ClearanceHandler(Settings.CloudProxyApiUrl)
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.75 Safari/537.36",
                MaxTimeout = 60000,
            };

            var client = new HttpClient(handler);
            var response = await client.GetAsync(_protectedDownloadUri2);
            Assert.AreEqual(MediaTypeHeaderValue.Parse("application/octet-stream"), response.Content.Headers.ContentType);
        }
        
        [TestMethod]
        public async Task SolveError()
        {
            var uri = new Uri("https://www.google.bad1/");
            var handler = new ClearanceHandler(Settings.CloudProxyApiUrl)
            {
                UserAgent = null,
                MaxTimeout = 60000
            };

            var client = new HttpClient(handler);
            try
            {
                await client.GetAsync(uri);
                Assert.Fail("Exception not thrown");
            }
            catch (HttpRequestException e)
            {
                Assert.IsNotNull(e.InnerException);
                Assert.IsInstanceOfType(e.InnerException, typeof(SocketException));
                Assert.AreEqual(SocketError.HostNotFound, ((SocketException)e.InnerException).SocketErrorCode);
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception: " + e);
            }
        }

        [TestMethod]
        public async Task SolveErrorBadConfig()
        {
            var handler = new ClearanceHandler("http://localhost:44445")
            {
                UserAgent = null,
                MaxTimeout = 60000
            };

            var client = new HttpClient(handler);
            try
            {
                await client.GetAsync(_protectedUri);
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
 
        [TestMethod]
        public async Task SolveErrorNoConfig()
        {
            var handler = new ClearanceHandler("")
            {
                UserAgent = null,
                MaxTimeout = 60000
            };

            var client = new HttpClient(handler);
            try
            {
                await client.GetAsync(_protectedUri);
                Assert.Fail("Exception not thrown");
            }
            catch (CloudProxyException e)
            {
                Assert.IsTrue(e.Message.Contains("Challenge detected but CloudProxy is not configured"));
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception: " + e);
            }
        }

    }
}