# ========================================
# Script Instalare Pachete NuGet
# ValyanClinic - Package Installation
# ========================================

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "INSTALARE PACHETE NUGET" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# ========================================
# 1. SYNCFUSION
# ========================================
Write-Host "`n[1/17] Instalez Syncfusion..." -ForegroundColor Yellow
dotnet add ValyanClinic/ValyanClinic.csproj package Syncfusion.Blazor.Themes
dotnet add ValyanClinic/ValyanClinic.csproj package Syncfusion.Blazor.Grid
dotnet add ValyanClinic/ValyanClinic.csproj package Syncfusion.Blazor.Inputs
dotnet add ValyanClinic/ValyanClinic.csproj package Syncfusion.Blazor.Calendars
dotnet add ValyanClinic/ValyanClinic.csproj package Syncfusion.Blazor.DropDowns
dotnet add ValyanClinic/ValyanClinic.csproj package Syncfusion.Blazor.Navigations
Write-Host "  ? Syncfusion instalat" -ForegroundColor Green

# ========================================
# 2. DAPPER - Data Access
# ========================================
Write-Host "`n[2/17] Instalez Dapper..." -ForegroundColor Yellow
dotnet add ValyanClinic.Infrastructure/ValyanClinic.Infrastructure.csproj package Dapper
dotnet add ValyanClinic.Infrastructure/ValyanClinic.Infrastructure.csproj package Dapper.Contrib
Write-Host "  ? Dapper instalat" -ForegroundColor Green

# ========================================
# 3. SQL SERVER
# ========================================
Write-Host "`n[3/17] Instalez Microsoft.Data.SqlClient..." -ForegroundColor Yellow
dotnet add ValyanClinic.Infrastructure/ValyanClinic.Infrastructure.csproj package Microsoft.Data.SqlClient
Write-Host "  ? SQL Client instalat" -ForegroundColor Green

# ========================================
# 4. SERILOG - Structured Logging
# ========================================
Write-Host "`n[4/17] Instalez Serilog..." -ForegroundColor Yellow
dotnet add ValyanClinic/ValyanClinic.csproj package Serilog.AspNetCore
dotnet add ValyanClinic/ValyanClinic.csproj package Serilog.Sinks.Console
dotnet add ValyanClinic/ValyanClinic.csproj package Serilog.Sinks.File
dotnet add ValyanClinic/ValyanClinic.csproj package Serilog.Enrichers.Environment
dotnet add ValyanClinic/ValyanClinic.csproj package Serilog.Enrichers.Thread
dotnet add ValyanClinic/ValyanClinic.csproj package Serilog.Settings.Configuration
Write-Host "  ? Serilog instalat" -ForegroundColor Green

# ========================================
# 5. MEDIATR - CQRS si Vertical Slices
# ========================================
Write-Host "`n[5/17] Instalez MediatR..." -ForegroundColor Yellow
dotnet add ValyanClinic.Application/ValyanClinic.Application.csproj package MediatR
dotnet add ValyanClinic/ValyanClinic.csproj package MediatR
Write-Host "  ? MediatR instalat" -ForegroundColor Green

# ========================================
# 6. FLUENTVALIDATION - Validare Avansata
# ========================================
Write-Host "`n[6/17] Instalez FluentValidation..." -ForegroundColor Yellow
dotnet add ValyanClinic.Application/ValyanClinic.Application.csproj package FluentValidation
dotnet add ValyanClinic.Application/ValyanClinic.Application.csproj package FluentValidation.DependencyInjectionExtensions
Write-Host "  ? FluentValidation instalat" -ForegroundColor Green

# ========================================
# 7. AUTOMAPPER - Object Mapping
# ========================================
Write-Host "`n[7/17] Instalez AutoMapper..." -ForegroundColor Yellow
dotnet add ValyanClinic.Application/ValyanClinic.Application.csproj package AutoMapper
dotnet add ValyanClinic.Application/ValyanClinic.Application.csproj package AutoMapper.Extensions.Microsoft.DependencyInjection
Write-Host "  ? AutoMapper instalat" -ForegroundColor Green

# ========================================
# 8. HEALTH CHECKS - Monitoring
# ========================================
Write-Host "`n[8/17] Instalez Health Checks..." -ForegroundColor Yellow
dotnet add ValyanClinic/ValyanClinic.csproj package Microsoft.Extensions.Diagnostics.HealthChecks
dotnet add ValyanClinic/ValyanClinic.csproj package AspNetCore.HealthChecks.SqlServer
dotnet add ValyanClinic/ValyanClinic.csproj package AspNetCore.HealthChecks.UI
dotnet add ValyanClinic/ValyanClinic.csproj package AspNetCore.HealthChecks.UI.Client
dotnet add ValyanClinic/ValyanClinic.csproj package AspNetCore.HealthChecks.UI.InMemory.Storage
Write-Host "  ? Health Checks instalat" -ForegroundColor Green

# ========================================
# 9. RESPONSE CACHING - Performance
# ========================================
Write-Host "`n[9/17] Instalez Response Caching..." -ForegroundColor Yellow
dotnet add ValyanClinic/ValyanClinic.csproj package Microsoft.AspNetCore.ResponseCaching
dotnet add ValyanClinic.Infrastructure/ValyanClinic.Infrastructure.csproj package Microsoft.Extensions.Caching.Memory
dotnet add ValyanClinic.Infrastructure/ValyanClinic.Infrastructure.csproj package Microsoft.Extensions.Caching.StackExchangeRedis
Write-Host "  ? Response Caching instalat" -ForegroundColor Green

# ========================================
# 10. ASP.NET CORE IDENTITY + JWT
# ========================================
Write-Host "`n[10/17] Instalez Identity si JWT..." -ForegroundColor Yellow
dotnet add ValyanClinic/ValyanClinic.csproj package Microsoft.AspNetCore.Identity
dotnet add ValyanClinic/ValyanClinic.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add ValyanClinic.Infrastructure/ValyanClinic.Infrastructure.csproj package System.IdentityModel.Tokens.Jwt
Write-Host "  ? Identity + JWT instalat" -ForegroundColor Green

# ========================================
# 11. DATA PROTECTION API
# ========================================
Write-Host "`n[11/17] Instalez Data Protection..." -ForegroundColor Yellow
dotnet add ValyanClinic/ValyanClinic.csproj package Microsoft.AspNetCore.DataProtection
dotnet add ValyanClinic/ValyanClinic.csproj package Microsoft.AspNetCore.DataProtection.EntityFrameworkCore
Write-Host "  ? Data Protection instalat" -ForegroundColor Green

# ========================================
# 12. POLLY - Resilience
# ========================================
Write-Host "`n[12/17] Instalez Polly..." -ForegroundColor Yellow
dotnet add ValyanClinic.Infrastructure/ValyanClinic.Infrastructure.csproj package Polly
dotnet add ValyanClinic.Infrastructure/ValyanClinic.Infrastructure.csproj package Microsoft.Extensions.Http.Polly
Write-Host "  ? Polly instalat" -ForegroundColor Green

# ========================================
# 13. NEWTONSOFT.JSON
# ========================================
Write-Host "`n[13/17] Instalez Newtonsoft.Json..." -ForegroundColor Yellow
dotnet add ValyanClinic/ValyanClinic.csproj package Newtonsoft.Json
Write-Host "  ? Newtonsoft.Json instalat" -ForegroundColor Green

# ========================================
# 14. SCRUTOR - Decorator Pattern
# ========================================
Write-Host "`n[14/17] Instalez Scrutor..." -ForegroundColor Yellow
dotnet add ValyanClinic/ValyanClinic.csproj package Scrutor
Write-Host "  ? Scrutor instalat" -ForegroundColor Green

# ========================================
# 15. SWASHBUCKLE (pentru API documentation)
# ========================================
Write-Host "`n[15/17] Instalez Swashbuckle..." -ForegroundColor Yellow
dotnet add ValyanClinic/ValyanClinic.csproj package Swashbuckle.AspNetCore
Write-Host "  ? Swashbuckle instalat" -ForegroundColor Green

# ========================================
# 16. BOGUS (pentru date de test)
# ========================================
Write-Host "`n[16/17] Instalez Bogus..." -ForegroundColor Yellow
dotnet add DevSupport/DevSupport.csproj package Bogus
Write-Host "  ? Bogus instalat" -ForegroundColor Green

# ========================================
# 17. XUNIT (pentru teste)
# ========================================
Write-Host "`n[17/17] Instalez xUnit..." -ForegroundColor Yellow
dotnet add DevSupport/DevSupport.csproj package xunit
dotnet add DevSupport/DevSupport.csproj package xunit.runner.visualstudio
dotnet add DevSupport/DevSupport.csproj package Microsoft.NET.Test.Sdk
Write-Host "  ? xUnit instalat" -ForegroundColor Green

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "TOATE PACHETELE INSTALATE!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nSe restoresc dependentele..." -ForegroundColor Yellow
dotnet restore

Write-Host "`n? Instalare completa!" -ForegroundColor Green
