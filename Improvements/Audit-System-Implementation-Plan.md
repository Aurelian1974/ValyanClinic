# Improvements - Sistem de Auditare pentru ValyanClinic

**Creat:** Septembrie 2025  
**Status:** Planificat pentru implementare  
**Prioritate:** Medie-Ridicata  
**Tehnologii:** .NET 9, Blazor Server, Dapper  

---

## Prezentare Generala

Acest document descrie planul de implementare pentru sistemul de auditare al aplicatiei ValyanClinic. in loc de triggeri de baza de date, vom implementa un sistem modern de auditare la nivel de aplicatie cu multiple strategii si functionalitati avansate.

## Context Actual

in prezent, tabela `Personal` si alte tabele au campuri de auditare de baza:
- `Data_Crearii`
- `Data_Ultimei_Modificari` 
- `Creat_De`
- `Modificat_De`

Dar nu avem un sistem complet de auditare implementat.

---

## Strategii de Implementare

### 1. 🔥 Interceptors si Decorators (PRIORITATE iNALTa)

#### Descriere
Implementarea unui service pentru auditare centralizata care intercepteaza toate operatiunile CRUD.

#### Componente
```csharp
// ValyanClinic.Application/Services/IAuditService.cs
public interface IAuditService
{
    Task LogOperationAsync<T>(string operation, T entity, string userId, string details = null);
    Task LogOperationAsync<T>(string operation, T oldEntity, T newEntity, string userId, string details = null);
    Task<List<AuditEntry>> GetAuditHistoryAsync(string tableName, string entityId);
    Task<List<AuditEntry>> GetUserActivityAsync(string userId, DateTime from, DateTime to);
}

// ValyanClinic.Application/Services/AuditService.cs
public class AuditService : IAuditService
{
    private readonly IDbConnection _connection;
    private readonly ILogger<AuditService> _logger;
    private readonly ICurrentUserService _currentUser;
    
    // Implementare detaliata pentru toate metodele
}
```

#### Beneficii
- ✅ Control complet in aplicatie
- ✅ Flexibilitate maxima in configurare
- ✅ Integrare cu sistemul de logging existent
- ✅ Testare usoara

---

### 2. 🔧 Repository Pattern cu Auditare Automata

#### Descriere
Extinderea repository pattern-ului existent pentru a include auditare automata in toate operatiunile.

#### Exemplu de Implementare
```csharp
// ValyanClinic.Infrastructure/Repositories/BaseAuditableRepository.cs
public abstract class BaseAuditableRepository<T> where T : IAuditableEntity
{
    protected readonly IDbConnection _connection;
    protected readonly IAuditService _auditService;
    protected readonly ICurrentUserService _currentUser;
    
    protected async Task<T> CreateWithAuditAsync(T entity, string sql)
    {
        // Auditare inainte de creeare
        await _auditService.LogOperationAsync("CREATE_BEFORE", entity, _currentUser.UserId);
        
        // Setare metadate de auditare
        entity.Data_Crearii = DateTime.UtcNow;
        entity.Creat_De = _currentUser.Username;
        entity.Data_Ultimei_Modificari = DateTime.UtcNow;
        entity.Modificat_De = _currentUser.Username;
        
        // Executare operatiune
        await _connection.ExecuteAsync(sql, entity);
        
        // Auditare dupa creare
        await _auditService.LogOperationAsync("CREATE_AFTER", entity, _currentUser.UserId);
        
        return entity;
    }
    
    protected async Task<T> UpdateWithAuditAsync(T oldEntity, T newEntity, string sql)
    {
        // Auditare inainte de update cu comparatie
        await _auditService.LogOperationAsync("UPDATE_BEFORE", oldEntity, newEntity, _currentUser.UserId);
        
        // Setare metadate de auditare
        newEntity.Data_Ultimei_Modificari = DateTime.UtcNow;
        newEntity.Modificat_De = _currentUser.Username;
        
        // Executare operatiune
        await _connection.ExecuteAsync(sql, newEntity);
        
        // Auditare dupa update
        await _auditService.LogOperationAsync("UPDATE_AFTER", newEntity, _currentUser.UserId);
        
        return newEntity;
    }
}

// ValyanClinic.Infrastructure/Repositories/PersonalRepository.cs
public class PersonalRepository : BaseAuditableRepository<Personal>, IPersonalRepository
{
    public async Task<Personal> CreateAsync(Personal personal)
    {
        const string sql = @"INSERT INTO Personal (...) VALUES (...)";
        return await CreateWithAuditAsync(personal, sql);
    }
    
    public async Task<Personal> UpdateAsync(Personal personal)
    {
        var oldPersonal = await GetByIdAsync(personal.Id_Personal);
        const string sql = @"UPDATE Personal SET ... WHERE Id_Personal = @Id_Personal";
        return await UpdateWithAuditAsync(oldPersonal, personal, sql);
    }
}
```

---

### 3. 📊 Aspect-Oriented Programming (AOP)

#### Descriere
Folosirea atributelor pentru marcarea metodelor care necesita auditare automata.

#### Implementare
```csharp
// ValyanClinic.Application/Attributes/AuditableAttribute.cs
[AttributeUsage(AttributeTargets.Method)]
public class AuditableAttribute : Attribute
{
    public string Operation { get; set; }
    public bool LogBefore { get; set; } = true;
    public bool LogAfter { get; set; } = true;
    public bool CompareEntities { get; set; } = false;
}

// Usage in repository
[Auditable(Operation = "CREATE_PERSONAL")]
public async Task<Personal> CreateAsync(Personal personal) { ... }

[Auditable(Operation = "UPDATE_PERSONAL", CompareEntities = true)]
public async Task<Personal> UpdateAsync(Personal personal) { ... }

[Auditable(Operation = "DELETE_PERSONAL")]
public async Task DeleteAsync(Guid id) { ... }
```

#### Interceptor Implementation
```csharp
// ValyanClinic.Application/Interceptors/AuditInterceptor.cs
public class AuditInterceptor : IInterceptor
{
    public async Task InterceptAsync(MethodInfo method, object[] args, Func<Task<object>> next)
    {
        var auditAttribute = method.GetCustomAttribute<AuditableAttribute>();
        if (auditAttribute != null)
        {
            // Logica de auditare inainte
            if (auditAttribute.LogBefore)
            {
                await LogBeforeOperation(auditAttribute.Operation, args);
            }
            
            var result = await next();
            
            // Logica de auditare dupa
            if (auditAttribute.LogAfter)
            {
                await LogAfterOperation(auditAttribute.Operation, args, result);
            }
            
            return result;
        }
        
        return await next();
    }
}
```

---

### 4. 🎪 Domain Events Pattern

#### Descriere
Implementarea unui sistem de evenimente pentru auditare bazat pe Domain Events.

#### Componente
```csharp
// ValyanClinic.Domain/Events/IDomainEvent.cs
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
}

// ValyanClinic.Domain/Events/PersonalEvents.cs
public class PersonalCreatedEvent : IDomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "PersonalCreated";
    public Personal Personal { get; set; }
    public string UserId { get; set; }
}

public class PersonalUpdatedEvent : IDomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "PersonalUpdated";
    public Personal OldPersonal { get; set; }
    public Personal NewPersonal { get; set; }
    public string UserId { get; set; }
    public List<string> ChangedFields { get; set; }
}

// ValyanClinic.Application/Handlers/AuditEventHandlers.cs
public class PersonalAuditEventHandler : 
    INotificationHandler<PersonalCreatedEvent>,
    INotificationHandler<PersonalUpdatedEvent>
{
    public async Task Handle(PersonalCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _auditService.LogDomainEventAsync(notification);
    }
    
    public async Task Handle(PersonalUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await _auditService.LogDomainEventAsync(notification);
        await _auditService.LogFieldChangesAsync(notification.ChangedFields, notification.UserId);
    }
}
```

---

### 5. 🔄 Blazor Server Lifecycle Hooks

#### Descriere
Integrarea auditarii direct in componentele Blazor pentru tracking complet al actiunilor utilizatorului.

#### Implementare
```csharp
// ValyanClinic.Components/Base/AuditableComponentBase.cs
public abstract class AuditableComponentBase : ComponentBase
{
    [Inject] protected IAuditService AuditService { get; set; }
    [Inject] protected ICurrentUserService CurrentUser { get; set; }
    
    protected async Task<T> ExecuteWithAuditAsync<T>(
        string operation, 
        Func<Task<T>> action, 
        object entity = null)
    {
        try
        {
            // Auditare incercare operatiune
            await AuditService.LogOperationAsync($"{operation}_ATTEMPT", entity, CurrentUser.UserId);
            
            var result = await action();
            
            // Auditare succes
            await AuditService.LogOperationAsync($"{operation}_SUCCESS", result, CurrentUser.UserId);
            
            return result;
        }
        catch (Exception ex)
        {
            // Auditare eroare
            await AuditService.LogOperationAsync($"{operation}_ERROR", entity, CurrentUser.UserId, ex.Message);
            throw;
        }
    }
}

// ValyanClinic.Components/Pages/Personal/EditPersonal.razor.cs
public partial class EditPersonal : AuditableComponentBase
{
    private async Task SavePersonal()
    {
        var result = await ExecuteWithAuditAsync(
            "EDIT_PERSONAL", 
            () => PersonalService.UpdateAsync(Personal),
            Personal
        );
        
        // UI updates
        StateHasChanged();
        ShowSuccessMessage("Personal actualizat cu succes!");
    }
}
```

---

## Tabela de Auditare

### Schema Bazei de Date
```sql
-- Tabel principal pentru log-urile de auditare
CREATE TABLE AuditLog (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    
    -- Identificarea operatiunii
    TableName NVARCHAR(50) NOT NULL,
    Operation NVARCHAR(20) NOT NULL, -- CREATE, UPDATE, DELETE, VIEW, EXPORT
    EntityId NVARCHAR(50), -- ID-ul entitatii modificate
    
    -- Datele modificarii
    OldValues NVARCHAR(MAX), -- JSON cu valorile vechi
    NewValues NVARCHAR(MAX), -- JSON cu valorile noi
    ChangedFields NVARCHAR(500), -- Lista campurilor modificate
    
    -- Informatii utilizator
    UserId NVARCHAR(50) NOT NULL,
    UserName NVARCHAR(100),
    UserRole NVARCHAR(50),
    
    -- Metadata
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Details NVARCHAR(500), -- Detalii suplimentare
    
    -- Context tehnic
    IpAddress NVARCHAR(45),
    UserAgent NVARCHAR(500),
    SessionId NVARCHAR(100),
    
    -- Pentru correlation
    CorrelationId UNIQUEIDENTIFIER, -- Pentru a grupa operatiuni multiple
    ParentAuditId UNIQUEIDENTIFIER -- Pentru operatiuni in cascada
);

-- Indexuri pentru performanta
CREATE INDEX IX_AuditLog_TableName ON AuditLog(TableName);
CREATE INDEX IX_AuditLog_Operation ON AuditLog(Operation);
CREATE INDEX IX_AuditLog_UserId ON AuditLog(UserId);
CREATE INDEX IX_AuditLog_Timestamp ON AuditLog(Timestamp);
CREATE INDEX IX_AuditLog_EntityId ON AuditLog(EntityId);
CREATE INDEX IX_AuditLog_CorrelationId ON AuditLog(CorrelationId) WHERE CorrelationId IS NOT NULL;

-- Tabel pentru tracking-ul sesiunilor utilizatorilor
CREATE TABLE UserSessions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    UserId NVARCHAR(50) NOT NULL,
    SessionId NVARCHAR(100) NOT NULL,
    StartTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    EndTime DATETIME2,
    IpAddress NVARCHAR(45),
    UserAgent NVARCHAR(500),
    IsActive BIT NOT NULL DEFAULT 1,
    LastActivity DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_UserSessions_UserId ON UserSessions(UserId);
CREATE INDEX IX_UserSessions_SessionId ON UserSessions(SessionId);
CREATE INDEX IX_UserSessions_IsActive ON UserSessions(IsActive);
```

---

## Functionalitati Avansate de Implementat

### 1. 📊 Dashboard de Auditare
- **Vizualizare in timp real** a activitatii utilizatorilor
- **Statistici** pe operatiuni si utilizatori
- **Alerte** pentru activitati suspicioase
- **Exporturi** pentru compliance si raportare

### 2. 🔍 Cautare si Filtrare Avansata
- **Cautare pe perioada de timp**
- **Filtrare dupa utilizator, operatiune, tabel**
- **Comparatia versiunilor** unei inregistrari
- **Timeline** cu toate modificarile unei entitati

### 3. 🚨 Alerting si Monitoring
- **Alerturi in timp real** pentru operatiuni critice
- **Threshold monitoring** pentru volume mari de operatiuni
- **Integration cu sistemele de monitoring** existente
- **Notificari** catre administratori

### 4. 📈 Raportare si Analytics
- **Rapoarte de activitate** per utilizator/departament
- **Trending** si pattern recognition
- **Compliance reports** pentru audituri externe
- **Data retention policies**

---

## Planul de Implementare

### Faza 1: Fundatia (2-3 saptamani)
1. ✅ Crearea tabelelor de auditare
2. ✅ Implementarea IAuditService si AuditService
3. ✅ Integrarea cu ICurrentUserService
4. ✅ Testarea de baza

### Faza 2: Repository Integration (2 saptamani)
1. ✅ Extinderea BaseRepository cu auditare
2. ✅ Implementarea in PersonalRepository
3. ✅ Testing si debugging
4. ✅ Performance optimization

### Faza 3: Blazor Integration (2 saptamani)
1. ✅ Crearea AuditableComponentBase
2. ✅ Integrarea in componentele existente
3. ✅ UI pentru viewing audit logs
4. ✅ Real-time notifications

### Faza 4: Advanced Features (3-4 saptamani)
1. ✅ Dashboard de auditare
2. ✅ Advanced search si filtering
3. ✅ Alerting system
4. ✅ Reporting si analytics

### Faza 5: Production Readiness (1 saptamana)
1. ✅ Performance tuning
2. ✅ Security review
3. ✅ Documentation completion
4. ✅ Deployment si monitoring

---

## Consideratii Tehnice

### Performance
- **Async operations** pentru toate operatiunile de auditare
- **Batching** pentru volume mari de log-uri
- **Background processing** pentru operatiuni non-critice
- **Database partitioning** pentru tabela AuditLog

### Security
- **Encryption** pentru datele sensibile in audit logs
- **Access control** pentru viewing audit logs
- **Tamper-proof** design pentru integritatea log-urilor
- **Audit of audit** - cine acceseaza log-urile de auditare

### Scalability
- **Horizontal scaling** prin microservices
- **Event sourcing** pentru aplicatii mari
- **Caching strategies** pentru queries frecvente
- **Archive policies** pentru log-uri vechi

---

## Resurse Necesare

### Dezvoltare
- **1 Senior Developer** pentru arhitectura si design
- **1 Mid-level Developer** pentru implementare
- **1 QA Engineer** pentru testing

### Infrastructura
- **Database storage** suplimentar pentru audit logs
- **Monitoring tools** pentru performance tracking
- **Backup solutions** pentru audit data

### Timp Estimat
- **Total: 10-12 saptamani**
- **MVP (Minimum Viable Product): 6-8 saptamani**

---

## Criteriile de Succes

### Functionale
- ✅ Toate operatiunile CRUD sunt auditate automat
- ✅ Dashboard functional pentru vizualizarea activitatii
- ✅ Search si filtering functioneaza performant
- ✅ Alerting system functional

### Non-Functionale
- ✅ Performance impact < 5% in operatiunile normale
- ✅ Audit logs disponibile in < 1 secunda
- ✅ 99.9% uptime pentru sistemul de auditare
- ✅ Compliance cu reglementarile medicale

---

## Riscuri si Mitigari

### Riscuri Tehnice
- **Performance degradation** → Optimizari proactive si monitoring
- **Storage overflow** → Policies de arhivare si curatare
- **Complex queries** → Indexuri optimizate si query tuning

### Riscuri de Business
- **User resistance** → Training si communication
- **Compliance gaps** → Review cu legal si compliance team
- **Cost overrun** → Iterative development si MVP approach

---

*Acest document va fi actualizat pe masura ce implementam functionalitatile de auditare. Pentru detalii tehnice suplimentare, consultati documentatia de dezvoltare.*

**Status:** 📋 Planificat pentru implementare  
**Data urmatoarei review:** Dupa implementarea tabelei Personal  
**Owner:** Echipa de dezvoltare ValyanClinic
