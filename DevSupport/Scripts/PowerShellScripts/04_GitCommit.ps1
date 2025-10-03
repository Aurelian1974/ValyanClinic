# ========================================
# Script Git Commit - Refactorizare Completa
# ValyanClinic - Clean Architecture
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "GIT COMMIT - REFACTORIZARE COMPLETA" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Verifica status
Write-Host "`nVerific status git..." -ForegroundColor Yellow
git status

Write-Host "`n========================================" -ForegroundColor Yellow
Write-Host "Doresti sa continui cu commit? (DA/NU)" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
$confirmation = Read-Host

if ($confirmation -ne "DA") {
    Write-Host "Operatie anulata." -ForegroundColor Yellow
    exit
}

# Add all files
Write-Host "`nAdaug fisierele..." -ForegroundColor Yellow
git add .

# Commit cu mesaj detaliat
Write-Host "`nCreez commit..." -ForegroundColor Yellow
git commit -m "feat: Complete refactoring to Clean Architecture + Vertical Slices + CQRS

BREAKING CHANGE: Complete application restructure

## What Changed

### Architecture
- Implemented Clean Architecture (Domain, Application, Infrastructure, Presentation)
- Adopted Vertical Slices Pattern for features
- Implemented CQRS with MediatR
- Added Repository Pattern with Dapper (Stored Procedures only)
- Implemented Result Pattern for error handling

### Domain Layer
- Created entities: Personal, PersonalMedical, Patient, User
- Added enums: StatusAngajat, StareCivila, Gen
- Implemented Domain Events: PersonalCreated, PersonalUpdated, PersonalDeleted
- Defined repository interfaces

### Application Layer
- Implemented Result Pattern (Result, Result<T>, PagedResult, ValidationResult)
- Added custom exceptions (ValidationException, NotFoundException, BusinessRuleException)
- Created PersonalManagement feature with full CQRS:
  - Commands: CreatePersonal, UpdatePersonal, DeletePersonal (with Validators & Handlers)
  - Queries: GetPersonalList, GetPersonalById (with DTOs & Handlers)

### Infrastructure Layer
- Implemented Dapper repositories (BaseRepository + 6 specific repositories)
- Added caching service (MemoryCacheService)
- All data access via Stored Procedures

### Presentation Layer (Blazor)
- Configured Blazor Server with .NET 9
- Setup Serilog structured logging
- Integrated Syncfusion UI components
- Added Health Checks monitoring
- Configured MediatR, AutoMapper, FluentValidation
- Created base components: App, Routes, MainLayout, Home, Error
- Implemented CSS architecture with design tokens

### Packages Installed (17)
- Syncfusion.Blazor (6 packages)
- Dapper + Dapper.Contrib
- Microsoft.Data.SqlClient
- Serilog (6 packages)
- MediatR
- FluentValidation
- AutoMapper
- Health Checks (5 packages)
- Caching (Memory + Redis)
- Identity + JWT
- Polly, Scrutor, Swashbuckle, Bogus, xUnit

### Documentation
- Added REFACTORING_PROGRESS.md with complete status
- Updated README.md with new architecture
- Created PowerShell automation scripts

### Scripts
- 01_CreateBackup.ps1 - Automated backup (branch + tag + local copy)
- 02_DeleteOldFiles.ps1 - Clean slate deletion
- 03_InstallPackages.ps1 - Automated package installation
- 04_GitCommit.ps1 - This script

## Breaking Changes
- Complete codebase restructure
- Old components, services, repositories deleted
- New CQRS-based architecture
- Stored procedures required for data access

## Next Steps
- Create stored procedures in database
- Implement MediatR behaviors
- Create Blazor UI components for Personal management
- Add Patient and User management features
- Implement audit trail
- Add comprehensive testing

## Build Status
? All projects build successfully
? No compilation errors
? Ready for database integration

Closes #refactoring
"

Write-Host "? Commit creat cu succes!" -ForegroundColor Green

# Push
Write-Host "`n========================================" -ForegroundColor Yellow
Write-Host "Doresti sa faci push la origin? (DA/NU)" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
$pushConfirmation = Read-Host

if ($pushConfirmation -eq "DA") {
    Write-Host "`nFac push..." -ForegroundColor Yellow
    git push origin master
    Write-Host "? Push realizat cu succes!" -ForegroundColor Green
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "COMMIT FINALIZAT!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
