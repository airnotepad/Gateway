using Gateway.Entity;
using Gateway.Helper;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Services
{
    public class BalanceService
    {
        protected readonly ILogger<BalanceService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private RWLock locker = new RWLock();

        public BalanceService(ILogger<BalanceService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public void UpdateBalance(Func<User, bool> predicate, double amount, BookkeepingRecordForm form, BookkeepingRecordType type, string reason)
        {
            using (locker.WriteLock())
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<Context>();

                    var user = _context.Users.AsNoTracking().FirstOrDefault(predicate);
                    if (user == null) throw new Exception($"UpdateBalance: user is null");

                    if (_context.BookkeepingRecords.Any(x => x.Reason == reason)) throw new Exception($"UpdateBalance: bookkeeping record with such reason already exist (reason: {reason})");

                    var lastRecord = _context.BookkeepingRecords.AsNoTracking().LastOrDefault(r => r.User_id == user.Id);
                    var lastBalance = lastRecord != null ? lastRecord.NewBalance : 0;
                    var newBalance = form == BookkeepingRecordForm.Incoming ? lastBalance + amount : lastBalance - amount;

                    var record = new BookkeepingRecord()
                    {
                        Date = Date.Now,
                        Amount = amount,
                        Form = form,
                        Type = type,
                        Reason = reason,
                        Username = user.Username,
                        User_id = user.Id,
                        PrevBalance = lastBalance,
                        NewBalance = newBalance
                    };

                    _context.BookkeepingRecords.Add(record);
                    _context.SaveChanges();
                }
            }
        }

        public void UpdateBalance(Func<User, bool> predicate, Transaction transaction)
        {
            using (locker.WriteLock())
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<Context>();
                    var _settingsService = scope.ServiceProvider.GetRequiredService<SettingService>();
                    var _settings = _settingsService.GetSettings();

                    var user = _context.Users.AsNoTracking().FirstOrDefault(predicate);
                    if (user == null) throw new Exception($"UpdateBalance: user is null");

                    var reason = FormatReason(transaction);

                    if (_context.BookkeepingRecords.AsNoTracking().Any(x => x.Reason == reason)) throw new Exception($"UpdateBalance: bookkeeping record with such reason already exist (reason: {reason})");

                    var merchantExpense = _settings.MerchantsExpenses?.FirstOrDefault(x => x.MerchantName == transaction.Merchant_name);

                    var userPercent = 100 - user.Percent;
                    var expensePercent = merchantExpense != null ? merchantExpense.ExpensePercent : 0;
                    var adminPercent = 100 - userPercent - expensePercent;

                    var usersAmount = transaction.Amount * userPercent / 100;
                    var expenseAmount = transaction.Amount * expensePercent / 100;
                    var adminAmount = transaction.Amount * adminPercent / 100;

                    var lastRecord = _context.BookkeepingRecords.AsNoTracking().LastOrDefault(r => r.User_id == user.Id);
                    var lastBalance = lastRecord != null ? lastRecord.NewBalance : 0;
                    var newBalance = lastBalance + usersAmount;

                    var record = new BookkeepingRecord()
                    {
                        Date = Date.Now,
                        Amount = transaction.Amount,
                        UserAmount = usersAmount,
                        ExpenseAmount = expenseAmount,
                        AdminAmount = adminAmount,
                        Form = BookkeepingRecordForm.Incoming,
                        Type = BookkeepingRecordType.Transaction,
                        Reason = reason,
                        Username = user.Username,
                        User_id = user.Id,
                        PrevBalance = lastBalance,
                        NewBalance = newBalance
                    };

                    _context.BookkeepingRecords.Add(record);
                    _context.SaveChanges();
                }
            }
        }

        public void CreateWithdraw(Func<User, bool> predicate, double amount, string description)
        {
            using (locker.WriteLock())
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<Context>();

                    var user = _context.Users.AsNoTracking().FirstOrDefault(predicate);
                    if (user == null) throw new Exception($"UpdateBalance: user is null");

                    var lastRecord = _context.BookkeepingRecords.AsNoTracking().LastOrDefault(r => r.User_id == user.Id);
                    var lastBalance = lastRecord != null ? lastRecord.NewBalance : 0;

                    if (lastBalance < amount) throw new Exception($"UpdateBalance: lacks balance");
                    var newBalance = lastBalance - amount;

                    var withdraw = new Withdraw()
                    {
                        Date = Date.Now,
                        User_id = user.Id,
                        Username = user.Username,
                        Amount = amount,
                        Status = WithdrawStatus.AwaitingPay,
                        Description = description
                    };
                    _context.Withdraws.Add(withdraw);
                    _context.SaveChanges();

                    var record = new BookkeepingRecord()
                    {
                        Date = Date.Now,
                        Amount = amount,
                        Form = BookkeepingRecordForm.Outgoing,
                        Type = BookkeepingRecordType.Withdraw,
                        Reason = FormatReason(withdraw),
                        Username = user.Username,
                        User_id = user.Id,
                        PrevBalance = lastBalance,
                        NewBalance = newBalance
                    };

                    _context.BookkeepingRecords.Add(record);
                    _context.SaveChanges();
                }
            }
        }

        public void CancelWithdraw(Func<User, bool> predicate, int id)
        {
            using (locker.WriteLock())
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<Context>();

                    var user = _context.Users.AsNoTracking().FirstOrDefault(predicate);
                    if (user == null) throw new Exception($"CancelWithdraw: user is null");

                    var lastRecord = _context.BookkeepingRecords.AsNoTracking().LastOrDefault(r => r.User_id == user.Id);
                    var lastBalance = lastRecord != null ? lastRecord.NewBalance : 0;

                    var withdraw = _context.Withdraws.FirstOrDefault(r => r.Id == id);
                    if (withdraw == null) throw new Exception($"CancelWithdraw: withdraw is null");

                    if (_context.BookkeepingRecords.Any(x => x.Reason == FormatReasonCancel(withdraw))) throw new Exception($"CancelWithdraw: bookkeeping record with such reason already exist (reason: {FormatReasonCancel(withdraw)})");

                    withdraw.Status = WithdrawStatus.Canceled;

                    var newBalance = lastBalance + withdraw.Amount;

                    var record = new BookkeepingRecord()
                    {
                        Date = Date.Now,
                        Amount = withdraw.Amount,
                        Form = BookkeepingRecordForm.Incoming,
                        Type = BookkeepingRecordType.Withdraw,
                        Reason = FormatReasonCancel(withdraw),
                        Username = user.Username,
                        User_id = user.Id,
                        PrevBalance = lastBalance,
                        NewBalance = newBalance
                    };

                    _context.BookkeepingRecords.Add(record);
                    _context.SaveChanges();
                }
            }
        }

        public double GetBalance(Func<User, bool> predicate)
        {
            using (locker.ReadLock())
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<Context>();
                    var user = _context.Users.AsNoTracking().FirstOrDefault(predicate);
                    if (user == null) throw new Exception($"GetBalance: user is null");

                    var lastRecord = _context.BookkeepingRecords.AsNoTracking().LastOrDefault(r => r.User_id == user.Id);
                    var lastBalance = lastRecord != null ? lastRecord.NewBalance : 0;

                    return lastBalance;
                }
            }
        }

        public string FormatReason(Transaction transaction) => $"Trx #{transaction.Id}";
        public string FormatReason(Withdraw withdraw) => $"Withdraw {withdraw.Id}";
        public string FormatReasonCancel(Withdraw withdraw) => $"Cancel withdraw {withdraw.Id}";
    }
}
