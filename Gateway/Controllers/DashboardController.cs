using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gateway.Models.FrontendAPI;
using Gateway.Models.Base;
using Gateway.Models.JS;
using Gateway.Services;
using Gateway.Entity;
using Gateway.Helper;

namespace Gateway.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly Context _context;
        private readonly SettingService _settings;
        private readonly TransactionService _transactionService;

        public DashboardController(ILogger<DashboardController> logger, Context context, SettingService settings, TransactionService transactionService)
        {
            _logger = logger;
            _context = context;
            _settings = settings;
            _transactionService = transactionService;
        }

        public IActionResult Index()
        {
            var settings = _settings.GetSettings();

            var products = settings.MerchantsProducts.Any()
                ? settings.MerchantsProducts.Select(x => new MerchantProductAmount() { Id = x.Id, Amount = x.Amount }).ToList()
                : new List<MerchantProductAmount>();
            ViewBag.Products = products;

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

                var user = _context.Users.AsNoTracking().FirstOrDefault(u => u.Username == HttpContext.User.Identity.Name);
                if (user == null) throw new Exception("No such user");
                var today = Date.Now.AddDays(-1);
                var result = _context.Transactions.AsNoTracking().Where(x => x.User_id == user.Id && x.Create_date > today).Select(x => x.ToUser()).ToList();
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
                });
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return new JsonResult(response);
        }

        [HttpPost]
        public JsonResult CreateTransaction([FromBody] DashboardCreate create)
        {
            var response = new APIResponse<bool>();

            try
            {
                var settings = _settings.GetSettings();
                var product = settings.MerchantsProducts.FirstOrDefault(x => x.Id == create.ProductId);
                if (product == null) throw new Exception($"No such product");

                if (!settings.IsCreateTransactionsEnabled) throw new Exception($"Creation new deals is suspended");

                response.Result = _transactionService.CreateTransaction(product, create.Description, create.Email, u => u.Username == HttpContext.User.Identity.Name);
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return new JsonResult(response);
        }
    }
}
