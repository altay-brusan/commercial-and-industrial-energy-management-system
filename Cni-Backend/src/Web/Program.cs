using Backend.Application.Common.Interfaces;
using Backend.Infrastructure.BackgroundJobs;
using Backend.Infrastructure.Data;
using Backend.Infrastructure.Logging;
using Backend.Web.Hubs;
using Backend.Web.Middleware;
using Backend.Web.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;
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
// HTTP isteklerini otomatik loglar
app.UseMiddleware<MultiTenantMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Test")
{
    var lineArgs = Environment.GetCommandLineArgs();
    if (!lineArgs.Any(p => p.Contains("NSwag.AspNetCore.Launcher.dll")))
    {
        await app.InitialiseDatabaseAsync();
    }
}
else
{
    await app.InitialiseDatabaseAsync();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});

app.UseExceptionHandler(options => { });

app.Map("/", () => Results.Redirect("/api"));

app.MapHub<LiveDataHub>("/liveDataHub");
app.MapEndpoints();

// Skip Hangfire in Test environment
if (app.Environment.EnvironmentName != "Test")
{
    app.UseHangfireDashboard("/jobs-dashboard");
    RecurringJob.AddOrUpdate<GrpcFacilityWorker>(
        "grpc-facility-job",
        worker => worker.CheckFacilities(),
        "*/10 * * * *");

    // Energy price fetch jobs
    // Morning job - fetch current day prices (0:01 AM UTC)
    RecurringJob.AddOrUpdate<EnergyPriceFetchWorker>(
        "energy-price-morning-job",
        worker => worker.FetchMorningPrices(),
        "1 0 * * *");

    // Afternoon job - fetch tomorrow prices (12:01 PM UTC = 13:01 CET)
    RecurringJob.AddOrUpdate<EnergyPriceFetchWorker>(
        "energy-price-afternoon-job",
        worker => worker.FetchAfternoonPrices(),
        "1 12 * * *");
}
app.Run();

public partial class Program { }
