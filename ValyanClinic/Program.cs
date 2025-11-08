using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
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
using ValyanClinic.Services.Authentication;
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
    // CONTROLLERS (pentru API endpoints)
    // ========================================
    builder.Services.AddControllers();

    // ========================================
    // HTTP CLIENT (pentru apeluri API interne)
    // ========================================
    builder.Services.AddScoped(sp =>
    {
        var navigationManager = sp.GetRequiredService<NavigationManager>();
     return new HttpClient
   {
            BaseAddress = new Uri(navigationManager.BaseUri)
        };
    });

    // ========================================
    // AUTHENTICATION & AUTHORIZATION
    // ========================================
    
    // ASP.NET Core Authentication Services (REQUIRED for AuthorizeRouteView)
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
   .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
   {
   options.Cookie.Name = "ValyanClinic.Auth";
            options.LoginPath = "/login";
  options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
  
   // ✅ PRODUCTION MODE: Session timeout
            // Cookie-ul expire după 30 minute de inactivitate
      options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true; // Resetează timeout-ul la fiecare request
      
     // ✅ Cookie settings - Browser session cookie
       options.Cookie.IsEssential = true;
            options.Cookie.HttpOnly = true; // Nu poate fi accesat din JavaScript
         options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Lax;
     
            // ✅ CRITICAL: MaxAge = null pentru session cookie
// Cookie-ul nu are Max-Age header → browser-ul decide când să-l șteargă
            options.Cookie.MaxAge = null;
        
      // ✅ Events pentru validare și logging
            options.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
         {
                OnSigningIn = context =>
                {
        var logger = context.HttpContext.RequestServices
       .GetRequiredService<ILogger<Program>>();
     
 logger.LogInformation("========== User Login ==========");
       
          if (context.Properties != null)
      {
           // ✅ Force session-only cookie
 context.Properties.IsPersistent = false;
           context.Properties.ExpiresUtc = null;
             context.Properties.AllowRefresh = true; // Allow sliding expiration
         
           logger.LogInformation("Session Properties:");
    logger.LogInformation("  IsPersistent: {IsPersistent}", context.Properties.IsPersistent);
        logger.LogInformation("  ExpiresUtc: {ExpiresUtc}", context.Properties.ExpiresUtc?.ToString() ?? "NULL");
        logger.LogInformation("  AllowRefresh: {AllowRefresh}", context.Properties.AllowRefresh);
                 }
     
   logger.LogInformation("================================");
                    return Task.CompletedTask;
 },
    OnValidatePrincipal = async context =>
      {
       var logger = context.HttpContext.RequestServices
      .GetRequiredService<ILogger<Program>>();
     
      // Verificare expirare session (30 minute cu sliding expiration)
      if (context.Properties?.ExpiresUtc.HasValue == true)
  {
             if (context.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow)
 {
    logger.LogWarning("Session EXPIRED! Forcing logout...");
   context.RejectPrincipal();
         await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
          }
 }
     }
    };
        });
    
    // Authorization Services
    builder.Services.AddAuthorizationCore();
    
    // Blazor Authentication State Provider
    builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
    builder.Services.AddScoped<CustomAuthenticationStateProvider>(sp => 
        (CustomAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
    
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
  builder.Services.AddScoped<IUtilizatorRepository, UtilizatorRepository>();
    builder.Services.AddScoped<IProgramareRepository, ProgramareRepository>(); // ✅ NOU - Programari
    
    // Phase1 Settings Repositories
    builder.Services.AddScoped<ISystemSettingsRepository, ValyanClinic.Infrastructure.Repositories.Settings.SystemSettingsRepository>();
    builder.Services.AddScoped<IAuditLogRepository, ValyanClinic.Infrastructure.Repositories.Settings.AuditLogRepository>();
    builder.Services.AddScoped<IUserSessionRepository, ValyanClinic.Infrastructure.Repositories.Settings.UserSessionRepository>();

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
    builder.Services.AddScoped<ValyanClinic.Domain.Interfaces.Security.IPasswordHasher>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<ValyanClinic.Infrastructure.Security.BCryptPasswordHasher>>();
        return new ValyanClinic.Infrastructure.Security.BCryptPasswordHasher(logger);
    });
    // ========================================
    // NOTIFICATION SERVICES
    // ========================================
    builder.Services.AddScoped<ValyanClinic.Services.INotificationService, ValyanClinic.Services.NotificationService>();

    // ========================================
    // EXPORT SERVICES
    // ========================================
    builder.Services.AddScoped<ValyanClinic.Services.Export.IExcelExportService, ValyanClinic.Services.Export.ExcelExportService>();

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
  opt.AddHealthCheckEndpoint("ValyanClinic API", "/health-json"); // ✅ CORECTAT: endpoint-ul JSON pentru UI
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
    
    // Authentication & Authorization Middleware (REQUIRED)
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.UseAntiforgery();

    // ========================================
    // HEALTH CHECKS
    // ========================================
    // Endpoint principal pentru health check (HTML)
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Endpoint JSON pentru API
    app.MapHealthChecks("/health-json", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Health Checks UI Dashboard
    app.MapHealthChecksUI(config =>
    {
      config.UIPath = "/health-ui";
    });

    // ========================================
    // API CONTROLLERS (TREBUIE SĂ FIE ÎNAINTEA BLAZOR)
    // ========================================
    app.MapControllers();

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
