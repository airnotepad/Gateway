using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gateway.Models.Base;
using Gateway.Merchants;

namespace Gateway.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class AdminController : Controller
    {
        public IActionResult Settings()
        {
            ViewBag.Merchants = MerchantSwitcher.GetMerchantNames();

            return View();
        }

        public JsonResult GetSettings()
        {
            return new JsonResult(new APIResponse<Settings>(_settings.GetSettings()));
        }

        [HttpPost]
        public JsonResult SaveSettings([FromBody] Settings data)
        {
            var result = new APIResponse<bool>(true);

            try
            {
                var settings = _settings.GetSettings();

                settings.IsEnabled = data.IsEnabled;
                settings.IsHostedTransactionCheckerEnabled = data.IsHostedTransactionCheckerEnabled;
                settings.IsCreateTransactionsEnabled = data.IsCreateTransactionsEnabled;

                result.Result = _settings.SetSettings(settings);
            }
            catch (Exception ex)
            {
                result.SetException(ex);
                result.Result = false;
            }

            return new JsonResult(result);
        }

        [HttpPost]
        public JsonResult SaveMerchantProduct([FromBody] MerchantProduct data)
        {
            var result = new APIResponse<bool>(true);

            try
            {
                var settings = _settings.GetSettings();

                if (settings.MerchantsProducts.Any(x => x.Id == data.Id))
                {
                    settings.MerchantsProducts[settings.MerchantsProducts.FindIndex(x => x.Id == data.Id)] = data;
                }
                else
                {
                    var last = settings.MerchantsProducts.LastOrDefault();
                    data.Id = last != null ? last.Id + 1  : 1;
                    settings.MerchantsProducts.Add(data);
                }

                result.Result = _settings.SetSettings(settings);
            }
            catch (Exception ex)
            {
                result.SetException(ex);
                result.Result = false;
            }

            return new JsonResult(result);
        }

        [HttpPost]
        public JsonResult DeleteMerchantProduct([FromBody] MerchantProduct data)
        {
            var result = new APIResponse<bool>(true);

            try
            {
                var settings = _settings.GetSettings();

                if (settings.MerchantsProducts.Any(x => x.Id == data.Id))
                    settings.MerchantsProducts.RemoveAt(settings.MerchantsProducts.FindIndex(x => x.Id == data.Id));
                else
                    throw new Exception("Нет продукта с таким ID");

                result.Result = _settings.SetSettings(settings);
            }
            catch (Exception ex)
            {
                result.SetException(ex);
                result.Result = false;
            }

            return new JsonResult(result);
        }

        [HttpPost]
        public JsonResult SaveMerchantExpense([FromBody] MerchantExpense data)
        {
            var result = new APIResponse<bool>(true);

            try
            {
                var settings = _settings.GetSettings();

                if (settings.MerchantsExpenses.Any(x => x.Id == data.Id))
                {
                    settings.MerchantsExpenses[settings.MerchantsExpenses.FindIndex(x => x.Id == data.Id)] = data;
                }
                else
                {
                    var last = settings.MerchantsExpenses.LastOrDefault();
                    data.Id = last != null ? last.Id + 1 : 1;
                    settings.MerchantsExpenses.Add(data);
                }

                result.Result = _settings.SetSettings(settings);
            }
            catch (Exception ex)
            {
                result.SetException(ex);
                result.Result = false;
            }

            return new JsonResult(result);
        }

        [HttpPost]
        public JsonResult DeleteMerchantExpense([FromBody] MerchantExpense data)
        {
            var result = new APIResponse<bool>(true);

            try
            {
                var settings = _settings.GetSettings();

                if (settings.MerchantsExpenses.Any(x => x.Id == data.Id))
                    settings.MerchantsExpenses.RemoveAt(settings.MerchantsExpenses.FindIndex(x => x.Id == data.Id));
                else
                    throw new Exception("Нет записи с таким ID");

                result.Result = _settings.SetSettings(settings);
            }
            catch (Exception ex)
            {
                result.SetException(ex);
                result.Result = false;
            }

            return new JsonResult(result);
        }

        public JsonResult GetMyIp()
        {
            var result = new APIResponse<string>(String.Empty);

            try
            {
                result.Result = Helper.Myip.GetServerIp();
            }
            catch (Exception ex)
            {
                result.SetException(ex);
            }

            return new JsonResult(result);
        }
    }
}
