using Microsoft.EntityFrameworkCore;

namespace Gateway.Entity
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<BookkeepingRecord> BookkeepingRecords { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Withdraw> Withdraws { get; set; }
        public DbSet<MerchantOrderId> MerchantOrderIds { get; set; }
    }

    public static class ContextSeed
    {
        public static async Task SeedSampleDataAsync(Context context)
        {
            if (!context.Users.Any())
            {
                context.Users.AddRange(new[]
                {
                    new User() { Username = "admin", Password = "admin", Role = "Admin", Percent = 16 },
                    new User() { Username = "user", Password = "user", Role = "User", Percent = 16 }
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
