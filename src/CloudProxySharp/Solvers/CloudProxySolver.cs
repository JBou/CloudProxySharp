using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CloudProxySharp.Exceptions;
using CloudProxySharp.Types;
using CloudProxySharp.Utilities;
using Newtonsoft.Json;

namespace CloudProxySharp.Solvers
{
    public class CloudProxy
    {
        private static readonly SemaphoreLocker Locker = new SemaphoreLocker();
        private HttpClient _httpClient;
        private readonly Uri _cloudProxyUri;

        public int MaxTimeout = 60000;

        public CloudProxy(string cloudProxyApiUrl)
        {
            var apiUrl = cloudProxyApiUrl;
            if (!apiUrl.EndsWith("/"))
                apiUrl += "/";
            _cloudProxyUri = new Uri(apiUrl + "v1");
        }

        public async Task<CloudProxyResponse> Solve(HttpRequestMessage request)
        {
            CloudProxyResponse result = null;
            Uri originalUri = request.RequestUri;

            await Locker.LockAsync(async () =>
            {
                HttpResponseMessage response;
                try
                {
                    _httpClient = new HttpClient();
                    var builder = new UriBuilder(originalUri) {Path = string.Empty};
                    request.RequestUri = builder.Uri;

                    response = await _httpClient.PostAsync(_cloudProxyUri, GenerateCloudProxyRequest(request));
                    request.RequestUri = originalUri;
                }
                catch (HttpRequestException e)
                {
                    throw new CloudProxyException("Error connecting to CloudProxy server: " + e);
                }
                catch (Exception e)
                {
                    throw new CloudProxyException(e.ToString());
                }
                finally
                {
                    _httpClient.Dispose();
                }

                var resContent = await response.Content.ReadAsStringAsync();
                try
                {
                    result = JsonConvert.DeserializeObject<CloudProxyResponse>(resContent);
                }
                catch (Exception)
                {
                    throw new CloudProxyException("Error parsing response, check CloudProxy version. Response: " + resContent);
                }

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new CloudProxyException(result.Message);
            });

            return result;
        }

        private HttpContent GenerateCloudProxyRequest(HttpRequestMessage request)
        {
            var req = new CloudProxyRequest
            {
                Url = request.RequestUri.ToString(),
                MaxTimeout = MaxTimeout
            };

            var userAgent = request.Headers.UserAgent.ToString();
            if (!string.IsNullOrWhiteSpace(userAgent))
                req.UserAgent = userAgent;

            var payload = JsonConvert.SerializeObject(req);
            HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
            return content;
        }
 
    }
}