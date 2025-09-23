# Database Configuration with Dapper - Technical Documentation

**File:** Database connection configuration for ValyanClinic application  
**Technology:** Dapper ORM with SQL Server  
**Created:** September 2025  
**Last Updated:** September 2025  
**Author:** ValyanMed Development Team  
**Target Framework:** .NET 9  
**Database:** SQL Server (TS1828\ERP instance)  

---

## Overview

This document describes the database configuration setup for the ValyanClinic application using Dapper as the data access technology. The configuration ensures secure, efficient, and scalable database operations while maintaining proper separation between server-side and client-side code in our Blazor Server application.

### Key Features
- **Dapper ORM Integration** - Lightweight, fast micro-ORM for .NET
- **SQL Server Connectivity** - Direct connection to TS1828\ERP server instance
- **Secure Configuration** - Connection strings never exposed to client
- **Environment-Specific Settings** - Different configurations for Development/Production
- **Romanian Character Support** - UTF-8 encoding for proper diacritics handling
- **Connection Pooling** - Optimized database connection management
- **Health Checks** - Automatic database connectivity validation

---

## Database Connection Architecture

### 1. Server Instance Configuration

**Target Database Server:**
```
Server: TS1828\ERP
Database: ValyanMed
Authentication: Windows Authentication (Trusted_Connection=True)
```

**Security Configuration:**
- **Encrypt=False** - For internal network communication
- **TrustServerCertificate=True** - For self-signed certificates
- **Trusted_Connection=True** - Uses Windows Authentication
- **Connection pooling** enabled by default

### 2. Why Dapper Over Entity Framework

**Performance Benefits:**
- **Faster Execution** - Direct SQL execution without ORM overhead
- **Memory Efficient** - Lower memory footprint than EF Core
- **Full SQL Control** - Complete control over SQL queries
- **Micro-ORM Approach** - Minimal abstraction layer

**Medical Application Requirements:**
- **High Performance** - Critical for patient data access
- **Predictable Queries** - No hidden N+1 problems
- **Custom Optimizations** - Medical-specific query patterns
- **Stored Procedure Support** - Legacy system integration

---

## Configuration Files Structure

### 1. Base Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TS1828\\ERP;Database=ValyanMed;Trusted_Connection=True;Encrypt=False"
  },
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

**Purpose:** Base configuration inherited by all environments

### 2. Development Configuration (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TS1828\\ERP;Database=ValyanMed;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information",
      "Microsoft.EntityFrameworkCore.Infrastructure": "Warning"
    }
  }
}
```

**Features:**
- **Detailed Logging** - SQL command logging enabled
- **TrustServerCertificate** - For development SSL handling
- **Extended Timeouts** - Longer timeouts for debugging

### 3. Production Configuration (appsettings.Production.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TS1828\\ERP;Database=ValyanMed;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;Command Timeout=60"
  },
  "Database": {
    "CommandTimeout": 60,
    "ConnectionPoolSize": 100,
    "EnableDetailedErrors": false,
    "EnableSensitiveDataLogging": false
  },
  "Security": {
    "RequireHttps": true,
    "DatabaseEncryption": false,
    "AuditConnections": true
  }
}
```

**Production Features:**
- **Optimized Timeouts** - 30s connection, 60s command timeout
- **Large Connection Pool** - 100 concurrent connections
- **Security Focused** - Minimal logging, no sensitive data
- **Audit Trail** - Connection auditing enabled

---

## Program.cs Integration

### 1. Dependency Injection Configuration

```csharp
// DAPPER DATABASE CONFIGURATION - SECURIZAT PENTRU BLAZOR SERVER
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configurare IDbConnection pentru Dapper - SECURIZAT COMPLET
builder.Services.AddScoped<IDbConnection>(provider => 
{
    var connection = new SqlConnection(connectionString);
    connection.ConnectionString = connectionString;
    return connection;
});

// Configurare pentru pool de conexiuni optimizat pentru Dapper
builder.Services.AddScoped<Func<IDbConnection>>(provider => 
    () => new SqlConnection(connectionString));
```

**Security Measures:**
- **Server-Side Only** - Connection string never leaves the server
- **Scoped Lifetime** - New connection per request scope
- **Automatic Disposal** - Connections properly disposed by DI container
- **Exception Handling** - Throws if connection string missing

### 2. Connection Testing at Startup

```csharp
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
    
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("✅ Database connection established successfully via Dapper");
    logger.LogInformation("📊 Connected to database: {DatabaseName}", databaseName);
    
    dbConnection.Close();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "❌ Failed to establish database connection");
}
```

**Startup Validation:**
- **Connection Test** - Validates connectivity at application startup
- **Database Verification** - Confirms correct database connection
- **Error Logging** - Logs connection issues for troubleshooting
- **Non-Blocking** - Application continues even if connection fails

---

## Security Implementation

### 1. Client-Side Protection

**Blazor Server Security:**
```csharp
// Connection string DOAR pe server - NICIODATa nu ajunge la client
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
```

**Security Features:**
- **Server-Only Processing** - All database operations on server
- **No Client Exposure** - Connection details never sent to browser
- **SignalR Protection** - Only results transmitted to client
- **Authentication Required** - Database access requires user authentication

### 2. Connection String Security

**Security Measures:**
- **Windows Authentication** - No passwords in connection strings
- **Trusted Connection** - Uses current user context
- **Network Security** - Internal network communication only
- **Encryption Options** - Configurable per environment

### 3. SQL Injection Prevention

**Dapper Protection:**
```csharp
// Parameterized queries prevent SQL injection
var users = connection.Query<User>(
    "SELECT * FROM Users WHERE Department = @department", 
    new { department = userDepartment });
```

**Protection Methods:**
- **Parameterized Queries** - All user input parameterized
- **Input Validation** - Server-side validation before database
- **Stored Procedures** - When additional security needed
- **Audit Logging** - All database operations logged

---

## Performance Optimization

### 1. Connection Pooling

**Configuration:**
```json
{
  "Database": {
    "ConnectionPoolSize": 100,
    "CommandTimeout": 60
  }
}
```

**Benefits:**
- **Connection Reuse** - Efficient connection management
- **Reduced Latency** - No connection establishment overhead
- **Scalability** - Supports many concurrent users
- **Resource Management** - Automatic connection lifecycle

### 2. Query Optimization

**Dapper Advantages:**
```csharp
// Direct SQL control for optimal performance
var activeUsers = await connection.QueryAsync<User>(@"
    SELECT u.Id, u.FirstName, u.LastName, u.Email, u.LastLoginDate
    FROM Users u WITH (NOLOCK)
    WHERE u.Status = @status 
    AND u.LastLoginDate >= @sinceDate
    ORDER BY u.LastLoginDate DESC",
    new { status = UserStatus.Active, sinceDate = DateTime.Now.AddDays(-30) });
```

**Performance Features:**
- **Custom Indexes** - Queries optimized for specific indexes
- **Minimal Data Transfer** - Select only needed columns
- **Efficient Joins** - Hand-crafted JOIN operations
- **Caching Strategy** - Result caching where appropriate

### 3. Async Operations

**Async Pattern:**
```csharp
public async Task<User> GetUserByIdAsync(int userId)
{
    using var connection = _connectionFactory();
    return await connection.QueryFirstOrDefaultAsync<User>(
        "SELECT * FROM Users WHERE Id = @userId", 
        new { userId });
}
```

**Benefits:**
- **Non-Blocking** - UI remains responsive
- **Scalability** - More concurrent operations
- **Resource Efficient** - Better thread utilization

---

## Data Access Patterns

### 1. Repository Pattern Implementation

**Interface Definition:**
```csharp
public interface IUserRepository
{
    Task<User> GetByIdAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
}
```

**Dapper Implementation:**
```csharp
public class UserRepository : IUserRepository
{
    private readonly IDbConnection _connection;
    
    public UserRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    
    public async Task<User> GetByIdAsync(int id)
    {
        return await _connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Id = @id", new { id });
    }
}
```

### 2. Medical Data Patterns

**Patient Data Access:**
```csharp
public async Task<Patient> GetPatientWithMedicalHistoryAsync(int patientId)
{
    var sql = @"
        SELECT p.*, mh.* 
        FROM Patients p
        LEFT JOIN MedicalHistory mh ON p.Id = mh.PatientId
        WHERE p.Id = @patientId";
        
    var patientDictionary = new Dictionary<int, Patient>();
    
    var patients = await _connection.QueryAsync<Patient, MedicalHistory, Patient>(
        sql,
        (patient, medicalHistory) =>
        {
            if (!patientDictionary.TryGetValue(patient.Id, out var patientEntry))
            {
                patientEntry = patient;
                patientEntry.MedicalHistory = new List<MedicalHistory>();
                patientDictionary.Add(patientEntry.Id, patientEntry);
            }
            
            if (medicalHistory != null)
                patientEntry.MedicalHistory.Add(medicalHistory);
                
            return patientEntry;
        },
        new { patientId },
        splitOn: "Id");
        
    return patients.FirstOrDefault();
}
```

---

## Error Handling and Logging

### 1. Database Exception Handling

**Exception Strategy:**
```csharp
public async Task<Result<User>> CreateUserAsync(User user)
{
    try
    {
        using var connection = _connectionFactory();
        
        var sql = @"INSERT INTO Users (FirstName, LastName, Email, Username, CreatedDate)
                   VALUES (@FirstName, @LastName, @Email, @Username, @CreatedDate);
                   SELECT CAST(SCOPE_IDENTITY() as int)";
                   
        var userId = await connection.QuerySingleAsync<int>(sql, user);
        user.Id = userId;
        
        return Result<User>.Success(user);
    }
    catch (SqlException ex) when (ex.Number == 2627) // Duplicate key
    {
        return Result<User>.Failure("Un utilizator cu acest email exista deja.");
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Database error creating user: {Email}", user.Email);
        return Result<User>.Failure("Eroare la crearea utilizatorului.");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error creating user: {Email}", user.Email);
        return Result<User>.Failure("Eroare neasteptata la crearea utilizatorului.");
    }
}
```

### 2. Connection Monitoring

**Health Check Implementation:**
```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnection _connection;
    
    public DatabaseHealthCheck(IDbConnection connection)
    {
        _connection = connection;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _connection.QueryFirstAsync<int>("SELECT 1");
            return result == 1 
                ? HealthCheckResult.Healthy("Database connection is working")
                : HealthCheckResult.Unhealthy("Database returned unexpected result");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database connection failed", ex);
        }
    }
}
```

---

## Deployment Considerations

### 1. Environment-Specific Configuration

**Development Environment:**
- **Local SQL Server** or shared development server
- **Detailed logging** for debugging
- **Extended timeouts** for development tools
- **Sensitive data logging** enabled

**Production Environment:**
- **Dedicated database server** (TS1828\ERP)
- **Minimal logging** for performance
- **Optimized timeouts** for user experience
- **Security hardening** enabled

### 2. Database Schema Management

**Migration Strategy:**
```sql
-- Example migration script structure
-- Migration: 001_CreateUsersTable.sql
CREATE TABLE Users (
    Id int IDENTITY(1,1) PRIMARY KEY,
    FirstName nvarchar(50) NOT NULL,
    LastName nvarchar(50) NOT NULL,
    Email nvarchar(255) NOT NULL UNIQUE,
    Username nvarchar(50) NOT NULL UNIQUE,
    PasswordHash nvarchar(255) NOT NULL,
    Role int NOT NULL,
    Status int NOT NULL DEFAULT 1,
    Department nvarchar(100),
    JobTitle nvarchar(100),
    CreatedDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate datetime2,
    LastLoginDate datetime2
);

-- Indexes for performance
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Department ON Users(Department);
CREATE INDEX IX_Users_Status ON Users(Status);
```

### 3. Backup and Recovery

**Backup Strategy:**
- **Automated backups** of ValyanMed database
- **Point-in-time recovery** capability
- **Offsite backup storage** for disaster recovery
- **Regular backup testing** procedures

---

## Romanian Language Support

### 1. Character Encoding

**UTF-8 Configuration:**
```csharp
// Ensure proper encoding for Romanian characters
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Console.OutputEncoding = Encoding.UTF8;
```

**Database Collation:**
- **Romanian_CI_AS** - Case-insensitive, accent-sensitive
- **SQL_Latin1_General_CP1_CI_AS** - Alternative for compatibility
- **UTF-8 support** in SQL Server 2019+

### 2. Data Validation

**Romanian-Specific Validation:**
```csharp
public class RomanianNameValidator : AbstractValidator<string>
{
    public RomanianNameValidator()
    {
        RuleFor(name => name)
            .NotEmpty().WithMessage("Numele este obligatoriu")
            .Length(2, 50).WithMessage("Numele trebuie sa aiba intre 2 si 50 de caractere")
            .Matches("^[a-zA-Zaaistaaist\\s-]+$")
            .WithMessage("Numele poate contine doar litere romanesti, spatii si cratime");
    }
}
```

---

## Troubleshooting Guide

### 1. Common Connection Issues

**Issue:** "A network-related or instance-specific error occurred"
**Causes:**
- SQL Server service not running
- Network connectivity issues
- Firewall blocking connection
- Instance name incorrect

**Solutions:**
1. Verify SQL Server is running on TS1828
2. Test network connectivity: `telnet TS1828 1433`
3. Check Windows Firewall settings
4. Verify instance name: `TS1828\ERP`

### 2. Authentication Problems

**Issue:** "Login failed for user"
**Causes:**
- Windows Authentication not enabled
- User account not granted database access
- SQL Server authentication mode incorrect

**Solutions:**
1. Enable Windows Authentication in SQL Server
2. Add user to database with appropriate permissions
3. Verify SQL Server authentication mode

### 3. Performance Issues

**Issue:** Slow query execution
**Diagnostics:**
```sql
-- Check for blocking processes
SELECT 
    blocking_session_id,
    session_id,
    wait_type,
    wait_time,
    wait_resource
FROM sys.dm_exec_requests
WHERE blocking_session_id <> 0;

-- Check for missing indexes
SELECT TOP 10
    ROUND(avg_total_user_cost * avg_user_impact * (user_seeks + user_scans),0) AS [Total Cost],
    d.[statement] AS [Table/Index],
    equality_columns,
    inequality_columns,
    included_columns
FROM sys.dm_db_missing_index_groups g
INNER JOIN sys.dm_db_missing_index_group_stats s ON s.group_handle = g.index_group_handle
INNER JOIN sys.dm_db_missing_index_details d ON d.index_handle = g.index_handle
ORDER BY [Total Cost] DESC;
```

---

## Best Practices

### 1. Query Optimization

**Best Practices:**
- **Use parameterized queries** to prevent SQL injection
- **Select only needed columns** to reduce data transfer
- **Use appropriate indexes** for WHERE and JOIN clauses
- **Avoid N+1 queries** through proper JOIN operations
- **Use async methods** for all database operations

### 2. Connection Management

**Best Practices:**
- **Use dependency injection** for connection management
- **Implement proper disposal** patterns
- **Use connection pooling** for better performance
- **Monitor connection usage** in production
- **Implement circuit breaker** for resilience

### 3. Security Practices

**Best Practices:**
- **Never expose connection strings** to client
- **Use Windows Authentication** when possible
- **Implement audit logging** for sensitive operations
- **Validate all inputs** before database operations
- **Use least privilege principle** for database accounts

---

## Monitoring and Maintenance

### 1. Performance Monitoring

**Key Metrics:**
- **Connection pool usage**
- **Query execution times**
- **Database CPU and memory usage**
- **Disk I/O patterns**
- **Blocking and deadlocks**

**Monitoring Tools:**
- **SQL Server Profiler** for query analysis
- **Performance Monitor** for system metrics
- **Application logs** for error tracking
- **Health checks** for availability

### 2. Regular Maintenance

**Maintenance Tasks:**
- **Update statistics** weekly
- **Rebuild/reorganize indexes** monthly
- **Check database integrity** weekly
- **Review query performance** monthly
- **Update connection string** as needed

---

## Future Enhancements

### 1. Planned Improvements

**Performance Enhancements:**
- **Query result caching** for frequently accessed data
- **Read replica support** for reporting queries
- **Partitioning** for large tables
- **Compression** for archival data

**Security Enhancements:**
- **Always Encrypted** for sensitive data
- **Row-level security** for multi-tenant scenarios
- **Audit improvements** with more detailed logging
- **Certificate-based authentication**

### 2. Technology Upgrades

**Future Considerations:**
- **SQL Server 2025** features
- **Azure SQL Database** migration
- **Docker containerization** for development
- **Kubernetes** deployment options

---

*This documentation should be updated whenever changes are made to the database configuration or connection setup. Regular review ensures accuracy and security compliance.*

**Version:** 1.0  
**Last Updated:** September 2025  
**Next Review:** December 2025  
**Maintained by:** ValyanMed Development Team
