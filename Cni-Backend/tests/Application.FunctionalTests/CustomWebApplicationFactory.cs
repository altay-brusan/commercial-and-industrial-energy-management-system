using System.Data.Common;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backend.Application.FunctionalTests;

using static Testing;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly DbConnection _connection;

    public CustomWebApplicationFactory(DbConnection connection)
    {
        _connection = connection;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Add in-memory configuration with required connection string
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:BackendDb"] = "DataSource=:memory:"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Mock user service
            services
                .RemoveAll<IUser>()
                .AddTransient(provider => Mock.Of<IUser>(s => s.Id == GetUserId()));

            // Mock multi-tenant service for tests (always return tenant 1, not super admin)
            services
                .RemoveAll<IMultiTenantService>()
                .AddScoped<IMultiTenantService>(provider =>
                    Mock.Of<IMultiTenantService>(s =>
                        s.GetAuthenticatedTenantId() == 1 &&
                        s.GetSelectedTenantId() == null &&
                        s.GetEffectiveTenantId() == 1 &&
                        s.IsSuperAdmin() == false &&
                        s.IsTenantScoped() == true &&
                        s.IsImpersonating() == false));

            // Remove existing ApplicationDbContext and all related services
            var descriptorToRemove = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptorToRemove != null)
                services.Remove(descriptorToRemove);

            // Remove all DbContextOptions registrations
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

            // Remove the existing ApplicationDbContext registration
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ApplicationDbContext));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            // Remove IApplicationDbContext registration
            services.RemoveAll<IApplicationDbContext>();

            // Add ApplicationDbContext with SQLite in-memory
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseSqlite(_connection);
            });

            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            // Add Identity EntityFrameworkStores with SQLite DbContext
            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
        });

        builder.UseEnvironment("Test");
    }

    public void EnsureDatabaseCreated()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
    }
}
