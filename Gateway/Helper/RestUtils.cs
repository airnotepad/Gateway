using Newtonsoft.Json;
using RestSharp;
using Serilog;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace Gateway.Helper
{
    public static class RestUtils
    {
        public static string SerializeCookiesToString(this IList<RestResponseCookie> cookieContainer)
        {
            List<RestResponseCookie> cookies = new List<RestResponseCookie>();
            cookieContainer.ToList().ForEach((cookie) =>
            {
                if (cookies.Find(c => c.Name == cookie.Name) == null)
                    cookies.Add(cookie);
            });

            using (var stream = new MemoryStream())
            {
                try
                {
                    new BinaryFormatter().Serialize(stream, JsonConvert.SerializeObject(cookies));
                    return Convert.ToBase64String(stream.ToArray());
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }
        public static void SetCookies(this RestClient client, string cookies)
        {
            try
            {
                using (var stream = new MemoryStream(Convert.FromBase64String(cookies)))
                {
                    List<RestResponseCookie> container = JsonConvert.DeserializeObject<List<RestResponseCookie>>((string)new BinaryFormatter().Deserialize(stream));
                    client.CookieContainer = new CookieContainer();
                    string cookiestring = "";
                    container.Reverse();
                    container.ForEach((cookie) =>
                    {
                        client.CookieContainer.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
                        cookiestring += $"{cookie.Name}={cookie.Value}; ";
                    });
                    client.AddDefaultHeader("Cookie", cookiestring);
                }
            }
            catch (Exception ex)
            {
                Log.ForContext("url", client.BaseUrl.ToString())
                   .ForContext("cookies", cookies)
                   .Error(ex, "DeserializeCookies error");
            }
        }

        public static void SetProxy(this RestClient client, string proxy)
        {
            try
            {
                if (!string.IsNullOrEmpty(proxy))
                    client.Proxy = GetWebProxy(proxy);
            }
            catch (Exception ex)
            {
                Log.ForContext("url", client.BaseUrl.ToString())
                   .ForContext("proxy", proxy)
                   .Error(ex, "DeserializeProxy error");
            }
        }

        public static IWebProxy GetWebProxy(string connectionString)
        {
            string host = connectionString.Split(':')[0];
            string port = connectionString.Split(':')[1];

            IWebProxy proxy = new WebProxy($"{host}:{port}");
            if (connectionString.Split(':').Length > 2)
            {
                string username = connectionString.Split(':')[2];
                string password = connectionString.Split(':')[3];

                proxy.Credentials = new NetworkCredential(username, password);
            }

            return proxy;
        }
    }
}
