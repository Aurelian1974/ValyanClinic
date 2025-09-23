# Arhitectura Modulului Personal - Technical Documentation

## 📋 Overview

This document provides a comprehensive architectural overview of the Personal Management Module in ValyanClinic. The module demonstrates advanced .NET 9 Blazor Server architecture patterns with Clean Architecture principles, CQRS implementation, and enterprise-grade scalability considerations.

## 🏗️ High-Level Architecture

### System Architecture Diagram
```
┌─────────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │ AdministrarePer │  │ AdaugaEditezaPe │  │ VizualizeazaPer │ │
│  │ sonal.razor     │  │ rsonal.razor    │  │ sonal.razor     │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
│           │                    │                    │           │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │ PersonalPageSt  │  │ PersonalFormMo  │  │ LocationDepen   │ │
│  │ ate             │  │ del             │  │ dentDropdowns   │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                               │
┌─────────────────────────────────────────────────────────────────┐
│                   APPLICATION LAYER                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │ IPersonalServ   │  │ IValidationSer  │  │ ILocationServ   │ │
│  │ ice             │  │ vice            │  │ ice             │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
│           │                    │                    │           │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │ PersonalServi   │  │ ValidationSer   │  │ LocationServi   │ │
│  │ ce              │  │ vice            │  │ ce              │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                               │
┌─────────────────────────────────────────────────────────────────┐
│                    DOMAIN LAYER                                │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │ PersonalModel   │  │ Departament     │  │ StatusAngajat   │ │
│  │                 │  │ (Enum)          │  │ (Enum)          │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │ IPersonalRepo   │  │ Validators      │  │ Domain Events   │ │
│  │ sitory          │  │                 │  │                 │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                               │
┌─────────────────────────────────────────────────────────────────┐
│                 INFRASTRUCTURE LAYER                           │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │ PersonalRepo    │  │ Dapper Context  │  │ SQL Server      │ │
│  │ sitory          │  │                 │  │ Database        │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │ Caching Layer   │  │ Logging (Seri   │  │ Health Checks   │ │
│  │                 │  │ log)            │  │                 │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

## 🧱 Clean Architecture Implementation

### Layer Responsibilities

#### 1. Presentation Layer (ValyanClinic)
```csharp
namespace ValyanClinic.Components.Pages.Administrare.Personal
{
    // Main page component
    public partial class AdministrarePersonal : ComponentBase, IAsyncDisposable
    {
        // Responsibilities:
        // - User interface coordination
        // - State management
        // - Event handling
        // - Component lifecycle management
    }
    
    // Add/Edit modal component
    public partial class AdaugaEditezaPersonal : ComponentBase
    {
        // Responsibilities:
        // - Form presentation and validation
        // - User input handling
        // - Real-time feedback
        // - Modal state management
    }
    
    // View details modal component
    public partial class VizualizeazaPersonal : ComponentBase
    {
        // Responsibilities:
        // - Read-only data presentation
        // - Dashboard layout management
        // - Export functionality
        // - Interactive elements (email, phone)
    }
}
```

#### 2. Application Layer (ValyanClinic.Application)
```csharp
namespace ValyanClinic.Application.Services
{
    public interface IPersonalService
    {
        Task<ServiceResult<PersonalSearchResult>> GetPersonalAsync(PersonalSearchRequest request);
        Task<ServiceResult<PersonalModel>> CreatePersonalAsync(PersonalModel personal, string createdBy);
        Task<ServiceResult<PersonalModel>> UpdatePersonalAsync(PersonalModel personal, string updatedBy);
        Task<ServiceResult<bool>> DeletePersonalAsync(Guid personalId, string deletedBy);
        Task<PersonalStatisticsModel> GetStatisticsAsync();
        Task<string> GetNextCodAngajatAsync();
    }
    
    public class PersonalService : IPersonalService
    {
        private readonly IPersonalRepository _repository;
        private readonly IValidationService _validationService;
        private readonly ILogger<PersonalService> _logger;
        private readonly ICacheService _cacheService;
        
        // Business logic implementation
        // Orchestrates domain operations
        // Handles cross-cutting concerns
        // Maintains transaction boundaries
    }
}
```

#### 3. Domain Layer (ValyanClinic.Domain)
```csharp
namespace ValyanClinic.Domain.Models
{
    public class PersonalModel
    {
        // Core entity with business rules
        public Guid Id_Personal { get; set; }
        public string Cod_Angajat { get; set; } = "";
        public string CNP { get; set; } = "";
        // ... other properties
        
        // Domain methods
        public string NumeComplet => $"{Nume} {Prenume}";
        public bool IsActiv => Status_Angajat == StatusAngajat.Activ;
        public int Varsta => CalculateAge(Data_Nasterii);
        
        // Business rule validation
        public ValidationResult ValidateForCreation();
        public ValidationResult ValidateForUpdate();
    }
}

namespace ValyanClinic.Domain.Enums
{
    public enum Departament
    {
        [Display(Name = "Administrație")]
        Administratie = 1,
        
        [Display(Name = "Financiar")]
        Financiar = 2,
        
        [Display(Name = "IT")]
        IT = 3,
        // ... other departments
    }
}
```

#### 4. Infrastructure Layer (ValyanClinic.Infrastructure)
```csharp
namespace ValyanClinic.Infrastructure.Repositories
{
    public class PersonalRepository : IPersonalRepository
    {
        private readonly IDbConnection _connection;
        private readonly ILogger<PersonalRepository> _logger;
        
        public async Task<PersonalModel?> GetByIdAsync(Guid id)
        {
            const string sql = "EXEC SP_Personal_GetById @Id";
            return await _connection.QueryFirstOrDefaultAsync<PersonalModel>(sql, new { Id = id });
        }
        
        public async Task<IEnumerable<PersonalModel>> GetAllAsync(PersonalSearchRequest request)
        {
            const string sql = "EXEC SP_Personal_GetAll @PageNumber, @PageSize, @SearchText, @Departament, @Status";
            return await _connection.QueryAsync<PersonalModel>(sql, request);
        }
        
        // High-performance data access with Dapper
        // Stored procedure execution
        // Connection management
        // Error handling and logging
    }
}
```

## 🏛️ Design Patterns Implementation

### 1. Repository Pattern
```csharp
public interface IPersonalRepository
{
    Task<PersonalModel?> GetByIdAsync(Guid id);
    Task<IEnumerable<PersonalModel>> GetAllAsync(PersonalSearchRequest request);
    Task<PersonalModel> CreateAsync(PersonalModel personal);
    Task<PersonalModel> UpdateAsync(PersonalModel personal);
    Task<bool> DeleteAsync(Guid id);
    Task<PersonalStatisticsModel> GetStatisticsAsync();
    Task<bool> ExistsByCNPAsync(string cnp, Guid? excludeId = null);
    Task<string> GetNextEmployeeCodeAsync();
}
```

### 2. Service Layer Pattern
```csharp
public class PersonalService : IPersonalService
{
    public async Task<ServiceResult<PersonalModel>> CreatePersonalAsync(PersonalModel personal, string createdBy)
    {
        try
        {
            // 1. Validation
            var validationResult = await _validationService.ValidateForCreateAsync(personal);
            if (!validationResult.IsValid)
                return ServiceResult<PersonalModel>.Failure(validationResult.Errors);
            
            // 2. Business rules
            if (await _repository.ExistsByCNPAsync(personal.CNP))
                return ServiceResult<PersonalModel>.Failure("CNP-ul există deja în sistem");
            
            // 3. Domain operation
            personal.Cod_Angajat = await GetNextCodAngajatAsync();
            personal.Data_Crearii = DateTime.UtcNow;
            personal.Creat_De = createdBy;
            
            // 4. Persistence
            var result = await _repository.CreateAsync(personal);
            
            // 5. Cache invalidation
            _cacheService.InvalidatePattern("personal_*");
            
            // 6. Domain events (if implemented)
            // await _mediator.Publish(new PersonalCreatedEvent(result));
            
            return ServiceResult<PersonalModel>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating personal with CNP: {CNP}", personal.CNP);
            return ServiceResult<PersonalModel>.Failure("Eroare la crearea personalului");
        }
    }
}
```

### 3. Factory Pattern for Components
```csharp
public class PersonalComponentFactory
{
    public static RenderFragment CreatePersonalGrid(List<PersonalModel> data, EventCallback<PersonalModel> onEdit)
    {
        return builder =>
        {
            builder.OpenComponent<SfGrid<PersonalModel>>(0);
            builder.AddAttribute(1, "DataSource", data);
            builder.AddAttribute(2, "AllowPaging", true);
            builder.AddAttribute(3, "AllowSorting", true);
            // ... configure grid
            builder.CloseComponent();
        };
    }
    
    public static RenderFragment CreatePersonalModal(PersonalModel? personal, bool isEdit)
    {
        return builder =>
        {
            if (isEdit)
            {
                builder.OpenComponent<AdaugaEditezaPersonal>(0);
                builder.AddAttribute(1, "EditingPersonal", personal);
            }
            else
            {
                builder.OpenComponent<AdaugaEditezaPersonal>(0);
            }
            builder.CloseComponent();
        };
    }
}
```

### 4. Strategy Pattern for Validation
```csharp
public interface IValidationStrategy<T>
{
    Task<ValidationResult> ValidateAsync(T entity);
}

public class CreatePersonalValidationStrategy : IValidationStrategy<PersonalModel>
{
    public async Task<ValidationResult> ValidateAsync(PersonalModel personal)
    {
        var result = new ValidationResult();
        
        // CNP validation
        if (!ValidateCNP(personal.CNP))
            result.AddError("CNP", "CNP-ul nu este valid");
        
        // Email validation
        if (!IsValidEmail(personal.Email_Personal))
            result.AddError("Email_Personal", "Email-ul nu este valid");
        
        // Business rules validation
        if (await CNPExistsInDatabase(personal.CNP))
            result.AddError("CNP", "CNP-ul există deja în sistem");
        
        return result;
    }
}

public class UpdatePersonalValidationStrategy : IValidationStrategy<PersonalModel>
{
    public async Task<ValidationResult> ValidateAsync(PersonalModel personal)
    {
        var result = new ValidationResult();
        
        // Similar validation but exclude current record
        if (await CNPExistsInDatabase(personal.CNP, personal.Id_Personal))
            result.AddError("CNP", "CNP-ul este folosit de alt angajat");
        
        return result;
    }
}
```

## 🚀 Performance Architecture

### 1. Caching Strategy
```csharp
public class PersonalCachingService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(15);
    
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        // L1 Cache - Memory (fastest)
        if (_memoryCache.TryGetValue(key, out T? memoryValue))
            return memoryValue;
        
        // L2 Cache - Distributed (Redis, if configured)
        var distributedValue = await _distributedCache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(distributedValue))
        {
            var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue);
            _memoryCache.Set(key, deserializedValue, _defaultExpiry);
            return deserializedValue;
        }
        
        return null;
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var actualExpiry = expiry ?? _defaultExpiry;
        
        // Set in both caches
        _memoryCache.Set(key, value, actualExpiry);
        
        var serializedValue = JsonSerializer.Serialize(value);
        await _distributedCache.SetStringAsync(key, serializedValue, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = actualExpiry
        });
    }
}
```

### 2. Query Optimization
```csharp
public class PersonalQueryOptimizer
{
    public static PersonalSearchRequest OptimizeSearchRequest(PersonalSearchRequest request)
    {
        // Optimize page size
        if (request.PageSize > 100)
            request = request with { PageSize = 100 };
        
        // Optimize search text
        if (!string.IsNullOrEmpty(request.SearchText))
        {
            request = request with 
            { 
                SearchText = request.SearchText.Trim().ToLowerInvariant() 
            };
        }
        
        // Add search hints based on patterns
        if (IsLikelyCNP(request.SearchText))
        {
            request = request with { SearchHint = SearchHint.CNP };
        }
        else if (IsLikelyEmail(request.SearchText))
        {
            request = request with { SearchHint = SearchHint.Email };
        }
        
        return request;
    }
}
```

### 3. Connection Management
```csharp
public class DatabaseConnectionManager : IDbConnectionFactory
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseConnectionManager> _logger;
    
    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(_connectionString);
        
        // Configure connection for optimal performance
        connection.ConnectionString += ";Application Name=ValyanClinic;";
        connection.ConnectionString += ";Connection Timeout=30;";
        connection.ConnectionString += ";Command Timeout=60;";
        
        return connection;
    }
    
    public async Task<T> ExecuteWithRetryAsync<T>(Func<IDbConnection, Task<T>> operation, int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();
                return await operation(connection);
            }
            catch (SqlException ex) when (IsTransientError(ex) && attempt < maxRetries)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                _logger.LogWarning("Database operation failed (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}s", 
                    attempt, maxRetries, delay.TotalSeconds);
                await Task.Delay(delay);
            }
        }
        
        throw new InvalidOperationException($"Database operation failed after {maxRetries} attempts");
    }
}
```

## 🔒 Security Architecture

### 1. Authorization Framework
```csharp
public class PersonalAuthorizationService : IPersonalAuthorizationService
{
    public async Task<bool> CanViewPersonalAsync(ClaimsPrincipal user, Guid personalId)
    {
        // Role-based authorization
        if (user.IsInRole("Admin") || user.IsInRole("HR_Manager"))
            return true;
        
        // Resource-based authorization
        if (user.IsInRole("HR_Assistant"))
        {
            var userId = user.GetUserId();
            return await _repository.IsInSameDepartmentAsync(userId, personalId);
        }
        
        return false;
    }
    
    public async Task<bool> CanEditPersonalAsync(ClaimsPrincipal user, Guid personalId)
    {
        if (user.IsInRole("Admin"))
            return true;
        
        if (user.IsInRole("HR_Manager"))
        {
            // HR Managers can edit all personal except other HR Managers
            var personal = await _repository.GetByIdAsync(personalId);
            return personal?.Departament != Departament.ResurseUmane;
        }
        
        return false;
    }
}
```

### 2. Input Validation Framework
```csharp
public class PersonalSecurityValidator : ISecurityValidator<PersonalModel>
{
    public ValidationResult ValidateForSecurity(PersonalModel personal)
    {
        var result = new ValidationResult();
        
        // SQL Injection prevention (already handled by Dapper parameters)
        // XSS prevention
        if (ContainsPotentialXSS(personal.Observatii))
            result.AddError("Observatii", "Conținut potențial periculos detectat");
        
        // Data leak prevention
        if (ContainsSensitiveData(personal.Observatii))
            result.AddError("Observatii", "Nu includeți informații sensibile");
        
        // Business logic validation
        if (personal.Status_Angajat == StatusAngajat.Activ && 
            !IsValidActivationRequest(personal))
            result.AddError("Status", "Activarea necesită validări suplimentare");
        
        return result;
    }
}
```

### 3. Audit Trail Implementation
```csharp
public class PersonalAuditService : IAuditService
{
    public async Task LogPersonalOperationAsync(PersonalAuditEvent auditEvent)
    {
        var auditEntry = new AuditEntry
        {
            Id = Guid.NewGuid(),
            EntityType = "Personal",
            EntityId = auditEvent.PersonalId.ToString(),
            Operation = auditEvent.Operation,
            UserId = auditEvent.UserId,
            UserName = auditEvent.UserName,
            Timestamp = DateTime.UtcNow,
            OldValues = JsonSerializer.Serialize(auditEvent.OldValues),
            NewValues = JsonSerializer.Serialize(auditEvent.NewValues),
            IPAddress = auditEvent.IPAddress,
            UserAgent = auditEvent.UserAgent
        };
        
        await _auditRepository.CreateAsync(auditEntry);
        
        // Send to external audit system if configured
        if (_auditConfiguration.ExternalAuditEnabled)
        {
            await _externalAuditService.SendAsync(auditEntry);
        }
    }
}
```

## 🧪 Testing Architecture

### 1. Test Pyramid Structure
```
                    ┌─────────────────┐
                    │   E2E Tests     │  ← Few, High Value
                    └─────────────────┘
                ┌─────────────────────────┐
                │  Integration Tests      │  ← Some, Focused
                └─────────────────────────┘
        ┌─────────────────────────────────────────┐
        │           Unit Tests                    │  ← Many, Fast
        └─────────────────────────────────────────┘
```

### 2. Unit Testing Framework
```csharp
[TestFixture]
public class PersonalServiceTests
{
    private Mock<IPersonalRepository> _repositoryMock;
    private Mock<IValidationService> _validationMock;
    private Mock<ILogger<PersonalService>> _loggerMock;
    private PersonalService _service;
    
    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IPersonalRepository>();
        _validationMock = new Mock<IValidationService>();
        _loggerMock = new Mock<ILogger<PersonalService>>();
        _service = new PersonalService(_repositoryMock.Object, _validationMock.Object, _loggerMock.Object);
    }
    
    [Test]
    public async Task CreatePersonalAsync_ValidPersonal_ReturnsSuccess()
    {
        // Arrange
        var personal = PersonalTestData.CreateValidPersonal();
        _validationMock.Setup(x => x.ValidateForCreateAsync(personal))
                      .ReturnsAsync(ValidationResult.Success());
        _repositoryMock.Setup(x => x.ExistsByCNPAsync(personal.CNP, null))
                      .ReturnsAsync(false);
        _repositoryMock.Setup(x => x.CreateAsync(personal))
                      .ReturnsAsync(personal);
        
        // Act
        var result = await _service.CreatePersonalAsync(personal, "test_user");
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<PersonalModel>()), Times.Once);
    }
}
```

### 3. Integration Testing Framework
```csharp
[TestFixture]
public class PersonalRepositoryIntegrationTests : IntegrationTestBase
{
    private IPersonalRepository _repository;
    
    [SetUp]
    public void Setup()
    {
        _repository = GetService<IPersonalRepository>();
    }
    
    [Test]
    public async Task CreateAndRetrievePersonal_ValidData_ShouldPersist()
    {
        // Arrange
        var personal = PersonalTestData.CreateValidPersonal();
        
        // Act
        var created = await _repository.CreateAsync(personal);
        var retrieved = await _repository.GetByIdAsync(created.Id_Personal);
        
        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(created.CNP, retrieved.CNP);
        Assert.AreEqual(created.Nume, retrieved.Nume);
    }
}
```

## 📊 Monitoring and Observability

### 1. Structured Logging
```csharp
public partial class AdministrarePersonal
{
    private void LogPersonalOperation(string operation, Guid? personalId = null, object? data = null)
    {
        using var activity = Activity.StartActivity("Personal.Operation");
        activity?.SetTag("operation", operation);
        activity?.SetTag("personalId", personalId?.ToString());
        
        _logger.LogInformation("Personal operation: {Operation} for ID: {PersonalId} with data: {@Data}",
            operation, personalId, data);
    }
}
```

### 2. Health Checks
```csharp
public class PersonalModuleHealthCheck : IHealthCheck
{
    private readonly IPersonalRepository _repository;
    private readonly IDbConnection _connection;
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check database connectivity
            await _connection.QueryFirstOrDefaultAsync("SELECT 1");
            
            // Check basic functionality
            var count = await _repository.GetCountAsync();
            
            // Check performance
            var stopwatch = Stopwatch.StartNew();
            await _repository.GetAllAsync(new PersonalSearchRequest(1, 10, "", null, null));
            stopwatch.Stop();
            
            if (stopwatch.ElapsedMilliseconds > 5000) // 5 second threshold
            {
                return HealthCheckResult.Degraded($"Query took {stopwatch.ElapsedMilliseconds}ms");
            }
            
            return HealthCheckResult.Healthy($"Personal module healthy. Records: {count}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Personal module health check failed", ex);
        }
    }
}
```

### 3. Performance Metrics
```csharp
public class PersonalMetrics
{
    private static readonly Counter PersonalOperations = Metrics
        .CreateCounter("personal_operations_total", "Total personal operations", new[] { "operation", "result" });
    
    private static readonly Histogram PersonalOperationDuration = Metrics
        .CreateHistogram("personal_operation_duration_seconds", "Personal operation duration");
    
    public static void RecordOperation(string operation, string result, double duration)
    {
        PersonalOperations.WithLabels(operation, result).Inc();
        PersonalOperationDuration.Observe(duration);
    }
}
```

## 🔄 Deployment Architecture

### 1. Environment Configuration
```yaml
# appsettings.Production.json
{
  "Personal": {
    "CacheEnabled": true,
    "CacheExpiryMinutes": 15,
    "MaxPageSize": 100,
    "EnableAuditLogging": true,
    "PerformanceLoggingThresholdMs": 1000
  },
  "Database": {
    "ConnectionString": "Server=production;Database=ValyanMed;Trusted_Connection=true;",
    "CommandTimeout": 60,
    "EnableRetry": true,
    "MaxRetryAttempts": 3
  }
}
```

### 2. Container Deployment
```dockerfile
# Dockerfile for Personal Module
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ValyanClinic/ValyanClinic.csproj", "ValyanClinic/"]
COPY ["ValyanClinic.Domain/ValyanClinic.Domain.csproj", "ValyanClinic.Domain/"]
COPY ["ValyanClinic.Application/ValyanClinic.Application.csproj", "ValyanClinic.Application/"]
COPY ["ValyanClinic.Infrastructure/ValyanClinic.Infrastructure.csproj", "ValyanClinic.Infrastructure/"]

RUN dotnet restore "ValyanClinic/ValyanClinic.csproj"
COPY . .
WORKDIR "/src/ValyanClinic"
RUN dotnet build "ValyanClinic.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ValyanClinic.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ValyanClinic.dll"]
```

---

**🎯 Architecture Principles Applied**:
1. **Clean Architecture** with clear separation of concerns
2. **SOLID Principles** throughout all layers
3. **Domain-Driven Design** for business logic organization
4. **CQRS** for read/write separation (where applicable)
5. **Repository Pattern** for data access abstraction
6. **Dependency Injection** for loose coupling
7. **Factory Pattern** for complex object creation
8. **Strategy Pattern** for validation and business rules

**🔗 Related Documentation**:
- **Implementation Details** - Individual component technical docs
- **Database Design** - Schema and stored procedures
- **Security Guidelines** - Authentication and authorization
- **Performance Optimization** - Caching and query optimization

**📞 Architecture Support**: architecture@valyanmed.ro  
**📖 Design Patterns Guide**: Internal architecture documentation  
**🏗️ Deployment Guide**: DevOps team documentation

**Document Version**: 2.0  
**Last Updated**: December 2024  
**Author**: ValyanMed Architecture Team
