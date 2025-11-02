using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Services.Background;

/// <summary>
/// Service care curăță periodic connection pool-ul pentru a preveni conexiuni stale
/// </summary>
public class DatabaseConnectionCleanupService : BackgroundService
{
    private readonly ILogger<DatabaseConnectionCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);

    public DatabaseConnectionCleanupService(ILogger<DatabaseConnectionCleanupService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Database Connection Cleanup Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    break;

                _logger.LogDebug("Cleaning up database connection pools...");

                // Clear ALL connection pools
                SqlConnection.ClearAllPools();

                _logger.LogInformation("Database connection pools cleared successfully");
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database connection pool cleanup");
            }
        }

        _logger.LogInformation("Database Connection Cleanup Service stopped");
    }
}
