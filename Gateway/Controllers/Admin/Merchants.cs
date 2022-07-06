using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Gateway.Models.FrontendAPI;
using Gateway.Models.Base;
using Gateway.Models.JS;
using Gateway.Merchants;
using Gateway.Entity;
using Gateway.Helper;

namespace Gateway.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class AdminController : Controller
    {
        public IActionResult Merchants()
        {
            ViewBag.Merchants = MerchantSwitcher.GetMerchantNames();

            return View();
        }

        [HttpPost]
        public JsonResult MerchantOrders([FromBody] DTParameters dtParameters, string merchant)
        {
            var response = new APIResponse<DTResult<MerchantOrderId>>();

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
                    ? _context.MerchantOrderIds.AsNoTracking().Where(x => x.Merchant == merchant).ToList()
                    : _context.MerchantOrderIds.AsNoTracking().Where(x => x.Merchant == merchant && (
                            x.OrderId.ToString().ToUpper().Contains(searchBy.ToUpper()) ||
                            x.Description.ToString().ToUpper().Contains(searchBy.ToUpper()) ||
                            x.Amount.ToString().ToUpper().Contains(searchBy.ToUpper())
                            ))
                        .ToList();
                var totalResultsCount = result.Count;

                result = orderAscendingDirection
                ? result.AsQueryable().OrderByDynamic(orderCriteria, LinqExtensions.Order.Asc).ToList()
                : result.AsQueryable().OrderByDynamic(orderCriteria, LinqExtensions.Order.Desc).ToList();

                response.Result = (new DTResult<MerchantOrderId>
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

        public JsonResult MerchantTransactionCreate([FromBody] MerchantCreate create, string merchant)
        {
            var response = new APIResponse<string>();

            try
            {
                response.Result = _transactionService.CreateTransaction(merchant, create.OrderId, create.Amount, create.Product, create.Email, create.ProductId, "Тестовая транзакция").content;
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return new JsonResult(response);
        }

        public JsonResult MerchantTransactionStatus([FromBody] MerchantStatus status, string merchant)
        {
            var response = new APIResponse<string>();

            try
            {
                response.Result = _transactionService.StatusTransaction(merchant, status.OrderId).content;
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return new JsonResult(response);
        }
    }
}