using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Gateway.Models.Base;
using Gateway.Models.JS;
using Gateway.Services;
using Gateway.Entity;
using Gateway.Helper;

namespace Gateway.Controllers
{
    [Authorize]
    public class PayoutController : Controller
    {
        private readonly ILogger<PayoutController> _logger;
        private readonly Context _context;
        private readonly SettingService _settings;
        private readonly BalanceService _balanceService;

        public PayoutController(ILogger<PayoutController> logger, Context context, SettingService settings, BalanceService balanceService)
        {
            _logger = logger;
            _context = context;
            _settings = settings;
            _balanceService = balanceService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public JsonResult AccountInfo()
        {
            var response = new APIResponse<AccountInfoPayout>();

            try
            {
                var username = HttpContext.User.Identity.Name;
                var user = _context.Users.AsNoTracking().FirstOrDefault(x => x.Username == username);
                if (user == null) throw new Exception("No such user");

                var total = _context.Withdraws.Where(t => t.User_id == user.Id && t.Status == WithdrawStatus.Paid).Sum(x => x.Amount);

                response.Result = new AccountInfoPayout()
                {
                    Balance = _balanceService.GetBalance(x => x.Id == user.Id),
                    Total = total
                };
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return new JsonResult(response);
        }

        [HttpPost]
        public JsonResult Table([FromBody] DTParameters dtParameters)
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

                var username = HttpContext.User.Identity.Name;
                var result = string.IsNullOrEmpty(searchBy)
                    ? _context.Withdraws.AsNoTracking().Where(x => x.Username == username).ToList()
                    : _context.Withdraws.AsNoTracking().Where(x => x.Username == username && (
                            x.Status.ToString().ToUpper().Contains(searchBy.ToUpper()) ||
                            x.Username.ToString().ToUpper().Contains(searchBy.ToUpper()) ||
                            x.Amount.ToString().ToUpper().Contains(searchBy.ToUpper())
                            ))
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

        public JsonResult CreateWithdraw(double amount, string description)
        {
            var response = new APIResponse<bool>();

            try
            {
                var username = HttpContext.User.Identity.Name;
                var user = _context.Users.AsNoTracking().FirstOrDefault(x => x.Username == username);
                if (user == null) throw new Exception("No such user");

                var balance = _balanceService.GetBalance(x => x.Id == user.Id);
                if (balance < amount) throw new Exception("No balance"); 

                _balanceService.CreateWithdraw(x => x.Username == username, amount, description);

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return new JsonResult(response);
        }

        public JsonResult CancelWithdraw(int id)
        {
            var response = new APIResponse<bool>();

            try
            {
                var username = HttpContext.User.Identity.Name;
                var user = _context.Users.AsNoTracking().FirstOrDefault(x => x.Username == username);
                if (user == null) throw new Exception("No such user");

                _balanceService.CancelWithdraw(x => x.Username == username, id);

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
