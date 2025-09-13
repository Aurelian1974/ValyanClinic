using ValyanClinic.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using FluentValidation;
using ValyanClinic.Core.Services;
using ValyanClinic.Core.HealthChecks;
using ValyanClinic.Core.Middleware;
using ValyanClinic.Application.Services;
using ValyanClinic.Components.Pages.LoginPage; // ACTUALIZAT pentru noua structur?
using ValyanClinic.Domain.Models;
using Syncfusion.Blazor;
using System.Text;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// CONFIGURARE ENCODING COMPLET? PENTRU DIACRITICE ROMÂNE?TI
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// Setare cultur? implicit? înainte de orice altceva
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
    // Configur?ri pentru UTF-8
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

// REFACTORING: FluentValidation înlocuie?te Data Annotations - ACTUALIZAT pentru structura organizat?
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>(); // Din LoginPage namespace

// REFACTORING: Rich Services înlocuiesc simple pass-through
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddScoped<ISecurityAuditService, SecurityAuditService>();

// Add existing services
builder.Services.AddScoped<ICacheService, MemoryCacheService>();
builder.Services.AddScoped<IStockMonitoringService, StockMonitoringService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IUserService, UserService>();

// Add background services
builder.Services.AddHostedService<StockMonitoringBackgroundService>();

// Add health checks
builder.Services.AddValyanClinicHealthChecks();

// Add localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

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

// MIDDLEWARE CRITICI PENTRU UTF-8 - PRIMUL MIDDLEWARE
app.Use(async (context, next) =>
{
    // FOR?EAZ? UTF-8 pentru toate r?spunsurile
    context.Response.OnStarting(() =>
    {
        var response = context.Response;
        
        // Seteaz? charset=UTF-8 pentru toate tipurile de con?inut
        if (!response.HasStarted && !string.IsNullOrEmpty(response.ContentType))
        {
            if (!response.ContentType.Contains("charset", StringComparison.OrdinalIgnoreCase))
            {
                response.ContentType += "; charset=UTF-8";
            }
            else if (response.ContentType.Contains("charset", StringComparison.OrdinalIgnoreCase) && 
                     !response.ContentType.Contains("UTF-8", StringComparison.OrdinalIgnoreCase))
            {
                response.ContentType = response.ContentType.Replace("charset=iso-8859-1", "charset=UTF-8", StringComparison.OrdinalIgnoreCase);
                response.ContentType = response.ContentType.Replace("charset=windows-1252", "charset=UTF-8", StringComparison.OrdinalIgnoreCase);
            }
        }
        else if (!response.HasStarted && string.IsNullOrEmpty(response.ContentType))
        {
            response.ContentType = "text/html; charset=UTF-8";
        }
        
        // Adaug? header-e suplimentare pentru UTF-8
        response.Headers.Append("Content-Language", "ro-RO");
        
        return Task.CompletedTask;
    });
    
    await next();
});

// Add global exception handling middleware
app.UseGlobalExceptionHandling();

// Use request localization cu prioritate pentru român?
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("ro-RO"),
    SupportedCultures = new List<CultureInfo> { new CultureInfo("ro-RO"), new CultureInfo("en-US") },
    SupportedUICultures = new List<CultureInfo> { new CultureInfo("ro-RO"), new CultureInfo("en-US") }
});

// Add health checks endpoints
app.UseValyanClinicHealthChecks();

app.UseHttpsRedirection();
app.UseAntiforgery();

// Configure static files cu UTF-8 FOR?AT
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.Name.ToLowerInvariant();
        var response = ctx.Context.Response;
        
        // FOR?EAZ? UTF-8 pentru toate fi?ierele text
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
        
        // Adaug? cache control pentru a evita problemele de cache
        response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
        response.Headers.Append("Pragma", "no-cache");
        response.Headers.Append("Expires", "0");
    }
});

// Map API controllers
app.MapControllers();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
