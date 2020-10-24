using System.Net.Http;

namespace CloudProxySharp.Exceptions
{
    /// <summary>
    /// The exception that is thrown if CloudProxy fails
    /// </summary>
    public class CloudProxyException : HttpRequestException
    {
        public CloudProxyException(string message) : base(message)
        {
        }
    }
}
