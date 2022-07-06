using Gateway.Services;
using Microsoft.Extensions.Primitives;

namespace Gateway.Middlewares
{
    public class OnlineUsersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly OnlineUsersService _online;

        public OnlineUsersMiddleware(ILogger<OnlineUsersMiddleware> logger, OnlineUsersService online, RequestDelegate next)
        {
            _next = next;
            _logger = logger;
            _online = online;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                string ip = GetRequestIP(context);
                string method = context.Request.Method.ToString();
                string path = context.Request.Path.ToString();

                if (context.User.Identity.IsAuthenticated)
                    _online.AddUserOnline(context.User.Identity.IsAuthenticated, context.User.Identity.Name, path, ip, method);
                else
                    _online.AddUserOnline(context.User.Identity.IsAuthenticated, "Неизвестный", path, ip, method); ;

                try
                {
                    await _next.Invoke(context);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Invoke error");
                }

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OnlineUsersMiddleware error");
            }
        }

        private string GetRequestIP(HttpContext context)
        {
            var result = string.Empty;

            if (context.Request.Headers != null)
            {
                var forwardedHeader = context.Request.Headers["X-Forwarded-For"];
                if (!StringValues.IsNullOrEmpty(forwardedHeader))
                    result = forwardedHeader.FirstOrDefault();
            }

            if (string.IsNullOrEmpty(result) && context.Connection.RemoteIpAddress != null)
                result = context.Connection.RemoteIpAddress.ToString();

            return result;
        }
    }
}
