using Microsoft.Extensions.Hosting;
using Serilog;

namespace ValyanClinic.Services;

/// <summary>
/// Hosted Service pentru curățarea fișierelor de log la shutdown-ul aplicației
/// Alternativă la LogCleanupService pentru mai multă control și flexibilitate
/// </summary>
public class LogCleanupHostedService : IHostedService, IDisposable
{
    private readonly ILogger<LogCleanupHostedService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly string _logsDirectory;
    private Timer? _cleanupTimer;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24); // Cleanup zilnic

    public LogCleanupHostedService(ILogger<LogCleanupHostedService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
        _logsDirectory = Path.Combine(environment.ContentRootPath, "Logs");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🧹 Log Cleanup Service starting");
        
        // Optional: Set up periodic cleanup (commented out for now)
        // _cleanupTimer = new Timer(PeriodicCleanup, null, TimeSpan.Zero, _cleanupInterval);
        
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🧹 Log Cleanup Service stopping - performing final cleanup");
        
        try
        {
            // Stop the timer
            _cleanupTimer?.Change(Timeout.Infinite, 0);
            
            // Perform final cleanup
            await PerformLogCleanup();
            
            _logger.LogInformation("✅ Log Cleanup Service stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during log cleanup service shutdown");
        }
    }

    private void PeriodicCleanup(object? state)
    {
        try
        {
            _logger.LogInformation("🔄 Performing periodic log cleanup");
            PerformLogCleanup().Wait();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during periodic log cleanup");
        }
    }

    private async Task PerformLogCleanup()
    {
        try
        {
            if (!Directory.Exists(_logsDirectory))
            {
                _logger.LogWarning("Logs directory does not exist: {LogsDirectory}", _logsDirectory);
                return;
            }

            _logger.LogInformation("🧹 Starting log cleanup in directory: {LogsDirectory}", _logsDirectory);

            // Ensure Serilog flushes before cleanup
            await Log.CloseAndFlushAsync();
            
            // Wait a moment for file handles to be released
            await Task.Delay(500);

            // Get all log files
            var logFiles = Directory.GetFiles(_logsDirectory, "*.log", SearchOption.AllDirectories);
            var deletedCount = 0;
            var clearedCount = 0;
            var errorCount = 0;

            foreach (var logFile in logFiles)
            {
                try
                {
                    // Try to delete the file first
                    File.Delete(logFile);
                    deletedCount++;
                    _logger.LogDebug("Deleted log file: {FileName}", Path.GetFileName(logFile));
                }
                catch (UnauthorizedAccessException)
                {
                    // File is still in use, try to clear its content instead
                    try
                    {
                        await File.WriteAllTextAsync(logFile, string.Empty);
                        clearedCount++;
                        _logger.LogDebug("Cleared content of log file: {FileName}", Path.GetFileName(logFile));
                    }
                    catch (Exception clearEx)
                    {
                        errorCount++;
                        _logger.LogWarning(clearEx, "Could not clear log file: {FileName}", Path.GetFileName(logFile));
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    _logger.LogWarning(ex, "Could not delete log file: {FileName}", Path.GetFileName(logFile));
                }
            }

            _logger.LogInformation("🎯 Log cleanup completed: {DeletedCount} deleted, {ClearedCount} cleared, {ErrorCount} errors", 
                deletedCount, clearedCount, errorCount);

            // Recreate fresh log files
            await RecreateEmptyLogFiles();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ General error during log cleanup");
        }
    }

    private async Task RecreateEmptyLogFiles()
    {
        try
        {
            var today = DateTime.Now.ToString("yyyyMMdd");
            var logFiles = new[]
            {
                $"valyan-clinic-{today}.log",
                $"errors-{today}.log"
            };

            foreach (var logFile in logFiles)
            {
                var filePath = Path.Combine(_logsDirectory, logFile);
                try
                {
                    var header = $"# ValyanClinic Log File - Created: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                                $"# Application: ValyanMed Clinical Management System\n" +
                                $"# Log Level: {(logFile.Contains("errors") ? "Warning+" : "Information+")}\n\n";
                    
                    await File.WriteAllTextAsync(filePath, header);
                    _logger.LogDebug("Recreated empty log file: {LogFile}", logFile);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not recreate log file: {LogFile}", logFile);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error recreating log files");
        }
    }

    public void Dispose()
    {
        try
        {
            _cleanupTimer?.Dispose();
            
            // Perform one final cleanup on dispose
            PerformLogCleanup().Wait(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Error during LogCleanupHostedService disposal: {ex.Message}");
        }
    }
}
