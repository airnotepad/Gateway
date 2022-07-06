namespace Gateway.Models.FrontendAPI
{
    public class MerchantCreate
    {
        public int OrderId { get; set; }
        public double Amount { get; set; }
        public string Product { get; set; }
        public string ProductId { get; set; }
        public string Email { get; set; }
    }
}
