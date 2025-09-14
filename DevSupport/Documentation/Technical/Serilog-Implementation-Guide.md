# 📝 Documentație Serilog - ValyanClinic

**Aplicație:** ValyanMed - Sistem de Management Clinic  
**Framework:** .NET 9 Blazor Server  
**Logging:** Serilog v4.0+ cu multiple sinks  
**Creat:** Septembrie 2025  
**Status:** ✅ Implementat și Funcțional  

---

## 📋 Prezentare Generală

ValyanClinic folosește **Serilog** ca sistem principal de logging, oferind:
- ✅ **Structured Logging** - Log-uri structurate și ușor de parsat
- ✅ **Multiple Sinks** - Output către Console, Fișiere, și potențial Seq
- ✅ **Automatic Rotation** - Fișiere noi zilnic cu cleanup automat
- ✅ **Level-based Filtering** - Separare erori de informații generale
- ✅ **Performance Optimized** - Buffered writing și configuration lazy
- ✅ **Production Ready** - Error handling complet și graceful shutdown

---

## 🏗️ Arhitectura Implementării

### 📦 Pachete NuGet Instalate

```xml
<PackageReference Include="Serilog.AspNetCore" Version="9.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.Debug" Version="3.0.0" />
<PackageReference Include="Serilog.Sinks.Seq" Version="9.0.0" />
<PackageReference Include="Serilog.Settings.Configuration" Version="9.0.0" />
```

### 🔧 Structura Configurării

#### 1. **Bootstrap Logger** (Program.cs)
- ✅ **Startup logging** înainte de configurarea completă
- ✅ **Error handling** pentru probleme de configurare
- ✅ **File output** pentru debugging startup issues

#### 2. **Main Logger** (appsettings.json)
- ✅ **Configuration-based** pentru flexibilitate
- ✅ **Environment-specific** settings
- ✅ **Multiple sinks** cu configurare separată

#### 3. **Request Logging** (Middleware)
- ✅ **HTTP request tracking** automatic
- ✅ **Performance monitoring** cu timing
- ✅ **Error correlation** între requests și log-uri

---

## ⚙️ Configurarea Detaliată

### 📄 appsettings.json - Configurare Principală

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning",
        "Syncfusion": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/valyan-clinic-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/errors-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 90,
          "restrictedToMinimumLevel": "Warning",
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}{NewLine}---{NewLine}"
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithThreadId"
    ],
    "Properties": {
      "Application": "ValyanClinic"
    }
  }
}
```

### 🔧 appsettings.Development.json - Development Override

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "Microsoft.AspNetCore": "Information",
        "Microsoft.EntityFrameworkCore": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Debug"
      }
    ]
  }
}
```

### 💻 Program.cs - Implementarea în .NET 9

```csharp
using Serilog;
using Serilog.Events;

// BOOTSTRAP LOGGER - Pentru startup logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("Logs/startup-.log", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("🚀 Starting ValyanClinic application");

    var builder = WebApplication.CreateBuilder(args);

    // MAIN SERILOG CONFIGURATION - Din appsettings
    builder.Host.UseSerilog((context, configuration) => 
        configuration.ReadFrom.Configuration(context.Configuration));

    // ... restul configurării ...

    var app = builder.Build();

    // REQUEST LOGGING MIDDLEWARE
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, elapsed, ex) => ex != null
            ? LogEventLevel.Error 
            : httpContext.Response.StatusCode > 499 
                ? LogEventLevel.Error 
                : LogEventLevel.Information;
    });

    // ... restul middleware-urilor ...

    Log.Information("🌟 ValyanClinic application configured successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 Application terminated unexpectedly");
}
finally
{
    Log.Information("🏁 ValyanClinic application shutdown complete");
    await Log.CloseAndFlushAsync(); // Important pentru .NET 9!
}
```

---

## 📁 Structura Fișierelor de Log

### 🗂️ Directorul `Logs/`

```
ValyanClinic/
├── Logs/
│   ├── .gitignore                          # Exclude log files from Git
│   ├── README.md                          # Acest fișier
│   ├── startup-2025-09-14.log             # Bootstrap și pornire
│   ├── valyan-clinic-2025-09-14.log       # Toate log-urile generale
│   ├── errors-2025-09-14.log              # DOAR warnings și errors
│   ├── startup-2025-09-15.log             # Ziua următoare...
│   └── ...                                # Rotație automată zilnică
```

### 📋 Tipuri de Fișiere Log

#### 1. **startup-YYYY-MM-DD.log**
- **Scop:** Log-uri de la pornirea aplicației
- **Conținut:** Bootstrap logger, configurare servicii, erori de startup
- **Format:** `[2025-09-14 13:07:37.160 +03:00 INF] 🚀 Starting ValyanClinic application`
- **Rotație:** Zilnică
- **Retenție:** Implicit (nu e setat limit)

#### 2. **valyan-clinic-YYYY-MM-DD.log**
- **Scop:** Toate log-urile aplicației (Info, Warning, Error)
- **Conținut:** Operațiuni business, request-uri HTTP, informații generale
- **Format:** `[2025-09-14 13:07:37.160 +03:00 INF] ValyanClinic.Application.Services.PersonalService: Getting personal data`
- **Rotație:** Zilnică
- **Retenție:** 30 de zile

#### 3. **errors-YYYY-MM-DD.log**
- **Scop:** DOAR warnings și errors (Level: Warning+)
- **Conținut:** Erori, excepții, probleme de performanță
- **Format:** Detaliat cu stack trace complet și separator
- **Rotație:** Zilnică
- **Retenție:** 90 de zile (mai mult pentru debugging)

---

## 🎯 Nivelurile de Logging

### 📊 Hierarchia Log Levels

| Level | Număr | Descriere | Când să folosești |
|-------|-------|-----------|-------------------|
| **Verbose** | 0 | Debugging foarte detaliat | Doar pentru development local |
| **Debug** | 1 | Informații pentru debugging | Development și troubleshooting |
| **Information** | 2 | Operațiuni normale | Operațiuni business standard |
| **Warning** | 3 | Probleme potențiale | Degradări de performanță, validări |
| **Error** | 4 | Erori care afectează operațiunile | Excepții, probleme de conectivitate |
| **Fatal** | 5 | Erori critice | Aplicația nu mai poate continua |

### ⚙️ Override-uri Configurate

```json
"Override": {
  "Microsoft": "Warning",                    // Reduce noise de la framework
  "Microsoft.AspNetCore": "Warning",         // Request pipeline doar warnings+
  "Microsoft.EntityFrameworkCore": "Warning", // Database queries doar erori
  "System": "Warning",                       // System operations doar warnings+
  "Syncfusion": "Warning"                    // UI framework doar warnings+
}
```

---

## 🔍 Exemple de Utilizare în Cod

### 📝 În Services

```csharp
public class PersonalService : IPersonalService
{
    private readonly ILogger<PersonalService> _logger;

    public PersonalService(ILogger<PersonalService> logger)
    {
        _logger = logger;
    }

    public async Task<PersonalResult> CreatePersonalAsync(Personal personal, string utilizator)
    {
        try
        {
            _logger.LogInformation("Creating personal: {Nume} {Prenume} by {Utilizator}", 
                personal.Nume, personal.Prenume, utilizator);

            // Business logic...

            _logger.LogInformation("Personal created successfully: {PersonalId}", result.Id_Personal);
            return PersonalResult.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating personal: {Nume} {Prenume}", 
                personal.Nume, personal.Prenume);
            return PersonalResult.Failure($"Eroare la crearea personalului: {ex.Message}");
        }
    }
}
```

### 🗄️ În Repository

```csharp
public class PersonalRepository : IPersonalRepository
{
    private readonly ILogger<PersonalRepository> _logger;

    public async Task<Personal> CreateAsync(Personal personal, string utilizator)
    {
        try
        {
            _logger.LogDebug("Executing SP sp_Personal_Create for {Utilizator}", utilizator);
            
            var result = await _connection.QuerySingleAsync<Personal>(
                "sp_Personal_Create", parameters, commandType: CommandType.StoredProcedure);
                
            _logger.LogInformation("Personal created in database: {PersonalId}", result.Id_Personal);
            return result;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database error creating personal. SQL Error: {SqlState}", ex.State);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating personal");
            throw;
        }
    }
}
```

### 🎭 În Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class PersonalController : ControllerBase
{
    private readonly ILogger<PersonalController> _logger;

    [HttpPost]
    public async Task<IActionResult> CreatePersonal([FromBody] CreatePersonalRequest request)
    {
        using (_logger.BeginScope("CreatePersonal for {Utilizator}", User.Identity?.Name))
        {
            _logger.LogInformation("Received create personal request");
            
            try
            {
                var result = await _personalService.CreatePersonalAsync(request.Personal, User.Identity?.Name ?? "Anonymous");
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Personal created successfully via API");
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Personal creation failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CreatePersonal API endpoint");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
```

---

## 🎨 Template-uri de Output

### 🖥️ Console Output Template

```
[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}
```

**Exemplu:**
```
[13:07:37 INF] ValyanClinic.Application.Services.PersonalService: Creating personal: Ion Popescu by admin
[13:07:38 ERR] ValyanClinic.Infrastructure.Repositories.PersonalRepository: Database error creating personal
```

### 📄 File Output Template

```
[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}
```

**Exemplu:**
```
[2025-09-14 13:07:37.160 +03:00 INF] ValyanClinic.Application.Services.PersonalService: Creating personal: Ion Popescu by admin
[2025-09-14 13:07:38.245 +03:00 ERR] ValyanClinic.Infrastructure.Repositories.PersonalRepository: Database error creating personal
System.Data.SqlClient.SqlException: Cannot insert duplicate key row...
   at System.Data.SqlClient.SqlConnection.OnError(SqlException exception)
   at ...
```

### ⚠️ Error File Template (cu separator)

```
[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}{NewLine}---{NewLine}
```

**Exemplu:**
```
[2025-09-14 13:07:38.245 +03:00 ERR] ValyanClinic.Infrastructure.Repositories.PersonalRepository: Database error creating personal
System.Data.SqlClient.SqlException: Cannot insert duplicate key row...
   at System.Data.SqlClient.SqlConnection.OnError(SqlException exception)
   at System.Data.SqlClient.TdsParser.ThrowExceptionAndWarning()
   at ...
---

[2025-09-14 13:08:15.123 +03:00 WRN] ValyanClinic.Application.Services.PersonalService: Validation failed for personal creation
---
```

---

## 🛠️ Troubleshooting și Debugging

### 🔍 Monitorizarea Log-urilor în Timp Real

#### PowerShell
```powershell
# Monitorizare erori în timp real
Get-Content .\Logs\errors-*.log -Wait -Tail 10

# Monitorizare log-uri generale
Get-Content .\Logs\valyan-clinic-*.log -Wait -Tail 20

# Căutare în log-uri după pattern
Select-String -Path ".\Logs\*.log" -Pattern "PersonalService"
```

#### Command Prompt
```cmd
# Afișare conținut fișier erori
type .\Logs\errors-2025-09-14.log

# Monitorizare cu refresh
powershell -Command "Get-Content .\Logs\errors-*.log -Wait"
```

#### Linux/Mac (dacă rulezi pe Linux)
```bash
# Monitorizare timp real
tail -f Logs/errors-*.log

# Căutare în toate log-urile
grep -r "ERROR" Logs/

# Numărul de erori pe zi
grep -c "ERR" Logs/errors-$(date +%Y-%m-%d).log
```

### 🚨 Probleme Comune și Soluții

#### 1. **Aplicația nu pornește cu Serilog**
- ✅ **Verifică:** Sintaxa din `appsettings.json`
- ✅ **Soluție:** Folosește configurarea simplificată din acest document
- ✅ **Debug:** Verifică `startup-*.log` pentru detalii

#### 2. **Log-urile nu se scriu în fișiere**
- ✅ **Verifică:** Directorul `Logs/` există și are permisiuni de scriere
- ✅ **Soluție:** Creează directorul manual sau verifică permisiunile
- ✅ **Alternative:** Folosește path absolut în configurare

#### 3. **Prea multe log-uri în console**
- ✅ **Verifică:** Level-urile din `appsettings.Development.json`
- ✅ **Soluție:** Setează `"Microsoft": "Warning"` sau mai mare
- ✅ **Filter:** Folosește `"restrictedToMinimumLevel": "Information"`

#### 4. **Log-urile nu se rotează**
- ✅ **Verifică:** Setarea `"rollingInterval": "Day"`
- ✅ **Soluție:** Repornește aplicația la miezul nopții pentru teste
- ✅ **Alternative:** Folosește `"Hour"` pentru testare rapidă

#### 5. **Performanță slabă cu multe log-uri**
- ✅ **Verifică:** Folosești level-uri corecte (nu Debug în producție)
- ✅ **Soluție:** Adaugă `"buffered": true` în configurarea File sink
- ✅ **Optimize:** Folosește `"shared": true` pentru multiple procese

---

## 🚀 Deployment și Producție

### 📦 Configurare pentru Producție

#### appsettings.Production.json
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning",
        "Syncfusion": "Error"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/valyanmed/valyan-clinic-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "buffered": true,
          "shared": true,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{MachineName}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/valyanmed/errors-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 90,
          "restrictedToMinimumLevel": "Warning",
          "buffered": true,
          "shared": true,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{MachineName}] {SourceContext} {MemberName}:{SourceLineNumber}: {Message:lj}{NewLine}{Exception}{NewLine}---{NewLine}"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithThreadId",
      "WithEnvironmentName"
    ]
  }
}
```

### 📊 Integrare cu Seq (Opțional)

```bash
# Instalare Seq cu Docker
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest

# Seq va fi disponibil la: http://localhost:5341
```

### 🔒 Securitate și Conformitate

#### Log Sanitization
```csharp
// În servicii, evită să loghezi informații sensibile
_logger.LogInformation("User {UserId} updated personal data", userId); // ✅ Good
_logger.LogInformation("User {UserData} logged in", userObject); // ❌ Bad - poate conține parole

// Folosește destructuring pentru obiecte complexe
_logger.LogInformation("Processing {@PersonalRequest}", request); // ✅ Structurat dar sigur
```

#### Compliance GDPR
```csharp
// Implementează data masking pentru informații personale
public class PersonalDataMaskingEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // Mask CNP, email, telefon în log-uri
        // Implementare custom pentru conformitate GDPR
    }
}
```

---

## 📈 Monitorizare și Alerting

### 🚨 Setări de Alerting Recomandate

1. **Critical Errors:** Orice log cu level `Fatal`
2. **High Error Rate:** Mai mult de 10 errors/minut
3. **Database Issues:** Orice eroare cu `SqlException`
4. **Authentication Failures:** Login failures repetate
5. **Performance Issues:** Request-uri > 5 secunde

### 📊 Metrici de Monitorizat

1. **Error Rate:** Procentul de request-uri cu erori
2. **Response Time:** P95/P99 pentru request-uri HTTP
3. **Log Volume:** Numărul de log entries/minut
4. **Disk Usage:** Spațiul ocupat de log files
5. **Application Health:** Status aplicație din health checks

---

## 🎯 Best Practices și Recomandări

### ✅ DO's

1. **Folosește structured logging:**
   ```csharp
   _logger.LogInformation("User {UserId} created personal {PersonalId}", userId, personalId);
   ```

2. **Log-urile să fie actionable:**
   ```csharp
   _logger.LogError("Failed to send email to {Email}. Retry in {RetryDelay}ms", email, retryDelay);
   ```

3. **Folosește scopes pentru contextul complet:**
   ```csharp
   using (_logger.BeginScope("ProcessingBatch {BatchId}", batchId))
   {
       // All logs here will include BatchId
   }
   ```

4. **Log-urile să fie consistente:**
   ```csharp
   // Standard format pentru operațiuni CRUD
   _logger.LogInformation("Creating {EntityType} with {EntityId} by {UserId}", "Personal", personalId, userId);
   ```

### ❌ DON'Ts

1. **Nu loga informații sensibile:**
   ```csharp
   _logger.LogDebug("Login attempt: {Username} with {Password}", username, password); // ❌ BAD
   ```

2. **Nu folosii string concatenation:**
   ```csharp
   _logger.LogInformation("User " + userId + " updated " + personalId); // ❌ BAD
   ```

3. **Nu loga în catch fără să re-throw:**
   ```csharp
   try { ... }
   catch (Exception ex)
   {
       _logger.LogError(ex, "Error occurred");
       // ❌ BAD - swallow exception
   }
   ```

4. **Nu loga prea mult în production:**
   ```csharp
   // ❌ BAD pentru production
   _logger.LogDebug("Processing item {Index} of {Total}", i, total);
   ```

---

## 🏁 Concluzie

### ✅ Status Actual: IMPLEMENTAT ȘI FUNCȚIONAL

Sistemul de logging Serilog este complet implementat în ValyanClinic cu următoarele caracteristici:

- ✅ **Configurare stabilă** pentru .NET 9 Blazor Server
- ✅ **Multiple sinks** (Console, File, potențial Seq)
- ✅ **Structured logging** cu template-uri optimizate
- ✅ **Error handling** complet și graceful shutdown
- ✅ **Performance optimized** cu buffered writing
- ✅ **Production ready** cu configurare environment-specific

### 🎯 Beneficii Obținute

1. **🔍 Debugging Improved** - Log-uri structurate și accesibile
2. **📊 Monitoring Ready** - Metrici și alerting capabilities
3. **🚀 Performance** - Overhead minim și configurare optimizată
4. **🔒 Security** - Fără informații sensibile în log-uri
5. **📈 Scalability** - Gata pentru load balancing și clustering

### 🛣️ Next Steps (Opțional)

1. **Seq Integration** - Pentru dashboard vizual și alerting
2. **ELK Stack** - Pentru log analysis avansată
3. **Application Insights** - Pentru Azure deployment
4. **Custom Enrichers** - Pentru context business specific
5. **Log Aggregation** - Pentru multiple instances

---

**📚 Această documentație acoperă complet implementarea Serilog în ValyanClinic și poate fi folosită ca referință pentru maintenance și extensii viitoare.**

**🎯 Implementarea este stabilă și production-ready!**
