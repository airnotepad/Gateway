namespace Gateway.Models.FrontendAPI
{
    public class DashboardCreate
    {
        public int ProductId { get; set; }
        public double Amount { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
    }
}
