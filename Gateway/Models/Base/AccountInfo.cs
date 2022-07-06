namespace Gateway.Models.Base
{
    public class AccountInfo
    {
        public double Balance { get; set; }
        public double Turnover { get; set; }
        public double Profit { get; set; }
    }

    public class AccountInfoPayout
    {
        public double Balance { get; set; }
        public double Total { get; set; }
    }
}
