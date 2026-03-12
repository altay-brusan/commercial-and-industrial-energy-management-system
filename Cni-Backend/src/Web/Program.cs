using Backend.Application.Common.Interfaces;
using Backend.Infrastructure.BackgroundJobs;
using Backend.Infrastructure.Configurations;
using Backend.Infrastructure.Data;
using Backend.Infrastructure.Logging;
using Backend.Web.Hubs;
using Backend.Web.Infrastructure;
using Backend.Web.Middleware;
using Backend.Web.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddKeyVaultIfConfigured();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<MultiTenantMiddleware>();
builder.Services.AddScoped<IMultiTenantService, MultiTenantService>();
builder.AddApplicationServices();
builder.AddInfrastructureServices(builder.Configuration);
builder.AddWebServices();

builder.Host.UseSerilog((context, services, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
          .Enrich.FromLogContext()
          .Enrich.With(new UserEnricher(services.GetRequiredService<IHttpContextAccessor>()));
});
builder.Services.AddSignalR();
var app = builder.Build();
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    dbContext.Database.Migrate();
//}
app.UseSerilogRequestLogging();

// Prometheus HTTP request duration middleware
app.Use(async (context, next) =>
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    await next();
    stopwatch.Stop();

    // Skip /metrics and /health to avoid self-instrumentation noise
    var path = context.Request.Path.Value ?? "";
    if (!path.StartsWith("/metrics", StringComparison.OrdinalIgnoreCase)
        && !path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
    {
        var endpoint = path.Length > 0 ? path : "/";
        AppMetrics.HttpRequestDuration
            .WithLabels(context.Request.Method, endpoint, context.Response.StatusCode.ToString())
            .Observe(stopwatch.Elapsed.TotalSeconds);
    }
});

app.UseMiddleware<MultiTenantMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Test")
{
    var lineArgs = Environment.GetCommandLineArgs();
    if (!lineArgs.Any(p => p.Contains("NSwag.AspNetCore.Launcher.dll")))
    {
        await app.InitializeDatabaseAsync();
    }
}
else
{
    await app.InitializeDatabaseAsync();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseStaticFiles();
app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});

app.UseExceptionHandler(options => { });

app.UseRateLimiter();

app.Map("/", () => Results.Redirect("/api"));

app.MapHub<LiveDataHub>("/liveDataHub");
app.MapEndpoints();

// Skip Hangfire in Test environment
if (app.Environment.EnvironmentName != "Test")
{
    app.UseHangfireDashboard("/hangfire/dashboard");

    // Hangfire retry: 2 min, 5 min, 10 min delays between attempts
    GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
    {
        Attempts = 3,
        DelaysInSeconds = [120, 300, 600]
    });

    // Energy price fetch jobs — both fetch today's prices, dedup ensures no duplicates
    // Early morning run (00:01 UTC)
    RecurringJob.AddOrUpdate<EnergyPriceFetchWorker>(
        "energy-price-early-morning",
        worker => worker.FetchDailyPrices(),
        "1 0 * * *");

    // Afternoon run (13:01 UTC / ~14:01 CET) — catches any updates after ENTSO-E publishes
    RecurringJob.AddOrUpdate<EnergyPriceFetchWorker>(
        "energy-price-afternoon",
        worker => worker.FetchDailyPrices(),
        "1 13 * * *");

    // Huawei FusionSolar sync — every 5 minutes (matches API rate limits)
    var huaweiSettings = app.Configuration.GetSection("HuaweiSettings").Get<HuaweiSettings>();
    if (huaweiSettings?.Enabled == true)
    {
        var intervalMinutes = Math.Max(1, huaweiSettings.PollIntervalSeconds / 60);
        RecurringJob.AddOrUpdate<HuaweiFusionSolarSyncWorker>(
            "huawei-fusionsolar-sync",
            worker => worker.SyncAllAccounts(CancellationToken.None),
            $"*/{intervalMinutes} * * * *");
    }
}
app.MapMetrics(); // Expose /metrics endpoint for Prometheus scraping

app.Run();

public partial class Program { }
