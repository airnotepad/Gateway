using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Gateway.Models.Base;
using Gateway.Models.JS;
using Gateway.Entity;
using Gateway.Helper;

namespace Gateway.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class AdminController : Controller
    {
        public IActionResult Transactions()
        {
            return View();
        }

        [HttpPost]
        public JsonResult TransactionsTable([FromBody] DTParameters dtParameters)
        {
            var response = new APIResponse<DTResult<Transaction>>();

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

                var result = string.IsNullOrEmpty(searchBy)
                    ? _context.Transactions.AsNoTracking().ToList()
                    : _context.Transactions.AsNoTracking().Where(x =>
                            x.Status.ToString().ToUpper().Contains(searchBy.ToUpper()) ||
                            x.Username.ToString().ToUpper().Contains(searchBy.ToUpper()) ||
                            x.Amount.ToString().ToUpper().Contains(searchBy.ToUpper())
                            )
                        .ToList();
                var totalResultsCount = result.Count;

                result = orderAscendingDirection
                ? result.AsQueryable().OrderByDynamic(orderCriteria, LinqExtensions.Order.Asc).ToList()
                : result.AsQueryable().OrderByDynamic(orderCriteria, LinqExtensions.Order.Desc).ToList();

                response.Result = (new DTResult<Transaction>
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
