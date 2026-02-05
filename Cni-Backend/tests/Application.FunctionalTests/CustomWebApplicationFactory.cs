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

            // Mock multi-tenant service for tests (super admin, not tenant scoped - can see all data)
            services
                .RemoveAll<IMultiTenantService>()
                .AddScoped<IMultiTenantService>(provider =>
                {
                    var mock = new Mock<IMultiTenantService>();
                    mock.Setup(s => s.GetAuthenticatedTenantId()).Returns((int?)null);
                    mock.Setup(s => s.GetSelectedTenantId()).Returns((int?)null);
                    mock.Setup(s => s.GetEffectiveTenantId()).Returns((int?)null);
                    mock.Setup(s => s.IsSuperAdmin()).Returns(true);
                    mock.Setup(s => s.IsTenantScoped()).Returns(false);
                    mock.Setup(s => s.IsImpersonating()).Returns(false);
                    // Return the requested tenant ID when creating entities (super admin can create for any tenant)
                    mock.Setup(s => s.GetTenantIdForCreation(It.IsAny<int?>()))
                        .Returns((int? requestedTenantId) => requestedTenantId ?? 1);
                    return mock.Object;
                });

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
