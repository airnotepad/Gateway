namespace Gateway.Entity
{
    public class Withdraw
    {
        public int Id { get; set; }
        public int User_id { get; set; }
        public string Username { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public double Amount { get; set; }
        public WithdrawStatus Status { get; set; }
    }

    public enum WithdrawStatus
    {
        AwaitingPay,
        InProcess,
        Paid,
        Canceled,
    }

    public static class WithdrawStatusHelper
    {
        public static string GetValue(this WithdrawStatus status)
        {
            switch (status)
            {
                case WithdrawStatus.AwaitingPay: return "Ожидает выплаты";
                case WithdrawStatus.InProcess: return "В обработке";
                case WithdrawStatus.Paid: return "Выплачена";
                case WithdrawStatus.Canceled: return "Отменена";
                default: return "Ошибка";
            }
        }
    }
}
