using Newtonsoft.Json;
using RestSharp;
using Log = Serilog.Log;

namespace Gateway.Helper
{
    public static class Myip
    {
        //public static void Get(Proxy proxy)
        //{
        //    RestClient client = new RestClient("https://api.myip.com");
        //    RestRequest request = new RestRequest(Method.GET);

        //    client.Proxy = new WebProxy($"{proxy.host}:{proxy.port}");
        //    if (proxy.WithAuthenticate())
        //        client.Proxy.Credentials = new NetworkCredential(proxy.username,
        //                                                         proxy.pass);

        //    client.ClearHandlers();
        //    client.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";

        //    client.ReadWriteTimeout = 10000;
        //    client.Timeout = 10000;
        //    request.ReadWriteTimeout = 10000;
        //    request.Timeout = 10000;

        //    System.Diagnostics.Stopwatch stopwatch = new();
        //    stopwatch.Start();
        //    IRestResponse response = client.Execute(request);
        //    stopwatch.Stop();

        //    proxy.is_valid = response.IsSuccessful;

        //    if (response.IsSuccessful)
        //    {
        //        try
        //        {
        //            var json = JsonConvert.DeserializeObject<JsonMyip>(response.Content);
        //            proxy.information = json.ToString();
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error(ex, "Proxy check error {ConnectionString}", proxy.ConnectionString());
        //        }
        //    }

        //    Log.ForContext("Proxy", proxy, true)
        //        .ForContext("Total seconds", stopwatch.Elapsed.TotalSeconds)
        //        .ForContext("Status code", response.StatusCode)
        //        .Debug("Myip proxy {ConnectionString} checked, valid: {Is_valid}", proxy.ConnectionString(), proxy.is_valid);
        //}

        public static string GetServerIp()
        {
            var result = string.Empty;

            RestClient client = new RestClient("https://api.myip.com");
            RestRequest request = new RestRequest(Method.GET);

            client.ClearHandlers();
            client.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";

            client.ReadWriteTimeout = 10000;
            client.Timeout = 10000;
            request.ReadWriteTimeout = 10000;
            request.Timeout = 10000;

            System.Diagnostics.Stopwatch stopwatch = new();
            stopwatch.Start();
            IRestResponse response = client.Execute(request);
            stopwatch.Stop();
            
            if (response.IsSuccessful)
            {
                try
                {
                    var json = JsonConvert.DeserializeObject<JsonMyip>(response.Content);
                    result = json.ToString();
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                    Log.Error(ex, "GetServerIp error");
                }
            }

            Log.ForContext("Total seconds", stopwatch.Elapsed.TotalSeconds)
                .ForContext("Status code", response.StatusCode)
                .Debug("Myip check serverip, valid: {Is_valid}", response.IsSuccessful);

            return result;
        }

        public class JsonMyip
        {
            public string ip { get; set; }
            public string country { get; set; }
            public string cc { get; set; }

            public override string ToString()
            {
                return $"{cc.ToUpper()} {ip}";
            }
        }
    }
}
