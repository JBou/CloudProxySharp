using System.Collections.Generic;
using Newtonsoft.Json;

namespace CloudProxySharp.Types
{
    public class CloudProxyRequest
    {
        [JsonProperty("cmd")]
        public string Command = "request.get";

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("userAgent")]
        public string UserAgent;

        [JsonProperty("maxTimeout")]
        public int MaxTimeout;
        
        [JsonProperty("headers")]
        public Dictionary<string, string> Headers;
        
        [JsonProperty("cookies")]
        public Cookie[] Cookies;
    }
}