using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CloudProxySharp.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudProxySharp.Tests
{
    [TestClass]
    public class ClearanceHandlerTests
    {
        private readonly Uri _protectedUri = new Uri("https://dailyiptvlist.com/");
        private readonly Uri _protectedDownloadUri = new Uri("https://dailyiptvlist.com/dl/de-m3uplaylist-2020-10-23-1.m3u");
        private readonly Uri _solveUrlOverrideUri = new Uri("https://www.spigotmc.org/resources/hubkick.2/download?version=203285");

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
        public async Task SolveOkCloudflareDownloadOnRootUrl()
        {
            var handler = new ClearanceHandler(Settings.CloudProxyApiUrl)
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.75 Safari/537.36",
                MaxTimeout = 60000,
                SolveOnRootUrl = true
            };

            var client = new HttpClient(handler);
            var response = await client.GetAsync(_protectedDownloadUri);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task SolveOkCloudflareDownloadSolveUrlOverride()
        {
            var handler = new ClearanceHandler(Settings.CloudProxyApiUrl)
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.75 Safari/537.36",
                MaxTimeout = 60000,
            };
            //https://regexr.com/5flcd
            handler.SolveUrlOverrides.Add(uri => new Regex(@"https?:\/\/(www.)?spigotmc.org\/resources\/.*?.\d+?\/download\?version=\d+")
                .IsMatch(uri.ToString()), uri => new Uri(@"https://www.spigotmc.org/resources/fast-async-worldedit-voxelsniper.13932/download?version=320370"));

            var client = new HttpClient(handler);
            var response = await client.GetAsync(_solveUrlOverrideUri);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
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
                Assert.IsTrue(e.Message.Contains("Name or service not know"));
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