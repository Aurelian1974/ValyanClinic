using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Syncfusion.Blazor;
using ValyanClinic.Infrastructure.Data;
using ValyanClinic.Infrastructure.Repositories;
using ValyanClinic.Infrastructure.Caching;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Components.Layout;
using ValyanClinic.Services.DataGrid;
using ValyanClinic.Services.Caching;
using ValyanClinic.Services.Security;
using ValyanClinic.Services.Blazor;
using Microsoft.AspNetCore.Components.Server.Circuits;
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
            options.MaxBufferedUnacknowledgedRenderBatches = 20;
        });

    // ========================================
    // CIRCUIT HANDLER - Pentru gestionarea reconectărilor
    // ========================================
    builder.Services.AddScoped<CircuitHandler, ValyanCircuitHandler>();

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
    // DATABASE - Connection Pooling Configuration OPTIMIZED
    // ========================================
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    
    // CRITICAL: Configurare optimizată connection pooling pentru Blazor Server
    var connectionStringBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString)
    {
        // Connection Pooling
        Pooling = true,
        MinPoolSize = 5,
        MaxPoolSize = 100,
        
        // Timeouts
        ConnectTimeout = 30,
        CommandTimeout = 30,
        
        // Connection Resilience
        ConnectRetryCount = 3,
        ConnectRetryInterval = 10,
        
        // CRITICAL: Cleanup stale connections
        LoadBalanceTimeout = 60, // Seconds before connection is returned to pool
        
        // Performance
        MultipleActiveResultSets = false, // MARS disabled pentru Dapper
        
        // Security
        Encrypt = false, // Set true for production with SSL
        TrustServerCertificate = true
    };
    
    var optimizedConnectionString = connectionStringBuilder.ConnectionString;
    
    Log.Information("Connection string optimized: Pooling={Pooling}, Min={Min}, Max={Max}, Timeout={Timeout}",
        connectionStringBuilder.Pooling,
        connectionStringBuilder.MinPoolSize,
        connectionStringBuilder.MaxPoolSize,
        connectionStringBuilder.ConnectTimeout);
    
    builder.Services.AddSingleton<IDbConnectionFactory>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<SqlConnectionFactory>>();
        return new SqlConnectionFactory(optimizedConnectionString, logger);
    });

    // ========================================
    // REPOSITORIES
    // ========================================
    builder.Services.AddScoped<IPersonalRepository, PersonalRepository>();
    builder.Services.AddScoped<IPersonalMedicalRepository, PersonalMedicalRepository>();
    builder.Services.AddScoped<IOcupatieISCORepository, OcupatieISCORepository>();
    builder.Services.AddScoped<ILocationRepository, LocationRepository>();
    builder.Services.AddScoped<IDepartamentRepository, DepartamentRepository>();
    builder.Services.AddScoped<ITipDepartamentRepository, TipDepartamentRepository>();
    builder.Services.AddScoped<IPozitieRepository, PozitieRepository>();
    builder.Services.AddScoped<ISpecializareRepository, SpecializareRepository>();
    builder.Services.AddScoped<IPacientRepository, PacientRepository>();

    // ========================================
    // CACHING
    // ========================================
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
    builder.Services.AddScoped<IQueryCacheService, QueryCacheService>();

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
    // SECURITY SERVICES
    // ========================================
    builder.Services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();

    // ========================================
    // BUSINESS SERVICES
    // ========================================
    builder.Services.AddScoped<ValyanClinic.Application.Services.IPersonalBusinessService, 
                                ValyanClinic.Application.Services.PersonalBusinessService>();

    // ========================================
    // BACKGROUND SERVICES
    // ========================================
    builder.Services.AddHostedService<ValyanClinic.Services.Background.DatabaseConnectionCleanupService>();

    // ========================================
    // HEALTH CHECKS
    // ========================================
    builder.Services.AddHealthChecks()
        .AddSqlServer(
            optimizedConnectionString, // FOLOSEȘTE connection string optimizat
            name: "database", 
            tags: new[] { "db", "sql", "sqlserver" },
            timeout: TimeSpan.FromSeconds(3));

    builder.Services.AddHealthChecksUI(opt =>
    {
        opt.SetEvaluationTimeInSeconds(60);
        opt.MaximumHistoryEntriesPerEndpoint(50);
        opt.AddHealthCheckEndpoint("ValyanClinic", "/health-ui");
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
    Log.Information("Connection string configured with pooling");
    
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
