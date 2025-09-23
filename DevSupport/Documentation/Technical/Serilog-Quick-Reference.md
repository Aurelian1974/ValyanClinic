# 🚀 Quick Reference - Serilog in ValyanClinic

**📋 Ghid rapid pentru dezvoltatori**

---

## ⚡ Utilizare Rapida

### 📝 in Services
```csharp
private readonly ILogger<PersonalService> _logger;

// ✅ GOOD - Structured logging
_logger.LogInformation("Creating personal: {Nume} {Prenume} by {User}", 
    personal.Nume, personal.Prenume, utilizator);

// ✅ GOOD - Error cu context
_logger.LogError(ex, "Failed to create personal {PersonalId}", personalId);

// ✅ GOOD - Warning cu detalii
_logger.LogWarning("Validation failed for {Operation}: {Errors}", 
    "CreatePersonal", string.Join(", ", errors));
```

### 🗄️ in Repository
```csharp
// ✅ Debug pentru SQL operations
_logger.LogDebug("Executing {StoredProcedure} with {ParameterCount} parameters", 
    "sp_Personal_Create", parameters.Count);

// ✅ Performance monitoring
var stopwatch = Stopwatch.StartNew();
var result = await _connection.QueryAsync<Personal>(sql);
_logger.LogInformation("Query completed in {ElapsedMs}ms, returned {Count} records", 
    stopwatch.ElapsedMilliseconds, result.Count());
```

### 🎯 Scopes pentru Context
```csharp
using (_logger.BeginScope("ProcessingBatch {BatchId}", batchId))
{
    _logger.LogInformation("Starting batch processing");
    // Toate log-urile vor include BatchId automat
    _logger.LogInformation("Processing item {Index}", i);
}
```

---

## 🔍 Log Levels - Cand sa Folosesti

| Level | Cand | Exemple |
|-------|------|---------|
| **Debug** | Development, debugging detaliat | SQL queries, parametri, flow control |
| **Information** | Operatiuni normale business | "User created", "Email sent", "Data exported" |
| **Warning** | Probleme minore, degradari | "Slow query", "Validation failed", "Cache miss" |
| **Error** | Erori care afecteaza operatiunile | Exceptii, database errors, API failures |
| **Fatal** | Aplicatia nu mai poate continua | Startup failures, critical system errors |

---

## 📁 Fisiere Log Generate

```
Logs/
├── startup-2025-09-14.log          # Bootstrap si startup
├── valyan-clinic-2025-09-14.log    # Toate log-urile (Info+)
└── errors-2025-09-14.log           # Doar Warning si Error
```

---

## 🛠️ Monitoring in Timp Real

```powershell
# Monitorizare erori
Get-Content .\Logs\errors-*.log -Wait -Tail 10

# Cautare specifica
Select-String -Path ".\Logs\*.log" -Pattern "PersonalService"
```

---

## ❌ Common Mistakes

```csharp
// ❌ BAD - String concatenation
_logger.LogInformation("User " + userId + " did something");

// ✅ GOOD - Structured
_logger.LogInformation("User {UserId} performed {Action}", userId, action);

// ❌ BAD - Sensitive data
_logger.LogDebug("Login: {Username} {Password}", user, pass);

// ✅ GOOD - Safe logging
_logger.LogInformation("Login attempt for {Username}", username);

// ❌ BAD - Swallow exceptions
catch (Exception ex) { _logger.LogError(ex, "Error"); }

// ✅ GOOD - Log and re-throw
catch (Exception ex) { _logger.LogError(ex, "Error in {Method}", nameof(SomeMethod)); throw; }
```

---

## 🎯 Template-uri Utile

```csharp
// CRUD Operations
_logger.LogInformation("Creating {EntityType} for {UserId}", "Personal", userId);
_logger.LogInformation("Updated {EntityType} {EntityId}", "Personal", personalId);
_logger.LogWarning("Failed to delete {EntityType} {EntityId}: {Reason}", "Personal", id, reason);

// Performance
_logger.LogInformation("Operation {OperationName} completed in {ElapsedMs}ms", operation, elapsed);
_logger.LogWarning("Slow operation detected: {OperationName} took {ElapsedMs}ms", operation, elapsed);

// Validation
_logger.LogWarning("Validation failed for {EntityType}: {ValidationErrors}", "Personal", errors);
_logger.LogError("Business rule violation in {Operation}: {Rule}", operation, rule);

// External Services  
_logger.LogInformation("Calling external API: {ApiName} {Endpoint}", apiName, endpoint);
_logger.LogError("External API failed: {ApiName} returned {StatusCode}", apiName, statusCode);
```

---

**🎯 Keep it simple, structured, and actionable!**
