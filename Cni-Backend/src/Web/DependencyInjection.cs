using Azure.Identity;
using Backend.Application.Common.Interfaces;
using Backend.Infrastructure.Data;
using Backend.Web.Hubs.Implementations;
using Backend.Web.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Mvc;

using NSwag;
using NSwag.Generation.Processors.Security;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddScoped<IUser, CurrentUser>();

        //builder.Services.AddHttpContextAccessor();
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        builder.Services.AddExceptionHandler<CustomExceptionHandler>();

        // Skip Hangfire in Test environment - it requires PostgreSQL
        var isTestEnvironment = string.Equals(builder.Environment.EnvironmentName, "Test", StringComparison.OrdinalIgnoreCase);
        if (!isTestEnvironment)
        {
            var connectionString = builder.Configuration.GetConnectionString("BackendDb");
            builder.Services.AddHangfire(config =>
            {
                config.UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(connectionString);
                }, new PostgreSqlStorageOptions
                {
                    SchemaName = "hangfire"
                });
            });
            builder.Services.AddHangfireServer();
        }
        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSingleton<IRealTimeNotifier, SignalRRealTimeNotifier>();

        builder.Services.AddOpenApiDocument((configure, sp) =>
        {
            configure.Title = "DERMS API";

            // Add JWT
            configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Type into the textbox: Bearer {your JWT token}."
            });

            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });
    }

    public static void AddKeyVaultIfConfigured(this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
    }
}
