using ValyanClinic.Core.Exceptions;

namespace ValyanClinic.Core.Services;

public interface IStockMonitoringService
{
    Task CheckLowStockAsync();
    Task<List<LowStockAlert>> GetLowStockAlertsAsync();
    Task ProcessStockUpdateAsync(int medicationId, int newQuantity);
}

public class StockMonitoringBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StockMonitoringBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // Check every 6 hours

    public StockMonitoringBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<StockMonitoringBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stock Monitoring Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformStockCheckAsync();
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stock Monitoring Background Service cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Stock Monitoring Background Service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes on error
            }
        }
    }

    private async Task PerformStockCheckAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var stockService = scope.ServiceProvider.GetRequiredService<IStockMonitoringService>();

        try
        {
            await stockService.CheckLowStockAsync();
            _logger.LogInformation("Stock check completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during stock check");
            throw;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stock Monitoring Background Service stopped");
        return base.StopAsync(cancellationToken);
    }
}

public class StockMonitoringService : IStockMonitoringService
{
    private readonly ILogger<StockMonitoringService> _logger;
    // TODO: Add repository dependencies when implemented
    // private readonly IMedicationRepository _medicationRepository;
    // private readonly INotificationService _notificationService;

    public StockMonitoringService(ILogger<StockMonitoringService> logger)
    {
        _logger = logger;
    }

    public async Task CheckLowStockAsync()
    {
        try
        {
            // TODO: Replace with actual repository call
            var lowStockItems = await GetSimulatedLowStockItemsAsync();

            foreach (var item in lowStockItems)
            {
                _logger.LogWarning("Low stock alert: {MedicationName} has only {Quantity} units left", 
                    item.MedicationName, item.CurrentStock);

                // TODO: Send notifications
                await ProcessLowStockAlertAsync(item);
            }

            _logger.LogInformation("Stock monitoring completed. Found {Count} low stock items", lowStockItems.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during stock monitoring");
            throw new ExternalServiceException("StockMonitoring", "Eroare la monitorizarea stocului", ex);
        }
    }

    public async Task<List<LowStockAlert>> GetLowStockAlertsAsync()
    {
        try
        {
            // TODO: Replace with actual repository call
            return await GetSimulatedLowStockItemsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving low stock alerts");
            throw new ExternalServiceException("StockMonitoring", "Eroare la obtinerea alertelor de stoc", ex);
        }
    }

    public async Task ProcessStockUpdateAsync(int medicationId, int newQuantity)
    {
        try
        {
            _logger.LogInformation("Processing stock update for medication {MedicationId}: new quantity {Quantity}", 
                medicationId, newQuantity);

            // Business Logic: Check if stock is below threshold
            const int lowStockThreshold = 10;
            if (newQuantity <= lowStockThreshold)
            {
                await ProcessLowStockAlertAsync(new LowStockAlert
                {
                    MedicationId = medicationId,
                    MedicationName = $"Medication-{medicationId}",
                    CurrentStock = newQuantity,
                    MinimumThreshold = lowStockThreshold,
                    AlertLevel = newQuantity <= 5 ? AlertLevel.Critical : AlertLevel.Warning,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // TODO: Update actual repository
            await Task.Delay(100); // Simulate processing time
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing stock update for medication {MedicationId}", medicationId);
            throw;
        }
    }

    private async Task ProcessLowStockAlertAsync(LowStockAlert alert)
    {
        try
        {
            // TODO: Send notifications based on alert level
            switch (alert.AlertLevel)
            {
                case AlertLevel.Critical:
                    _logger.LogCritical("CRITICAL: {MedicationName} has only {Stock} units left!", 
                        alert.MedicationName, alert.CurrentStock);
                    // TODO: Send immediate notification to pharmacy manager
                    break;
                    
                case AlertLevel.Warning:
                    _logger.LogWarning("WARNING: {MedicationName} is running low ({Stock} units)", 
                        alert.MedicationName, alert.CurrentStock);
                    // TODO: Send notification to pharmacy staff
                    break;
            }

            await Task.Delay(50); // Simulate notification processing
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing low stock alert for {MedicationName}", alert.MedicationName);
        }
    }

    // Simulation method until actual implementation
    private async Task<List<LowStockAlert>> GetSimulatedLowStockItemsAsync()
    {
        await Task.Delay(100); // Simulate database call

        return new List<LowStockAlert>
        {
            new()
            {
                MedicationId = 1,
                MedicationName = "Paracetamol 500mg",
                CurrentStock = 3,
                MinimumThreshold = 10,
                AlertLevel = AlertLevel.Critical,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                MedicationId = 2,
                MedicationName = "Ibuprofen 400mg",
                CurrentStock = 8,
                MinimumThreshold = 15,
                AlertLevel = AlertLevel.Warning,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                MedicationId = 3,
                MedicationName = "Amoxicilina 500mg",
                CurrentStock = 1,
                MinimumThreshold = 20,
                AlertLevel = AlertLevel.Critical,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30)
            }
        };
    }
}

public class LowStockAlert
{
    public int MedicationId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinimumThreshold { get; set; }
    public AlertLevel AlertLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsProcessed { get; set; }
}

public enum AlertLevel
{
    Info = 1,
    Warning = 2,
    Critical = 3
}