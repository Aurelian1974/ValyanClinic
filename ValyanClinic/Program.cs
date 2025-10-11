using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Syncfusion.Blazor;
using ValyanClinic.Infrastructure.Data;
using ValyanClinic.Infrastructure.Repositories;
using ValyanClinic.Infrastructure.Caching;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Components.Layout;
using ValyanClinic.Services.DataGrid;
using MediatR;

// ========================================
// SERILOG CONFIGURATION
// ========================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    Log.Information("Pornire aplicatie ValyanClinic...");

    var builder = WebApplication.CreateBuilder(args);

    // ========================================
    // SERILOG
    // ========================================
    builder.Host.UseSerilog();

    // ========================================
    // BLAZOR SERVICES - WITH DETAILED ERRORS
    // ========================================
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents(options =>
        {
            options.DetailedErrors = builder.Environment.IsDevelopment();
            options.DisconnectedCircuitMaxRetained = 100;
            options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
            options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
        });

    // ========================================
    // SYNCFUSION
    // ========================================
    var syncfusionLicenseKey = builder.Configuration["Syncfusion:LicenseKey"];
    if (!string.IsNullOrEmpty(syncfusionLicenseKey))
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);
    }
    builder.Services.AddSyncfusionBlazor();

    // ========================================
    // DATABASE
    // ========================================
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    
    builder.Services.AddSingleton<IDbConnectionFactory>(sp =>
        new SqlConnectionFactory(connectionString));

    // ========================================
    // REPOSITORIES
    // ========================================
    builder.Services.AddScoped<IPersonalRepository, PersonalRepository>();
    builder.Services.AddScoped<IPersonalMedicalRepository, PersonalMedicalRepository>();
    builder.Services.AddScoped<IOcupatieISCORepository, OcupatieISCORepository>();

    // ========================================
    // CACHING
    // ========================================
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

    // ========================================
    // MEDIATR (CQRS)
    // ========================================
    builder.Services.AddMediatR(cfg => 
    {
        cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        cfg.RegisterServicesFromAssemblyContaining<ValyanClinic.Application.Common.Results.Result>();
    });

    // ========================================
    // AUTOMAPPER
    // ========================================
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    // ========================================
    // LAYOUT SERVICES
    // ========================================
    builder.Services.AddScoped<BreadcrumbService>();

    // ========================================
    // DATAGRID SERVICES
    // ========================================
    builder.Services.AddScoped(typeof(IDataGridStateService<>), typeof(DataGridStateService<>));
    builder.Services.AddScoped<IFilterOptionsService, FilterOptionsService>();
    builder.Services.AddScoped<IDataFilterService, DataFilterService>();

    // ========================================
    // HEALTH CHECKS
    // ========================================
    builder.Services.AddHealthChecks()
        .AddSqlServer(
            connectionString, 
            name: "database", 
            tags: new[] { "db", "sql", "sqlserver" },
            timeout: TimeSpan.FromSeconds(3));

    builder.Services.AddHealthChecksUI(opt =>
    {
        opt.SetEvaluationTimeInSeconds(60);
        opt.MaximumHistoryEntriesPerEndpoint(50);
        opt.AddHealthCheckEndpoint("ValyanClinic", "/health-json");
    })
    .AddInMemoryStorage();

    // ========================================
    // HTTP CONTEXT
    // ========================================
    builder.Services.AddHttpContextAccessor();

    // ========================================
    // BUILD APP
    // ========================================
    var app = builder.Build();

    // ========================================
    // MIDDLEWARE PIPELINE
    // ========================================
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();

    // ========================================
    // HEALTH CHECKS
    // ========================================
    app.MapHealthChecks("/health-json", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecksUI(config =>
    {
        config.UIPath = "/health-ui";
    });

    // ========================================
    // BLAZOR
    // ========================================
    app.MapRazorComponents<ValyanClinic.Components.App>()
        .AddInteractiveServerRenderMode();

    Log.Information("Aplicatie pornita cu succes!");
    Log.Information("Health Check Dashboard: /health-ui");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicatia s-a oprit neasteptat");
}
finally
{
    Log.CloseAndFlush();
}
