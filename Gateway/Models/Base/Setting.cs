using Gateway.Helper;

namespace Gateway.Models.Base
{
    public class Settings : ICloneable<Settings>
    {
        public bool IsEnabled { get; set; }
        public bool IsHostedTransactionCheckerEnabled { get; set; }
        public bool IsCreateTransactionsEnabled { get; set; }
        public List<MerchantProduct> MerchantsProducts { get; set; }
        public List<MerchantExpense> MerchantsExpenses { get; set; }

        public Settings Clone()
        {
            return new Settings()
            {
                IsEnabled = this.IsEnabled,
                IsHostedTransactionCheckerEnabled = this.IsHostedTransactionCheckerEnabled,
                IsCreateTransactionsEnabled = this.IsCreateTransactionsEnabled,
                MerchantsProducts = this.MerchantsProducts?.CloneList(),
                MerchantsExpenses = this.MerchantsExpenses?.CloneList()
            };
        }

        public static Settings NewSettings() => new Settings()
        {
            IsEnabled = false,
            IsHostedTransactionCheckerEnabled = false,
            IsCreateTransactionsEnabled = false,
            MerchantsProducts = new List<MerchantProduct>()
            {
                new MerchantProduct()
                {
                    Id = 1,
                    MerchantName = "TestMerchant",
                    ProductName = "Продукт 1",
                    ProductId = 1,
                    Amount = 3500
                }
            },
            MerchantsExpenses= new List<MerchantExpense>()
            {
                new MerchantExpense()
                {
                    Id = 1,
                    MerchantName = "TestMerchant",
                    ExpensePercent = 9
                }
            }
        };
    }

    public class MerchantProduct : ICloneable<MerchantProduct>
    {
        public int Id { get; set; }
        public string MerchantName { get; set; }
        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public double Amount { get; set; }

        public MerchantProduct Clone()
        {
            return new MerchantProduct()
            {
                Id = this.Id,
                MerchantName = this.MerchantName,
                ProductName = this.ProductName,
                ProductId = this.ProductId,
                Amount = this.Amount
            };
        }
    }

    public class MerchantExpense : ICloneable<MerchantExpense>
    {
        public int Id { get; set; }
        public string MerchantName { get; set; }
        public double ExpensePercent { get; set; }

        public MerchantExpense Clone()
        {
            return new MerchantExpense()
            {
                Id = this.Id,
                MerchantName = this.MerchantName,
                ExpensePercent = this.ExpensePercent
            };
        }
    }
}