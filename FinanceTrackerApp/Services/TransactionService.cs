using FinanceTrackerApp.Data;
using FinanceTrackerApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceTrackerApp.Services
{
    public class TransactionService
    {
        private readonly FinanceDbContext _db;
        private readonly ILogger<TransactionService> _log;

        public TransactionService(FinanceDbContext db, ILogger<TransactionService> log)
        {
            _db = db;
            _log = log;
        }

        public async Task AddAsync(TxType type, decimal amount, string category, string? description)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive.");
            var tx = new Transaction { Type = type, Amount = amount, Category = category, Description = description };
            _db.Add(tx);
            await _db.SaveChangesAsync();
            _log.LogInformation("Added {Type} {Amount} in {Category}", type, amount, category);
        }

        public Task<List<Transaction>> ListAllAsync() =>
            _db.Transactions.OrderByDescending(t => t.CreatedAt).ToListAsync();

        public Task<List<Transaction>> ListMonthlyAsync(int year, int month) =>
            _db.Transactions.Where(t => t.CreatedAt.Year == year && t.CreatedAt.Month == month)
                            .OrderByDescending(t => t.CreatedAt).ToListAsync();

        public async Task<(decimal income, decimal expense, decimal net)> MonthlySummaryAsync(int year, int month)
        {
            var q = _db.Transactions.Where(t => t.CreatedAt.Year == year && t.CreatedAt.Month == month);
            var income = await q.Where(t => t.Type == TxType.Income).SumAsync(t => (decimal?)t.Amount) ?? 0m;
            var expense = await q.Where(t => t.Type == TxType.Expense).SumAsync(t => (decimal?)t.Amount) ?? 0m;
            return (income, expense, income - expense);
        }

        public async Task<bool> UpdateAsync(int id, decimal? amount, string? description)
        {
            var t = await _db.Transactions.FindAsync(id);
            if (t == null) return false;
            if (amount.HasValue && amount.Value > 0) t.Amount = amount.Value;
            if (description != null) t.Description = description;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var t = await _db.Transactions.FindAsync(id);
            if (t == null) return false;
            _db.Remove(t);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<int> SeedDemoAsync()
        {
            var now = DateTime.UtcNow;
            var seed = new List<Transaction>{
                new(){ Type=TxType.Income, Amount=2200, Category="Salary", Description="Monthly salary", CreatedAt=now.AddDays(-20)},
                new(){ Type=TxType.Expense, Amount=600, Category="Rent", Description="Rent", CreatedAt=now.AddDays(-18)},
                new(){ Type=TxType.Expense, Amount=120, Category="Groceries", Description="Groceries", CreatedAt=now.AddDays(-15)},
                new(){ Type=TxType.Income, Amount=150, Category="Side", Description="Freelance", CreatedAt=now.AddDays(-12)},
                new(){ Type=TxType.Expense, Amount=60, Category="Transport", Description="Transport", CreatedAt=now.AddDays(-10)},
            };
            await _db.AddRangeAsync(seed);
            return await _db.SaveChangesAsync();
        }

        public async Task<int> ExportCsvAsync(string path, int? year = null, int? month = null)
        {
            IEnumerable<Transaction> list = (year == null || month == null)
                ? await ListAllAsync()
                : await ListMonthlyAsync(year.Value, month.Value);

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".");
            using var sw = new StreamWriter(path);
            await sw.WriteLineAsync("Id,CreatedAt,Type,Category,Description,Amount");
            foreach (var t in list)
                await sw.WriteLineAsync($"{t.Id},{t.CreatedAt:o},{t.Type},{t.Category},{t.Description},{t.Amount}");
            return list.Count();
        }
    }
}
