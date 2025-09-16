using ValyanClinic.Core.Services;
using ValyanClinic.Application.Services;
using ValyanClinic.Infrastructure.Data;
using ValyanClinic.Application.Validators;
using ValyanClinic.Middleware;
using ValyanClinic.Core.Components;
using Syncfusion.Blazor;
using System.Text;
using System.Globalization;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Localization;
using System.Text.Encodings.Web;
using Dapper;
using ValyanClinic.Infrastructure.Repositories;
using ValyanClinic.Application.Extensions;
using ValyanClinic.Core.HealthChecks;
using ValyanClinic.Components;
using Serilog;

// ===== SERILOG BOOTSTRAP LOGGER =====
// Configure early bootstrap logger pentru a loga erorile din startup
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("🚀 Starting ValyanClinic application with SERILOG STRUCTURED LOGGING");

    var builder = WebApplication.CreateBuilder(args);

    // ===== SERILOG CONFIGURATION =====
    // Replace default logging with Serilog configured from appsettings.json
    builder.Host.UseSerilog((context, configuration) => 
        configuration.ReadFrom.Configuration(context.Configuration));

    Log.Information("✅ Serilog configured from appsettings.json");

    // CONFIGURARE ENCODING COMPLET PENTRU DIACRITICE ROMANESTI
    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    Console.OutputEncoding = Encoding.UTF8;
    Console.InputEncoding = Encoding.UTF8;
    Log.Information("✅ Console encoding configured for UTF-8 support");

    // Setare cultura implicita
    Thread.CurrentThread.CurrentCulture = new CultureInfo("ro-RO");
    Thread.CurrentThread.CurrentUICulture = new CultureInfo("ro-RO");

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

    // Set default culture to Romanian
    var culture = new CultureInfo("ro-RO");
    culture.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
    culture.DateTimeFormat.LongDatePattern = "dd MMMM yyyy";
    culture.NumberFormat.NumberDecimalSeparator = ",";
    culture.NumberFormat.NumberGroupSeparator = ".";

    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;

    Log.Information("✅ Culture and encoding configured");

    // Register Syncfusion license
    var syncfusionLicense = builder.Configuration["Syncfusion:LicenseKey"];
    if (!string.IsNullOrEmpty(syncfusionLicense))
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicense);
        Log.Information("✅ Syncfusion license registered from configuration");
    }
    else
    {
        // Fallback license
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZfcXRUR2lcVUV2V0BWYEg=");
        Log.Warning("⚠️ Using fallback Syncfusion license");
    }

    // DAPPER DATABASE CONFIGURATION - SECURIZAT
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    Log.Information("🔗 Configuring database connection");

    // Configurare IDbConnectionFactory pentru backward compatibility
    builder.Services.AddSingleton<IDbConnectionFactory>(provider =>
        new SqlConnectionFactory(connectionString));

    // Configurare IDbConnection pentru Dapper
    builder.Services.AddScoped<IDbConnection>(provider => 
    {
        var connection = new SqlConnection(connectionString);
        connection.ConnectionString = connectionString;
        return connection;
    });

    // 🔥 SERVICII CRITICE PENTRU REZOLVAREA PROBLEMELOR
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<ICacheService, MemoryCacheService>();
    builder.Services.AddSimpleGridStateService(); // Grid state persistence simplificat

    // Add Blazor services
    builder.Services.AddRazorComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    })
    .AddInteractiveServerComponents();

    // Configure JSON cu UTF-8
    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

    // Add Syncfusion Blazor services
    builder.Services.AddSyncfusionBlazor();

    Log.Information("✅ UI Components and cache configured");

    // === CONFIGURARE FLUENTVALIDATION ===
    builder.Services.AddValyanClinicValidation();
    
    // === REPOSITORY LAYER ===
    builder.Services.AddScoped<IPersonalRepository, PersonalRepository>();
    
    // === APPLICATION SERVICES ===
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IPersonalService, PersonalService>();
    builder.Services.AddScoped<IValidationService, ValidationService>();

    // === AUTHENTICATION & SECURITY SERVICES ===
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
    builder.Services.AddScoped<IUserSessionService, UserSessionService>();
    builder.Services.AddScoped<ISecurityAuditService, SecurityAuditService>();

    // === MANAGEMENT SERVICES ===
    builder.Services.AddScoped<IUserManagementService, UserManagementService>();
    
    // === STOCK MONITORING SERVICES ===
    builder.Services.AddScoped<IStockMonitoringService, StockMonitoringService>();

    // === BACKGROUND SERVICES ===
    builder.Services.AddHostedService<StockMonitoringBackgroundService>();

    // === HEALTH CHECKS ===
    builder.Services.AddValyanClinicHealthChecks();

    // Localization services
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

    Log.Information("✅ ALL SERVICES registered successfully: {ServiceCount} service types", 15);

    var app = builder.Build();

    Log.Information("✅ Application built successfully");

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
        
        Log.Information("✅ Database connection established successfully via Dapper to {DatabaseName}", databaseName);
        Log.Debug("Database server info: {ServerInfo}", serverInfo?.Split('\n')[0]);
        
        dbConnection.Close();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Failed to establish database connection to server TS1828\\ERP");
        Log.Warning("⚠️ Application will continue but database operations may fail");
    }

    // Configure the HTTP request pipeline
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
    app.UseExceptionHandling();

    // Request localization
    app.UseRequestLocalization(new RequestLocalizationOptions
    {
        DefaultRequestCulture = new RequestCulture("ro-RO"),
        SupportedCultures = new List<CultureInfo> { new CultureInfo("ro-RO"), new CultureInfo("en-US") },
        SupportedUICultures = new List<CultureInfo> { new CultureInfo("ro-RO"), new CultureInfo("en-US") }
    });

    // ===== SERILOG HTTP REQUEST LOGGING =====
    // Add detailed HTTP request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, elapsed, ex) => ex != null
            ? Serilog.Events.LogEventLevel.Error
            : httpContext.Response.StatusCode > 499
                ? Serilog.Events.LogEventLevel.Error
                : httpContext.Response.StatusCode > 399
                    ? Serilog.Events.LogEventLevel.Warning
                    : Serilog.Events.LogEventLevel.Information;
    });

    // Health checks endpoints
    app.UseValyanClinicHealthChecks();

    app.UseHttpsRedirection();
    
    // Static files cu UTF-8 support
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

    app.UseAntiforgery();

    // Map routes
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    Log.Information("🌟 ValyanClinic application configured successfully with SERILOG STRUCTURED LOGGING");
    Log.Information("✅ Features: Memory leak prevention, Grid state persistence, Error handling, Authentication, Stock monitoring");
    Log.Information("🌐 Listening on: https://localhost:7164 and http://localhost:5007");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 ValyanClinic application terminated unexpectedly during startup");
    throw;
}
finally
{
    Log.Information("🔚 ValyanClinic application shutdown complete - disposing Serilog");
    await Log.CloseAndFlushAsync();
}
