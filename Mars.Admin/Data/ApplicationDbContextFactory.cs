using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mars.Admin.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Get environment from environment variable or default to Development
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // Set connection string based on environment (same as Program.cs)
            string connectionString = environmentName.ToLowerInvariant() switch
            {
                "development" => "Server=Dev;Database=DBDev;User Id=User;Password=****;MultipleActiveResultSets=true;TrustServerCertificate=True",
                "staging" => "Server=Stg;Database=DBStaging;User Id=User;Password=****;MultipleActiveResultSets=true;TrustServerCertificate=True",
                "production" => "Server=Prd;Database=DBProduction;User Id=User;Password=****;MultipleActiveResultSets=true;TrustServerCertificate=True",
                _ => throw new InvalidOperationException($"Unknown environment: {environmentName}")
            };

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
