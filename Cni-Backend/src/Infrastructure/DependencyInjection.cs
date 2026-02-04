using Backend.Application.Common.Interfaces;
using Backend.Domain.Constants;
using Backend.Infrastructure.Configurations;
using Backend.Infrastructure.Data;
using Backend.Infrastructure.Data.Interceptors;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Backend.Infrastructure.Identity;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Logging;
using Serilog.Core;
using Backend.Infrastructure.Services;
using Backend.Infrastructure.Mappings;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        // Skip DbContext and Identity EntityFrameworkStores registration in Test environment
        // Tests will register their own DbContext with SQLite
        var environment = builder.Environment.EnvironmentName;
        var isTestEnvironment = string.Equals(environment, "Test", StringComparison.OrdinalIgnoreCase);

        if (!isTestEnvironment)
        {
            var connectionString = builder.Configuration.GetConnectionString("BackendDb");
            Guard.Against.Null(connectionString, message: "Connection string 'BackendDb' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseNpgsql(connectionString);
            });
            builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        }

        builder.Services.AddAutoMapper(typeof(GrpcMappingProfile).Assembly);

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        builder.Services.AddAuthorizationBuilder();

        var identityBuilder = builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>();

        // Only add EntityFrameworkStores in non-test environment
        // Tests will configure their own stores with SQLite
        if (!isTestEnvironment)
        {
            identityBuilder.AddEntityFrameworkStores<ApplicationDbContext>();
        }

        identityBuilder.AddApiEndpoints();

        builder.Services.AddSingleton(TimeProvider.System);
        // Startup.cs veya Program.cs içinde
        builder.Services.AddTransient<IIdentityService, IdentityService>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));

        builder.Services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        builder.Services.AddSingleton<IEmailService, EmailService>();
        builder.Services.AddSingleton<IVirtualClockService, VirtualClockService>();
        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityEmailAdapter<ApplicationUser>>();
        builder.Services.AddSingleton<ILogEventEnricher, UserEnricher>();
        builder.Services.AddSingleton<IExternalDataService, ExternalDataService>();
        // gRPC disabled
        // builder.Services.AddSingleton<GrpcStreamManager>();

        // TSO Service for fetching energy prices from external providers
        builder.Services.AddHttpClient<ITsoService, TsoService>();

        // Country mapping service for area codes and timezones
        builder.Services.AddSingleton<ICountryMappingService, CountryMappingService>();
    }
}
