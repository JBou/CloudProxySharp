using System.Collections.Generic;
using Newtonsoft.Json;

namespace CloudProxySharp.Types
{
    public class CloudProxyRequest
    {
        [JsonProperty("cmd")]
        public string Command = "request.cookies";

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("userAgent")]
        public string UserAgent;

        [JsonProperty("maxTimeout")]
        public int MaxTimeout;

        [JsonProperty("proxy", NullValueHandling = NullValueHandling.Ignore)]
        public string Proxy;
        
        [JsonProperty("headers")]
        public Dictionary<string, string> Headers;
        
        [JsonProperty("cookies")]
        public Cookie[] Cookies;
    }
}