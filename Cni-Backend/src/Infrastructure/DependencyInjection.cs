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
        var connectionString = builder.Configuration.GetConnectionString("BackendDb");
        Guard.Against.Null(connectionString, message: "Connection string 'BackendDb' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });
        builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(GrpcMappingProfile).Assembly));
        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

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
        builder.Services.AddSingleton<GrpcStreamManager>();
    }
}
