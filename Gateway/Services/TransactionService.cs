using Gateway.Entity;
using Gateway.Helper;
using Gateway.Merchants;
using Gateway.Models.Base;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Services
{
    public class TransactionService
    {
        protected readonly ILogger<TransactionService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private RWLock locker = new RWLock();

        public TransactionService(ILogger<TransactionService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public bool CreateTransaction(MerchantProduct product, string description, string email, Func<User, bool> userPredicate)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _settings = scope.ServiceProvider.GetService<SettingService>();

                if (!_settings.GetSettings().IsCreateTransactionsEnabled) return false;

                var _context = scope.ServiceProvider.GetRequiredService<Context>();

                var user = _context.Users.AsNoTracking().FirstOrDefault(userPredicate);
                if (user == null) throw new Exception($"CreateTransaction: user is null");

                var merchant = MerchantSwitcher.GetMerchant(product.MerchantName);
                if (merchant == null) throw new Exception($"CreateTransaction: merchant is null (product {product.Id} : {product.MerchantName})");

                using (locker.WriteLock())
                {
                    var lastOrderId = _context.MerchantOrderIds.AsNoTracking().OrderBy(o => o.Id).LastOrDefault(x => x.Merchant == merchant.Name);
                    var orderId = lastOrderId != null ? int.Parse(lastOrderId.OrderId) + 1 : 10;

                    var response = merchant.Create(orderId, product.Amount, product.ProductName);

                    if (response.isSuccess)
                    {
                        var create = response.data;
                        _context.MerchantOrderIds.Add(new MerchantOrderId()
                        {
                            Merchant = merchant.Name,
                            OrderId = orderId.ToString(),
                            OrderIdMerchant = create.merchantOrderId,
                            Date = Date.Now,
                            Description = description,
                            Email = email,
                            Product = product.ProductName,
                            ProductId = product.ProductId.ToString(),
                            Amount = product.Amount,
                            IsPaid = false,
                            CreateDate = Date.Now,
                            CreateResponse = response.content
                        });

                        var userAmount = product.Amount * (100 - user.Percent) / 100;

                        var transaction = new Transaction()
                        {
                            Create_date = Date.Now,
                            Amount = product.Amount,
                            Product = product.ProductName,
                            Description = description,
                            Email = email,
                            Order_id = orderId.ToString(),
                            Merchant_order_id = create.merchantOrderId,
                            Merchant_name = merchant.Name,
                            Pay_ref = create.paymentRef,
                            Status = TransactionStatus.AwaitingPay,
                            User_amount = userAmount,
                            Username = user.Username,
                            User_id = user.Id
                        };

                        _context.Transactions.Add(transaction);
                        _context.SaveChanges();
                    }
                    else
                    {
                        throw new Exception($"CreateTransaction: response is not success", response.exception);
                    }

                    return response.isSuccess;
                }
            }
        }

        public Response<ICreateMerchantTransaction> CreateTransaction(string merchantName, int orderId, double amount, string product, string email, string productId, string description)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<Context>();

                var merchant = MerchantSwitcher.GetMerchant(merchantName);
                if (merchant == null) throw new Exception($"CreateTransaction: merchant is null ({merchantName})");

                if (_context.MerchantOrderIds.AsNoTracking().Any(x => x.Merchant == merchantName && x.OrderId == orderId.ToString()))
                    throw new Exception($"CreateTransaction: merchant_trx alreadu exist (order {orderId} already exist for merchant {merchantName})");

                using (locker.WriteLock())
                {
                    var response = merchant.Create(orderId, amount, product);

                    if (response.isSuccess)
                    {
                        _context.MerchantOrderIds.Add(new MerchantOrderId()
                        {
                            Merchant = merchant.Name,
                            OrderId = orderId.ToString(),
                            OrderIdMerchant = response.data.merchantOrderId,
                            Date = Date.Now,
                            Description = description,
                            Email = email,
                            Product = product,
                            ProductId = productId,
                            Amount = amount,
                            IsPaid = false,
                            CreateDate = Date.Now,
                            CreateResponse = response.content
                        });
                        _context.SaveChanges();
                    }

                    return response;
                }
            }
        }

        public Response<IStatusMerchantTransaction> StatusTransaction(string merchantName, int orderId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<Context>();

                var merchant_trx = _context.MerchantOrderIds.FirstOrDefault(x => x.Merchant == merchantName && x.OrderId == orderId.ToString());
                if (merchant_trx == null) throw new Exception($"StatusTransaction: merchant_trx is null (not found order {orderId} for merchant {merchantName})");

                var merchant = MerchantSwitcher.GetMerchant(merchantName);
                if (merchant == null) throw new Exception($"StatusTransaction: merchant is null (not found merchant {merchantName})");

                using (locker.WriteLock())
                {
                    var response = merchant.Status(merchant_trx.OrderIdMerchant);

                    if (response.isSuccess)
                    {
                        if (response.data.transactionStatus == TransactionStatus.Paid && !merchant_trx.IsPaid)
                        {
                            var responseSuccessful = merchant.Successful(
                                email: merchant_trx.Email,
                                productName: merchant_trx.Product,
                                productId: merchant_trx.ProductId,
                                formId: "4",
                                price: merchant_trx.Amount.ToString(),
                                payDate: Date.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                orderId: merchant_trx.OrderId);

                            merchant_trx.SuccessfulDate = Date.Now;
                            merchant_trx.SuccessfulResponse = responseSuccessful.content;

                            merchant_trx.IsPaid = true;
                        }

                        merchant_trx.StatusDate = Date.Now;
                        merchant_trx.StatusResponse = response.content;

                        _context.SaveChanges();
                    }

                    return response;
                }
            }
        }
    }
}