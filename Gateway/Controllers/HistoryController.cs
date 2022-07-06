using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Gateway.Models.Base;
using Gateway.Models.JS;
using Gateway.Entity;
using Gateway.Helper;

namespace Gateway.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {
        private readonly ILogger<HistoryController> _logger;
        private readonly Context _context;

        public HistoryController(ILogger<HistoryController> logger, Context context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Table([FromBody] DTParameters dtParameters)
        {
            var response = new APIResponse<DTResult<TransactionUser>>();

            try
            {
                var searchBy = dtParameters.Search?.Value;

                var orderCriteria = string.Empty;
                var orderAscendingDirection = true;

                if (dtParameters.Order != null)
                {
                    orderCriteria = dtParameters.Columns[dtParameters.Order[0].Column].Data;
                    orderAscendingDirection = dtParameters.Order[0].Dir.ToString().ToLower() == "asc";
                }
                else
                {
                    orderCriteria = "Id";
                    orderAscendingDirection = true;
                }

                var username = HttpContext.User.Identity.Name;
                var result = string.IsNullOrEmpty(searchBy)
                    ? _context.Transactions.AsNoTracking().Where(x => x.Username == username).Select(x => x.ToUser()).ToList()
                    : _context.Transactions.AsNoTracking().Where(x => x.Username == username && (
                            x.Status.ToString().ToUpper().Contains(searchBy.ToUpper()) ||
                            x.Username.ToString().ToUpper().Contains(searchBy.ToUpper()) ||
                            x.Description.ToString().ToUpper().Contains(searchBy.ToUpper()) ||
                            x.Amount.ToString().ToUpper().Contains(searchBy.ToUpper())
                            ))
                        .Select(x => x.ToUser()).ToList();
                var totalResultsCount = result.Count;

                result = orderAscendingDirection
                ? result.AsQueryable().OrderByDynamic(orderCriteria, LinqExtensions.Order.Asc).ToList()
                : result.AsQueryable().OrderByDynamic(orderCriteria, LinqExtensions.Order.Desc).ToList();

                response.Result = (new DTResult<TransactionUser>
                {
                    draw = dtParameters.Draw,
                    recordsTotal = totalResultsCount,
                    recordsFiltered = result.Count,
                    data = result
                        .Skip(dtParameters.Start)
                        .Take(dtParameters.Length)
                        .ToList()
                });
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return new JsonResult(response);
        }
    }
}
