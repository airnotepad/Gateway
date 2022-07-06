using Gateway.Entity;
using Gateway.Helper;
using Gateway.HostedServices.Base;
using Gateway.Services;

namespace Gateway.HostedServices
{
    class TransactionCheckerHostedService : ScheduledHostedServiceBase
    {
        public TransactionCheckerHostedService(IServiceScopeFactory serviceScopeFactory, ILogger<TransactionCheckerHostedService> logger)
            : base(serviceScopeFactory, logger)
        {
        }

        protected override string Schedule => "*/30 * * * * *"; //every 30 
        protected override bool IncludingSeconds => true;
        protected override string DisplayName => "TransactionChecker";

        private HostedServiceLogger? _logs { get; set; }

        protected override async Task ProcessInScopeAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            _logs = serviceProvider.GetService<HostedServiceLogger>();
            var _settings = serviceProvider.GetService<SettingService>();

            if (!_settings.GetSettings().IsHostedTransactionCheckerEnabled)
            {
                Log($"Disabled");
                return;
            }

            var _context = serviceProvider.GetService<Context>();
            var _transactionService = serviceProvider.GetService<TransactionService>();
            var _balanceService = serviceProvider.GetService<BalanceService>();

            var transactions = _context.Transactions.Where(x => x.Status == TransactionStatus.AwaitingPay).ToList();

            Log($"Transactions {transactions.Count}");

            if (transactions.Any())
            {
                for (int i = 0; i < transactions.Count; i++)
                {
                    try
                    {
                        var transaction = transactions[i];

                        Log($"Trx #{transaction.Id} {transaction.Status}");

                        var response = _transactionService.StatusTransaction(transaction.Merchant_name, int.Parse(transaction.Order_id));

                        Log($"Trx #{transaction.Id} {response.data.transactionStatus} ({response.data.statusCodeDescription})");

                        transaction.Check_date = Date.Now;
                        transaction.Status = response.data.transactionStatus;
                        transaction.Status_description = response.data.statusCodeDescription;

                        if (response.data.transactionStatus == TransactionStatus.Paid)
                        {
                            transaction.Paid_date = Date.Now;
                            _balanceService.UpdateBalance(x => x.Id == transaction.User_id, transaction);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log(ex);
                    }
                }

                int saved = _context.SaveChanges();
                Log($"Saved to database {saved}");
                _logs.NewLine(DisplayName);
            }
        }

        private void Log(string log)
        {
            if (_logs != null)
                _logs.Push(DisplayName, log);
            Logger.LogInformation($"{DisplayName} : {log}");
        }
        private void Log(Exception ex)
        {
            if (_logs != null)
                _logs.Push(DisplayName, $"Error {ex.Message} in {ex.Source}");
            Logger.LogError(ex, $"{DisplayName} : Error {ex.Message} in {ex.Source}");
        }
    }
}