using Gateway.Entity;

namespace Gateway.Merchants
{
    public interface IMerchant
    {
        public string Name { get; }
        public Response<ICreateMerchantTransaction> Create(int orderId, double amount, string description);
        public Response<IStatusMerchantTransaction> Status(string merchantOrderId);
        public Response<string> Successful(string email, string productName, string productId, string formId, string price, string payDate, string orderId);
    }

    public interface ICreateMerchantTransaction
    {
        public string merchantOrderId { get; set; }
        public string paymentRef { get; set; }
    }

    public interface IStatusMerchantTransaction
    {
        public string statusCode { get; set; }
        public string statusCodeDescription { get; }
        public TransactionStatus transactionStatus { get; }
        public string processingStatusCode { get; set; }
        public string processingStatusCodeDescription { get; set; }
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public string paymentWay { get; set; }
    }

    public class Response<T> where T : class
    {
        public T data { get; set; }
        public bool isSuccess { get; set; }
        public string content { get; set; }
        public Exception exception { get; set; }
    }

    public static class MerchantSwitcher
    {
        public static IEnumerable<string> GetMerchantNames()
        {
            var type = typeof(IMerchant);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.IsClass)
                .Select(x => type.GetType().GetProperty("Name").GetValue(x).ToString());

            return types != null ? types : new List<string>();
        }

        public static IMerchant GetMerchant(string name)
        {
            var merchant_type = typeof(IMerchant);

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => merchant_type.IsAssignableFrom(p) && p.IsClass);

            var type = types.FirstOrDefault(x => merchant_type.GetType().GetProperty("Name").GetValue(x).ToString() == name);

            if (type == null) throw new Exception($"No such merchant : {name}");

            IMerchant merchant = Activator.CreateInstance(type) as IMerchant;

            return merchant;
        }
    }

}