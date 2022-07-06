namespace Gateway.Entity
{
    public class Transaction
    {
        public int Id { get; set; }
        public int User_id { get; set; }
        public string Username { get; set; }
        public DateTime Create_date { get; set; }
        public DateTime? Check_date { get; set; }
        public DateTime? Paid_date { get; set; }
        public DateTime? Cancel_date { get; set; }
        public string? Description { get; set; }
        public string? Email { get; set; }
        public double Amount { get; set; }
        public double User_amount { get; set; }
        public string? Order_id { get; set; }
        public string? Merchant_order_id { get; set; }
        public string Merchant_name{ get; set; }
        public string Product{ get; set; }
        public TransactionStatus Status { get; set; }
        public string? Status_description { get; set; }
        public string? Pay_ref { get; set; }

        public TransactionUser ToUser()
        {
            return new TransactionUser()
            {
                Id = this.Id,
                Create_date = this.Create_date,
                Description = this.Description,
                Email = this.Email,
                Amount = this.Amount,
                User_amount = this.User_amount,
                Status = this.Status,
                Status_description = this.Status_description,
                Pay_ref = this.Pay_ref
            };
        }
    }

    public enum TransactionStatus
    {
        AwaitingPay,
        Paid,
        ExpiredPay,
        PaymentError,
        Canceled,
        Rejected,
        Refund
    }

    public static class TransactionStatusHelper
    {
        public static string GetValue(this TransactionStatus status)
        {
            switch (status)
            {
                case TransactionStatus.AwaitingPay: return "Ожидает оплаты";
                case TransactionStatus.Paid: return "Оплачена";
                case TransactionStatus.ExpiredPay: return "Вышло время для оплаты";
                case TransactionStatus.PaymentError: return "Ошибка оплаты";
                case TransactionStatus.Canceled: return "Отменена";
                case TransactionStatus.Rejected: return "Отклонена";
                case TransactionStatus.Refund: return "Возврат средств";
                default: return "Ошибка";
            }
        }
    }

    public class TransactionUser
    {
        public int Id { get; set; }
        public DateTime Create_date { get; set; }
        public string? Description { get; set; }
        public string? Email { get; set; }
        public double Amount { get; set; }
        public double User_amount { get; set; }
        public TransactionStatus Status { get; set; }
        public string? Status_description { get; set; }
        public string? Pay_ref { get; set; }
    }
}