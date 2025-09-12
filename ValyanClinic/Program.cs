using ValyanClinic.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using FluentValidation;
using ValyanClinic.Core.Services;
using ValyanClinic.Core.HealthChecks;
using ValyanClinic.Core.Middleware;
using ValyanClinic.Application.Services;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

// Register Syncfusion license from configuration
var syncfusionLicense = builder.Configuration["Syncfusion:LicenseKey"];
if (!string.IsNullOrEmpty(syncfusionLicense))
{
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicense);
}
else
{
    // Fallback to direct license if not in config
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZfcXRUR2lcVUV2V0BWYEg=");
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Controllers for API endpoints
builder.Services.AddControllers();

// Add Fluent UI services
builder.Services.AddFluentUIComponents();

// Add Syncfusion Blazor services
builder.Services.AddSyncfusionBlazor();

// Add memory cache
builder.Services.AddMemoryCache();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add custom services
builder.Services.AddScoped<ICacheService, MemoryCacheService>();
builder.Services.AddScoped<IStockMonitoringService, StockMonitoringService>();
builder.Services.AddScoped<IUserService, UserService>();

// Add background services
builder.Services.AddHostedService<StockMonitoringBackgroundService>();

// Add health checks
builder.Services.AddValyanClinicHealthChecks();

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Add global exception handling middleware
app.UseGlobalExceptionHandling();

// Add health checks endpoints
app.UseValyanClinicHealthChecks();

app.UseHttpsRedirection();
app.UseAntiforgery();

// Map API controllers
app.MapControllers();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
