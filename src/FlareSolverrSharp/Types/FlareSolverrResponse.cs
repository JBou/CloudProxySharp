
// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global
namespace FlareSolverrSharp.Types
{
    public class FlareSolverrResponse
    {
        public string Status;
        public string Message;
        public long StartTimestamp;
        public long EndTimestamp;
        public string Version;
        public Solution Solution;
    }

    public class Solution
    {
        public string Url;
        public string Response;
        public Cookie[] Cookies;
        public string UserAgent;
    }

    public class Cookie
    {
        public string Name;
        public string Value;
        public string Domain;
        public string Path;
        public double Expires;
        public int Size;
        public bool HttpOnly;
        public bool Secure;
        public bool Session;
        public string SameSite;

        public string ToHeaderValue() => $"{Name}={Value}";
        public System.Net.Cookie ToCookieObj()
        {
            System.Net.Cookie cookie = new System.Net.Cookie(Name, Value, Path, Domain);
            cookie.HttpOnly = HttpOnly;
            cookie.Secure = Secure;
            
            //Adding this sometimes returned an expired cookie date, resulting in the cookie being expired
            //and therefore not being used, even if it is working otherwise.
            //cookie.Expires = DateTimeOffset.FromUnixTimeSeconds((long) Expires).DateTime;
            
            return cookie;
        }
    }
}