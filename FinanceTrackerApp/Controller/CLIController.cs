using FinanceTrackerApp.Models;
using FinanceTrackerApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace FinanceTrackerApp.Controller
{
    public static class CLIController
    {
        public static RootCommand Build(IServiceProvider sp)
        {
            var root = new RootCommand("Personal Finance Tracker (C#)");

            // ----- Arguments & Options for commands -----
            var amountArg = new Argument<decimal>("amount");
            var categoryArg = new Argument<string>("category");
            var descriptionOpt = new Option<string?>("--description", () => null);

            var yearOpt = new Option<int>("--year", () => DateTime.UtcNow.Year);
            var monthOpt = new Option<int>("--month", () => DateTime.UtcNow.Month);

            var idArg = new Argument<int>("id");
            var updateAmountOpt = new Option<decimal?>("--amount");
            var updateDescriptionOpt = new Option<string?>("--description");

            var pathOpt = new Option<string>("--path", () => "export.csv");
            var exportYearOpt = new Option<int?>("--year");
            var exportMonthOpt = new Option<int?>("--month");

            // ----- add-income -----
            var addIncome = new Command("add-income", "Add income <amount> <category> [--description]")
            {
                amountArg,
                categoryArg,
                descriptionOpt
            };
            addIncome.SetHandler(async (decimal amount, string category, string? description) =>
            {
                using var scope = sp.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<TransactionService>();
                await svc.AddAsync(TxType.Income, amount, category, description);
                Console.WriteLine("Income added.");
            }, amountArg, categoryArg, descriptionOpt);

            // ----- add-expense -----
            var addExpense = new Command("add-expense", "Add expense <amount> <category> [--description]")
            {
                amountArg,
                categoryArg,
                descriptionOpt
            };
            addExpense.SetHandler(async (decimal amount, string category, string? description) =>
            {
                using var scope = sp.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<TransactionService>();
                await svc.AddAsync(TxType.Expense, amount, category, description);
                Console.WriteLine("Expense added.");
            }, amountArg, categoryArg, descriptionOpt);

            // ----- list-all -----
            var listAll = new Command("list-all", "List all transactions");
            listAll.SetHandler(async () =>
            {
                using var scope = sp.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<TransactionService>();
                var items = await svc.ListAllAsync();
                foreach (var t in items)
                    Console.WriteLine($"{t.Id,3} {t.CreatedAt:yyyy-MM-dd} {t.Type,-7} {t.Amount,10}  {t.Category}  {t.Description}");
            });

            // ----- list-monthly -----
            var listMonthly = new Command("list-monthly", "List transactions in a month")
            {
                yearOpt,
                monthOpt
            };
            listMonthly.SetHandler(async (int year, int month) =>
            {
                using var scope = sp.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<TransactionService>();
                var items = await svc.ListMonthlyAsync(year, month);
                foreach (var t in items)
                    Console.WriteLine($"{t.Id,3} {t.CreatedAt:yyyy-MM-dd} {t.Type,-7} {t.Amount,10}  {t.Category}  {t.Description}");
            }, yearOpt, monthOpt);

            // ----- monthly-summary -----
            var monthly = new Command("monthly-summary", "Show summary for month")
            {
                yearOpt,
                monthOpt
            };
            monthly.SetHandler(async (int year, int month) =>
            {
                using var scope = sp.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<TransactionService>();
                var (inc, exp, net) = await svc.MonthlySummaryAsync(year, month);
                Console.WriteLine($"Income: {inc}\nExpense: {exp}\nNet: {net}");
            }, yearOpt, monthOpt);

            // ----- update-transaction -----
            var update = new Command("update-transaction", "Update transaction by id")
            {
                idArg,
                updateAmountOpt,
                updateDescriptionOpt
            };
            update.SetHandler(async (int id, decimal? amount, string? description) =>
            {
                using var scope = sp.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<TransactionService>();
                var ok = await svc.UpdateAsync(id, amount, description);
                Console.WriteLine(ok ? "Updated." : "Not found.");
            }, idArg, updateAmountOpt, updateDescriptionOpt);

            // ----- delete-transaction -----
            var del = new Command("delete-transaction", "Delete transaction by id")
            {
                idArg
            };
            del.SetHandler(async (int id) =>
            {
                using var scope = sp.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<TransactionService>();
                var ok = await svc.DeleteAsync(id);
                Console.WriteLine(ok ? "Deleted." : "Not found.");
            }, idArg);

            // ----- export-csv -----
            var export = new Command("export-csv", "Export transactions to CSV")
            {
                pathOpt,
                exportYearOpt,
                exportMonthOpt
            };
            export.SetHandler(async (string path, int? year, int? month) =>
            {
                using var scope = sp.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<TransactionService>();
                var n = await svc.ExportCsvAsync(path, year, month);
                Console.WriteLine($"Exported {n} rows to {path}");
            }, pathOpt, exportYearOpt, exportMonthOpt);

            // ----- seed-demo-data -----
            var seed = new Command("seed-demo-data", "Seed demo transactions for testing");
            seed.SetHandler(async () =>
            {
                using var scope = sp.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<TransactionService>();
                var n = await svc.SeedDemoAsync();
                Console.WriteLine($"Seeded {n} transactions.");
            });

            // ----- migrate-db -----
            var migrate = new Command("migrate-db", "Apply EF Core migrations");
            migrate.SetHandler(() =>
            {
                using var scope = sp.CreateScope();
                var ctx = scope.ServiceProvider.GetRequiredService<FinanceTrackerApp.Data.FinanceDbContext>();
                ctx.Database.Migrate();
                Console.WriteLine("Migrations applied.");
            });

            // ----- help -----
            var help = new Command("help", "Show available commands and descriptions");
            help.SetHandler(() =>
            {
                Console.WriteLine("Available commands:");
                Console.WriteLine("  add-income <amount> <category> [--description]     Add income");
                Console.WriteLine("  add-expense <amount> <category> [--description]    Add expense");
                Console.WriteLine("  list-all                                           List all transactions");
                Console.WriteLine("  list-monthly --year <YYYY> --month <MM>            List transactions for a month");
                Console.WriteLine("  monthly-summary --year <YYYY> --month <MM>         Show summary for a month");
                Console.WriteLine("  update-transaction <id> [--amount] [--description] Update transaction");
                Console.WriteLine("  delete-transaction <id>                            Delete transaction");
                Console.WriteLine("  export-csv --path <path> [--year] [--month]        Export to CSV");
                Console.WriteLine("  seed-demo-data                                     Seed demo data");
                Console.WriteLine("  migrate-db                                         Apply pending migrations");
                Console.WriteLine("  help                                               Show this help");
            });

            // ----- Add commands to root -----
            root.AddCommand(addIncome);
            root.AddCommand(addExpense);
            root.AddCommand(listAll);
            root.AddCommand(listMonthly);
            root.AddCommand(monthly);
            root.AddCommand(update);
            root.AddCommand(del);
            root.AddCommand(export);
            root.AddCommand(seed);
            root.AddCommand(migrate);
            root.AddCommand(help);

            return root;
        }
    }
}
