using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gateway.Models.Base;

namespace Gateway.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class AdminController : Controller
    {
        public IActionResult HostedLogger()
        {
            ViewBag.Services = _serviceLogger.GetLoggedServices();

            return View();
        }

        public JsonResult HostedLogs(string service)
        {
            var response = new APIResponse<List<string>>();

            try
            {
                response.Result = _serviceLogger.GetLogs(service);
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return new JsonResult(response);
        }
    }
}