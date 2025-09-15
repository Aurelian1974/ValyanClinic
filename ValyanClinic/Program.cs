using ValyanClinic.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using FluentValidation;
using ValyanClinic.Core.Services;
using ValyanClinic.Core.HealthChecks;
using ValyanClinic.Core.Middleware;
using ValyanClinic.Application.Services;
using ValyanClinic.Components.Pages.LoginPage;
using ValyanClinic.Domain.Models;
using Syncfusion.Blazor;
using System.Text;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using Serilog;
using Serilog.Events;

// BOOTSTRAP LOGGER MINIMAL - PENTRU DEBUGGING
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("🚀 Starting ValyanClinic application");

    var builder = WebApplication.CreateBuilder(args);

    // TESTARE PROGRESIVĂ SERILOG
    try
    {
        Log.Information("📝 Configuring Serilog from appsettings");
        
        // CONFIGURARE SERILOG COMPLET DIN APPSETTINGS
        builder.Host.UseSerilog((context, configuration) => 
            configuration.ReadFrom.Configuration(context.Configuration));
            
        Log.Information("✅ Serilog configured successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Failed to configure Serilog from appsettings");
        // Continuă cu bootstrap logger
    }

    // CONFIGURARE ENCODING COMPLET PENTRU DIACRITICE ROMÂNEȘTI
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    // Păstrăm Console encoding pentru suportul UTF-8 în output
    Console.OutputEncoding = Encoding.UTF8;
    Console.InputEncoding = Encoding.UTF8;
    Log.Information("✅ Console encoding configured for UTF-8 support");

    // Setare cultură implicită înainte de orice altceva
    Thread.CurrentThread.CurrentCulture = new CultureInfo("ro-RO");
    Thread.CurrentThread.CurrentUICulture = new CultureInfo("ro-RO");

    // Configure web encoding
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
    });

    // Configure localization with UTF-8 support
    builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        var supportedCultures = new[] { "ro-RO", "en-US" };
        options.SetDefaultCulture("ro-RO")
               .AddSupportedCultures(supportedCultures)
               .AddSupportedUICultures(supportedCultures);
        
        options.DefaultRequestCulture = new RequestCulture("ro-RO", "ro-RO");
        
        options.RequestCultureProviders = new List<IRequestCultureProvider>
        {
            new QueryStringRequestCultureProvider(),
            new CookieRequestCultureProvider(),
            new AcceptLanguageHeaderRequestCultureProvider()
        };
    });

    // Set default culture to Romanian with UTF-8
    var culture = new CultureInfo("ro-RO");
    culture.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
    culture.DateTimeFormat.LongDatePattern = "dd MMMM yyyy";
    culture.NumberFormat.NumberDecimalSeparator = ",";
    culture.NumberFormat.NumberGroupSeparator = ".";

    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;

    Log.Information("✅ Culture and encoding configured");

    // Register Syncfusion license from configuration
    var syncfusionLicense = builder.Configuration["Syncfusion:LicenseKey"];
    if (!string.IsNullOrEmpty(syncfusionLicense))
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicense);
        Log.Information("✅ Syncfusion license registered from configuration");
    }
    else
    {
        // Fallback to direct license if not in config
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZfcXRUR2lcVUV2V0BWYEg=");
        Log.Warning("⚠️ Using fallback Syncfusion license");
    }

    // DAPPER DATABASE CONFIGURATION
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    Log.Information("🔗 Configuring database connection");

    // Configurare IDbConnection pentru Dapper
    builder.Services.AddScoped<IDbConnection>(provider => 
    {
        var connection = new SqlConnection(connectionString);
        connection.ConnectionString = connectionString;
        return connection;
    });

    // Configurare pentru pool de conexiuni optimizat pentru Dapper
    builder.Services.AddScoped<Func<IDbConnection>>(provider => 
        () => new SqlConnection(connectionString));

    // Add services to the container
    builder.Services.AddRazorComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    })
    .AddInteractiveServerComponents();

    // Add Controllers cu UTF-8 support
    builder.Services.AddControllers(options =>
    {
        options.RespectBrowserAcceptHeader = true;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Configurări pentru UTF-8
    });

    // Configure JSON cu UTF-8
    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

    // Add Fluent UI services
    builder.Services.AddFluentUIComponents();

    // Add Syncfusion Blazor services
    builder.Services.AddSyncfusionBlazor();

    // Add memory cache
    builder.Services.AddMemoryCache();

    Log.Information("✅ UI Components and cache configured");

    // FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();

    // Rich Services
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
    builder.Services.AddScoped<IUserSessionService, UserSessionService>();
    builder.Services.AddScoped<ISecurityAuditService, SecurityAuditService>();

    // Personal Management Services
    builder.Services.AddScoped<ValyanClinic.Infrastructure.Repositories.IPersonalRepository, ValyanClinic.Infrastructure.Repositories.PersonalRepository>();
    builder.Services.AddScoped<ValyanClinic.Application.Services.IPersonalService, ValyanClinic.Application.Services.PersonalService>();

    // Existing services
    builder.Services.AddScoped<ICacheService, MemoryCacheService>();
    builder.Services.AddScoped<IStockMonitoringService, StockMonitoringService>();
    builder.Services.AddScoped<IUserManagementService, UserManagementService>();
    builder.Services.AddScoped<IUserService, UserService>();

    // Background services
    builder.Services.AddHostedService<StockMonitoringBackgroundService>();
    
    // OPTIONAL: Add Log Cleanup as Hosted Service (commented out to use callback approach)
    // builder.Services.AddHostedService<ValyanClinic.Services.LogCleanupHostedService>();

    Log.Information("✅ Application services configured");

    // Health checks
    builder.Services.AddValyanClinicHealthChecks();

    // Localization services
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

    // Register Application Log Preservation Service (renamed from LogCleanupService)
    builder.Services.AddSingleton<LogCleanupService>();

    Log.Information("✅ All services registered successfully");

    var app = builder.Build();

    Log.Information("✅ Application built successfully");

    // ADD SERILOG REQUEST LOGGING - DOAR DACĂ SERILOG E CONFIGURAT CORECT
    try
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (httpContext, elapsed, ex) => ex != null
                ? LogEventLevel.Error 
                : httpContext.Response.StatusCode > 499 
                    ? LogEventLevel.Error 
                    : LogEventLevel.Information;
        });
        Log.Information("✅ Serilog request logging configured");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "⚠️ Could not configure Serilog request logging");
    }

    // TEST DATABASE CONNECTION LA STARTUP
    try
    {
        using var scope = app.Services.CreateScope();
        var dbConnection = scope.ServiceProvider.GetRequiredService<IDbConnection>();
        if (dbConnection.State != ConnectionState.Open)
        {
            dbConnection.Open();
        }
        
        var serverInfo = dbConnection.QueryFirstOrDefault<string>("SELECT @@VERSION");
        var databaseName = dbConnection.QueryFirstOrDefault<string>("SELECT DB_NAME()");
        
        Log.Information("✅ Database connection established successfully via Dapper");
        Log.Information("📊 Connected to database: {DatabaseName}", databaseName);
        Log.Information("🔒 Connection string secured - never exposed to client");
        
        dbConnection.Close();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Failed to establish database connection");
        Log.Error("💡 Verify that SQL Server is running and accessible at: TS1828\\ERP");
        Log.Error("💡 Ensure the ValyanMed database exists and permissions are correct");
    }

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

    // UTF-8 Middleware
    app.Use(async (context, next) =>
    {
        context.Response.OnStarting(() =>
        {
            var response = context.Response;
            
            if (!response.HasStarted && !string.IsNullOrEmpty(response.ContentType))
            {
                if (!response.ContentType.Contains("charset", StringComparison.OrdinalIgnoreCase))
                {
                    response.ContentType += "; charset=UTF-8";
                }
            }
            else if (!response.HasStarted && string.IsNullOrEmpty(response.ContentType))
            {
                response.ContentType = "text/html; charset=UTF-8";
            }
            
            response.Headers.Append("Content-Language", "ro-RO");
            
            return Task.CompletedTask;
        });
        
        await next();
    });

    // Global exception handling middleware
    app.UseGlobalExceptionHandling();

    // Request localization
    app.UseRequestLocalization(new RequestLocalizationOptions
    {
        DefaultRequestCulture = new RequestCulture("ro-RO"),
        SupportedCultures = new List<CultureInfo> { new CultureInfo("ro-RO"), new CultureInfo("en-US") },
        SupportedUICultures = new List<CultureInfo> { new CultureInfo("ro-RO"), new CultureInfo("en-US") }
    });

    // Health checks endpoints
    app.UseValyanClinicHealthChecks();

    app.UseHttpsRedirection();
    app.UseAntiforgery();

    // Static files
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            var path = ctx.File.Name.ToLowerInvariant();
            var response = ctx.Context.Response;
            
            if (path.EndsWith(".css"))
            {
                response.Headers.ContentType = "text/css; charset=UTF-8";
            }
            else if (path.EndsWith(".js"))
            {
                response.Headers.ContentType = "application/javascript; charset=UTF-8";
            }
            else if (path.EndsWith(".html") || path.EndsWith(".htm"))
            {
                response.Headers.ContentType = "text/html; charset=UTF-8";
            }
            else if (path.EndsWith(".json"))
            {
                response.Headers.ContentType = "application/json; charset=UTF-8";
            }
        }
    });

    // Map routes
    app.MapControllers();
    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    // CONFIGURE LOG CLEANUP ON SHUTDOWN
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    var logCleanupService = app.Services.GetRequiredService<LogCleanupService>();
    
    // Register shutdown callbacks
    lifetime.ApplicationStopping.Register(() =>
    {
        Log.Information("📊 Application stopping - preparing log preservation");
        try
        {
            logCleanupService.PrepareForShutdown();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Warning: Error preparing log preservation");
        }
    });
    
    lifetime.ApplicationStopped.Register(() =>
    {
        try
        {
            logCleanupService.CleanupLogsOnShutdown();
            Log.Information("✅ Log files preserved successfully on shutdown");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Warning: Error preserving logs");
        }
    });

    Log.Information("🌟 ValyanClinic application configured successfully");
    Log.Information("🌐 Listening on: https://localhost:7164 and http://localhost:5007");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 Application terminated unexpectedly");
}
finally
{
    Log.Information("🏁 ValyanClinic application shutdown complete");
    await Log.CloseAndFlushAsync();
}

/// <summary>
/// Service pentru păstrarea fișierelor de log la shutdown - NU MAI ȘTERGE LOG-URILE
/// </summary>
public class LogCleanupService
{
    private readonly ILogger<LogCleanupService> _logger;
    private readonly string _logsDirectory;
    private bool _shutdownPrepared = false;

    public LogCleanupService(ILogger<LogCleanupService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _logsDirectory = Path.Combine(environment.ContentRootPath, "Logs");
    }

    public void PrepareForShutdown()
    {
        _logger.LogInformation("🔄 Preparing log preservation - flushing all pending logs");
        
        try
        {
            // Flush Serilog to ensure all logs are written
            Log.CloseAndFlush();
            _shutdownPrepared = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flushing logs during shutdown preparation");
        }
    }

    public void CleanupLogsOnShutdown()
    {
        if (!_shutdownPrepared)
        {
            _logger.LogWarning("Warning: Shutdown not properly prepared, logs will be preserved anyway");
        }

        try
        {
            if (!Directory.Exists(_logsDirectory))
            {
                _logger.LogWarning("Logs directory does not exist: {LogsDirectory}", _logsDirectory);
                return;
            }

            _logger.LogInformation("📊 Preserving logs in directory: {LogsDirectory}", _logsDirectory);

            // Get all log files - DOAR PENTRU RAPORTARE, NU PENTRU ȘTERGERE
            var logFiles = Directory.GetFiles(_logsDirectory, "*.log", SearchOption.AllDirectories);
            var preservedCount = 0;
            var totalSize = 0L;

            foreach (var logFile in logFiles)
            {
                try
                {
                    var fileInfo = new FileInfo(logFile);
                    totalSize += fileInfo.Length;
                    preservedCount++;
                    _logger.LogInformation("✅ Preserved log file: {FileName} ({Size})", 
                        Path.GetFileName(logFile), FormatBytes(fileInfo.Length));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("⚠️ Could not read info for {FileName}: {Error}", 
                        Path.GetFileName(logFile), ex.Message);
                }
            }

            _logger.LogInformation("🎯 Log preservation summary: {PreservedCount} files preserved, total size: {TotalSize}", 
                preservedCount, FormatBytes(totalSize));
            _logger.LogInformation("💡 All logs have been preserved for debugging and analysis purposes");
            _logger.LogInformation("📍 Log directory: {LogDirectory}", _logsDirectory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ General error during log preservation check");
        }
    }

    private static string FormatBytes(long bytes)
    {
        const int scale = 1024;
        string[] orders = { "GB", "MB", "KB", "Bytes" };
        long max = (long)Math.Pow(scale, orders.Length - 1);

        foreach (string order in orders)
        {
            if (bytes > max)
                return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

            max /= scale;
        }
        return "0 Bytes";
    }
}
