using System.Collections.Generic;
using Newtonsoft.Json;

namespace FlareSolverrSharp.Types
{
    public class FlareSolverrRequest
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