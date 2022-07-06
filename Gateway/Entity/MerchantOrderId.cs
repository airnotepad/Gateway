namespace Gateway.Entity
{
    public class MerchantOrderId
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Merchant { get; set; }
        public string OrderId { get; set; }
        public string OrderIdMerchant { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string Product { get; set; }
        public string ProductId { get; set; }
        public double Amount { get; set; }
        public bool IsPaid{ get; set; }
        public DateTime SuccessfulDate { get; set; }
        public string? SuccessfulResponse { get; set; }
        public DateTime CreateDate { get; set; }
        public string? CreateResponse { get; set; }
        public DateTime StatusDate { get; set; }
        public string? StatusResponse { get; set; }
    }
}
