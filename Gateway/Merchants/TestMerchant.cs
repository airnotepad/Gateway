using Gateway.Entity;
using Newtonsoft.Json;
using RestSharp;
using Serilog;

namespace Gateway.Merchants
{
    public class TestMerchant : IMerchant
    {
        public string Name { get => "TestMerchant"; }
        private static readonly Serilog.ILogger logger = Log.ForContext<TestMerchant>();
        private static string URL => "";
        public Response<ICreateMerchantTransaction> Create(int orderId, double amount, string description)
        {
            var result = new Response<ICreateMerchantTransaction>() { data = new TestMerchantCreate() };

            try
            {
                var _amount = amount * 100;

                var client = new RestClient();
                var request = new RestRequest(URL);
                request.AddQueryParameter("orderid", orderId.ToString());
                request.AddQueryParameter("amount", _amount.ToString());
                request.AddQueryParameter("description", description);

                logger.Information($"Try create new order {orderId} for {_amount} {description}");

                var response = client.ExecuteAsync(request).Result;
                result.isSuccess = response.IsSuccessful;
                result.content = response.Content;
                if (response.IsSuccessful)
                {
                    var status = JsonConvert.DeserializeObject<Create>(response.Content);

                    if (!string.IsNullOrEmpty(status.errorCode)) throw new Exception($"TestMerchant create error {status.errorCode} ({status.errorMessage})");

                    result.data.merchantOrderId = status.orderId;
                    result.data.paymentRef = status.formUrl;
                }
            }
            catch (Exception ex)
            {
                result.exception = ex;
                result.isSuccess = false;

                logger.Error(ex, "TestMerchant create order error {@result}", result);
            }

            return result;
        }

        public Response<IStatusMerchantTransaction> Status(string merchantOrderId)
        {
            var result = new Response<IStatusMerchantTransaction>() { data = new TestMerchantStatus() };

            try
            {
                var client = new RestClient();
                var request = new RestRequest(URL);
                request.AddQueryParameter("orderId", merchantOrderId);

                logger.Information($"Try get status for {merchantOrderId}");

                var response = client.ExecuteAsync(request).Result;
                result.isSuccess = response.IsSuccessful;
                result.content = response.Content;
                if (response.IsSuccessful)
                {
                    var status = JsonConvert.DeserializeObject<Status>(response.Content);
                    result.data.statusCode = status.orderStatus.ToString();
                    result.data.processingStatusCode = status.actionCode.ToString();
                    result.data.processingStatusCodeDescription = status.actionCodeDescription;
                    result.data.errorCode = status.errorCode;
                    result.data.errorMessage = status.errorMessage;
                    result.data.paymentWay = status.paymentWay;
                }
            }
            catch (Exception ex)
            {
                result.exception = ex;
                result.isSuccess = false;

                logger.Error(ex, "TestMerchant status order error {@result}", result);
            }

            return result;
        }

        public Response<string> Successful(string email, string productName, string productId, string formId, string price, string payDate, string orderId)
        {
            var result = new Response<string>() { data = String.Empty };

            try
            {
                var client = new RestClient();
                var request = new RestRequest(URL, Method.POST);
                request.AddParameter("form[email]", email);
                request.AddParameter("form[conditions]", "on");
                request.AddParameter("form[product_name]", productName);
                request.AddParameter("form[price]", price);
                request.AddParameter("form[product_id]", productId);
                request.AddParameter("form[formId]", "4");
                request.AddParameter("datePayment", payDate);
                request.AddParameter("order", orderId);

                logger.Information($"Try get successful for {orderId}");

                var response = client.ExecuteAsync(request).Result;
                result.isSuccess = response.IsSuccessful;
                result.content = response.Content;
                if (response.IsSuccessful)
                {
                    result.data = response.Content;
                }
            }
            catch (Exception ex)
            {
                result.exception = ex;
                result.isSuccess = false;

                logger.Error(ex, "TestMerchant successful order error {@result}", result);
            }

            return result;
        }
    }

    public class TestMerchantCreate : ICreateMerchantTransaction
    {
        public string merchantOrderId { get; set; }
        public string paymentRef { get; set; }
    }

    public class TestMerchantStatus : IStatusMerchantTransaction
    {
        public string statusCode { get; set; } //orderStatus

        public string statusCodeDescription
        {
            get
            {
                switch (statusCode)
                {
                    case "0": return "Заказ зарегистрирован, но не оплачен";
                    case "1": return "Предавторизованная сумма удержана(для двухстадийных платежей)"; // нет подтверждения
                    case "2": return "Проведена полная авторизация суммы заказа";
                    case "3": return "Авторизация отменена";
                    case "4": return "По транзакции была проведена операция возврата";
                    case "5": return "Инициирована авторизация через сервер контроля доступа банка-эмитента";
                    case "6": return "Авторизация отклонена";
                    default: throw new Exception($"Implement TestMerchant status error {statusCode}");
                };
            }
        }

        public TransactionStatus transactionStatus
        {
            get
            {
                switch (statusCode)
                {
                    case "0": return TransactionStatus.AwaitingPay;
                    case "1": return TransactionStatus.AwaitingPay; // не потдерживается
                    case "2": return TransactionStatus.Paid;
                    case "3": return TransactionStatus.Canceled;
                    case "4": return TransactionStatus.Refund;
                    case "5": return TransactionStatus.AwaitingPay;
                    case "6": return TransactionStatus.Rejected;
                    default: throw new Exception($"Implement TestMerchant status error {statusCode}");
                }
            }
        }

        public string processingStatusCode { get; set; } //actionCode

        public string processingStatusCodeDescription { get; set; } //actionCodeDescription

        public string errorCode { get; set; } //errorCode

        public string errorMessage { get; set; } //errorMessage

        public string paymentWay { get; set; } //paymentWay
    }

    public class Status
    {
        public class Attribute
        {
            public string name { get; set; }
            public string value { get; set; }
        }

        public class PaymentAmountInfo
        {
            public string paymentState { get; set; }
            public int approvedAmount { get; set; }
            public int depositedAmount { get; set; }
            public int refundedAmount { get; set; }
            public int feeAmount { get; set; }
            public int totalAmount { get; set; }
        }

        public class BankInfo
        {
            public string bankCountryCode { get; set; }
            public string bankCountryName { get; set; }
        }

        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public string orderNumber { get; set; }
        public int orderStatus { get; set; }
        public int actionCode { get; set; }
        public string actionCodeDescription { get; set; }
        public int amount { get; set; }
        public string currency { get; set; }
        public long date { get; set; }
        public string orderDescription { get; set; }
        public List<object> merchantOrderParams { get; set; }
        public List<Attribute> transactionAttributes { get; set; }
        public List<Attribute> attributes { get; set; }
        public string terminalId { get; set; }
        public PaymentAmountInfo paymentAmountInfo { get; set; }
        public BankInfo bankInfo { get; set; }
        public bool chargeback { get; set; }
        public string paymentWay { get; set; }
    }

    public class Create
    {
        public string orderId { get; set; }
        public string formUrl { get; set; }
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
    }
}