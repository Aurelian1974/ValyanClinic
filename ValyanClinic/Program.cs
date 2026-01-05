using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Localization;
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
using ValyanClinic.Services;
using ValyanClinic.Application.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using MediatR;
using FluentValidation;

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
    // WINDOWS SERVICE SUPPORT (Production ONLY)
    // ========================================
    // ✅ Permite aplicației să ruleze ca Windows Service DOAR în Production
    // În Development (Visual Studio), rulează normal
    if (!builder.Environment.IsDevelopment())
    {
        builder.Host.UseWindowsService();
    }

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
    // AUTHENTICATION & AUTHORIZATION - Cookie Configuration
    // ========================================

    // ASP.NET Core Authentication Services (REQUIRED for AuthorizeRouteView)
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = "ValyanClinic.Auth";
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/access-denied";

            // ✅ SESSION COOKIE - Simplu și eficient
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true; // ✅ SCHIMBAT: True pentru UX mai bun

            // ✅ Cookie settings
            options.Cookie.IsEssential = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.MaxAge = null; // Session cookie - se șterge când închizi browser-ul

            // ✅ Events simplificate - doar validare esențială
            options.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = async context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    // Verificare simplă - cookie valid?
                    if (context.Principal?.Identity?.IsAuthenticated != true)
                    {
                        logger.LogWarning("❌ Principal invalid - reject");
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    }
                    else
                    {
                        logger.LogDebug("✅ Principal valid: {Name}", context.Principal.Identity.Name);
                    }
                }
            };
        });

    // ========================================
    // AUTHORIZATION - Policy-Based Authorization
    // ========================================
    // Înlocuiește AddAuthorizationCore() simplu cu configurația noastră
    builder.Services.AddValyanClinicAuthorization();

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

    // Add Syncfusion Blazor service
    builder.Services.AddSyncfusionBlazor();

    // Configure localization for Romanian
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
    builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        var supportedCultures = new[] { "ro-RO", "ro" };
        options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("ro-RO");
        options.SupportedCultures = supportedCultures.Select(c => new System.Globalization.CultureInfo(c)).ToList();
        options.SupportedUICultures = supportedCultures.Select(c => new System.Globalization.CultureInfo(c)).ToList();
    });

    // ========================================
    // DATABASE - Connection Pooling Configuration OPTIMIZED
    // ========================================
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    // CRITICAL: Configurare optimizată connection pooling pentru Blazor Server
    var connectionStringBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString)
    {
        // Connection Pooling - OPTIMIZED pentru startup rapid
        Pooling = true,
        MinPoolSize = 0,  // ✅ CHANGED: 0 pentru startup rapid (conexiuni create on-demand)
        MaxPoolSize = 50, // ✅ CHANGED: 50 suficient pentru clinică mică-medie (era 100)

        // Timeouts - OPTIMIZED
        ConnectTimeout = 15, // ✅ CHANGED: 15 secunde (era 30)
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
    builder.Services.AddScoped<IProgramareRepository, ProgramareRepository>();
    builder.Services.AddScoped<ValyanClinic.Infrastructure.Repositories.Interfaces.IConsultatieRepository,
                                ValyanClinic.Infrastructure.Repositories.ConsultatieRepository>(); // ✅ NOU - Consultatii
    builder.Services.AddScoped<ValyanClinic.Domain.Interfaces.Repositories.IICD10Repository, 
                                ValyanClinic.Infrastructure.Repositories.ICD10Repository>(); // ✅ NOU - ICD10 Autocomplete
    builder.Services.AddScoped<IPacientPersonalMedicalRepository, PacientPersonalMedicalRepository>(); // ✅ NOU - Relații Pacient-Doctor
    // ✅ NOU - Analize Medicale (Nomenclator + Consultație)
    builder.Services.AddScoped<IAnalizaMedicalaRepository, AnalizaMedicalaRepository>();
    builder.Services.AddScoped<IConsultatieAnalizaMedicalaRepository, ConsultatieAnalizaMedicalaRepository>();
    builder.Services.AddScoped<IRolRepository, RolRepository>(); // ✅ NOU - Administrare Roluri și Permisiuni
    builder.Services.AddScoped<ValyanClinic.Application.Interfaces.IFieldPermissionService, 
                                ValyanClinic.Application.Services.FieldPermissionService>(); // ✅ NOU - Permisiuni granulare la nivel de câmp

    // Phase1 Settings Repositories
    builder.Services.AddScoped<ISystemSettingsRepository, ValyanClinic.Infrastructure.Repositories.Settings.SystemSettingsRepository>();
    builder.Services.AddScoped<IAuditLogRepository, ValyanClinic.Infrastructure.Repositories.Settings.AuditLogRepository>();
    builder.Services.AddScoped<IUserSessionRepository, ValyanClinic.Infrastructure.Repositories.Settings.UserSessionRepository>();

    // ========================================
    // NAVIGATION SERVICES
    // ========================================
    builder.Services.AddScoped<IConsultatieNavigationService, ConsultatieNavigationService>();

    // ========================================
    // CACHING
    // ========================================
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
    builder.Services.AddScoped<IQueryCacheService, QueryCacheService>();

    // ========================================
    // MEDIATR (CQRS) - OPTIMIZED
    // ========================================
    builder.Services.AddMediatR(cfg =>
    {
        // ✅ OPTIMIZED: Scanează doar assembly-urile necesare
        cfg.RegisterServicesFromAssemblyContaining<ValyanClinic.Application.Common.Results.Result>();
        // Nu mai scanăm toate assembly-urile - doar Application layer unde sunt handlers
    });

    // ========================================
    // FLUENTVALIDATION - Automatic validation for MediatR Commands/Queries
    // ========================================
    // ✅ Scanează toate validators din Application layer și le înregistrează automat
    builder.Services.AddValidatorsFromAssemblyContaining<ValyanClinic.Application.Common.Results.Result>();
    
    // ✅ IMPORTANT: PipelineBehavior pentru validare automată
    // Când trimiți un Command/Query prin MediatR, validatorii vor rula automat ÎNAINTE de handler
    // Dacă validarea eșuează, va arunca ValidationException cu toate erorile
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValyanClinic.Application.Common.Behaviors.ValidationBehavior<,>));

    // ========================================
    // AUTOMAPPER - OPTIMIZED
    // ========================================
    // ✅ OPTIMIZED: Scanează doar assembly-urile cu profiluri AutoMapper
    builder.Services.AddAutoMapper(cfg => {}, 
        typeof(ValyanClinic.Application.Common.Results.Result).Assembly // Application layer
                                                                        // Adaugă aici alte assembly-uri cu profiluri AutoMapper dacă sunt necesare
    );

    // ========================================
    // LAYOUT SERVICES
    // ========================================
    builder.Services.AddScoped<BreadcrumbService>();

    // ========================================
    // EXPORT SERVICES
    // ========================================
    // Service to export PersonalMedical lists to CSV/Excel
    builder.Services.AddScoped<ValyanClinic.Application.Services.Export.IPersonalMedicalExportService, ValyanClinic.Application.Services.Export.PersonalMedicalExportService>();

    // ========================================
    // SIGNALR: Realtime notifications
    // ========================================
    builder.Services.AddSignalR();

    // Application-level notifier implementations (uses IHubContext)
    builder.Services.AddScoped<ValyanClinic.Application.Interfaces.IPersonalMedicalNotifier, ValyanClinic.Services.SignalR.PersonalMedicalNotifier>();
    builder.Services.AddScoped<ValyanClinic.Application.Interfaces.IPacientNotifier, ValyanClinic.Services.SignalR.PacientNotifier>();

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
    // PHOTON API (Street Autocomplete)
    // ========================================
    // Serviciu pentru autocomplete de străzi folosind Photon API (OpenStreetMap)
    builder.Services.AddHttpClient<ValyanClinic.Application.Services.Location.IPhotonService, ValyanClinic.Application.Services.Location.PhotonService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
        client.DefaultRequestHeaders.Add("User-Agent", "ValyanClinic/1.0");
    });

    // ========================================
    // NOMENCLATOR MEDICAMENTE ANM
    // ========================================
    // Configurare sincronizare automată nomenclator
    builder.Services.Configure<ValyanClinic.Application.Services.Medicamente.NomenclatorSyncOptions>(
        builder.Configuration.GetSection(ValyanClinic.Application.Services.Medicamente.NomenclatorSyncOptions.SectionName));
    
    // Serviciu pentru căutare și sincronizare medicamente
    builder.Services.AddHttpClient<ValyanClinic.Application.Services.Medicamente.INomenclatorMedicamenteService, 
        ValyanClinic.Application.Services.Medicamente.NomenclatorMedicamenteService>(client =>
    {
        client.Timeout = TimeSpan.FromMinutes(5); // Download poate dura
        client.DefaultRequestHeaders.Add("User-Agent", "ValyanClinic/1.0");
    });
    
    // Background service pentru sincronizare automată săptămânală
    builder.Services.AddHostedService<ValyanClinic.Application.Services.Medicamente.NomenclatorSyncBackgroundService>();

    // ========================================
    // NOTIFICATION SERVICES
    // ========================================
    builder.Services.AddScoped<ValyanClinic.Services.INotificationService, ValyanClinic.Services.NotificationService>();
    builder.Services.AddScoped<ValyanClinic.Services.ToastService>(); // Toast notifications
    builder.Services.AddScoped<ValyanClinic.Services.INavigationGuardService, ValyanClinic.Services.NavigationGuardService>(); // ✅ ADDED: Navigation guard for unsaved changes

    // ========================================
    // EMAIL SERVICES
    // ========================================
    builder.Services.AddScoped<ValyanClinic.Services.Email.IEmailService, ValyanClinic.Services.Email.EmailService>();

    // ========================================
    // SMS SERVICES (MOCK MODE - Production Ready Infrastructure)
    // ========================================
    // ✅ CURRENT: MockSmsService pentru testare UI fără cost
    // ⏳ FUTURE: Când ai buget, switch la TwilioSmsService sau alt provider
    // 
    // Setup Production (2 min):
    // 1. dotnet add package Twilio (sau Vonage/SMS-Gateway.ro SDK)
    // 2. dotnet user-secrets set "TwilioSettings:AccountSid" "ACxxxx"
    // 3. dotnet user-secrets set "TwilioSettings:AuthToken" "xxxx"
    // 4. dotnet user-secrets set "TwilioSettings:PhoneNumber" "+1xxxx"
    // 5. dotnet user-secrets set "TwilioSettings:Enabled" "true"
    // 6. Uncomment TwilioSmsService registration și comment MockSmsService
    //
    // Estimare cost Twilio: $15 FREE credit = 450 SMS-uri
    // După trial: $0.025/SMS în România
    builder.Services.AddScoped<ValyanClinic.Services.Sms.ISmsService, ValyanClinic.Services.Sms.MockSmsService>();

    // ⏳ TODO când ai buget: Uncomment această linie și comment linia de mai sus
    // var smsEnabled = builder.Configuration["TwilioSettings:Enabled"] == "true";
    // if (smsEnabled)
    // {
    //     builder.Services.AddScoped<ValyanClinic.Services.Sms.ISmsService, ValyanClinic.Services.Sms.TwilioSmsService>();
    // }
    // else
    // {
    //     builder.Services.AddScoped<ValyanClinic.Services.Sms.ISmsService, ValyanClinic.Services.Sms.MockSmsService>();
    // }

    // ========================================
    // EXPORT SERVICES
    // ========================================
    builder.Services.AddScoped<ValyanClinic.Services.Export.IExcelExportService, ValyanClinic.Services.Export.ExcelExportService>();

    // ========================================
    // BUSINESS SERVICES
    // ========================================
    builder.Services.AddScoped<ValyanClinic.Application.Services.IPersonalBusinessService,
                                ValyanClinic.Application.Services.PersonalBusinessService>();

    // ✅ NEW: PacientDataService - Business logic for patient list management
    builder.Services.AddScoped<ValyanClinic.Application.Services.Pacienti.IPacientDataService,
                                ValyanClinic.Application.Services.Pacienti.PacientDataService>();

    // ✅ JudeteService - Centralized service for loading Judete/Localitati from database with caching
    builder.Services.AddScoped<ValyanClinic.Application.Services.Location.IJudeteService,
                                ValyanClinic.Application.Services.Location.JudeteService>();

    // ========================================
    // IMC CALCULATOR SERVICE - Medical Services
    // ========================================
    builder.Services.AddScoped<ValyanClinic.Application.Services.IMC.IIMCCalculatorService,
                                ValyanClinic.Application.Services.IMC.IMCCalculatorService>();

    // ========================================
    // CONSULTATION SERVICES - Timer & Progress
    // ========================================
    // ✅ Timer service pentru măsurarea duratei consultațiilor
    builder.Services.AddScoped<ValyanClinic.Application.Services.Consultatii.IConsultationTimerService,
                                ValyanClinic.Application.Services.Consultatii.ConsultationTimerService>();

    // ✅ Form progress service pentru calculul completării formularelor
    builder.Services.AddScoped<ValyanClinic.Application.Services.Consultatii.IFormProgressService,
                                ValyanClinic.Application.Services.Consultatii.FormProgressService>();

    // ========================================
    // SCRISOARE MEDICALA SERVICE - Document Generation
    // ========================================
    builder.Services.AddScoped<ValyanClinic.Application.Services.ScrisoareMedicala.IScrisoareMedicalaService,
                                ValyanClinic.Application.Services.ScrisoareMedicala.ScrisoareMedicalaService>();

    // ========================================
    // DRAFT STORAGE SERVICE - LocalStorage Management
    // ========================================
    builder.Services.AddScoped(typeof(ValyanClinic.Infrastructure.Services.DraftStorage.IDraftStorageService<>),
                                typeof(ValyanClinic.Infrastructure.Services.DraftStorage.LocalStorageDraftService<>));

    // ========================================
    // DRAFT AUTO-SAVE HELPER - Blazor Timer Management (Hybrid Approach)
    // ========================================
    builder.Services.AddScoped(typeof(ValyanClinic.Application.Services.Draft.DraftAutoSaveHelper<>));

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
     timeout: TimeSpan.FromSeconds(1)); // ✅ CHANGED: 1 secund (era 3) pentru startup mai rapid

    // ⚠️ DEZACTIVAT TEMPORAR: HealthCheckUI are probleme cu IdentityModel
    // builder.Services.AddHealthChecksUI(opt =>
    //   {
    //       opt.SetEvaluationTimeInSeconds(60);
    //       opt.MaximumHistoryEntriesPerEndpoint(50);
    //       opt.AddHealthCheckEndpoint("ValyanClinic API", "/health-json");
    //   })
    //   .AddInMemoryStorage();

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

    // Localization
    app.UseRequestLocalization();

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

    // ⚠️ DEZACTIVAT TEMPORAR: Health Checks UI Dashboard
    // app.MapHealthChecksUI(config =>
    // {
    //     config.UIPath = "/health-ui";
    // });

    // ========================================
    // API CONTROLLERS (TREBUIE SĂ FIE ÎNAINTEA BLAZOR)
    // ========================================
    app.MapControllers();

    // ========================================
    // SIGNALR HUBS
    // ========================================
    app.MapHub<ValyanClinic.Hubs.PersonalMedicalHub>("/personalmedicalHub");
    app.MapHub<ValyanClinic.Hubs.PacientHub>("/pacientHub");

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
