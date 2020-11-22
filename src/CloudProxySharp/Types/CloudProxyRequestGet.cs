using Newtonsoft.Json;

namespace CloudProxySharp.Types
{
    public class CloudProxyRequestGet : CloudProxyRequest
    {
        [JsonProperty("cmd")] public new string Command = "request.get";
    }
}