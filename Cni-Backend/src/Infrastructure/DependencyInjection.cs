using System.Text;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Constants;
using Backend.Infrastructure.Configurations;
using Backend.Infrastructure.Data;
using Backend.Infrastructure.Data.Interceptors;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Backend.Infrastructure.Identity;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Logging;
using Serilog.Core;
using Backend.Infrastructure.Services;
using Backend.Infrastructure.BackgroundJobs;
using Backend.Infrastructure.Huawei;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddMemoryCache();
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

        builder.Services.AddScoped<ApplicationDbContextInitializer>();

        builder.Services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        if (jwtSettings != null && !string.IsNullOrEmpty(jwtSettings.Secret))
        {
            var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { error = new { code = "UNAUTHORIZED", message = "Access token is missing or invalid" } });
                        await context.Response.Body.WriteAsync(json);
                    }
                };
            });
        }
        else
        {
            // Fallback for test environment where JwtSettings may not be configured
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            var json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { error = new { code = "UNAUTHORIZED", message = "Access token is missing or invalid" } });
                            await context.Response.Body.WriteAsync(json);
                        }
                    };
                });
        }

        builder.Services.AddAuthorizationBuilder();

        var identityBuilder = builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        // Only add EntityFrameworkStores in non-test environment
        // Tests will configure their own stores with SQLite
        if (!isTestEnvironment)
        {
            identityBuilder.AddEntityFrameworkStores<ApplicationDbContext>();
        }

        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
        builder.Services.AddScoped<IPermissionService, PermissionService>();

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

        // Energy Price Service for fetching prices from Market Operators
        builder.Services.AddHttpClient<IEnergyPriceService, EnergyPriceService>();

        // Country mapping service for area codes and timezones
        builder.Services.AddSingleton<ICountryMappingService, CountryMappingService>();

        // Background job workers for Hangfire
        builder.Services.AddScoped<EnergyPriceFetchWorker>();

        // MQTT subscriber for direct EMQX telemetry ingestion + publisher for RTU commands
        builder.Services.Configure<MqttSettings>(configuration.GetSection("MqttSettings"));
        builder.Services.AddSingleton<IMqttPublisherService, MqttPublisherService>();
        if (!isTestEnvironment)
        {
            builder.Services.AddHostedService<MqttIngestionWorker>();
        }

        // Huawei FusionSolar integration
        builder.Services.Configure<HuaweiSettings>(configuration.GetSection("HuaweiSettings"));
        var huaweiSettings = configuration.GetSection("HuaweiSettings").Get<HuaweiSettings>();
        if (huaweiSettings?.Enabled == true)
        {
            builder.Services.AddSingleton(new HuaweiCredentialEncryptor(huaweiSettings.EncryptionKey));
            builder.Services.AddHttpClient<IHuaweiApiClient, HuaweiApiClient>()
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    UseCookies = false // So Set-Cookie headers are accessible in response
                });
            builder.Services.AddScoped<HuaweiTokenManager>();
            builder.Services.AddSingleton<HuaweiDataTranslator>();
            builder.Services.AddScoped<HuaweiFusionSolarSyncWorker>();
        }
    }
}
