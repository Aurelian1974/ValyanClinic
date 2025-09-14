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
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // CHANGED: Check daily instead of every 6 hours

    public StockMonitoringBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<StockMonitoringBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stock Monitoring Background Service started - checking every 24 hours");

        // WAIT 1 HOUR ON STARTUP TO AVOID SPAM DURING DEVELOPMENT
        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);

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
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // INCREASED: Wait 1 hour on error
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
            _logger.LogDebug("Stock check completed successfully"); // CHANGED: Debug instead of Information
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
    private static DateTime _lastAlertTime = DateTime.MinValue; // ADDED: Track last alert time
    private static readonly Dictionary<string, DateTime> _lastAlertsByMedication = new(); // ADDED: Track per medication
    
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
            // RATE LIMITING: Don't spam alerts more than once per hour
            if (DateTime.UtcNow - _lastAlertTime < TimeSpan.FromHours(1))
            {
                _logger.LogDebug("Stock monitoring skipped - alerts recently sent");
                return;
            }

            // TODO: Replace with actual repository call
            var lowStockItems = await GetSimulatedLowStockItemsAsync();

            var newAlertsCount = 0;
            foreach (var item in lowStockItems)
            {
                // PER-MEDICATION RATE LIMITING: Only alert once per day per medication
                if (_lastAlertsByMedication.TryGetValue(item.MedicationName, out var lastAlert) &&
                    DateTime.UtcNow - lastAlert < TimeSpan.FromDays(1))
                {
                    continue; // Skip this medication, already alerted recently
                }

                _logger.LogWarning("Low stock alert: {MedicationName} has only {Quantity} units left", 
                    item.MedicationName, item.CurrentStock);

                await ProcessLowStockAlertAsync(item);
                
                _lastAlertsByMedication[item.MedicationName] = DateTime.UtcNow;
                newAlertsCount++;
            }

            if (newAlertsCount > 0)
            {
                _logger.LogInformation("Stock monitoring completed. Sent {Count} new alerts", newAlertsCount);
                _lastAlertTime = DateTime.UtcNow;
            }
            else
            {
                _logger.LogDebug("Stock monitoring completed. No new alerts sent (rate limited)");
            }
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
            _logger.LogDebug("Processing stock update for medication {MedicationId}: new quantity {Quantity}", 
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
                    _logger.LogError("CRITICAL STOCK: {MedicationName} has only {Stock} units left! Immediate action required.", 
                        alert.MedicationName, alert.CurrentStock);
                    // TODO: Send immediate notification to pharmacy manager
                    break;
                    
                case AlertLevel.Warning:
                    _logger.LogWarning("LOW STOCK: {MedicationName} is running low ({Stock} units)", 
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

        // DEVELOPMENT MODE: Return fewer items to reduce log spam
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
                MedicationId = 3,
                MedicationName = "Amoxicilina 500mg",
                CurrentStock = 1,
                MinimumThreshold = 20,
                AlertLevel = AlertLevel.Critical,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30)
            }
            // REMOVED Ibuprofen to reduce log volume during development
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
