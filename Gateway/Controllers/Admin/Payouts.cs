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
        public IActionResult Payouts()
        {
            return View();
        }

        [HttpPost]
        public JsonResult PayoutsTable([FromBody] DTParameters dtParameters)
        {
            var response = new APIResponse<DTResult<Withdraw>>();

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
                    ? _context.Withdraws.AsNoTracking().ToList()
                    : _context.Withdraws.AsNoTracking().Where(x =>
                            x.Status.ToString().ToUpper().Contains(searchBy.ToUpper()) ||
                            x.Username.ToString().ToUpper().Contains(searchBy.ToUpper()) ||
                            x.Amount.ToString().ToUpper().Contains(searchBy.ToUpper())
                            )
                        .ToList();
                var totalResultsCount = result.Count;

                result = orderAscendingDirection
                ? result.AsQueryable().OrderByDynamic(orderCriteria, LinqExtensions.Order.Asc).ToList()
                : result.AsQueryable().OrderByDynamic(orderCriteria, LinqExtensions.Order.Desc).ToList();

                response.Result = (new DTResult<Withdraw>
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

        public JsonResult StartProcessingWithdraw(int id)
        {
            var response = new APIResponse<bool>();

            try
            {
                var withdraw = _context.Withdraws.FirstOrDefault(x => x.Id == id);
                if (withdraw == null) throw new Exception("No such withdraw");

                withdraw.Status = WithdrawStatus.InProcess;
                _context.SaveChanges();

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return new JsonResult(response);
        }

        public JsonResult CompleteProcessingWithdraw(int id)
        {
            var response = new APIResponse<bool>();

            try
            {
                var withdraw = _context.Withdraws.FirstOrDefault(x => x.Id == id);
                if (withdraw == null) throw new Exception("No such withdraw");

                withdraw.Status = WithdrawStatus.Paid;
                _context.SaveChanges();

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return new JsonResult(response);
        }

        public JsonResult CancelProcessingWithdraw(int id)
        {
            var response = new APIResponse<bool>();

            try
            {
                var withdraw = _context.Withdraws.AsNoTracking().FirstOrDefault(x => x.Id == id);
                if (withdraw == null) throw new Exception("No such withdraw");

                _balanceService.CancelWithdraw(x => x.Username == withdraw.Username, id);

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return new JsonResult(response);
        }
    }
}
