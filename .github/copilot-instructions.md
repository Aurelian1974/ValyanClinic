# ValyanClinic - AI Agent Instructions

> .NET 9 Blazor Server medical clinic management system using Clean Architecture

## Architecture Overview

```
ValyanClinic/              # Presentation - Blazor Server UI
ValyanClinic.Application/  # Business logic, MediatR CQRS, Services
ValyanClinic.Domain/       # Entities, Interfaces (no dependencies)
ValyanClinic.Infrastructure/ # Data access, Repositories
```

**Data flow:** UI Component → MediatR Handler → Repository → SQL Server

## Core Patterns

### MediatR CQRS (Required for ALL business operations)
```csharp
// Commands: ValyanClinic.Application/Features/{Feature}/Commands/
public record CreatePozitieCommand : IRequest<Result<Guid>> { ... }
public class CreatePozitieCommandHandler : IRequestHandler<CreatePozitieCommand, Result<Guid>> { ... }

// Queries: ValyanClinic.Application/Features/{Feature}/Queries/
public record GetPozitieByIdQuery(Guid Id) : IRequest<Result<PozitieDetailDto>>;
```

### Result Pattern (Always wrap returns)
```csharp
// ValyanClinic.Application/Common/Results/
return Result<Guid>.Success(id);
return Result<Guid>.Failure("Error message");
// Check: result.IsSuccess, result.Errors, result.FirstError
```

### Blazor Component Structure (Mandatory separation)
```
Component.razor      # Markup ONLY - no @code blocks, no C# logic
Component.razor.cs   # All logic - partial class with [Inject] services
Component.razor.css  # Scoped styles ONLY - use CSS variables
```

**IMPORTANT**: Never write C# code in `.razor` files. Use code-behind (`.razor.cs`) for all logic.

## Key Conventions

### UI Components
- **Grid**: Syncfusion `SfGrid` with server-side pagination (see `ValyanClinic/Components/Pages/Pacienti/AdministrarePacienti.razor.cs`)
- **Styling**: Use CSS variables from `ValyanClinic/wwwroot/css/variables.css` - blue pastel theme
- **Performance**: Override `ShouldRender()`, use `@key` directive, implement `IDisposable`

### Syncfusion Grid - Navigation Safety Pattern (CRITICAL)
When using Syncfusion `SfGrid` in pages with navigation, follow this pattern to prevent `removeChild` errors:

```csharp
// 1. Add @key to SfGrid in .razor file
<SfGrid @ref="GridRef" @key="@_gridKey" DataSource="@Data">

// 2. In code-behind (.razor.cs):
private string _gridKey = Guid.NewGuid().ToString();
private bool _disposed = false;

protected override async Task OnInitializedAsync()
{
    // Subscribe to navigation BEFORE it happens
    NavigationManager.LocationChanged += OnLocationChanged;
    // ... rest of initialization
}

// Handle navigation - cleanup BEFORE Blazor destroys DOM
private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
{
    _disposed = true;
    _ = JSRuntime.InvokeVoidAsync("cleanupSyncfusionBeforeNavigation");
}

public void Dispose()
{
    if (_disposed) return;
    _disposed = true;
    
    NavigationManager.LocationChanged -= OnLocationChanged;
    _ = JSRuntime.InvokeVoidAsync("cleanupSyncfusionBeforeNavigation");
    GridRef = null;
    // ... cleanup
}
```

```javascript
// 3. In wwwroot/js/fileDownload.js - add Syncfusion cleanup:
window.cleanupSyncfusionBeforeNavigation = function() {
    window._blazorNavigating = true;
    try {
        if (window.sfBlazor?.instances) {
            Object.keys(window.sfBlazor.instances).forEach(key => {
                try { window.sfBlazor.instances[key]?.destroy(); } catch(e) {}
            });
        }
    } catch(e) {}
    setTimeout(() => { window._blazorNavigating = false; }, 500);
};
```

### Design System (Strict - Blue Theme Only)
| Element | Style |
|---------|-------|
| Headers | `linear-gradient(135deg, #93c5fd, #60a5fa)` |
| Primary buttons | `linear-gradient(135deg, #60a5fa, #3b82f6)` |
| Typography | `--font-size-base: 14px`, `--font-size-2xl: 22px` (modals) |

### Testing
- **Unit tests**: xUnit + FluentAssertions + Moq for handlers/services
- **Component tests**: bUnit with `ComponentTestBase` (see `ValyanClinic.Tests/TESTING_GUIDE.md`)
- **Critical bUnit rule**: Wrap async calls in `cut.InvokeAsync()`

## Commands

```powershell
# Build & Run
dotnet build ValyanClinic.sln
dotnet run --project ValyanClinic

# Tests
dotnet test ValyanClinic.Tests

# Production Deployment (Windows Service)
dotnet publish -c Release -r win-x64 --self-contained true -o D:\Services\ValyanClinic
sc create ValyanClinic binPath="D:\Services\ValyanClinic\ValyanClinic.exe"
```

## Database Patterns

**Primary Keys**: Always use `UNIQUEIDENTIFIER` with `NEWSEQUENTIALID()` for all table IDs:
```sql
[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
CONSTRAINT [PK_TableName] PRIMARY KEY CLUSTERED ([Id] ASC)
```

**C# Mapping**: Use `Guid` type for all entity IDs:
```csharp
public Guid Id { get; set; }
```

**Stored Procedures** organized by feature in `DevSupport/01_Database/02_StoredProcedures/`:
```
02_StoredProcedures/
├── Consultatie/    # Medical consultations CRUD
├── ICD10/          # Diagnosis codes lookup
├── ISCO/           # Occupation codes (OcupatieISCO)
└── Programari/     # Appointments scheduling
```

**Naming convention**: `[Entity]_[Action]` (e.g., `Consultatie_GetById`, `Programari_Create`)

## Romanian Domain Terms
| Romanian | English | Entity |
|----------|---------|--------|
| Pacient | Patient | `Pacient.cs` |
| Consultatie | Consultation | `Consultatie.cs` |
| Programare | Appointment | `Programare.cs` |
| PersonalMedical | Medical Staff | `PersonalMedical.cs` |
| Specializare | Specialization | `Specializare.cs` |
| Departament | Department | `Departament.cs` |

## Security Requirements
- **Policy-Based Authorization** - use `[Authorize(Policy = Policies.XXX)]` instead of role-based
- FluentValidation on all MediatR Commands
- Never log CNP (Romanian SSN), passwords, or sensitive medical data
- **DateTime Convention**: Always use `DateTime.Now` (local time), NEVER use `DateTime.UtcNow` - the clinic operates in a single timezone (Romania)

### Authorization (Policy-Based)
```csharp
// Use policies from ValyanClinic.Application/Authorization/Policies.cs
@using ValyanClinic.Application.Authorization
@attribute [Authorize(Policy = Policies.CanViewPatients)]      // Pacient access
@attribute [Authorize(Policy = Policies.RequiresDoctor)]       // Medical staff only
@attribute [Authorize(Policy = Policies.CanViewAuditLog)]      // Admin audit access

// Available policies: CanViewPatients, CanManagePatients, CanViewConsultations,
// CanCreateConsultations, CanPrescribe, RequiresDoctor, RequiresMedicalStaff, etc.
```

**Permissions per Role** (defined in `RolePermissions.cs`):
| Role | Key Permissions |
|------|-----------------|
| Admin | Full access to all features |
| Doctor/Medic | Patients, own consultations, prescriptions |
| Asistent | View patients, manage appointments |
| Receptioner | Appointments, patient registration (no medical data) |

## Key File Locations
| What | Where |
|------|-------|
| Domain Entities | `ValyanClinic.Domain/Entities/` |
| CQRS Handlers | `ValyanClinic.Application/Features/{Feature}/Commands/` and `Queries/` |
| UI Pages | `ValyanClinic/Components/Pages/` |
| Shared Components | `ValyanClinic/Components/Shared/` |
| CSS Variables | `ValyanClinic/wwwroot/css/variables.css` |
| Database Scripts | `DevSupport/01_Database/` |
| Test Infrastructure | `ValyanClinic.Tests/Infrastructure/` |
| JS Utilities | `ValyanClinic/wwwroot/js/fileDownload.js` |

## Anti-Patterns to Avoid
- ❌ Logic in `.razor` files - use `.razor.cs` code-behind
- ❌ Direct DB access from UI - use MediatR Commands/Queries
- ❌ Hardcoded colors/sizes - use CSS variables
- ❌ Client-side pagination for large datasets - use server-side
- ❌ Global CSS - use scoped `.razor.css` files
- ❌ Skipping `IDisposable` - always clean up subscriptions/timers
- ❌ Using `DateTime.UtcNow` - always use `DateTime.Now` (local Romania time)
- ❌ Leaving compiler warnings - fix null reference issues with `??`, `?.` or `!` operators
- ❌ Ignoring async/await - use `_ = MethodAsync();` for intentional fire-and-forget
- ❌ Syncfusion Grid without `@key` directive - causes `removeChild` errors on navigation
- ❌ Not subscribing to `NavigationManager.LocationChanged` in pages with complex components

## Build Quality Standards
- **Zero Errors**: Build must complete without errors
- **Minimal Warnings**: Use `Directory.Build.props` to suppress intentional warnings (CS0414, CS4014, BL0005)
- **Null Safety**: Always handle nullable references with proper null checks or null-forgiving operators
- **Package Compatibility**: Keep NuGet packages compatible (avoid version conflicts like NU1608)

## Syncfusion Blazor Guidelines
- **Version**: Use Syncfusion.Blazor packages version 28.x compatible with .NET 9
- **Grid Navigation**: Always use `@key` directive and `LocationChanged` handler (see pattern above)
- **Disposal**: Never call `GridRef.Dispose()` directly - let Blazor handle it, just set `GridRef = null`
- **Animations**: Call `cleanupSyncfusionBeforeNavigation()` via JS interop before navigation
- **Server-side**: Use server-side paging/sorting for large datasets (>100 records)
