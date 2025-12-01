# 🔧 Plan Remediere AdministrarePacienti - Implementation Roadmap

**Data:** 06 Ianuarie 2025  
**Pagină:** `ValyanClinic/Components/Pages/Pacienti/AdministrarePacienti.razor`  
**Status:** 📋 **READY FOR IMPLEMENTATION**  
**Based On:** Analysis Report v2.0

---

## 📊 **Executive Summary**

**Current Grade:** D (64/100)  
**Target Grade:** A+ (95/100)  
**Total Effort:** ~14-19 hours  
**Critical Blockers:** 3 (Security, Performance, Testing)

### **Priority Breakdown:**

| Priority | Tasks | Time | Grade After |
|----------|-------|------|-------------|
| **P0** 🔴 | Security (Add [Authorize]) | 5 min | D → C (70/100) |
| **P1** 🔴 | Pagination + Tests | 11-16h | C → B (85/100) |
| **P2** 🟡 | CSS Variables + Performance | 2.5h | B → A (92/100) |
| **P3** 🟢 | Minor polish | 20 min | A → A+ (95/100) |

---

## 🔴 **P0 - CRITICAL SECURITY FIX (5 minutes)**

### **Task 1: Add [Authorize] Attribute**

**Why:** ⚠️ **PRODUCTION BLOCKER** - Date medicale EXPUSE fără autentificare (GDPR violation!)

**Affected Files:**
1. `ValyanClinic/Components/Pages/Pacienti/AdministrarePacienti.razor`
2. `ValyanClinic/Components/Pages/Pacienti/Modals/PacientAddEditModal.razor`
3. `ValyanClinic/Components/Pages/Pacienti/Modals/PacientViewModal.razor`
4. `ValyanClinic/Components/Pages/Pacienti/Modals/PacientHistoryModal.razor`
5. `ValyanClinic/Components/Pages/Pacienti/Modals/PacientDocumentsModal.razor`

**Implementation:**

#### **Step 1.1: Update AdministrarePacienti.razor (2 min)**

```razor
@page "/pacienti/administrare"
@attribute [Authorize] <!-- ✅ ADD THIS LINE -->
@rendermode InteractiveServer
@using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList
@using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById
@using ValyanClinic.Components.Pages.Pacienti.Modals
@using Syncfusion.Blazor.Grids
@using MediatR
```

#### **Step 1.2: Update PacientAddEditModal.razor (1 min)**

```razor
@attribute [Authorize] <!-- ✅ ADD THIS LINE -->
@rendermode InteractiveServer
@using ValyanClinic.Application.Features.PacientManagement.Commands.CreatePacient
<!-- ...existing code... -->
```

#### **Step 1.3: Update PacientViewModal.razor (1 min)**

```razor
@attribute [Authorize] <!-- ✅ ADD THIS LINE -->
@rendermode InteractiveServer
<!-- ...existing code... -->
```

#### **Step 1.4: Update Other Modals (1 min)**

Apply same pattern pentru:
- `PacientHistoryModal.razor`
- `PacientDocumentsModal.razor`

#### **Step 1.5: Verify & Test (1 min)**

**Test Plan:**
1. Logout (clear cookies)
2. Navigate to `https://localhost:5001/pacienti/administrare`
3. **Expected:** Redirect to `/login` (NOT see patient data)
4. Login with valid credentials
5. **Expected:** See AdministrarePacienti page

**Verification:**
```bash
# Run app
dotnet run --project ValyanClinic

# Test URLs (în browser incognito):
https://localhost:5001/pacienti/administrare  # Should redirect to /login
```

**Success Criteria:**
- ✅ Anonymous users CANNOT access `/pacienti/administrare`
- ✅ Anonymous users redirected to `/login`
- ✅ Authenticated users CAN access page
- ✅ NO data leakage in network tab (DevTools)

---

## 🔴 **P1 - CRITICAL PERFORMANCE & TESTING (11-16 hours)**

### **Task 2: Implement Server-Side Pagination (3-4 hours)**

**Why:** ⚠️ **SCALABILITY BLOCKER** - Browser freeze cu >10,000 pacienti

#### **Step 2.1: Create IPacientDataService Interface (30 min)**

**File:** `ValyanClinic.Application/Services/IPacientDataService.cs` (NEW)

```csharp
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;

namespace ValyanClinic.Application.Services;

/// <summary>
/// Service pentru gestionarea datelor pacienți cu paginare server-side
/// </summary>
public interface IPacientDataService
{
    /// <summary>
    /// Încarcă date pacienți cu paginare și filtrare server-side
    /// </summary>
    Task<Result<PagedResult<PacientListDto>>> LoadPagedDataAsync(
        PacientFilters filters,
        PaginationOptions pagination,
        SortOptions sorting,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Filtre pentru căutare pacienți
/// </summary>
public record PacientFilters
{
    public string? SearchText { get; init; }
    public string? Judet { get; init; }
    public bool? Asigurat { get; init; }
    public bool? Activ { get; init; }
}

/// <summary>
/// Opțiuni paginare
/// </summary>
public record PaginationOptions
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 25; // Default: 25 records per page
}

/// <summary>
/// Opțiuni sortare
/// </summary>
public record SortOptions
{
    public string? SortColumn { get; init; } = "Nume";
    public string? SortDirection { get; init; } = "ASC";
}
```

#### **Step 2.2: Implement PacientDataService (1.5 hours)**

**File:** `ValyanClinic.Application/Services/PacientDataService.cs` (NEW)

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;

namespace ValyanClinic.Application.Services;

/// <summary>
/// Implementare service pentru gestionarea datelor pacienți
/// </summary>
public class PacientDataService : IPacientDataService
{
    private readonly IMediator _mediator;
    private readonly ILogger<PacientDataService> _logger;

    public PacientDataService(IMediator mediator, ILogger<PacientDataService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<PagedResult<PacientListDto>>> LoadPagedDataAsync(
        PacientFilters filters,
        PaginationOptions pagination,
        SortOptions sorting,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[PacientDataService] Loading paged data - Page: {Page}, Size: {Size}, Search: {Search}",
            pagination.PageNumber,
            pagination.PageSize,
            filters.SearchText);

        try
        {
            // Validare input
            if (pagination.PageNumber < 1)
                return Result<PagedResult<PacientListDto>>.Failure("Numărul paginii trebuie să fie >= 1");

            if (pagination.PageSize < 1 || pagination.PageSize > 100)
                return Result<PagedResult<PacientListDto>>.Failure("PageSize trebuie să fie între 1 și 100");

            // Create query cu server-side filters
            var query = new GetPacientListQuery
            {
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                SearchText = filters.SearchText,
                Judet = filters.Judet,
                Asigurat = filters.Asigurat,
                Activ = filters.Activ,
                SortColumn = sorting.SortColumn ?? "Nume",
                SortDirection = sorting.SortDirection ?? "ASC"
            };

            // Send query prin MediatR
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess && result.Value != null)
            {
                _logger.LogInformation(
                    "[PacientDataService] Loaded {Count} records (Total: {Total})",
                    result.Value.Value?.Count() ?? 0,
                    result.Value.TotalRecords);

                return Result<PagedResult<PacientListDto>>.Success(result.Value);
            }

            _logger.LogWarning("[PacientDataService] Failed to load data: {Error}", result.FirstError);
            return Result<PagedResult<PacientListDto>>.Failure(result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PacientDataService] Exception loading paged data");
            return Result<PagedResult<PacientListDto>>.Failure($"Eroare la încărcarea datelor: {ex.Message}");
        }
    }
}
```

#### **Step 2.3: Register Service în DI Container (10 min)**

**File:** `ValyanClinic/Program.cs`

```csharp
// Add after MediatR registration
builder.Services.AddScoped<IPacientDataService, PacientDataService>();

_logger.LogInformation("✅ Application Services registered (IPacientDataService)");
```

#### **Step 2.4: Update AdministrarePacienti.razor.cs (1 hour)**

**Changes:**

1. **Inject IPacientDataService:**
```csharp
[Inject] private IPacientDataService DataService { get; set; } = default!;
```

2. **Add Pagination State:**
```csharp
// Pagination State
private int CurrentPage { get; set; } = 1;
private int PageSize { get; set; } = 25;
private int TotalPages { get; set; }
private int TotalRecords { get; set; }
```

3. **Update LoadDataAsync():**
```csharp
private async Task LoadDataAsync()
{
    if (_disposed) return;

    IsLoading = true;
    HasError = false;
    ErrorMessage = null;

    try
    {
        // Create filters
        var filters = new PacientFilters
        {
            SearchText = SearchText,
            Judet = FilterJudet,
            Asigurat = !string.IsNullOrEmpty(FilterAsigurat) ? bool.Parse(FilterAsigurat) : null,
            Activ = !string.IsNullOrEmpty(FilterActiv) ? bool.Parse(FilterActiv) : null
        };

        // Create pagination options
        var pagination = new PaginationOptions
        {
            PageNumber = CurrentPage,
            PageSize = PageSize
        };

        // Create sort options
        var sorting = new SortOptions
        {
            SortColumn = "Nume",
            SortDirection = "ASC"
        };

        // Load data through service
        var result = await DataService.LoadPagedDataAsync(filters, pagination, sorting);

        if (_disposed) return;

        if (result.IsSuccess && result.Value != null)
        {
            AllPacienti = result.Value.Value?.ToList() ?? new List<PacientListDto>();
            TotalRecords = result.Value.TotalRecords;
            TotalPages = result.Value.TotalPages;
            
            Logger.LogInformation(
                "Loaded page {CurrentPage}/{TotalPages} ({Count} records)",
                CurrentPage, TotalPages, AllPacienti.Count);
        }
        else
        {
            HasError = true;
            ErrorMessage = result.FirstError ?? "Eroare la încărcarea datelor.";
            AllPacienti = new List<PacientListDto>();
        }
    }
    catch (ObjectDisposedException)
    {
        Logger.LogDebug("Component disposed while loading data (navigation away)");
    }
    catch (Exception ex)
    {
        if (!_disposed)
        {
            HasError = true;
            ErrorMessage = $"Eroare neașteptată: {ex.Message}";
            AllPacienti = new List<PacientListDto>();
        }
    }
    finally
    {
        if (!_disposed)
        {
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
```

4. **Add Pagination Methods:**
```csharp
#region Pagination Methods

private async Task GoToPage(int page)
{
    if (_disposed || page < 1 || page > TotalPages) return;
    
    CurrentPage = page;
    await LoadDataAsync();
}

private async Task GoToFirstPage() => await GoToPage(1);
private async Task GoToPreviousPage() => await GoToPage(CurrentPage - 1);
private async Task GoToNextPage() => await GoToPage(CurrentPage + 1);
private async Task GoToLastPage() => await GoToPage(TotalPages);

private async Task ChangePageSize(int newPageSize)
{
    if (_disposed) return;
    
    PageSize = newPageSize;
    CurrentPage = 1; // Reset to first page
    await LoadDataAsync();
}

#endregion
```

5. **Remove Client-Side Filtering:**
```csharp
// ❌ DELETE THIS METHOD (no longer needed)
// private List<PacientListDto> ApplyClientFilters() { ... }

// ✅ REPLACE with simple property
private List<PacientListDto> FilteredPacienti => AllPacienti ?? new List<PacientListDto>();
```

#### **Step 2.5: Add Pagination UI Controls (30 min)**

**File:** `ValyanClinic/Components/Pages/Pacienti/AdministrarePacienti.razor`

Add after `<div class="data-grid-section">`:

```razor
<!-- Pagination Controls -->
@if (!IsLoading && !HasError && FilteredPacienti.Any())
{
    <div class="pagination-container">
        <div class="pagination-info">
            <span>
                Afișare <strong>@((CurrentPage - 1) * PageSize + 1)</strong> 
                - <strong>@Math.Min(CurrentPage * PageSize, TotalRecords)</strong> 
                din <strong>@TotalRecords</strong> înregistrări
            </span>
        </div>
        
        <div class="pagination-controls">
            <button class="btn btn-sm btn-outline-secondary" 
                    @onclick="GoToFirstPage" 
                    disabled="@(CurrentPage == 1)">
                <i class="fas fa-angle-double-left"></i> Prima
            </button>
            
            <button class="btn btn-sm btn-outline-secondary" 
                    @onclick="GoToPreviousPage" 
                    disabled="@(CurrentPage == 1)">
                <i class="fas fa-angle-left"></i> Anterioara
            </button>
            
            <span class="pagination-page-info">
                Pagina <strong>@CurrentPage</strong> din <strong>@TotalPages</strong>
            </span>
            
            <button class="btn btn-sm btn-outline-secondary" 
                    @onclick="GoToNextPage" 
                    disabled="@(CurrentPage >= TotalPages)">
                Următoarea <i class="fas fa-angle-right"></i>
            </button>
            
            <button class="btn btn-sm btn-outline-secondary" 
                    @onclick="GoToLastPage" 
                    disabled="@(CurrentPage >= TotalPages)">
                Ultima <i class="fas fa-angle-double-right"></i>
            </button>
        </div>
        
        <div class="pagination-page-size">
            <label>Înregistrări/pagină:</label>
            <select class="form-select form-select-sm" 
                    @bind="PageSize" 
                    @bind:after="@(() => ChangePageSize(PageSize))">
                <option value="25">25</option>
                <option value="50">50</option>
                <option value="100">100</option>
            </select>
        </div>
    </div>
}
```

#### **Step 2.6: Add Pagination CSS Styles (15 min)**

**File:** `ValyanClinic/Components/Pages/Pacienti/AdministrarePacienti.razor.css`

```css
/* ========================================
   PAGINATION CONTROLS
   ======================================== */

.pagination-container {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1rem 0;
    margin-top: 1rem;
    border-top: 2px solid #e5e7eb;
    flex-shrink: 0;
}

.pagination-info {
    font-size: var(--font-size-sm);
    color: #6b7280;
}

.pagination-info strong {
    color: #374151;
    font-weight: var(--font-weight-semibold);
}

.pagination-controls {
    display: flex;
    gap: 0.5rem;
    align-items: center;
}

.pagination-page-info {
    font-size: var(--font-size-sm);
    color: #374151;
    padding: 0 1rem;
}

.pagination-page-info strong {
    font-weight: var(--font-weight-semibold);
}

.pagination-page-size {
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.pagination-page-size label {
    font-size: var(--font-size-sm);
    color: #6b7280;
    margin: 0;
}

.pagination-page-size .form-select {
    width: auto;
    padding: 0.375rem 2rem 0.375rem 0.75rem;
}

.btn-outline-secondary {
    background: white;
    border: 1px solid #d1d5db;
    color: #374151;
    padding: 0.5rem 1rem;
    border-radius: 6px;
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    cursor: pointer;
    transition: all 0.2s ease;
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
}

.btn-outline-secondary:hover:not(:disabled) {
    background: #f3f4f6;
    border-color: #9ca3af;
}

.btn-outline-secondary:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}

/* Responsive */
@media (max-width: 768px) {
    .pagination-container {
        flex-direction: column;
        gap: 1rem;
        align-items: stretch;
    }
    
    .pagination-controls {
        justify-content: center;
    }
    
    .pagination-info,
    .pagination-page-size {
        justify-content: center;
        text-align: center;
    }
}
```

#### **Step 2.7: Test Server-Side Pagination (30 min)**

**Test Plan:**

1. **Test Basic Pagination:**
   - Navigate to AdministrarePacienti
   - Verify: Page 1 shows first 25 records
   - Click "Următoarea" → Verify: Page 2 shows next 25 records
   - Click "Ultima" → Verify: Last page shows remaining records
   - Click "Prima" → Verify: Back to page 1

2. **Test Page Size Change:**
   - Change dropdown: 25 → 50
   - Verify: Page shows 50 records
   - Verify: Total pages recalculated correctly

3. **Test Filters with Pagination:**
   - Search "Ion" → Verify: Pagination resets to page 1
   - Verify: Total records updated based on filter
   - Navigate to page 2 → Clear filter
   - Verify: Pagination resets again

4. **Test Performance:**
   - Use SQL to insert 15,000 test records
   - Navigate to AdministrarePacienti
   - Measure load time: **Should be <2 seconds**
   - Verify: NO browser freeze
   - Verify: SignalR stable (no disconnects)

**SQL Test Data:**
```sql
-- Insert 15,000 test records (run în SQL Server Management Studio)
DECLARE @i INT = 1
WHILE @i <= 15000
BEGIN
    INSERT INTO Pacienti (Id, Cod_Pacient, Nume, Prenume, Data_Nasterii, Sex, Activ, Data_Inregistrare, Creat_De)
    VALUES (
        NEWID(),
        'TEST' + RIGHT('00000' + CAST(@i AS VARCHAR), 5),
        'Test',
        'Pacient' + CAST(@i AS VARCHAR),
        DATEADD(YEAR, -30, GETDATE()),
        CASE WHEN @i % 2 = 0 THEN 'M' ELSE 'F' END,
        1,
        GETDATE(),
        'System'
    )
    SET @i = @i + 1
END
```

**Success Criteria:**
- ✅ Page load time <2 seconds (with 15K+ records)
- ✅ Browser responsive (NO freeze)
- ✅ Pagination UI functional (all buttons work)
- ✅ Filters reset pagination to page 1
- ✅ SignalR connection stable

---

### **Task 3: Add Unit Tests pentru Handlers (8-12 hours)**

**Why:** ⚠️ **MAINTENANCE RISK** - Zero confidence în regression detection

#### **Step 3.1: Setup Test Project Infrastructure (1 hour)**

**File:** `ValyanClinic.Tests/ValyanClinic.Tests.csproj`

Verify dependencies:
```xml
<ItemGroup>
  <PackageReference Include="xunit" Version="2.9.3" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  <PackageReference Include="FluentAssertions" Version="7.0.0" />
  <PackageReference Include="Moq" Version="4.20.72" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
  <PackageReference Include="coverlet.collector" Version="6.0.2" />
</ItemGroup>
```

**Create Base Test Class:**

**File:** `ValyanClinic.Tests/Application/PacientManagement/PacientTestBase.cs` (NEW)

```csharp
using Microsoft.Extensions.Logging;
using Moq;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Tests.Application.PacientManagement;

/// <summary>
/// Base class pentru teste pacient management
/// Provides common mocks și setup logic
/// </summary>
public abstract class PacientTestBase
{
    protected readonly Mock<IPacientRepository> MockRepository;
    protected readonly Mock<ILogger> MockLogger;

    protected PacientTestBase()
    {
        MockRepository = new Mock<IPacientRepository>();
        MockLogger = new Mock<ILogger>();
    }

    /// <summary>
    /// Reset all mocks to default state
    /// </summary>
    protected void ResetMocks()
    {
        MockRepository.Reset();
        MockLogger.Reset();
    }
}
```

#### **Step 3.2: Create CreatePacientCommandHandlerTests (2 hours)**

**File:** `ValyanClinic.Tests/Application/PacientManagement/Commands/CreatePacientCommandHandlerTests.cs` (NEW)

```csharp
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ValyanClinic.Application.Features.PacientManagement.Commands.CreatePacient;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using Xunit;

namespace ValyanClinic.Tests.Application.PacientManagement.Commands;

/// <summary>
/// Unit tests pentru CreatePacientCommandHandler
/// Pattern: AAA (Arrange, Act, Assert)
/// </summary>
public class CreatePacientCommandHandlerTests : PacientTestBase
{
    private readonly CreatePacientCommandHandler _handler;

    public CreatePacientCommandHandlerTests()
    {
        _handler = new CreatePacientCommandHandler(
            MockRepository.Object,
            MockLogger.Object as ILogger<CreatePacientCommandHandler>);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreatePacient()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            CNP = "1234567890123",
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M",
            Activ = true,
            CreatDe = "System"
        };

        var createdPacient = new Pacient
        {
            Id = Guid.NewGuid(),
            Cod_Pacient = "P00001",
            Nume = command.Nume,
            Prenume = command.Prenume,
            CNP = command.CNP
        };

        MockRepository
            .Setup(r => r.CheckUniqueAsync(command.CNP, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, false));

        MockRepository
            .Setup(r => r.GenerateNextCodPacientAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("P00001");

        MockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPacient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        result.SuccessMessage.Should().Contain("Popescu Ion");

        MockRepository.Verify(
            r => r.CreateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommandWithoutCNP_ShouldCreatePacient()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Test",
            Prenume = "User",
            CNP = null, // ← NO CNP provided
            Data_Nasterii = new DateTime(1995, 5, 15),
            Sex = "F",
            Activ = true,
            CreatDe = "System"
        };

        var createdPacient = new Pacient { Id = Guid.NewGuid(), Cod_Pacient = "P00002" };

        MockRepository
            .Setup(r => r.GenerateNextCodPacientAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("P00002");

        MockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPacient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        MockRepository.Verify(
            r => r.CheckUniqueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never); // ← Should NOT check uniqueness dacă CNP is null
    }

    #endregion

    #region Validation Error Tests

    [Fact]
    public async Task Handle_MissingNume_ShouldReturnError()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "", // ← INVALID: Empty
            Prenume = "Ion",
            Data_Nasterii = DateTime.Now.AddYears(-30),
            Sex = "M",
            CreatDe = "System"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Numele este obligatoriu"));
    }

    [Fact]
    public async Task Handle_MissingPrenume_ShouldReturnError()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "", // ← INVALID: Empty
            Data_Nasterii = DateTime.Now.AddYears(-30),
            Sex = "M",
            CreatDe = "System"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Prenumele este obligatoriu"));
    }

    [Fact]
    public async Task Handle_InvalidCNPLength_ShouldReturnError()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            CNP = "12345", // ← INVALID: Only 5 digits (trebuie 13)
            Data_Nasterii = DateTime.Now.AddYears(-30),
            Sex = "M",
            CreatDe = "System"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("CNP-ul trebuie să conțină exact 13 cifre"));
    }

    [Fact]
    public async Task Handle_CNPWithLetters_ShouldReturnError()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            CNP = "123456789ABCD", // ← INVALID: Contains letters
            Data_Nasterii = DateTime.Now.AddYears(-30),
            Sex = "M",
            CreatDe = "System"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("CNP-ul trebuie să conțină exact 13 cifre"));
    }

    [Fact]
    public async Task Handle_DuplicateCNP_ShouldReturnError()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            CNP = "1234567890123",
            Data_Nasterii = DateTime.Now.AddYears(-30),
            Sex = "M",
            CreatDe = "System"
        };

        MockRepository
            .Setup(r => r.CheckUniqueAsync(command.CNP, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, false)); // ← CNP already exists

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Un pacient cu CNP-ul 1234567890123 există deja"));
    }

    [Fact]
    public async Task Handle_InvalidSex_ShouldReturnError()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            Data_Nasterii = DateTime.Now.AddYears(-30),
            Sex = "X", // ← INVALID: Only M/F allowed
            CreatDe = "System"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Sexul trebuie să fie 'M' (Masculin) sau 'F' (Feminin)"));
    }

    [Fact]
    public async Task Handle_FutureDataNasterii_ShouldReturnError()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            Data_Nasterii = DateTime.Now.AddYears(1), // ← INVALID: Future date
            Sex = "M",
            CreatDe = "System"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Data nașterii nu poate fi în viitor"));
    }

    [Fact]
    public async Task Handle_DataNasteriiTooOld_ShouldReturnError()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            Data_Nasterii = new DateTime(1899, 12, 31), // ← INVALID: Before 1900
            Sex = "M",
            CreatDe = "System"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Data nașterii este invalidă"));
    }

    #endregion

    #region Repository Exception Tests

    [Fact]
    public async Task Handle_RepositoryException_ShouldReturnError()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            Data_Nasterii = DateTime.Now.AddYears(-30),
            Sex = "M",
            CreatDe = "System"
        };

        MockRepository
            .Setup(r => r.GenerateNextCodPacientAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Contain("Eroare la crearea pacientului");
    }

    #endregion
}
```

**Note:** Similar tests trebuie create pentru:
- `UpdatePacientCommandHandlerTests.cs` (8-10 tests)
- `DeletePacientCommandHandlerTests.cs` (5-6 tests)
- `GetPacientListQueryHandlerTests.cs` (6-8 tests)
- `GetPacientByIdQueryHandlerTests.cs` (5-6 tests)

**Estimate:** 6-8 ore pentru toate test files

#### **Step 3.3: Run Unit Tests & Verify Coverage (30 min)**

**Commands:**
```bash
# Run all tests
dotnet test ValyanClinic.Tests/ValyanClinic.Tests.csproj

# Run with code coverage
dotnet test ValyanClinic.Tests/ValyanClinic.Tests.csproj --collect:"XPlat Code Coverage"

# Generate coverage report (install reportgenerator first)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# Open coverage report
start coveragereport/index.html
```

**Success Criteria:**
- ✅ ALL tests pass (0 failures)
- ✅ Code coverage >80% pentru PacientManagement handlers
- ✅ Coverage report shows green pentru business logic critical paths

---

### **Task 4: Add Integration Tests (2-3 hours)**

**File:** `ValyanClinic.Tests/Integration/PacientManagementIntegrationTests.cs` (NEW)

```csharp
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ValyanClinic.Application.Features.PacientManagement.Commands.CreatePacient;
using ValyanClinic.Application.Features.PacientManagement.Commands.UpdatePacient;
using ValyanClinic.Application.Features.PacientManagement.Commands.DeletePacient;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;
using Xunit;

namespace ValyanClinic.Tests.Integration;

/// <summary>
/// Integration tests pentru complete workflows
/// Tests full flow: UI → MediatR → Application → Infrastructure → Database
/// </summary>
[Collection("Integration Tests")]
public class PacientManagementIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IMediator _mediator;

    public PacientManagementIntegrationTests(IntegrationTestFixture fixture)
    {
        _mediator = fixture.ServiceProvider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task CreateEditDeletePacient_CompleteWorkflow_ShouldSucceed()
    {
        // STEP 1: CREATE
        var createCommand = new CreatePacientCommand
        {
            Nume = "IntegrationTest",
            Prenume = "Pacient",
            CNP = "1234567890199", // Unique CNP for test
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M",
            Activ = true,
            CreatDe = "IntegrationTest"
        };

        var createResult = await _mediator.Send(createCommand);
        
        createResult.IsSuccess.Should().BeTrue();
        createResult.Value.Should().NotBe(Guid.Empty);
        var pacientId = createResult.Value;

        // STEP 2: GET BY ID (Verify creation)
        var getQuery = new GetPacientByIdQuery(pacientId);
        var getResult = await _mediator.Send(getQuery);
        
        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Should().NotBeNull();
        getResult.Value!.Nume.Should().Be("IntegrationTest");
        getResult.Value.Prenume.Should().Be("Pacient");

        // STEP 3: UPDATE
        var updateCommand = new UpdatePacientCommand
        {
            Id = pacientId,
            Nume = "IntegrationTest",
            Prenume = "PacientUpdated", // ← CHANGED
            CNP = "1234567890199",
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M",
            Telefon = "0721234567", // ← ADDED
            Activ = true,
            ModificatDe = "IntegrationTest"
        };

        var updateResult = await _mediator.Send(updateCommand);
        
        updateResult.IsSuccess.Should().BeTrue();

        // STEP 4: GET BY ID (Verify update)
        var getUpdatedQuery = new GetPacientByIdQuery(pacientId);
        var getUpdatedResult = await _mediator.Send(getUpdatedQuery);
        
        getUpdatedResult.IsSuccess.Should().BeTrue();
        getUpdatedResult.Value!.Prenume.Should().Be("PacientUpdated");
        getUpdatedResult.Value.Telefon.Should().Be("0721234567");

        // STEP 5: DELETE (soft delete)
        var deleteCommand = new DeletePacientCommand(pacientId, "IntegrationTest", hardDelete: false);
        var deleteResult = await _mediator.Send(deleteCommand);
        
        deleteResult.IsSuccess.Should().BeTrue();

        // STEP 6: GET BY ID (Verify soft delete - Activ=false)
        var getDeletedQuery = new GetPacientByIdQuery(pacientId);
        var getDeletedResult = await _mediator.Send(getDeletedQuery);
        
        getDeletedResult.IsSuccess.Should().BeTrue();
        getDeletedResult.Value!.Activ.Should().BeFalse(); // ← Soft deleted

        // CLEANUP: Hard delete
        var hardDeleteCommand = new DeletePacientCommand(pacientId, "IntegrationTest", hardDelete: true);
        await _mediator.Send(hardDeleteCommand);
    }

    [Fact]
    public async Task SearchAndFilterPacienti_ShouldReturnFilteredResults()
    {
        // Arrange: Create 3 test patients
        var testPacienti = new[]
        {
            new CreatePacientCommand { Nume = "SearchTest1", Prenume = "Alpha", Data_Nasterii = DateTime.Now.AddYears(-25), Sex = "M", Judet = "Bucuresti", Asigurat = true, Activ = true, CreatDe = "Test" },
            new CreatePacientCommand { Nume = "SearchTest2", Prenume = "Beta", Data_Nasterii = DateTime.Now.AddYears(-30), Sex = "F", Judet = "Cluj", Asigurat = false, Activ = true, CreatDe = "Test" },
            new CreatePacientCommand { Nume = "SearchTest3", Prenume = "Gamma", Data_Nasterii = DateTime.Now.AddYears(-35), Sex = "M", Judet = "Bucuresti", Asigurat = true, Activ = false, CreatDe = "Test" }
        };

        var createdIds = new List<Guid>();
        foreach (var cmd in testPacienti)
        {
            var result = await _mediator.Send(cmd);
            result.IsSuccess.Should().BeTrue();
            createdIds.Add(result.Value);
        }

        try
        {
            // TEST 1: Search by name
            var searchQuery = new GetPacientListQuery { SearchText = "SearchTest", PageSize = 100 };
            var searchResult = await _mediator.Send(searchQuery);
            
            searchResult.IsSuccess.Should().BeTrue();
            searchResult.Value!.Value.Should().HaveCountGreaterOrEqualTo(3);

            // TEST 2: Filter by Judet
            var filterJudetQuery = new GetPacientListQuery { Judet = "Bucuresti", PageSize = 100 };
            var filterJudetResult = await _mediator.Send(filterJudetQuery);
            
            filterJudetResult.IsSuccess.Should().BeTrue();
            filterJudetResult.Value!.Value.Should().Contain(p => p.Nume == "SearchTest1");
            filterJudetResult.Value.Value.Should().Contain(p => p.Nume == "SearchTest3");

            // TEST 3: Filter by Asigurat
            var filterAsiguratQuery = new GetPacientListQuery { Asigurat = true, SearchText = "SearchTest", PageSize = 100 };
            var filterAsiguratResult = await _mediator.Send(filterAsiguratQuery);
            
            filterAsiguratResult.IsSuccess.Should().BeTrue();
            filterAsiguratResult.Value!.Value.Should().HaveCount(2); // SearchTest1 + SearchTest3

            // TEST 4: Filter by Activ
            var filterActivQuery = new GetPacientListQuery { Activ = true, SearchText = "SearchTest", PageSize = 100 };
            var filterActivResult = await _mediator.Send(filterActivQuery);
            
            filterActivResult.IsSuccess.Should().BeTrue();
            filterActivResult.Value!.Value.Should().HaveCount(2); // SearchTest1 + SearchTest2
        }
        finally
        {
            // CLEANUP
            foreach (var id in createdIds)
            {
                await _mediator.Send(new DeletePacientCommand(id, "Test", hardDelete: true));
            }
        }
    }
}
```

**Note:** Trebuie create și:
- `IntegrationTestFixture.cs` - Setup DI container pentru tests
- Database seed/cleanup helpers

**Estimate:** 2-3 ore implementation + testing

---

## 🟡 **P2 - CODE QUALITY & PERFORMANCE (2.5 hours)**

### **Task 5: Replace Hardcoded CSS Values (1 hour)**

[CONTINUES WITH DETAILED CSS REFACTOR STEPS...]

### **Task 6: Add @key Directive (30 min)**

[CONTINUES WITH @KEY IMPLEMENTATION...]

### **Task 7: Optimize StateHasChanged() (1 hour)**

[CONTINUES WITH STATE OPTIMIZATION...]

---

## 🟢 **P3 - POLISH (20 minutes)**

### **Task 8: Update Debounce Timer (5 min)**

[CONTINUES WITH DEBOUNCE FIX...]

### **Task 9: Fix Responsive Breakpoints (15 min)**

[CONTINUES WITH RESPONSIVE FIX...]

---

## ✅ **Final Verification Checklist**

**Before Marking Complete:**
- [ ] P0: [Authorize] added to ALL pages/modals
- [ ] P0: Security test passed (anonymous blocked)
- [ ] P1: Server-side pagination implemented
- [ ] P1: Performance test passed (<2s load with 15K records)
- [ ] P1: Unit tests created (>80% coverage)
- [ ] P1: Integration tests created and passing
- [ ] P2: CSS variables replaced (15+ locations)
- [ ] P2: @key directive added
- [ ] P2: StateHasChanged() optimized
- [ ] P3: Debounce updated to 500ms
- [ ] P3: Responsive breakpoints fixed
- [ ] Build: 0 errors, 0 warnings
- [ ] Tests: ALL passing (unit + integration)
- [ ] UI: NO visual regressions

**Grade Verification:**
- [ ] After P0: Grade C (70/100) ✅
- [ ] After P1: Grade B (85/100) ✅
- [ ] After P2: Grade A (92/100) ✅
- [ ] After P3: Grade A+ (95/100) ✅

---

**Status:** 📋 **READY FOR IMPLEMENTATION**  
**Estimated Total Time:** 14-19 hours  
**Target Completion:** [YOUR SPRINT END DATE]

---

*END OF PLAN*
