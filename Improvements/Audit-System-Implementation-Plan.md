# Improvements - Sistem de Auditare pentru ValyanClinic

**Creat:** Septembrie 2025  
**Status:** Planificat pentru implementare  
**Prioritate:** Medie-Ridicată  
**Tehnologii:** .NET 9, Blazor Server, Dapper  

---

## Prezentare Generală

Acest document descrie planul de implementare pentru sistemul de auditare al aplicației ValyanClinic. În loc de triggeri de bază de date, vom implementa un sistem modern de auditare la nivel de aplicație cu multiple strategii și funcționalități avansate.

## Context Actual

În prezent, tabela `Personal` și alte tabele au câmpuri de auditare de bază:
- `Data_Crearii`
- `Data_Ultimei_Modificari` 
- `Creat_De`
- `Modificat_De`

Dar nu avem un sistem complet de auditare implementat.

---

## Strategii de Implementare

### 1. 🔥 Interceptors și Decorators (PRIORITATE ÎNALTĂ)

#### Descriere
Implementarea unui service pentru auditare centralizată care interceptează toate operațiunile CRUD.

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
    
    // Implementare detaliată pentru toate metodele
}
```

#### Beneficii
- ✅ Control complet în aplicație
- ✅ Flexibilitate maximă în configurare
- ✅ Integrare cu sistemul de logging existent
- ✅ Testare ușoară

---

### 2. 🔧 Repository Pattern cu Auditare Automată

#### Descriere
Extinderea repository pattern-ului existent pentru a include auditare automată în toate operațiunile.

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
        // Auditare înainte de creeare
        await _auditService.LogOperationAsync("CREATE_BEFORE", entity, _currentUser.UserId);
        
        // Setare metadate de auditare
        entity.Data_Crearii = DateTime.UtcNow;
        entity.Creat_De = _currentUser.Username;
        entity.Data_Ultimei_Modificari = DateTime.UtcNow;
        entity.Modificat_De = _currentUser.Username;
        
        // Executare operațiune
        await _connection.ExecuteAsync(sql, entity);
        
        // Auditare după creare
        await _auditService.LogOperationAsync("CREATE_AFTER", entity, _currentUser.UserId);
        
        return entity;
    }
    
    protected async Task<T> UpdateWithAuditAsync(T oldEntity, T newEntity, string sql)
    {
        // Auditare înainte de update cu comparație
        await _auditService.LogOperationAsync("UPDATE_BEFORE", oldEntity, newEntity, _currentUser.UserId);
        
        // Setare metadate de auditare
        newEntity.Data_Ultimei_Modificari = DateTime.UtcNow;
        newEntity.Modificat_De = _currentUser.Username;
        
        // Executare operațiune
        await _connection.ExecuteAsync(sql, newEntity);
        
        // Auditare după update
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
Folosirea atributelor pentru marcarea metodelor care necesită auditare automată.

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

// Usage în repository
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
            // Logica de auditare înainte
            if (auditAttribute.LogBefore)
            {
                await LogBeforeOperation(auditAttribute.Operation, args);
            }
            
            var result = await next();
            
            // Logica de auditare după
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
Integrarea auditării direct în componentele Blazor pentru tracking complet al acțiunilor utilizatorului.

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
            // Auditare încercare operațiune
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
    
    -- Identificarea operațiunii
    TableName NVARCHAR(50) NOT NULL,
    Operation NVARCHAR(20) NOT NULL, -- CREATE, UPDATE, DELETE, VIEW, EXPORT
    EntityId NVARCHAR(50), -- ID-ul entității modificate
    
    -- Datele modificării
    OldValues NVARCHAR(MAX), -- JSON cu valorile vechi
    NewValues NVARCHAR(MAX), -- JSON cu valorile noi
    ChangedFields NVARCHAR(500), -- Lista câmpurilor modificate
    
    -- Informații utilizator
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
    CorrelationId UNIQUEIDENTIFIER, -- Pentru a grupa operațiuni multiple
    ParentAuditId UNIQUEIDENTIFIER -- Pentru operațiuni în cascadă
);

-- Indexuri pentru performanță
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

## Funcționalități Avansate de Implementat

### 1. 📊 Dashboard de Auditare
- **Vizualizare în timp real** a activității utilizatorilor
- **Statistici** pe operațiuni și utilizatori
- **Alerte** pentru activități suspicioase
- **Exporturi** pentru compliance și raportare

### 2. 🔍 Căutare și Filtrare Avansată
- **Căutare pe perioada de timp**
- **Filtrare după utilizator, operațiune, tabel**
- **Comparația versiunilor** unei înregistrări
- **Timeline** cu toate modificările unei entități

### 3. 🚨 Alerting și Monitoring
- **Alerturi în timp real** pentru operațiuni critice
- **Threshold monitoring** pentru volume mari de operațiuni
- **Integration cu sistemele de monitoring** existente
- **Notificări** către administratori

### 4. 📈 Raportare și Analytics
- **Rapoarte de activitate** per utilizator/departament
- **Trending** și pattern recognition
- **Compliance reports** pentru audituri externe
- **Data retention policies**

---

## Planul de Implementare

### Faza 1: Fundația (2-3 săptămâni)
1. ✅ Crearea tabelelor de auditare
2. ✅ Implementarea IAuditService și AuditService
3. ✅ Integrarea cu ICurrentUserService
4. ✅ Testarea de bază

### Faza 2: Repository Integration (2 săptămâni)
1. ✅ Extinderea BaseRepository cu auditare
2. ✅ Implementarea în PersonalRepository
3. ✅ Testing și debugging
4. ✅ Performance optimization

### Faza 3: Blazor Integration (2 săptămâni)
1. ✅ Crearea AuditableComponentBase
2. ✅ Integrarea în componentele existente
3. ✅ UI pentru viewing audit logs
4. ✅ Real-time notifications

### Faza 4: Advanced Features (3-4 săptămâni)
1. ✅ Dashboard de auditare
2. ✅ Advanced search și filtering
3. ✅ Alerting system
4. ✅ Reporting și analytics

### Faza 5: Production Readiness (1 săptămână)
1. ✅ Performance tuning
2. ✅ Security review
3. ✅ Documentation completion
4. ✅ Deployment și monitoring

---

## Considerații Tehnice

### Performance
- **Async operations** pentru toate operațiunile de auditare
- **Batching** pentru volume mari de log-uri
- **Background processing** pentru operațiuni non-critice
- **Database partitioning** pentru tabela AuditLog

### Security
- **Encryption** pentru datele sensibile în audit logs
- **Access control** pentru viewing audit logs
- **Tamper-proof** design pentru integritatea log-urilor
- **Audit of audit** - cine accesează log-urile de auditare

### Scalability
- **Horizontal scaling** prin microservices
- **Event sourcing** pentru aplicații mari
- **Caching strategies** pentru queries frecvente
- **Archive policies** pentru log-uri vechi

---

## Resurse Necesare

### Dezvoltare
- **1 Senior Developer** pentru arhitectură și design
- **1 Mid-level Developer** pentru implementare
- **1 QA Engineer** pentru testing

### Infrastructură
- **Database storage** suplimentar pentru audit logs
- **Monitoring tools** pentru performance tracking
- **Backup solutions** pentru audit data

### Timp Estimat
- **Total: 10-12 săptămâni**
- **MVP (Minimum Viable Product): 6-8 săptămâni**

---

## Criteriile de Succes

### Funcționale
- ✅ Toate operațiunile CRUD sunt auditate automat
- ✅ Dashboard functional pentru vizualizarea activității
- ✅ Search și filtering funcționează performant
- ✅ Alerting system funcțional

### Non-Funcționale
- ✅ Performance impact < 5% în operațiunile normale
- ✅ Audit logs disponibile în < 1 secundă
- ✅ 99.9% uptime pentru sistemul de auditare
- ✅ Compliance cu reglementările medicale

---

## Riscuri și Mitigări

### Riscuri Tehnice
- **Performance degradation** → Optimizări proactive și monitoring
- **Storage overflow** → Policies de arhivare și curățare
- **Complex queries** → Indexuri optimizate și query tuning

### Riscuri de Business
- **User resistance** → Training și communication
- **Compliance gaps** → Review cu legal și compliance team
- **Cost overrun** → Iterative development și MVP approach

---

*Acest document va fi actualizat pe măsură ce implementăm funcționalitățile de auditare. Pentru detalii tehnice suplimentare, consultați documentația de dezvoltare.*

**Status:** 📋 Planificat pentru implementare  
**Data următoarei review:** După implementarea tabelei Personal  
**Owner:** Echipa de dezvoltare ValyanClinic
