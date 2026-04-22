using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Authentication.Infrastructure.Persistence;

public class AuthenticationDbContextFactory : IDesignTimeDbContextFactory<AuthenticationDbContext>
{
    public AuthenticationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthenticationDbContext>();

        // Use a default connection string for migrations
        var connectionString = "Server=localhost;Database=golf-app-auth;Integrated Security=true;TrustServerCertificate=true;";

        optionsBuilder.UseSqlServer(connectionString);

        return new AuthenticationDbContext(optionsBuilder.Options);
    }
}
