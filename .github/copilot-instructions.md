# ValyanClinic Project Instructions

## 🎯 Project Overview
ValyanClinic is a comprehensive medical clinic management system built with .NET 9 Blazor Server, following Clean Architecture principles.

---

## 🏗️ Architecture

### Clean Architecture Layers
- **Domain Layer** (`ValyanClinic.Domain`) - Core business entities and interfaces
- **Application Layer** (`ValyanClinic.Application`) - Business logic, DTOs, MediatR handlers, Services
- **Infrastructure Layer** (`ValyanClinic.Infrastructure`) - Data access, external services
- **Presentation Layer** (`ValyanClinic`) - Blazor Server UI components

### Key Principles
- ✅ **SOLID principles** - Always follow Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- ✅ **Dependency Injection** - Use constructor injection for all dependencies
- ✅ **Separation of Concerns** - Keep business logic separate from UI
- ✅ **MediatR Pattern** - Use for commands and queries (CQRS)
- ✅ **Repository Pattern** - Data access through repositories

---

## 🎨 Design System - Blue Pastel Theme

### Official Color Palette
**DO NOT DEVIATE FROM THESE COLORS!**

```css
/* Primary Blue Colors */
--primary-color: #60a5fa;        /* Blue 400 - Main primary */
--primary-dark: #3b82f6;         /* Blue 500 - Buttons, accents */
--primary-darker: #2563eb;       /* Blue 600 - Hover states */
--primary-light: #93c5fd;        /* Blue 300 - Backgrounds */
--primary-lighter: #bfdbfe;      /* Blue 200 - Subtle backgrounds */

/* Pastel Secondary Colors */
--secondary-color: #94a3b8;      /* Slate 400 */
--success-color: #6ee7b7;        /* Emerald 300 - Success states */
--danger-color: #fca5a5;         /* Red 300 - Errors, delete */
--warning-color: #fcd34d;        /* Yellow 300 - Warnings */
--info-color: #7dd3fc;           /* Sky 300 - Info messages */

/* Text Colors */
--text-color: #334155;           /* Slate 700 - Main text */
--text-secondary: #64748b;       /* Slate 500 - Secondary text */
--text-muted: #94a3b8;           /* Slate 400 - Muted text */

/* Background Colors */
--background-color: #f8fafc;     /* Slate 50 - Page background */
--background-secondary: #f1f5f9; /* Slate 100 - Card backgrounds */
--background-light: #eff6ff;     /* Blue 50 - Hover states */
--background-white: #ffffff;     /* White - Modals, cards */

/* Border Colors */
--border-color: #e2e8f0;         /* Slate 200 - Standard borders */
--border-light: #f1f5f9;         /* Slate 100 - Subtle borders */
```

### Gradient Patterns
**ALWAYS use these gradients for consistency:**

```css
/* Header Gradient (Pages & Modals) */
background: linear-gradient(135deg, #93c5fd 0%, #60a5fa 100%);

/* Button Primary Gradient */
background: linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%);

/* Button Primary Hover Gradient */
background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);

/* Subtle Background Gradient */
background: linear-gradient(135deg, #dbeafe 0%, #bfdbfe 100%);
```

### Color Usage Rules
1. **Headers (Page/Modal):** Blue gradient (`#93c5fd → #60a5fa`)
2. **Primary Buttons:** Blue gradient (`#60a5fa → #3b82f6`)
3. **Active States:** `#60a5fa` (Blue 400)
4. **Hover States:** `#eff6ff` (Blue 50 background) + `#60a5fa` (Blue 400 border)
5. **Focus States:** `#3b82f6` border + `rgba(59, 130, 246, 0.1)` shadow
6. **Success Actions:** `#6ee7b7` (Emerald 300 pastel)
7. **Delete/Danger Actions:** `#fca5a5` (Red 300 pastel)
8. **Status Badges:** Pastel variants (check `variables.css`)

### ❌ NEVER Use These Colors
- ❌ **NO GREEN** for primary elements (`#22c55e`, `#10b981`) - Only for success states!
- ❌ **NO PURPLE** for primary elements (`#667eea`, `#764ba2`)
- ❌ **NO DARK BLUE** for backgrounds (`#1e40af`, `#1e3a8a`)
- ❌ **Avoid custom colors** - Always use CSS variables!

---

## 🎨 CSS Best Practices

### 1. Scoped CSS ONLY
**CRITICAL RULE:** All component styles MUST be scoped!

```razor
<!-- ✅ CORRECT - Scoped CSS -->
@* MyComponent.razor *@
<div class="my-component">
    <h1 class="title">Title</h1>
</div>

@* MyComponent.razor.css - SCOPED! *@
.my-component {
    /* Styles only apply to MyComponent */
}

.title {
    /* Scoped to MyComponent */
}
```

```razor
<!-- ❌ WRONG - Global CSS -->
@* wwwroot/css/app.css *@
.my-component { /* This affects ALL components! BAD! */ }
```

### Scoped CSS Guidelines
- ✅ **Each component MUST have its own `.razor.css` file**
- ✅ **Use component-specific class names** (e.g., `.personal-container`, `.modal-header`)
- ✅ **Leverage CSS nesting** for clarity
- ✅ **Use CSS variables** from `variables.css` (`:root` level)
- ❌ **NEVER pollute global CSS** (`app.css`, `base.css`) with component styles
- ❌ **NEVER use inline styles** (`style="..."`) unless absolutely necessary

### 2. CSS Variables Usage
**ALWAYS prefer CSS variables over hardcoded values!**

```css
/* ✅ CORRECT - Using CSS variables */
.modal-header {
    background: linear-gradient(135deg, var(--primary-light), var(--primary-color));
    padding: var(--modal-header-padding);
    font-size: var(--modal-header-title);
    font-weight: var(--font-weight-semibold);
    color: white;
    border-radius: var(--modal-radius);
}

/* ❌ WRONG - Hardcoded values */
.modal-header {
    background: linear-gradient(135deg, #93c5fd, #60a5fa); /* BAD! */
    padding: 1.25rem 1.75rem; /* BAD! */
    font-size: 22px; /* BAD! */
}
```

### 3. Responsive Design
**Mobile-first approach with breakpoints:**

```css
/* Base styles (mobile) */
.container {
    padding: 12px;
}

/* Tablet (>768px) */
@media (min-width: 768px) {
    .container {
        padding: 20px;
    }
}

/* Desktop (>1024px) */
@media (min-width: 1024px) {
    .container {
        padding: 32px;
    }
}

/* Large Desktop (>1400px) */
@media (min-width: 1400px) {
    .container {
        max-width: 1800px;
        margin: 0 auto;
    }
}
```

---

## 🧩 Component Structure

### File Organization
```
MyComponent/
├── MyComponent.razor         # Markup (HTML/Razor)
├── MyComponent.razor.cs      # Code-behind (Logic)
└── MyComponent.razor.css     # Scoped styles
```

### Component Template
```csharp
// MyComponent.razor.cs
using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Pages.MyModule;

public partial class MyComponent : ComponentBase
{
    // Inject services
    [Inject] private IMyService MyService { get; set; } = default!;
    [Inject] private ILogger<MyComponent> Logger { get; set; } = default!;
    
    // Parameters
    [Parameter] public EventCallback OnSave { get; set; }
    [Parameter] public string? Title { get; set; }
    
    // State
    private bool IsLoading { get; set; }
    private MyModel Model { get; set; } = new();
    
    // Lifecycle
    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }
    
    // Methods
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            // Business logic here
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading data");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

---

## 🚫 NO LOGIC IN UI (Razor Files)

### ✅ CORRECT - Code-Behind Pattern
```razor
@* MyComponent.razor - ONLY markup! *@
<div class="container">
    @if (IsLoading)
    {
        <LoadingSpinner />
    }
    else
    {
        <DataGrid Data="@Items" />
    }
    
    <button @onclick="HandleSaveAsync" disabled="@IsSaving">
        Save
    </button>
</div>
```

```csharp
// MyComponent.razor.cs - ALL logic here!
public partial class MyComponent
{
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }
    private List<Item> Items { get; set; } = new();
    
    private async Task HandleSaveAsync()
    {
        // Complex logic in code-behind!
        IsSaving = true;
        try
        {
            await MyService.SaveAsync(Items);
            await OnSave.InvokeAsync();
        }
        finally
        {
            IsSaving = false;
        }
    }
}
```

### ❌ WRONG - Logic in Razor
```razor
@* ❌ BAD - Complex logic in .razor file! *@
<button @onclick="async () => {
    IsSaving = true;
    try {
        await MyService.SaveAsync(Items);
        await OnSave.InvokeAsync();
    } finally {
        IsSaving = false;
    }
}">
    Save
</button>
```

### Separation Rules
- ✅ **`.razor` files:** ONLY markup, bindings, and simple conditionals
- ✅ **`.razor.cs` files:** ALL logic, state management, service calls
- ❌ **NEVER put complex logic in `@code {}` blocks**
- ❌ **NEVER use inline lambda expressions for complex operations**

---

## 💾 Data Access & Business Logic

### MediatR Pattern (CQRS)
**ALWAYS use MediatR for business operations!**

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

// Query Handler
public class GetPersonalByIdQueryHandler : IRequestHandler<GetPersonalByIdQuery, Result<PersonalDto>>
{
    private readonly IPersonalRepository _repository;
    
    public async Task<Result<PersonalDto>> Handle(GetPersonalByIdQuery request, CancellationToken cancellationToken)
    {
        var personal = await _repository.GetByIdAsync(request.Id);
        if (personal == null)
            return Result<PersonalDto>.Failure("Personal nu a fost gasit");
            
        return Result<PersonalDto>.Success(MapToDto(personal));
    }
}
```

### Component Usage
```csharp
// In component code-behind
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

### Repository Pattern
```csharp
public interface IPersonalRepository
{
    Task<Personal?> GetByIdAsync(Guid id);
    Task<List<Personal>> GetAllAsync();
    Task AddAsync(Personal personal);
    Task UpdateAsync(Personal personal);
    Task DeleteAsync(Guid id);
}

// Implementation uses Dapper or EF Core
public class PersonalRepository : IPersonalRepository
{
    private readonly IDbConnection _dbConnection;
    
    public async Task<Personal?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM Personal WHERE Id = @Id";
        return await _dbConnection.QueryFirstOrDefaultAsync<Personal>(sql, new { Id = id });
    }
    
    // ... other implementations
}
```

---

## 🧪 Testing

### Unit Testing Guidelines
- ✅ **Test business logic** (MediatR handlers, services)
- ✅ **Use xUnit** as testing framework
- ✅ **Use bUnit** as testing framework
- ✅ **Use FluentAssertions** for readable assertions
- ✅ **Use Moq** for mocking dependencies
- ✅ **Follow AAA pattern:** Arrange, Act, Assert
- ✅ **Test edge cases** and error scenarios

### Test Example
```csharp
public class CreatePersonalCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var mockRepo = new Mock<IPersonalRepository>();
        var handler = new CreatePersonalCommandHandler(mockRepo.Object);
        var command = new CreatePersonalCommand("Popescu", "Ion");
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Personal>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_InvalidCommand_ReturnsFailure()
    {
        // Arrange
        var mockRepo = new Mock<IPersonalRepository>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<Personal>()))
                .ThrowsAsync(new Exception("Database error"));
        var handler = new CreatePersonalCommandHandler(mockRepo.Object);
        var command = new CreatePersonalCommand("", ""); // Invalid
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}
```

---

## 🔒 Security Best Practices

### Authentication & Authorization
- ✅ **Always validate user authentication** with `[Authorize]` attribute
- ✅ **Check role permissions** before sensitive operations
- ✅ **Validate all input** (never trust client data)
- ✅ **Sanitize output** to prevent XSS
- ✅ **Use parameterized queries** (Dapper/EF Core handles this)
- ❌ **NEVER store sensitive data** in browser storage (LocalStorage/SessionStorage)
- ❌ **NEVER log sensitive information** (passwords, CNP, credit cards)

### Input Validation
```csharp
// FluentValidation example
public class CreatePersonalCommandValidator : AbstractValidator<CreatePersonalCommand>
{
    public CreatePersonalCommandValidator()
    {
        RuleFor(x => x.Nume)
            .NotEmpty().WithMessage("Numele este obligatoriu")
            .MaximumLength(100).WithMessage("Numele nu poate depasi 100 caractere");
        
        RuleFor(x => x.Prenume)
            .NotEmpty().WithMessage("Prenumele este obligatoriu")
            .MaximumLength(100);
        
        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}
```

---

## 📐 Typography System

### Font Families
```css
/* System Font Stack (Native OS fonts for performance) */
--font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;

/* Monospace (for code) */
--font-family-mono: 'Courier New', Courier, monospace;
```

### Font Sizes Hierarchy
```css
/* Base & Standard */
--font-size-xs: 0.6875rem;    /* 11px - Badge small, micro text */
--font-size-sm: 0.8125rem;    /* 13px - Labels (uppercase) */
--font-size-base: 0.875rem;   /* 14px - ⭐ STANDARD (body, buttons, tabs) */
--font-size-md: 0.9375rem;    /* 15px - Values, emphasized text */
--font-size-lg: 1.025rem;     /* 16.4px - Card titles */
--font-size-xl: 1.125rem;     /* 18px - Icons in titles */
--font-size-2xl: 1.375rem;    /* 22px - Modal headers */
--font-size-3xl: 1.75rem;     /* 28px - Page headers */
```

### Font Weights
```css
--font-weight-normal: 400;    /* Body text */
--font-weight-medium: 500;    /* Tabs inactive */
--font-weight-semibold: 600;  /* Labels, buttons, tabs active */
--font-weight-bold: 700;      /* Page headers */
```

### Usage Examples
```css
/* Page Header */
.page-header h1 {
    font-size: var(--font-size-3xl);     /* 28px */
    font-weight: var(--font-weight-bold); /* 700 */
}

/* Modal Header */
.modal-header h2 {
    font-size: var(--font-size-2xl);         /* 22px */
    font-weight: var(--font-weight-semibold); /* 600 */
}

/* Tab Button */
.tab-button {
    font-size: var(--font-size-base);    /* 14px */
    font-weight: var(--font-weight-medium); /* 500 */
}

.tab-button.active {
    font-weight: var(--font-weight-semibold); /* 600 */
}

/* Card Title */
.card-title {
    font-size: var(--font-size-lg);          /* 16.4px */
    font-weight: var(--font-weight-semibold); /* 600 */
}

/* Label (uppercase) */
.info-label {
    font-size: var(--font-size-sm);          /* 13px */
    font-weight: var(--font-weight-semibold); /* 600 */
    text-transform: uppercase;
    letter-spacing: var(--letter-spacing-wider);
}

/* Value */
.info-value {
    font-size: var(--font-size-md);          /* 15px */
    font-weight: var(--font-weight-normal);  /* 400 */
}
```

---

## 🚀 Performance Guidelines

### Blazor Server Specific
- ✅ **Use `@key` directive** for dynamic lists
- ✅ **Implement `ShouldRender()`** for expensive components
- ✅ **Use `StateHasChanged()`** judiciously
- ✅ **Dispose subscriptions** in `IDisposable`
- ✅ **Lazy load large datasets** (pagination, virtual scrolling)
- ❌ **Avoid frequent re-renders** of large components
- ❌ **Don't bind directly to large collections** without virtualization

### Database Optimization
- ✅ **Use indexed columns** for frequent queries
- ✅ **Implement pagination** (server-side)
- ✅ **Use projection** (SELECT only needed columns)
- ✅ **Batch operations** when possible
- ✅ **Use async/await** for all I/O operations

---

## 📚 Documentation

### XML Documentation
**ALWAYS document public APIs:**

```csharp
/// <summary>
/// Creates a new personal record in the system.
/// </summary>
/// <param name="command">The command containing personal details.</param>
/// <returns>A result containing the created personal's ID if successful.</returns>
/// <exception cref="ValidationException">Thrown when validation fails.</exception>
public async Task<Result<Guid>> Handle(CreatePersonalCommand command, CancellationToken cancellationToken)
{
    // Implementation
}
```

### Code Comments
- ✅ **Explain WHY**, not WHAT (code should be self-explanatory)
- ✅ **Document complex algorithms**
- ✅ **Add TODO comments** for future improvements
- ❌ **Avoid redundant comments** (`// Set name to value` for `name = value;`)

---

## 🛠️ Development Workflow

### Git Commit Messages
**Follow Conventional Commits:**

```
feat: Add patient search functionality
fix: Resolve consultatie modal styling issue
refactor: Extract IMC calculation to service
docs: Update API documentation
test: Add unit tests for PersonalService
style: Apply blue theme to Departamente modals
chore: Update NuGet packages
```

### Code Review Checklist
- [ ] **Follows blue theme** (no green/purple colors)
- [ ] **Uses scoped CSS** (`.razor.css` files)
- [ ] **No logic in `.razor` files** (only in `.razor.cs`)
- [ ] **Uses CSS variables** (no hardcoded colors/sizes)
- [ ] **Proper error handling** (try-catch where appropriate)
- [ ] **Async/await** used correctly
- [ ] **Tests included** (for business logic)
- [ ] **XML documentation** for public APIs
- [ ] **Build succeeds** with zero warnings
- [ ] **Performance considered** (no unnecessary re-renders)

---

## 🎯 Common Patterns

### Modal Component Pattern
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
    background: rgba(30, 58, 138, 0.3); /* Blue transparent */
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

---

## ✅ Final Checklist for Every Component

### Before Commit
- [ ] **Blue theme** applied (gradient headers, buttons)
- [ ] **Scoped CSS** used (`.razor.css` file exists)
- [ ] **No logic in `.razor`** (all in `.razor.cs`)
- [ ] **CSS variables** used (no hardcoded values)
- [ ] **Responsive design** implemented
- [ ] **Error handling** in place
- [ ] **Loading states** handled
- [ ] **Async/await** used properly
- [ ] **XML documentation** added for public methods
- [ ] **Tests written** (if business logic)
- [ ] **Build succeeds** (0 errors, 0 warnings)
- [ ] **Performance optimized** (no unnecessary re-renders)

---

## 📞 Support & Resources

### Key Files Reference
- **Color Variables:** `ValyanClinic/wwwroot/css/variables.css`
- **Base Styles:** `ValyanClinic/wwwroot/css/base.css`
- **Typography Guide:** `DevSupport/Typography/Cheat-Sheet.md`
- **Theme Documentation:** `DevSupport/Improvement/PersonalViewModal-Theme-Update.md`

### Need Help?
- Check existing components for patterns (PersonalViewModal, DashboardMedic, etc.)
- Consult `variables.css` for available CSS variables
- Review test examples in `ValyanClinic.Tests` project
- Follow MediatR pattern examples in `ValyanClinic.Application`

---

**🎨 Remember: Blue Pastel Theme, Scoped CSS, No Logic in UI!**

**Status:** ✅ **COMPREHENSIVE GUIDELINES - v2.1**  
**Last Updated:** December 2025  
**Project:** ValyanClinic - Medical Clinic Management System

---

## 🔧 Business Logic Services Pattern

### **WHY Extract Business Logic to Services?**

**Problem:** Complex Blazor components with heavy business logic are **difficult to unit test** because:
- Third-party UI components (Syncfusion, DevExpress) require extensive DI setup in tests
- Component lifecycle (OnInitializedAsync, StateHasChanged) complicates testing
- UI state management mixes with business rules

**Solution:** **Extract business logic into separate Application Services** that can be easily unit tested.

---

### **Pattern: Service Extraction for Complex Components**

#### **❌ BEFORE (Logic in Component - Hard to Test):**

```csharp
// VizualizarePacienti.razor.cs - ❌ HARD TO TEST
public partial class VizualizarePacienti : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    
    private List<PacientListDto> CurrentPageData { get; set; } = new();
    private int CurrentPage { get; set; } = 1;
    private int CurrentPageSize { get; set; } = 20;
    
    // ❌ COMPLEX BUSINESS LOGIC IN COMPONENT
    private async Task LoadPagedData()
    {
        // Complex filtering logic
        var filters = new PacientFilters
        {
            SearchText = GlobalSearchText,
            Judet = FilterJudet,
            Asigurat = ParseAsiguratFilter(FilterAsigurat),
            Activ = ParseStatusFilter(FilterStatus)
        };
        
        // Complex sorting logic
        var sortOptions = new SortOptions
        {
            Column = CurrentSortColumn,
            Direction = CurrentSortDirection
        };
        
        // Complex query building
        var query = new GetPacientListQuery
        {
            PageNumber = CurrentPage,
            PageSize = CurrentPageSize,
            SearchText = filters.SearchText,
            Judet = filters.Judet,
            Asigurat = filters.Asigurat,
            Activ = filters.Activ,
            SortColumn = sortOptions.Column,
            SortDirection = sortOptions.Direction
        };
        
        var result = await Mediator.Send(query);
        CurrentPageData = result.Value?.Value?.ToList() ?? new();
    }
    
    private bool? ParseAsiguratFilter(string? value)
    {
        // Complex parsing logic
        if (string.IsNullOrEmpty(value)) return null;
        return value == "true";
    }
}
```

**Problem:** Testing requires mocking Syncfusion Grid, NavigationManager, JSInterop, etc. → **NIGHTMARE!**

---

#### **✅ AFTER (Logic in Service - Easy to Test):**

**Step 1: Create Application Service**

```csharp
// ValyanClinic.Application/Services/Pacienti/IPacientDataService.cs
namespace ValyanClinic.Application.Services.Pacienti;

/// <summary>
/// Service for managing patient data operations (filtering, sorting, pagination).
/// Encapsulates business logic for patient list management.
/// </summary>
public interface IPacientDataService
{
    /// <summary>
    /// Loads paged patient data with filters and sorting.
    /// </summary>
    Task<Result<PagedPacientData>> LoadPagedDataAsync(
        PacientFilters filters,
        PaginationOptions pagination,
        SortOptions sorting,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Loads filter options (unique judete, etc.) from server.
    /// </summary>
    Task<Result<PacientFilterOptions>> LoadFilterOptionsAsync(
        CancellationToken cancellationToken = default);
}

// Implementation
public class PacientDataService : IPacientDataService
{
    private readonly IMediator _mediator;
    private readonly ILogger<PacientDataService> _logger;
    
    public PacientDataService(IMediator mediator, ILogger<PacientDataService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    public async Task<Result<PagedPacientData>> LoadPagedDataAsync(
        PacientFilters filters,
        PaginationOptions pagination,
        SortOptions sorting,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Loading paged data: Page={Page}, Size={Size}, Search={Search}",
                pagination.PageNumber, pagination.PageSize, filters.SearchText);
            
            var query = new GetPacientListQuery
            {
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                SearchText = filters.SearchText,
                Judet = filters.Judet,
                Asigurat = filters.Asigurat,
                Activ = filters.Activ,
                SortColumn = sorting.Column,
                SortDirection = sorting.Direction
            };
            
            var result = await _mediator.Send(query, cancellationToken);
            
            if (!result.IsSuccess)
                return Result<PagedPacientData>.Failure(result.Errors);
            
            var pagedData = new PagedPacientData
            {
                Items = result.Value?.Value?.ToList() ?? new(),
                TotalCount = result.Value?.TotalCount ?? 0,
                CurrentPage = result.Value?.CurrentPage ?? 1,
                PageSize = result.Value?.PageSize ?? 20
            };
            
            return Result<PagedPacientData>.Success(pagedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading paged data");
            return Result<PagedPacientData>.Failure($"Eroare: {ex.Message}");
        }
    }
    
    public async Task<Result<PacientFilterOptions>> LoadFilterOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Load all data for filter options
            var query = new GetPacientListQuery
            {
                PageNumber = 1,
                PageSize = int.MaxValue
            };
            
            var result = await _mediator.Send(query, cancellationToken);
            
            if (!result.IsSuccess)
                return Result<PacientFilterOptions>.Failure(result.Errors);
            
            var data = result.Value?.Value?.ToList() ?? new();
            
            var options = new PacientFilterOptions
            {
                Judete = data
                    .Where(p => !string.IsNullOrEmpty(p.Judet))
                    .Select(p => p.Judet!)
                    .Distinct()
                    .OrderBy(j => j)
                    .ToList()
            };
            
            return Result<PacientFilterOptions>.Success(options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading filter options");
            return Result<PacientFilterOptions>.Failure($"Eroare: {ex.Message}");
        }
    }
}
```

**Step 2: Simplify Component (UI Only)**

```csharp
// VizualizarePacienti.razor.cs - ✅ EASY TO TEST (UI ONLY)
public partial class VizualizarePacienti : ComponentBase
{
    [Inject] private IPacientDataService DataService { get; set; } = default!;
    [Inject] private ILogger<VizualizarePacienti> Logger { get; set; } = default!;
    
    private List<PacientListDto> CurrentPageData { get; set; } = new();
    private int CurrentPage { get; set; } = 1;
    private int TotalRecords { get; set; }
    
    // ✅ SIMPLE - JUST CALLS SERVICE
    private async Task LoadPagedData()
    {
        IsLoading = true;
        
        var filters = new PacientFilters
        {
            SearchText = GlobalSearchText,
            Judet = FilterJudet,
            Asigurat = ParseAsiguratFilter(FilterAsigurat),
            Activ = ParseStatusFilter(FilterStatus)
        };
        
        var pagination = new PaginationOptions
        {
            PageNumber = CurrentPage,
            PageSize = CurrentPageSize
        };
        
        var sorting = new SortOptions
        {
            Column = CurrentSortColumn,
            Direction = CurrentSortDirection
        };
        
        var result = await DataService.LoadPagedDataAsync(filters, pagination, sorting);
        
        if (result.IsSuccess)
        {
            CurrentPageData = result.Value.Items;
            TotalRecords = result.Value.TotalCount;
        }
        else
        {
            HasError = true;
            ErrorMessage = result.FirstError;
        }
        
        IsLoading = false;
    }
}
```

**Step 3: Easy Unit Tests for Service**

```csharp
// PacientDataServiceTests.cs - ✅ EASY TO TEST!
public class PacientDataServiceTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<PacientDataService>> _mockLogger;
    private readonly PacientDataService _service;
    
    public PacientDataServiceTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<PacientDataService>>();
        _service = new PacientDataService(_mockMediator.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task LoadPagedDataAsync_WithFilters_ReturnsFilteredData()
    {
        // Arrange
        var filters = new PacientFilters { SearchText = "Popescu", Judet = "Bucuresti" };
        var pagination = new PaginationOptions { PageNumber = 1, PageSize = 20 };
        var sorting = new SortOptions { Column = "Nume", Direction = "ASC" };
        
        var expectedData = CreateMockPagedResult();
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPacientListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<PacientListDto>>.Success(expectedData));
        
        // Act
        var result = await _service.LoadPagedDataAsync(filters, pagination, sorting);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(10);
        result.Value.TotalCount.Should().Be(50);
        
        _mockMediator.Verify(m => m.Send(
            It.Is<GetPacientListQuery>(q => 
                q.SearchText == "Popescu" && 
                q.Judet == "Bucuresti"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    // NO NEED TO MOCK: Syncfusion Grid, NavigationManager, JSInterop! 🎉
}
```

---

### **When to Extract Business Logic to Services?**

✅ **Extract when:**
- Component has complex filtering/sorting/pagination logic
- Business rules need to be reused across multiple components
- Component logic exceeds **~200 lines** in code-behind
- Unit testing component requires mocking **>5 UI dependencies**

❌ **Keep in component when:**
- Simple UI state management (show/hide modal, toggle flags)
- Direct EventCallback invocations
- Simple parameter binding

---

### **Benefits of Service Extraction:**

1. ✅ **Testability:** Service can be unit tested with **ZERO UI dependencies**
2. ✅ **Reusability:** Same service can be used by multiple components
3. ✅ **Maintainability:** Business logic changes in ONE place
4. ✅ **Separation of Concerns:** UI components focus on **rendering**, services focus on **logic**
5. ✅ **Performance:** Service methods can be **cached/memoized** independently

---

## 🎭 Integration Testing with Playwright

### **WHY Playwright over Selenium?**

| Feature | Playwright | Selenium |
|---------|-----------|----------|
| **Speed** | ⚡ **3-5x faster** | Slower |
| **Auto-wait** | ✅ Built-in smart waits | ❌ Manual waits required |
| **Browser Support** | ✅ Chromium, Firefox, WebKit | ✅ Chrome, Firefox, Safari |
| **API Design** | ✅ Modern async/await | ❌ Older sync API |
| **Network Interception** | ✅ Native support | ⚠️ Limited |
| **Screenshots/Videos** | ✅ Built-in | ⚠️ External tools |
| **Maintenance** | ✅ Microsoft actively maintained | ⚠️ Community-driven |
| **Blazor Support** | ✅ Excellent (.NET 9+) | ⚠️ Requires workarounds |

**Recommendation:** **Playwright** is the **best choice** for ValyanClinic due to:
- Native **.NET 9 support** with `Microsoft.Playwright` NuGet package
- **Auto-waiting** eliminates 90% of flaky tests (no more `Thread.Sleep`!)
- **Fast execution** (parallel test runs)
- **Rich debugging** with Playwright Inspector

---

### **Setting Up Playwright for ValyanClinic**

#### **Step 1: Install NuGet Packages**

```xml
<!-- ValyanClinic.Tests.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.Playwright" Version="1.47.0" />
  <PackageReference Include="Microsoft.Playwright.NUnit" Version="1.47.0" />
  <!-- OR use xUnit adapter -->
  <PackageReference Include="xunit" Version="2.9.3" />
</ItemGroup>
```

#### **Step 2: Install Playwright Browsers**

```bash
# Run once after installing package
pwsh bin\Debug\net10.0\playwright.ps1 install
```

#### **Step 3: Create Base Test Class**

```csharp
// ValyanClinic.Tests/Integration/PlaywrightTestBase.cs
using Microsoft.Playwright;
using Xunit;

namespace ValyanClinic.Tests.Integration;

/// <summary>
/// Base class for Playwright integration tests.
/// Handles browser lifecycle and provides common utilities.
/// </summary>
public abstract class PlaywrightTestBase : IAsyncLifetime
{
    protected IPlaywright Playwright { get; private set; } = default!;
    protected IBrowser Browser { get; private set; } = default!;
    protected IBrowserContext Context { get; private set; } = default!;
    protected IPage Page { get; private set; } = default!;
    
    // ValyanClinic base URL (update for your environment)
    protected string BaseUrl { get; } = "https://localhost:5001";
    
    public async Task InitializeAsync()
    {
        // Create Playwright instance
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        
        // Launch browser (use Chromium by default, can switch to Firefox/WebKit)
        Browser = await Playwright.Chromium.LaunchAsync(new()
        {
            Headless = true, // Set to false for debugging
            SlowMo = 50 // Slow down actions for debugging
        });
        
        // Create browser context (isolated session)
        Context = await Browser.NewContextAsync(new()
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            RecordVideoDir = "videos/" // Record test videos
        });
        
        // Create page
        Page = await Context.NewPageAsync();
    }
    
    public async Task DisposeAsync()
    {
        await Page?.CloseAsync()!;
        await Context?.CloseAsync()!;
        await Browser?.CloseAsync()!;
        Playwright?.Dispose();
    }
    
    /// <summary>
    /// Navigate to a specific page and wait for load.
    /// </summary>
    protected async Task NavigateToAsync(string relativeUrl)
    {
        await Page.GotoAsync($"{BaseUrl}{relativeUrl}", new()
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
    }
    
    /// <summary>
    /// Wait for Blazor to finish rendering (SignalR ready).
    /// </summary>
    protected async Task WaitForBlazorAsync()
    {
        await Page.WaitForFunctionAsync("() => window.Blazor !== undefined");
    }
}
```

#### **Step 4: Write Integration Tests**

```csharp
// ValyanClinic.Tests/Integration/VizualizarePacientiIntegrationTests.cs
using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ValyanClinic.Tests.Integration;

/// <summary>
/// End-to-end integration tests pentru VizualizarePacienti page.
/// Tests full user workflows including UI interactions and data persistence.
/// </summary>
public class VizualizarePacientiIntegrationTests : PlaywrightTestBase
{
    [Fact]
    public async Task VizualizarePacienti_PageLoads_DisplaysPacientList()
    {
        // Arrange & Act
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();
        
        // Assert - Page header is visible
        var header = Page.Locator("h1:has-text('Vizualizare Pacienti')");
        await Expect(header).ToBeVisibleAsync();
        
        // Assert - Grid is rendered
        var grid = Page.Locator(".grid-container");
        await Expect(grid).ToBeVisibleAsync();
        
        // Assert - Total records counter exists
        var totalRecords = Page.Locator(".total-records");
        await Expect(totalRecords).ToContainTextAsync("Total:");
    }
    
    [Fact]
    public async Task VizualizarePacienti_GlobalSearch_FiltersResults()
    {
        // Arrange
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();
        
        // Act - Type in search box
        var searchInput = Page.Locator("input.search-input");
        await searchInput.FillAsync("Popescu");
        await searchInput.PressAsync("Enter");
        
        // Wait for results to update (Playwright auto-waits!)
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Results contain search term
        var firstRow = Page.Locator("table tbody tr").First;
        await Expect(firstRow).ToContainTextAsync("Popescu");
    }
    
    [Fact]
    public async Task VizualizarePacienti_SelectPacient_OpensViewModal()
    {
        // Arrange
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();
        
        // Act - Click first row to select
        var firstRow = Page.Locator("table tbody tr").First;
        await firstRow.ClickAsync();
        
        // Act - Click "Vizualizeaza Detalii" button
        var viewButton = Page.Locator("button.btn-view");
        await viewButton.ClickAsync();
        
        // Assert - Modal is visible
        var modal = Page.Locator(".modal-overlay.visible");
        await Expect(modal).ToBeVisibleAsync();
        
        // Assert - Modal contains patient data
        var modalTitle = Page.Locator(".modal-header h2");
        await Expect(modalTitle).ToContainTextAsync("Detalii Pacient");
    }
    
    [Fact]
    public async Task VizualizarePacienti_ApplyFilters_UpdatesResults()
    {
        // Arrange
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();
        
        // Act - Open advanced filters
        var filterButton = Page.Locator("button.btn-filter");
        await filterButton.ClickAsync();
        
        // Wait for filter panel to expand
        var filterPanel = Page.Locator(".advanced-filter-panel.expanded");
        await Expect(filterPanel).ToBeVisibleAsync();
        
        // Act - Select Judet filter
        var judetDropdown = Page.Locator(".filter-dropdown").First;
        await judetDropdown.ClickAsync();
        await Page.Locator("li:has-text('Bucuresti')").ClickAsync();
        
        // Act - Click Apply Filters
        var applyButton = Page.Locator("button.btn-apply");
        await applyButton.ClickAsync();
        
        // Wait for results
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Filter chip is visible
        var filterChip = Page.Locator(".filter-chip:has-text('Judet: Bucuresti')");
        await Expect(filterChip).ToBeVisibleAsync();
        
        // Assert - Results are filtered
        var rows = Page.Locator("table tbody tr");
        var count = await rows.CountAsync();
        count.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public async Task VizualizarePacienti_Paging_NavigatesToNextPage()
    {
        // Arrange
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();
        
        // Act - Click next page button (Syncfusion Grid pagination)
        var nextPageButton = Page.Locator(".e-nextpage");
        await nextPageButton.ClickAsync();
        
        // Wait for page change
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Page number updated
        var currentPage = Page.Locator(".e-currentitem");
        var pageText = await currentPage.TextContentAsync();
        pageText.Should().Contain("2"); // Page 2
    }
    
    [Fact]
    public async Task VizualizarePacienti_RefreshButton_ReloadsData()
    {
        // Arrange
        await NavigateToAsync("/pacienti/vizualizare");
        await WaitForBlazorAsync();
        
        // Act - Click refresh button
        var refreshButton = Page.Locator("button:has-text('Reincarca')");
        await refreshButton.ClickAsync();
        
        // Assert - Loading spinner appears (briefly)
        var spinner = Page.Locator(".spinner-border");
        // Spinner might disappear quickly, so check for toast instead
        
        // Assert - Success toast appears
        await Page.WaitForSelectorAsync(".e-toast-success", new()
        {
            Timeout = 5000
        });
        
        var toast = Page.Locator(".e-toast-success");
        await Expect(toast).ToContainTextAsync("reincarcate cu succes");
    }
}
```

---

### **Playwright Best Practices for ValyanClinic**

#### **1. Use Locator Strategies (in order of preference)**

```csharp
// ✅ BEST - Semantic locators (accessible to screen readers)
await Page.GetByRole(AriaRole.Button, new() { Name = "Reincarca" }).ClickAsync();
await Page.GetByLabel("Cautare rapida").FillAsync("Popescu");

// ✅ GOOD - Data-testid attributes (add to markup)
await Page.Locator("[data-testid='patient-grid']").WaitForAsync();

// ⚠️ OK - CSS classes (but can break with style changes)
await Page.Locator(".btn-primary").ClickAsync();

// ❌ AVOID - XPath (fragile, hard to read)
await Page.Locator("//div[@class='modal']//button[1]").ClickAsync();
```

#### **2. Add data-testid Attributes to Key Elements**

```razor
<!-- VizualizarePacienti.razor -->
<div class="pacienti-container" data-testid="pacienti-page">
    <input type="text" 
           class="search-input" 
           data-testid="search-input"
           placeholder="Cautare rapida..." />
    
    <button class="btn-filter" 
            data-testid="filter-button"
            @onclick="ToggleAdvancedFilter">
        Filtre
    </button>
    
    <div class="grid-container" data-testid="patient-grid">
        <SfGrid @ref="GridRef" DataSource="@CurrentPageData">
            <!-- Grid columns -->
        </SfGrid>
    </div>
</div>
```

#### **3. Parallel Test Execution**

```csharp
// Mark tests as parallel-safe
[Collection("Sequential")] // Use for tests that modify shared state
public class VizualizarePacientiIntegrationTests : PlaywrightTestBase
{
    // Tests run in parallel by default with xUnit + Playwright
}
```

#### **4. Visual Regression Testing**

```csharp
[Fact]
public async Task VizualizarePacienti_Screenshot_MatchesBaseline()
{
    // Arrange
    await NavigateToAsync("/pacienti/vizualizare");
    await WaitForBlazorAsync();
    
    // Act - Take screenshot
    await Page.ScreenshotAsync(new()
    {
        Path = "screenshots/vizualizare-pacienti.png",
        FullPage = true
    });
    
    // Assert - Compare with baseline (use external tool like Percy or Applitools)
    // Or manually review screenshots in CI/CD pipeline
}
```

#### **5. Network Interception for Testing**

```csharp
[Fact]
public async Task VizualizarePacienti_ApiFailure_DisplaysErrorMessage()
{
    // Arrange - Intercept API calls
    await Page.RouteAsync("**/api/pacienti/**", async route =>
    {
        await route.FulfillAsync(new()
        {
            Status = 500,
            Body = "{\"error\": \"Database connection failed\"}"
        });
    });
    
    // Act
    await NavigateToAsync("/pacienti/vizualizare");
    await WaitForBlazorAsync();
    
    // Assert - Error message displayed
    var errorAlert = Page.Locator(".alert-danger");
    await Expect(errorAlert).ToBeVisibleAsync();
    await Expect(errorAlert).ToContainTextAsync("Eroare");
}
```

---

### **CI/CD Integration**

```yaml
# .github/workflows/playwright-tests.yml
name: Playwright Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Install dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --configuration Release
      
      - name: Install Playwright browsers
        run: pwsh ValyanClinic.Tests/bin/Release/net10.0/playwright.ps1 install
      
      - name: Run Playwright tests
        run: dotnet test --filter "Category=Integration" --configuration Release
      
      - name: Upload test videos
        if: failure()
        uses: actions/upload-artifact@v4
        with:
          name: playwright-videos
          path: ValyanClinic.Tests/videos/
```

---

## 📊 Testing Strategy Summary

| Test Type | Tool | When to Use | Coverage Goal |
|-----------|------|-------------|---------------|
| **Unit Tests** | xUnit + FluentAssertions + Moq | Business logic, MediatR handlers, services | **80-90%** |
| **Component Tests** | bUnit | Simple Blazor components (modals, forms) | **60-70%** |
| **Integration Tests** | Playwright | Complex UI workflows, E2E scenarios | **Critical paths 100%** |

**Recommended Approach for ValyanClinic:**
1. ✅ **Unit test all business logic** (Application layer services)
2. ✅ **bUnit test simple components** (modals without third-party dependencies)
3. ✅ **Playwright test critical user journeys** (login, patient registration, consultation flow)

---
