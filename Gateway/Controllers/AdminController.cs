using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gateway.Models.Base;
using Gateway.Models.JS;
using Gateway.Services;
using Gateway.Entity;
using Gateway.Helper;

namespace Gateway.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class AdminController : Controller
    {
        protected readonly ILogger<AdminController> _logger;
        private readonly Context _context;
        private readonly OnlineUsersService _online;
        protected readonly SettingService _settings;
        private readonly BalanceService _balanceService;
        private readonly TransactionService _transactionService;
        private readonly HostedServiceLogger _serviceLogger;
        private readonly InviteService _inviteService;

        public AdminController(ILogger<AdminController> logger, Context context, OnlineUsersService online, SettingService settings, BalanceService balanceService, TransactionService transactionService, HostedServiceLogger serviceLogger, InviteService inviteService)
        {
            _logger = logger;
            _context = context;
            _online = online;
            _settings = settings;
            _balanceService = balanceService;
            _transactionService = transactionService;
            _serviceLogger = serviceLogger;
            _inviteService = inviteService;
        }

        [HttpPost]
        public JsonResult Online([FromBody] DTParameters dtParameters)
        {
            var response = new APIResponse<DTResult<OnlineUser>>();

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
                    orderCriteria = "LastDate";
                    orderAscendingDirection = true;
                }

                var result = _online.GetOnline().ToList();
                var totalResultsCount = result.Count;

                result = orderAscendingDirection
                ? result.AsQueryable().OrderByDynamic(orderCriteria, LinqExtensions.Order.Asc).ToList()
                : result.AsQueryable().OrderByDynamic(orderCriteria, LinqExtensions.Order.Desc).ToList();

                response.Result = (new DTResult<OnlineUser>
                {
                    draw = dtParameters.Draw,
                    recordsTotal = totalResultsCount,
                    recordsFiltered = result.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return new JsonResult(response);
        }

        public JsonResult AccountInfo(string username)
        {
            var response = new APIResponse<AccountInfo>();

            try
            {
                var user = _context.Users.FirstOrDefault(x => x.Username == username);
                if (user == null) throw new Exception("No such user");

                var turnover = _context.Transactions.AsNoTracking().Where(t => t.User_id == user.Id && t.Status == TransactionStatus.Paid).Sum(x => x.Amount);
                var profit = _context.BookkeepingRecords.AsNoTracking().Where(t => t.User_id == user.Id && t.Type == BookkeepingRecordType.Transaction && t.AdminAmount != null).Sum(x => x.AdminAmount);

                response.Result = new AccountInfo()
                {
                    Balance = _balanceService.GetBalance(x => x.Id == user.Id),
                    Turnover = turnover,
                    Profit = profit ?? 0
                };
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return new JsonResult(response);
        }
    }
}
