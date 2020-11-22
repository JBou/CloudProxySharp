// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace CloudProxySharp.Types
{
    public class CloudProxyResponseGet : CloudProxyResponse
    {
        public new SolutionGet Solution;
    }

    public class SolutionGet : Solution
    {
        public string Response;
        public string UserAgent;
    }
}