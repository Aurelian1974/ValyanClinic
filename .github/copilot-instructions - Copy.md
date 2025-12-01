# ValyanClinic Project Instructions

## 🎯 Project Overview
ValyanClinic is a comprehensive medical clinic management system built with .NET 9 Blazor Server, following Clean Architecture principles.

---

## 📋 DEVELOPMENT CHECKLIST (FOLLOW IN ORDER)

### ✅ **STEP 0: Initial Analysis & Documentation**
**⚠️ CRITICAL: Execute BEFORE any code changes!**

1. **Create Analysis Document** → `DevSupport/Analysis/[TaskName]-Analysis-[Date].md`
   - Document current state of the system
   - Identify all affected components/files
   - List dependencies and impacts
   - Define scope and approach
   
2. **Read & Understand Solution Structure**
   - Review Clean Architecture layers (Domain → Application → Infrastructure → Presentation)
   - Understand existing patterns (MediatR, Repository, Services)
   - Check related components/modals/pages
   
3. **Dependency Check**
   - Identify all files that depend on components being modified
   - Check for shared services, DTOs, interfaces
   - Review database schema if data layer is affected
   - Verify third-party library usage (Syncfusion, etc.)

**✅ Update Analysis Document after EACH major step!**

---

### ✅ **STEP 1: Architecture & Structure (MANDATORY)**

| Rule | Description | Priority |
|------|-------------|----------|
| **Clean Architecture** | Domain → Application → Infrastructure → Presentation | 🔴 CRITICAL |
| **SOLID Principles** | Single Responsibility, Dependency Injection, Interface Segregation | 🔴 CRITICAL |
| **MediatR Pattern** | ALL business operations through Commands/Queries | 🔴 CRITICAL |
| **Repository Pattern** | Data access ONLY through repositories | 🔴 CRITICAL |
| **Service Extraction** | Extract complex logic (>200 lines) to Application Services | 🟡 HIGH |

**File Organization:**
```
Component/
├── Component.razor         # Markup ONLY
├── Component.razor.cs      # Logic ONLY (no UI)
└── Component.razor.css     # Scoped styles ONLY
```

---

### ✅ **STEP 2: Code Separation (MANDATORY)**

| Rule | Description | Violation = REJECT |
|------|-------------|-------------------|
| **NO Logic in .razor** | ONLY markup, bindings, simple conditionals | ❌ Complex logic in @code{} |
| **ALL Logic in .razor.cs** | State management, service calls, business rules | ❌ Inline lambdas for complex ops |
| **Scoped CSS ONLY** | Each component has `.razor.css` | ❌ Global CSS pollution |
| **CSS Variables** | Use variables.css, NO hardcoded values | ❌ Hardcoded colors/sizes |

---

### ✅ **STEP 3: Design System (STRICT ENFORCEMENT)**

| Element | Color/Style | Never Use |
|---------|-------------|-----------|
| **Page/Modal Headers** | `linear-gradient(135deg, #93c5fd, #60a5fa)` | ❌ Green/Purple |
| **Primary Buttons** | `linear-gradient(135deg, #60a5fa, #3b82f6)` | ❌ Custom colors |
| **Hover States** | `#eff6ff` background + `#60a5fa` border | ❌ Dark blue |
| **Success** | `#6ee7b7` (Emerald 300 pastel) | ❌ Bright green |
| **Danger** | `#fca5a5` (Red 300 pastel) | ❌ Dark red |

**Typography:**
- Page Header: `var(--font-size-3xl)` (28px) + `var(--font-weight-bold)`
- Modal Header: `var(--font-size-2xl)` (22px) + `var(--font-weight-semibold)`
- Labels: `var(--font-size-sm)` (13px) + uppercase
- Body: `var(--font-size-base)` (14px)

**Responsive Breakpoints:**
- Mobile: Base styles (12px padding)
- Tablet: `@media (min-width: 768px)` (20px padding)
- Desktop: `@media (min-width: 1024px)` (32px padding)
- Large: `@media (min-width: 1400px)` (max-width: 1800px)

---

### ✅ **STEP 4: Data & Business Logic (MANDATORY)**

| Pattern | When to Use | Example |
|---------|-------------|---------|
| **MediatR Command** | Create, Update, Delete operations | `CreatePersonalCommand` |
| **MediatR Query** | Read operations | `GetPersonalByIdQuery` |
| **Application Service** | Complex logic, reusable business rules | `IPacientDataService` |
| **Repository** | Database access ONLY | `IPersonalRepository` |

**Service Extraction Criteria:**
- ✅ Complex filtering/sorting/pagination logic
- ✅ Logic >200 lines in component
- ✅ Needs reuse across multiple components
- ✅ Testing requires >5 UI dependency mocks

---

### ✅ **STEP 5: Testing Strategy (ENFORCE COVERAGE)**

| Test Type | Tool | Coverage Goal | When |
|-----------|------|---------------|------|
| **Unit Tests** | xUnit + FluentAssertions + Moq | 80-90% | Business logic, MediatR handlers, Services |
| **Component Tests** | bUnit | 60-70% | Simple modals/forms (no Syncfusion) |
| **Integration Tests** | Playwright | 100% critical paths | Complex UI workflows, E2E |

**Unit Test Template (AAA Pattern):**
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange - Setup mocks, data
    // Act - Execute method
    // Assert - Verify results with FluentAssertions
}
```

**Playwright Best Practices:**
- ✅ Use semantic locators (`GetByRole`, `GetByLabel`)
- ✅ Add `data-testid` attributes to key elements
- ✅ Use auto-wait (no `Thread.Sleep`!)
- ✅ Record videos for failed tests

---

### ✅ **STEP 6: Security & Validation (NON-NEGOTIABLE)**

| Rule | Implementation | Violation = SECURITY RISK |
|------|----------------|---------------------------|
| **Authentication** | `[Authorize]` attribute on pages | ❌ Unprotected sensitive pages |
| **Input Validation** | FluentValidation on ALL commands | ❌ Trusting client data |
| **Parameterized Queries** | Use Dapper/EF Core (automatic) | ❌ String concatenation SQL |
| **Sanitize Output** | NO raw HTML without encoding | ❌ XSS vulnerabilities |
| **NO Sensitive Logs** | NEVER log passwords, CNP, cards | ❌ Security breach |

---

### ✅ **STEP 7: Performance (BLAZOR SERVER SPECIFIC)**

| Optimization | How | Why |
|--------------|-----|-----|
| **@key directive** | Use on dynamic lists | Prevent unnecessary re-renders |
| **ShouldRender()** | Override for expensive components | Control render frequency |
| **StateHasChanged()** | Call ONLY when needed | Reduce SignalR traffic |
| **Pagination** | Server-side, NOT client-side | Handle large datasets |
| **Dispose** | Implement `IDisposable` for subscriptions | Prevent memory leaks |

---

### ✅ **STEP 8: Code Quality (BEFORE COMMIT)**

**Automated Checks:**
- [ ] Build succeeds (0 errors, 0 warnings)
- [ ] All unit tests pass (>80% coverage)
- [ ] Integration tests pass (critical paths)
- [ ] No StyleCop/Analyzer violations

**Manual Review:**
- [ ] Blue theme applied (no green/purple)
- [ ] Scoped CSS used (`.razor.css` exists)
- [ ] No logic in `.razor` files
- [ ] CSS variables used (no hardcoded values)
- [ ] XML documentation on public APIs
- [ ] Error handling with try-catch
- [ ] Async/await used correctly
- [ ] Responsive design tested (mobile/tablet/desktop)

---

### ✅ **STEP 9: Documentation & Handoff (MANDATORY)**

1. **Update Analysis Document** → `DevSupport/Analysis/[TaskName]-Analysis-[Date].md`
   - Mark completed steps ✅
   - Document decisions made
   - List all modified files
   - Note any breaking changes

2. **Create Final Documentation** → `DevSupport/Completed/[TaskName]-Final-[Date].md`
   - **Summary:** What was implemented
   - **Files Changed:** Complete list with descriptions
   - **Testing:** Unit/Integration test results
   - **Breaking Changes:** Migration guide if applicable
   - **Screenshots:** Before/After (if UI changes)
   - **Known Issues:** Any deferred work or limitations

3. **Commit Message (Conventional Commits):**
   ```
   feat: Add patient search functionality
   fix: Resolve consultatie modal styling issue
   refactor: Extract IMC calculation to service
   test: Add unit tests for PersonalService
   docs: Update API documentation
   ```

---

## 🔍 Key Files Reference

| File | Purpose |
|------|---------|
| `ValyanClinic/wwwroot/css/variables.css` | Color/Typography variables |
| `ValyanClinic/wwwroot/css/base.css` | Global base styles |
| `DevSupport/Typography/Cheat-Sheet.md` | Typography guide |
| `.github/copilot-instructions.md` | This file |

---

## ⚠️ CRITICAL RULES (NEVER VIOLATE)

1. **📖 READ FIRST:** Understand solution structure before ANY changes
2. **🔗 CHECK DEPENDENCIES:** Identify all dependent components/files
3. **📝 DOCUMENT FIRST:** Create analysis document BEFORE coding
4. **🎨 BLUE THEME ONLY:** NO green/purple for primary elements
5. **🔒 SCOPED CSS ONLY:** NO global CSS pollution
6. **🚫 NO LOGIC IN .razor:** ALL logic in `.razor.cs`
7. **✅ CSS VARIABLES:** NO hardcoded colors/sizes
8. **🧪 TEST EVERYTHING:** Unit tests for business logic (80%+)
9. **🔐 VALIDATE INPUT:** FluentValidation on ALL commands
10. **📄 DOCUMENT FINAL:** Create completion document with ALL changes

---

**Status:** ✅ **STREAMLINED CHECKLIST - v3.0**  
**Last Updated:** January 2025  
**Project:** ValyanClinic - Medical Clinic Management System

---

## 📚 Detailed Guidelines (Reference Only)

<details>
<summary><strong>Click to expand: Clean Architecture Details</strong></summary>

### Clean Architecture Layers
- **Domain Layer** (`ValyanClinic.Domain`) - Core business entities and interfaces
- **Application Layer** (`ValyanClinic.Application`) - Business logic, DTOs, MediatR handlers, Services
- **Infrastructure Layer** (`ValyanClinic.Infrastructure`) - Data access, external services
- **Presentation Layer** (`ValyanClinic`) - Blazor Server UI components

### Dependency Flow
- ✅ Presentation → Application → Domain (ALLOWED)
- ❌ Domain → Infrastructure (FORBIDDEN)
- ❌ Domain → Application (FORBIDDEN)

</details>

<details>
<summary><strong>Click to expand: MediatR Pattern Examples</strong></summary>

```csharp
// Command (Write operation)
public record CreatePersonalCommand(string Nume, string Prenume) : IRequest<Result<Guid>>;

// Command Handler
public class CreatePersonalCommandHandler : IRequestHandler<CreatePersonalCommand, Result<Guid>>
{
    private readonly IPersonalRepository _repository;
    
    public CreatePersonalCommandHandler(IPersonalRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<Result<Guid>> Handle(CreatePersonalCommand request, CancellationToken cancellationToken)
    {
        var personal = new Personal
        {
            Nume = request.Nume,
            Prenume = request.Prenume
        };
        
        await _repository.AddAsync(personal);
        return Result<Guid>.Success(personal.Id);
    }
}

// Query (Read operation)
public record GetPersonalByIdQuery(Guid Id) : IRequest<Result<PersonalDto>>;

// Component Usage
[Inject] private IMediator Mediator { get; set; } = default!;

private async Task HandleCreateAsync()
{
    var command = new CreatePersonalCommand(Nume, Prenume);
    var result = await Mediator.Send(command);
    
    if (result.IsSuccess)
    {
        // Success handling
    }
    else
    {
        // Error handling
    }
}
```

</details>

<details>
<summary><strong>Click to expand: Service Extraction Pattern</strong></summary>

### When to Extract Business Logic to Services

✅ **Extract when:**
- Component has complex filtering/sorting/pagination logic
- Business rules need to be reused across multiple components
- Component logic exceeds **~200 lines** in code-behind
- Unit testing component requires mocking **>5 UI dependencies**

❌ **Keep in component when:**
- Simple UI state management (show/hide modal, toggle flags)
- Direct EventCallback invocations
- Simple parameter binding

### Example: Application Service

```csharp
// ValyanClinic.Application/Services/Pacienti/IPacientDataService.cs
public interface IPacientDataService
{
    Task<Result<PagedPacientData>> LoadPagedDataAsync(
        PacientFilters filters,
        PaginationOptions pagination,
        SortOptions sorting,
        CancellationToken cancellationToken = default);
}

// Component becomes simple
public partial class VizualizarePacienti : ComponentBase
{
    [Inject] private IPacientDataService DataService { get; set; } = default!;
    
    private async Task LoadPagedData()
    {
        var result = await DataService.LoadPagedDataAsync(filters, pagination, sorting);
        // Handle result (UI logic only)
    }
}
```

</details>

<details>
<summary><strong>Click to expand: Playwright Integration Testing</strong></summary>

### Setup

```xml
<!-- ValyanClinic.Tests.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.Playwright" Version="1.47.0" />
  <PackageReference Include="Microsoft.Playwright.NUnit" Version="1.47.0" />
  <PackageReference Include="xunit" Version="2.9.3" />
</ItemGroup>
```

### Base Test Class

```csharp
public abstract class PlaywrightTestBase : IAsyncLifetime
{
    protected IPlaywright Playwright { get; private set; } = default!;
    protected IBrowser Browser { get; private set; } = default!;
    protected IPage Page { get; private set; } = default!;
    
    protected string BaseUrl { get; } = "https://localhost:5001";
    
    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new()
        {
            Headless = true,
            SlowMo = 50
        });
        Page = await Browser.NewPageAsync();
    }
    
    protected async Task NavigateToAsync(string relativeUrl)
    {
        await Page.GotoAsync($"{BaseUrl}{relativeUrl}", new()
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
    }
}
```

### Example Test

```csharp
[Fact]
public async Task VizualizarePacienti_PageLoads_DisplaysPacientList()
{
    // Arrange & Act
    await NavigateToAsync("/pacienti/vizualizare");
    
    // Assert - Page header is visible
    var header = Page.Locator("h1:has-text('Vizualizare Pacienti')");
    await Expect(header).ToBeVisibleAsync();
    
    // Assert - Grid is rendered
    var grid = Page.Locator(".grid-container");
    await Expect(grid).ToBeVisibleAsync();
}
```

</details>

<details>
<summary><strong>Click to expand: Modal Component Pattern</strong></summary>

```razor
@* PersonalFormModal.razor *@
<div class="modal-overlay @(IsVisible ? "visible" : "")" @onclick="HandleOverlayClick">
    <div class="modal-container" @onclick:stopPropagation>
        <!-- Modal Header (Blue Gradient) -->
        <div class="modal-header">
            <h2><i class="fas fa-user"></i> Title</h2>
            <button @onclick="Close" class="btn-close">×</button>
        </div>
        
        <!-- Modal Body -->
        <div class="modal-body">
            <EditForm Model="@Model" OnValidSubmit="HandleSubmitAsync">
                <DataAnnotationsValidator />
                <!-- Form fields -->
            </EditForm>
        </div>
        
        <!-- Modal Footer -->
        <div class="modal-footer">
            <button @onclick="HandleSubmitAsync" class="btn btn-primary" disabled="@IsSaving">
                Save
            </button>
            <button @onclick="Close" class="btn btn-secondary">
                Cancel
            </button>
        </div>
    </div>
</div>
```

```css
/* PersonalFormModal.razor.css - SCOPED! */
.modal-overlay {
    background: rgba(30, 58, 138, 0.3);
}

.modal-header {
    background: linear-gradient(135deg, var(--primary-light), var(--primary-color));
    color: white;
}

.btn-primary {
    background: linear-gradient(135deg, var(--primary-color), var(--primary-dark));
}

.btn-primary:hover {
    background: linear-gradient(135deg, var(--primary-dark), var(--primary-darker));
}
```

</details>

---
