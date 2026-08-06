using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TmsApi.Infrastructure.Persistence;

/// <summary>
/// Supplies EF Core with the same database configuration used by the API when
/// migrations are created or applied outside the running application.
/// </summary>
public sealed class TmsDbContextFactory : IDesignTimeDbContextFactory<TmsDbContext>
{
    public TmsDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(FindApiConfigurationDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("TmsDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'TmsDatabase' was not found.");

        var options = new DbContextOptionsBuilder<TmsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new TmsDbContext(options);
    }

    private static string FindApiConfigurationDirectory()
    {
        for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
             directory is not null;
             directory = directory.Parent)
        {
            var apiDirectory = Path.Combine(directory.FullName, "TmsApi.Api");

            if (File.Exists(Path.Combine(apiDirectory, "appsettings.json")))
            {
                return apiDirectory;
            }
        }

        throw new DirectoryNotFoundException(
            "Could not find TmsApi.Api/appsettings.json for EF Core design-time configuration.");
    }
}
