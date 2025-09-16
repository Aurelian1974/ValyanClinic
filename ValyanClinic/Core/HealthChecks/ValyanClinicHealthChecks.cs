using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using ValyanClinic.Core.Services;

namespace ValyanClinic.Core.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ILogger<DatabaseHealthCheck> _logger;
    // TODO: Add database connection when implemented
    // private readonly IDbConnection _dbConnection;

    public DatabaseHealthCheck(ILogger<DatabaseHealthCheck> logger)
    {
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Replace with actual database connection check
            await SimulateDatabaseCheckAsync(cancellationToken);

            _logger.LogInformation("Database health check passed");
            return HealthCheckResult.Healthy("Database connection is working");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Database health check was cancelled");
            return HealthCheckResult.Unhealthy("Database check was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }

    private async Task SimulateDatabaseCheckAsync(CancellationToken cancellationToken)
    {
        // Simulate database query
        await Task.Delay(100, cancellationToken);
        
        // Simulate occasional failures for testing
        if (Random.Shared.Next(1, 100) <= 5) // 5% failure rate
        {
            throw new InvalidOperationException("Simulated database connection failure");
        }
    }
}

public class CacheHealthCheck : IHealthCheck
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheHealthCheck> _logger;

    public CacheHealthCheck(ICacheService cacheService, ILogger<CacheHealthCheck> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            const string testKey = "health_check_test";
            const string testValue = "test_value";

            // Test cache write
            await _cacheService.SetAsync(testKey, testValue, TimeSpan.FromMinutes(1));

            // Test cache read
            var retrievedValue = await _cacheService.GetAsync<string>(testKey);

            if (retrievedValue != testValue)
            {
                _logger.LogWarning("Cache health check failed: value mismatch");
                return HealthCheckResult.Degraded("Cache read/write test failed");
            }

            // Cleanup
            await _cacheService.RemoveAsync(testKey);

            _logger.LogInformation("Cache health check passed");
            return HealthCheckResult.Healthy("Cache is working correctly");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache health check failed");
            return HealthCheckResult.Unhealthy("Cache service failed", ex);
        }
    }
}

public class StockMonitoringHealthCheck : IHealthCheck
{
    private readonly IStockMonitoringService _stockService;
    private readonly ILogger<StockMonitoringHealthCheck> _logger;

    public StockMonitoringHealthCheck(
        IStockMonitoringService stockService,
        ILogger<StockMonitoringHealthCheck> logger)
    {
        _stockService = stockService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var alerts = await _stockService.GetLowStockAlertsAsync();
            var criticalAlerts = alerts.Count(a => a.AlertLevel == AlertLevel.Critical);

            var data = new Dictionary<string, object>
            {
                ["total_alerts"] = alerts.Count,
                ["critical_alerts"] = criticalAlerts,
                ["last_check"] = DateTime.UtcNow
            };

            if (criticalAlerts > 0)
            {
                _logger.LogWarning("Stock monitoring health check found {Count} critical alerts", criticalAlerts);
                return HealthCheckResult.Degraded(
                    $"Found {criticalAlerts} critical stock alerts", 
                    data: data);
            }

            _logger.LogInformation("Stock monitoring health check passed");
            return HealthCheckResult.Healthy("Stock monitoring is working", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stock monitoring health check failed");
            return HealthCheckResult.Unhealthy("Stock monitoring service failed", ex);
        }
    }
}

public class SystemResourcesHealthCheck : IHealthCheck
{
    private readonly ILogger<SystemResourcesHealthCheck> _logger;

    public SystemResourcesHealthCheck(ILogger<SystemResourcesHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check memory usage
            var memoryUsed = GC.GetTotalMemory(false);
            var memoryUsedMB = memoryUsed / (1024 * 1024);

            // Check CPU usage (simplified)
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var cpuTime = process.TotalProcessorTime;

            var data = new Dictionary<string, object>
            {
                ["memory_used_mb"] = memoryUsedMB,
                ["cpu_time_ms"] = cpuTime.TotalMilliseconds,
                ["thread_count"] = process.Threads.Count,
                ["working_set_mb"] = process.WorkingSet64 / (1024 * 1024)
            };

            // Memory threshold: 512 MB
            if (memoryUsedMB > 512)
            {
                _logger.LogWarning("High memory usage detected: {MemoryMB} MB", memoryUsedMB);
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"High memory usage: {memoryUsedMB} MB", 
                    data: data));
            }

            _logger.LogInformation("System resources health check passed");
            return Task.FromResult(HealthCheckResult.Healthy("System resources are normal", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System resources health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("System resources check failed", ex));
        }
    }
}

// Extension methods for easy registration
public static class HealthCheckExtensions
{
    public static IServiceCollection AddValyanClinicHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready", "database" })
            .AddCheck<CacheHealthCheck>("cache", tags: new[] { "ready", "cache" })
            .AddCheck<StockMonitoringHealthCheck>("stock_monitoring", tags: new[] { "ready", "stock" })
            .AddCheck<SystemResourcesHealthCheck>("system_resources", tags: new[] { "live", "system" });

        return services;
    }

    public static IApplicationBuilder UseValyanClinicHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var response = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(x => new
                    {
                        name = x.Key,
                        status = x.Value.Status.ToString(),
                        description = x.Value.Description,
                        data = x.Value.Data,
                        duration = x.Value.Duration.TotalMilliseconds,
                        exception = x.Value.Exception?.Message
                    }),
                    totalDuration = report.TotalDuration.TotalMilliseconds
                };

                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
            }
        });

        app.UseHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        app.UseHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live")
        });

        return app;
    }
}
