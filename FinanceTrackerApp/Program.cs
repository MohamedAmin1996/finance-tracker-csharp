using DotNetEnv;
using FinanceTrackerApp.Controller;
using FinanceTrackerApp.Data;
using FinanceTrackerApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using System.CommandLine;


public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Load .env file (ignore errors if not present)
        try { Env.TraversePath().Load(); } catch { /* ignore */ }

        // Create host builder
        var builder = Host.CreateApplicationBuilder(args);

        // Configure Serilog for logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning) // SQL commands
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Migrations", Serilog.Events.LogEventLevel.Warning)   // migration logs
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();


        // Replace default logging with Serilog
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);


        // Load environment variables
        builder.Configuration.AddEnvironmentVariables();

        // Read DB connection info from env
        var hostEnv = builder.Configuration["DB_HOST"] ?? "localhost";
        var portEnv = builder.Configuration["DB_PORT"] ?? "5432";
        var dbName = builder.Configuration["DB_NAME"] ?? "finance_db";
        var dbUser = builder.Configuration["DB_USER"] ?? "finance_user";
        var dbPass = builder.Configuration["DB_PASSWORD"] ?? "supersecret";

        var connStr = $"Host={hostEnv};Port={portEnv};Database={dbName};Username={dbUser};Password={dbPass}";

        // Add DbContext with reduced logging (only warnings/errors)
        builder.Services.AddDbContext<FinanceDbContext>(options =>
        options.UseNpgsql(connStr)
           .LogTo(_ => { }, LogLevel.Warning)
        );

        // Add application services
        builder.Services.AddScoped<TransactionService>();

        // Build the host
        var app = builder.Build();

        // Auto-apply migrations at startup
        using (var scope = app.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();

            ctx.Database.Migrate();
        }

        // Build CLI and run
        var root = CLIController.Build(app.Services);
        return await root.InvokeAsync(args);
    }
}
