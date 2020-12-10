using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudProxySharp.Sample
{
    public static class ClearanceHandlerSample
    {
        public static async Task Sample()
        {
            var handler = new ClearanceHandler("http://localhost:8191/")
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36",
                MaxTimeout = 60000
            };

            var client = new HttpClient(handler);
            var content = await client.GetStringAsync("https://uam.hitmehard.fun/HIT");
            Console.WriteLine(content);
        }
    }
}