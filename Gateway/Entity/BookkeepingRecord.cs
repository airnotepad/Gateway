namespace Gateway.Entity
{
    public class BookkeepingRecord
    {
        public int Id { get; set; }
        public int User_id { get; set; }
        public string Username { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; }
        public double Amount { get; set; }
        public double UserAmount { get; set; }
        public double? ExpenseAmount { get; set; }
        public double? AdminAmount { get; set; }
        public double PrevBalance { get; set; }
        public double NewBalance { get; set; }
        public BookkeepingRecordForm Form { get; set; }
        public BookkeepingRecordType Type { get; set; }
    }

    public enum BookkeepingRecordForm
    {
        Incoming,
        Outgoing,
    }

    public enum BookkeepingRecordType
    {
        Transaction,
        Withdraw,
    }
}