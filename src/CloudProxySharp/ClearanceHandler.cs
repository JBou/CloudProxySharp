using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CloudProxySharp.Constants;
using CloudProxySharp.Exceptions;
using CloudProxySharp.Extensions;
using CloudProxySharp.Solvers;
using CloudProxySharp.Types;
using Cookie = System.Net.Cookie;

namespace CloudProxySharp
{
    /// <summary>
    /// A HTTP handler that transparently manages Cloudflare's protection bypass.
    /// </summary>
    public class ClearanceHandler : DelegatingHandler
    {
        private readonly CloudProxySolver _cloudProxySolver;

        /// <summary>
        /// The User-Agent which will be used across this session (null means default CloudProxy User-Agent).
        /// </summary>
        public string UserAgent = null;

        /// <summary>
        /// Max timeout to solve the challenge.
        /// </summary>
        public int MaxTimeout = 60000;

        /// <summary>
        /// Proxy server to use for solving the challenge.
        /// More information: <a href="https://www.chromium.org/developers/design-documents/network-settings">
        /// https://www.chromium.org/developers/design-documents/network-settings</a>
        /// </summary>
        public string Proxy;

        private HttpClientHandler HttpClientHandler => InnerHandler.GetMostInnerHandler() as HttpClientHandler;

        /// <summary>
        /// Creates a new instance of the <see cref="ClearanceHandler"/>.
        /// </summary>
        /// <param name="cloudProxyApiUrl">CloudProxy API URL. If null or empty it will detect the challenges, but
        /// they will not be solved. Example: "http://localhost:8191/"</param>
        public ClearanceHandler(string cloudProxyApiUrl)
            : base(new HttpClientHandler())
        {
            if (!string.IsNullOrWhiteSpace(cloudProxyApiUrl))
            {
                _cloudProxySolver = new CloudProxySolver(cloudProxyApiUrl)
                {
                    MaxTimeout = MaxTimeout,
                    Proxy = Proxy
                };
            }
        }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Change the User-Agent if required
            OverrideUserAgentHeader(request);

            // Perform the original user request
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // Detect if there is a challenge in the response
            if (ChallengeDetector.IsClearanceRequired(response))
            {
                if (_cloudProxySolver == null)
                    throw new CloudProxyException("Challenge detected but CloudProxy is not configured");

                // Resolve the challenge using CloudProxy API
                var cloudProxyResponse = await _cloudProxySolver.Solve(request);

                // Change the cookies in the original request with the cookies provided by CloudProxy
                InjectCookies(request, cloudProxyResponse);
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                // Detect if there is a challenge in the response
                if (ChallengeDetector.IsClearanceRequired(response))
                    throw new CloudProxyException("The cookies provided by CloudProxy are not valid");

                // Add the "Set-Cookie" header in the response with the cookies provided by CloudProxy
                InjectSetCookieHeader(response, cloudProxyResponse);
            }

            return response;
        }

        private void OverrideUserAgentHeader(HttpRequestMessage request)
        {
            if (UserAgent == null)
                return;
            if (request.Headers.UserAgent.ToString().Equals(UserAgent))
                return;
            request.Headers.UserAgent.Clear();
            request.Headers.Add(HttpHeaders.UserAgent, UserAgent);
        }

        private void InjectCookies(HttpRequestMessage request, CloudProxyResponse cloudProxyResponse)
        {
            var rCookies = cloudProxyResponse.Solution.Cookies;
            if (!rCookies.Any())
                return;
            var rCookiesList = rCookies.Select(x => x.Name).ToList();

            if (HttpClientHandler.UseCookies)
            {
                var oldCookies = HttpClientHandler.CookieContainer.GetCookies(request.RequestUri);
                foreach (Cookie oldCookie in oldCookies)
                    if (rCookiesList.Contains(oldCookie.Name))
                        oldCookie.Expired = true;
                foreach (var rCookie in rCookies)
                    HttpClientHandler.CookieContainer.Add(request.RequestUri, rCookie.ToCookieObj());
            }
            else
            {
                foreach (var rCookie in rCookies)
                    request.Headers.Add(HttpHeaders.Cookie, rCookie.ToHeaderValue());
            }
        }

        private void InjectSetCookieHeader(HttpResponseMessage response, CloudProxyResponse cloudProxyResponse)
        {
            var rCookies = cloudProxyResponse.Solution.Cookies;
            if (!rCookies.Any())
                return;

            // inject set-cookie headers in the response
            foreach (var rCookie in rCookies)
                response.Headers.Add(HttpHeaders.SetCookie, rCookie.ToHeaderValue());
        }

    }
}
