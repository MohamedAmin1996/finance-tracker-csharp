using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DotNetEnv;


namespace FinanceTrackerApp.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FinanceDbContext>
    {
        public FinanceDbContext CreateDbContext(string[] args)
        {
            try { Env.TraversePath().Load(); } catch { /* ignore */ }

            var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
            var db = Environment.GetEnvironmentVariable("DB_NAME") ?? "finance_db";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? "finance_user";
            var pwd = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "supersecret";

            var conn = $"Host={host};Port={port};Database={db};Username={user};Password={pwd}";
            var options = new DbContextOptionsBuilder<FinanceDbContext>().UseNpgsql(conn).Options;
            return new FinanceDbContext(options);
        }
    }
}
