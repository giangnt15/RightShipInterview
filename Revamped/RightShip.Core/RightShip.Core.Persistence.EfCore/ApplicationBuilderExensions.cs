using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RightShip.Core.Persistence.EfCore;

/// <summary>
/// The extensions for the application builder.
/// </summary>
public static class ApplicationBuilderExensions
{
    /// <summary>
    /// Ensure the database is migrated.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <typeparam name="T">The type of the unit of work.</typeparam>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder EnsureMigrateDb<TDbContext, TDbContextFactory>(this IApplicationBuilder builder) where TDbContext : BaseEfCoreDbContext where TDbContextFactory : BaseEfCoreDbContextFactory<TDbContext>
    {   
        using (var scope = builder.ApplicationServices.CreateScope())
        {
            var dbContextFactory = scope.ServiceProvider.GetService<TDbContextFactory>() ?? throw new InvalidOperationException("DbContextFactory not found");
            var dbContext = dbContextFactory.CreateDbContext();
            dbContext.Database.Migrate();
        }
        return builder;
    }
}