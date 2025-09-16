using ValyanClinic.Application.Services;
using ValyanClinic.Infrastructure.Data;
using ValyanClinic.Application.Validators;
using ValyanClinic.Middleware;
using Syncfusion.Blazor;
using Serilog;
using Serilog.Events;
using System.Text;
using System.Globalization;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Localization;
using System.Text.Encodings.Web;
using Dapper; // Pentru QueryFirstOrDefault

// BOOTSTRAP LOGGER MINIMAL - PENTRU DEBUGGING
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("🚀 Starting ValyanClinic application");

    var builder = WebApplication.CreateBuilder(args);

    // TESTARE PROGRESIVA SERILOG
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
        // Continua cu bootstrap logger
    }

    // CONFIGURARE ENCODING COMPLET PENTRU DIACRITICE ROMANESTI
    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    // Pastram Console encoding pentru suportul UTF-8 in output
    Console.OutputEncoding = Encoding.UTF8;
    Console.InputEncoding = Encoding.UTF8;
    Log.Information("✅ Console encoding configured for UTF-8 support");

    // Setare cultura implicita inainte de orice altceva
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
        // Configurari pentru UTF-8
    });

    // Configure JSON cu UTF-8
    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

    // Add Syncfusion Blazor services
    builder.Services.AddSyncfusionBlazor();

    // Add memory cache
    builder.Services.AddMemoryCache();

    Log.Information("✅ UI Components and cache configured");

    // Application services
    builder.Services.AddScoped<IPersonalService, PersonalService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IValidationService, ValidationService>();

    Log.Information("✅ Application services configured");

    // Localization services
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

    Log.Information("✅ All services registered successfully");

    var app = builder.Build();

    Log.Information("✅ Application built successfully");

    // ADD SERILOG REQUEST LOGGING - DOAR DACA SERILOG E CONFIGURAT CORECT
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
        Log.Information("🗄️ Connected to database: {DatabaseName}", databaseName);
        Log.Information("🔒 Connection string secured - never exposed to client");
        
        dbConnection.Close();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Failed to establish database connection");
        Log.Error("⚠️ Verify that SQL Server is running and accessible at: TS1828\\ERP");
        Log.Error("⚠️ Ensure the ValyanMed database exists and permissions are correct");
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
    app.UseExceptionHandling();

    // Request localization
    app.UseRequestLocalization(new RequestLocalizationOptions
    {
        DefaultRequestCulture = new RequestCulture("ro-RO"),
        SupportedCultures = new List<CultureInfo> { new CultureInfo("ro-RO"), new CultureInfo("en-US") },
        SupportedUICultures = new List<CultureInfo> { new CultureInfo("ro-RO"), new CultureInfo("en-US") }
    });

    app.UseHttpsRedirection();
    app.UseAntiforgery();

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

    // Map routes
    app.MapRazorComponents<ValyanClinic.Components.App>()
        .AddInteractiveServerRenderMode();

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
    Log.Information("🔚 ValyanClinic application shutdown complete");
    await Log.CloseAndFlushAsync();
}
