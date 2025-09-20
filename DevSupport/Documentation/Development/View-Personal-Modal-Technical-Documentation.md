# View Personal Modal - Technical Documentation

## 📋 Overview

The **VizualizeazaPersonal.razor** component is a premium dashboard-style modal for displaying comprehensive staff information in the ValyanClinic system. This component showcases advanced UI/UX design patterns with card-based layouts, interactive elements, and professional medical application aesthetics.

## 🏗️ Component Architecture

### File Structure
```
VizualizeazaPersonal.razor           # UI markup and dashboard layout
VizualizeazaPersonal.razor.cs        # Business logic and data formatting
view-personal-syncfusion.css         # Component-specific premium styling
```

### Technical Specifications
- **Framework**: .NET 9 Blazor Server
- **UI Pattern**: Dashboard with card-based information organization
- **Layout**: CSS Grid with responsive breakpoints
- **Styling**: Premium gradient cards with hover effects
- **Accessibility**: ARIA labels and semantic HTML structure

## 🎨 Design Philosophy

### Card-Based Information Architecture
The component organizes staff information into themed cards, each with:
- **Gradient headers** with contextual icons
- **Structured content areas** with consistent spacing
- **Interactive elements** where appropriate (emails, phone numbers)
- **Status indicators** with color-coded badges
- **Hover animations** for enhanced user experience

### Visual Hierarchy
```
Modal Header (Staff name + role)
├── Card 1: General Information (Identity, Basic details)
├── Card 2: Contact Information (Phone, Email with links)
├── Card 3: Address Information (Domicile, Residence)
├── Card 4: Professional Information (Role, Department, Status)
├── Card 5: Identity Documents (ID card details with validity)
└── Card 6: Observations (Full-width, if present)
```

## 🔧 Core Implementation

### Component Declaration
```csharp
public partial class VizualizeazaPersonal : ComponentBase
{
    [Parameter] public PersonalModel? PersonalData { get; set; }
    [Parameter] public EventCallback<(string Title, string Message, string CssClass)> OnToastMessage { get; set; }
}
```

### Service Dependencies
```csharp
[Inject] private ILogger<VizualizeazaPersonal> Logger { get; set; } = default!;
[Inject] private IJSRuntime JSRuntime { get; set; } = default!;
[Inject] private IToastNotificationService ToastService { get; set; } = default!;
```

### State Management
```csharp
private bool isLoading = true;
private bool hasError = false;
private string? errorMessage = null;
```

## 🎯 Card System Deep Dive

### 1. General Information Card

#### UI Structure
```razor
<div class="detail-card">
    <div class="card-header">
        <i class="fas fa-id-card"></i>
        <h3>Informații Generale</h3>
    </div>
    <div class="card-content">
        <div class="form-grid">
            <div class="form-field">
                <label>Cod Angajat</label>
                <div class="field-value">@PersonalData.Cod_Angajat</div>
            </div>
            <div class="form-field">
                <label>CNP</label>
                <div class="field-value">
                    @PersonalData.CNP
                    @if (IsValidCNP(PersonalData.CNP))
                    {
                        <span class="validity-badge valid">Valid</span>
                    }
                </div>
            </div>
            <!-- More fields... -->
        </div>
    </div>
</div>
```

#### Data Processing
```csharp
private bool IsValidCNP(string cnp)
{
    // Implementation matches the CNP validation from AdaugaEditezaPersonal
    return ValidateCNPComplete(cnp).IsValid;
}

private int CalculateAge(DateTime birthDate)
{
    var age = DateTime.Today.Year - birthDate.Year;
    if (birthDate.Date > DateTime.Today.AddYears(-age))
        age--;
    return age;
}
```

### 2. Contact Information Card

#### Interactive Contact Elements
```razor
<div class="form-field">
    <label>Email Personal</label>
    <div class="field-value">
        @if (!string.IsNullOrEmpty(PersonalData.Email_Personal))
        {
            <a href="mailto:@PersonalData.Email_Personal" 
               class="contact-link"
               @onclick="() => HandleEmailClick(PersonalData.Email_Personal)">
                <i class="fas fa-envelope"></i>
                @PersonalData.Email_Personal
            </a>
        }
        else
        {
            <span class="text-muted">Nu este specificat</span>
        }
    </div>
</div>

<div class="form-field">
    <label>Telefon Personal</label>
    <div class="field-value">
        @if (!string.IsNullOrEmpty(PersonalData.Telefon_Personal))
        {
            <a href="tel:@PersonalData.Telefon_Personal" 
               class="contact-link"
               @onclick="() => HandlePhoneClick(PersonalData.Telefon_Personal)">
                <i class="fas fa-phone"></i>
                @FormatPhoneNumber(PersonalData.Telefon_Personal)
            </a>
        }
        else
        {
            <span class="text-muted">Nu este specificat</span>
        }
    </div>
</div>
```

#### Contact Interaction Handlers
```csharp
private async Task HandleEmailClick(string email)
{
    try
    {
        Logger.LogInformation("📧 Email link clicked: {Email}", email);
        
        await OnToastMessage.InvokeAsync((
            "Email", 
            $"Se deschide aplicația de email pentru {email}", 
            "e-toast-info"
        ));
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error handling email click");
    }
}

private async Task HandlePhoneClick(string phone)
{
    try
    {
        Logger.LogInformation("📞 Phone link clicked: {Phone}", phone);
        
        await OnToastMessage.InvokeAsync((
            "Telefon", 
            $"Se inițiază apelul către {FormatPhoneNumber(phone)}", 
            "e-toast-info"
        ));
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error handling phone click");
    }
}
```

#### Phone Number Formatting
```csharp
private string FormatPhoneNumber(string phone)
{
    if (string.IsNullOrEmpty(phone) || phone.Length != 10)
        return phone;
    
    // Format: 0XXX-XXX-XXX
    return $"{phone.Substring(0, 4)}-{phone.Substring(4, 3)}-{phone.Substring(7, 3)}";
}
```

### 3. Address Information Card

#### Conditional Address Display
```razor
<div class="card-content">
    <div class="form-grid">
        <!-- Domicile Address -->
        <div class="form-field full-width">
            <label>Adresa Domiciliu</label>
            <div class="field-value">
                @GetFullAddress(PersonalData.Adresa_Domiciliu, 
                                PersonalData.Oras_Domiciliu, 
                                PersonalData.Judet_Domiciliu, 
                                PersonalData.Cod_Postal_Domiciliu)
            </div>
        </div>
        
        <!-- Residence Address (if different) -->
        @if (HasResidenceAddress())
        {
            <div class="address-separator">
                <span>Adresa de Reședință</span>
            </div>
            <div class="form-field full-width">
                <label>Adresa Reședință</label>
                <div class="field-value">
                    @GetFullAddress(PersonalData.Adresa_Resedinta, 
                                    PersonalData.Oras_Resedinta, 
                                    PersonalData.Judet_Resedinta, 
                                    PersonalData.Cod_Postal_Resedinta)
                </div>
            </div>
        }
    </div>
</div>
```

#### Address Processing Logic
```csharp
private bool HasResidenceAddress()
{
    return !string.IsNullOrEmpty(PersonalData?.Adresa_Resedinta) ||
           !string.IsNullOrEmpty(PersonalData?.Judet_Resedinta) ||
           !string.IsNullOrEmpty(PersonalData?.Oras_Resedinta) ||
           !string.IsNullOrEmpty(PersonalData?.Cod_Postal_Resedinta);
}

private string GetFullAddress(string? street, string? city, string? county, string? postalCode)
{
    var components = new List<string>();
    
    if (!string.IsNullOrEmpty(street)) components.Add(street);
    if (!string.IsNullOrEmpty(city)) components.Add(city);
    if (!string.IsNullOrEmpty(county)) components.Add($"Județul {county}");
    if (!string.IsNullOrEmpty(postalCode)) components.Add($"CP {postalCode}");
    
    return components.Count > 0 
        ? string.Join(", ", components) 
        : "Nu este specificată";
}
```

### 4. Professional Information Card

#### Status Badge System
```razor
<div class="form-field">
    <label>Status Angajat</label>
    <div class="field-value">
        <span class="status-badge status-@PersonalData.Status_Angajat.ToString().ToLower()">
            @GetStatusDisplay(PersonalData.Status_Angajat)
        </span>
    </div>
</div>
```

#### Status Display Logic
```csharp
private string GetStatusDisplay(StatusAngajat status)
{
    return status switch
    {
        StatusAngajat.Activ => "Activ",
        StatusAngajat.Inactiv => "Inactiv",
        _ => status.ToString()
    };
}

private string GetDepartmentDisplay(Departament? departament)
{
    return departament switch
    {
        Departament.Administratie => "Administrație",
        Departament.Financiar => "Financiar",
        Departament.IT => "IT",
        Departament.Intretinere => "Întreținere",
        Departament.Logistica => "Logistică",
        Departament.Marketing => "Marketing",
        Departament.Receptie => "Recepție",
        Departament.ResurseUmane => "Resurse Umane",
        Departament.Securitate => "Securitate",
        Departament.Transport => "Transport",
        Departament.Juridic => "Juridic",
        Departament.RelatiiClienti => "Relații Clienți",
        Departament.Calitate => "Calitate",
        Departament.CallCenter => "Call Center",
        _ => "Nu este specificat"
    };
}
```

### 5. Identity Documents Card

#### Advanced Validity Checking
```razor
<div class="form-field">
    <label>Valabil până la</label>
    <div class="field-value">
        @if (PersonalData.Valabil_CI_Pana.HasValue)
        {
            @PersonalData.Valabil_CI_Pana.Value.ToString("dd.MM.yyyy")
            @GetValidityBadge(PersonalData.Valabil_CI_Pana.Value)
        }
        else
        {
            <span class="text-muted">Nu este specificată</span>
        }
    </div>
</div>
```

#### Validity Badge Logic
```csharp
private MarkupString GetValidityBadge(DateTime validUntil)
{
    var today = DateTime.Today;
    var daysUntilExpiry = (validUntil.Date - today).Days;
    
    return daysUntilExpiry switch
    {
        < 0 => new MarkupString("<span class=\"validity-badge expired\">Expirat</span>"),
        <= 30 => new MarkupString("<span class=\"validity-badge warning\">Expiră în curând</span>"),
        _ => new MarkupString("<span class=\"validity-badge valid\">Valid</span>")
    };
}

private string GetValidityStatus(DateTime? validUntil)
{
    if (!validUntil.HasValue) return "unknown";
    
    var today = DateTime.Today;
    var daysUntilExpiry = (validUntil.Value.Date - today).Days;
    
    return daysUntilExpiry switch
    {
        < 0 => "expired",
        <= 30 => "warning", 
        _ => "valid"
    };
}
```

### 6. Observations Card (Conditional)

#### Full-Width Display
```razor
@if (!string.IsNullOrEmpty(PersonalData.Observatii))
{
    <div class="detail-card observations-card">
        <div class="card-header">
            <i class="fas fa-sticky-note"></i>
            <h3>Observații</h3>
        </div>
        <div class="card-content">
            <div class="observations-content">
                @((MarkupString)FormatObservations(PersonalData.Observatii))
            </div>
        </div>
    </div>
}
```

#### Text Formatting
```csharp
private string FormatObservations(string observations)
{
    if (string.IsNullOrEmpty(observations))
        return "";
    
    // Convert line breaks to HTML
    return observations
        .Replace("\r\n", "<br>")
        .Replace("\n", "<br>")
        .Replace("\r", "<br>");
}
```

## 🎨 Premium CSS Architecture

### Card System Styling
```css
/* Maximum specificity for override guarantee */
html body .view-personal-modal-content .detail-card {
    background: white !important;
    border-radius: 16px !important;
    box-shadow: 
        0 8px 32px rgba(0, 0, 0, 0.12),
        0 2px 12px rgba(0, 0, 0, 0.08) !important;
    overflow: hidden !important;
    position: relative !important;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1) !important;
    border: 1px solid rgba(0, 0, 0, 0.05) !important;
    margin: 0 !important;
    padding: 0 !important;
    min-height: 180px !important;
}

/* Gradient top border for visual distinction */
html body .view-personal-modal-content .detail-card::before {
    content: '' !important;
    position: absolute !important;
    top: 0 !important;
    left: 0 !important;
    right: 0 !important;
    height: 4px !important;
    background: linear-gradient(90deg, #ff6b6b 0%, #4ecdc4 50%, #45b7d1 100%) !important;
    z-index: 1 !important;
    display: block !important;
}
```

### Hover Effects and Animations
```css
html body .view-personal-modal-content .detail-card:hover {
    transform: translateY(-6px) scale(1.01) !important;
    box-shadow: 
        0 16px 48px rgba(0, 0, 0, 0.2),
        0 6px 20px rgba(0, 0, 0, 0.12) !important;
}

/* Card-specific gradient variations */
html body .view-personal-modal-content .detail-card:nth-child(2)::before {
    background: linear-gradient(90deg, #4ecdc4 0%, #45b7d1 50%, #667eea 100%) !important;
}

html body .view-personal-modal-content .detail-card:nth-child(3)::before {
    background: linear-gradient(90deg, #667eea 0%, #764ba2 50%, #ff6b6b 100%) !important;
}
```

### Header Styling with Glass Effect
```css
html body .view-personal-modal-content .card-header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
    color: white !important;
    padding: 18px 22px !important;
    display: flex !important;
    align-items: center !important;
    gap: 14px !important;
    position: relative !important;
    z-index: 2 !important;
    overflow: hidden !important;
}

/* Animated shine effect */
html body .view-personal-modal-content .card-header::before {
    content: '' !important;
    position: absolute !important;
    top: 0 !important;
    left: -100% !important;
    width: 100% !important;
    height: 100% !important;
    background: linear-gradient(90deg, transparent 0%, rgba(255, 255, 255, 0.3) 50%, transparent 100%) !important;
    transition: left 0.6s ease !important;
    z-index: 1 !important;
}

html body .view-personal-modal-content .card-header:hover::before {
    left: 100% !important;
}
```

### Field Value Styling
```css
html body .view-personal-modal-content .field-value {
    font-size: 14px !important;
    font-weight: 500 !important;
    color: #1f2937 !important;
    padding: 10px 14px !important;
    background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%) !important;
    border-radius: 10px !important;
    border: 2px solid #e5e7eb !important;
    transition: all 0.25s ease !important;
    position: relative !important;
    overflow: hidden !important;
}

/* Hover shimmer effect */
html body .view-personal-modal-content .field-value::before {
    content: '' !important;
    position: absolute !important;
    top: 0 !important;
    left: -100% !important;
    width: 100% !important;
    height: 100% !important;
    background: linear-gradient(90deg, transparent 0%, rgba(102, 126, 234, 0.08) 50%, transparent 100%) !important;
    transition: left 0.5s ease !important;
    z-index: 0 !important;
}

html body .view-personal-modal-content .field-value:hover::before {
    left: 100% !important;
}
```

### Status Badge System
```css
html body .view-personal-modal-content .status-badge {
    padding: 6px 12px !important;
    border-radius: 20px !important;
    font-size: 11px !important;
    font-weight: 600 !important;
    text-transform: uppercase !important;
    letter-spacing: 0.5px !important;
    display: inline-flex !important;
    align-items: center !important;
    justify-content: center !important;
    min-height: 28px !important;
    box-shadow: 0 3px 12px rgba(0, 0, 0, 0.15) !important;
    transition: all 0.25s ease !important;
}

html body .view-personal-modal-content .status-badge.status-activ {
    background: linear-gradient(135deg, #10b981, #059669) !important;
    color: white !important;
    box-shadow: 0 4px 16px rgba(16, 185, 129, 0.3) !important;
}

html body .view-personal-modal-content .status-badge.status-inactiv {
    background: linear-gradient(135deg, #ef4444, #dc2626) !important;
    color: white !important;
    box-shadow: 0 4px 16px rgba(239, 68, 68, 0.3) !important;
}
```

## 📱 Responsive Design Implementation

### Desktop Layout (1200px+)
```css
html body .view-personal-modal-content .personal-details-dashboard {
    display: grid !important;
    grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)) !important;
    gap: 18px !important;
    max-height: 580px !important;
    overflow-y: auto !important;
}
```

### Tablet Layout (768px - 1199px)
```css
@media (max-width: 1199px) {
    html body .view-personal-modal-content .personal-details-dashboard {
        grid-template-columns: 1fr 1fr !important;
        gap: 16px !important;
    }
}
```

### Mobile Layout (320px - 767px)
```css
@media (max-width: 767px) {
    html body .view-personal-modal-content .personal-details-dashboard {
        grid-template-columns: 1fr !important;
        gap: 16px !important;
        max-height: 500px !important;
    }
    
    html body .view-personal-modal-content .form-grid {
        grid-template-columns: 1fr !important;
        gap: 14px !important;
    }
}
```

## ♿ Accessibility Features

### ARIA Labels and Semantic HTML
```razor
<div class="detail-card" role="region" aria-labelledby="general-info-header">
    <div class="card-header">
        <h3 id="general-info-header">Informații Generale</h3>
    </div>
    <div class="card-content" aria-describedby="general-info-header">
        <!-- Content with proper labeling -->
    </div>
</div>
```

### Contact Links with Accessibility
```razor
<a href="mailto:@PersonalData.Email_Personal" 
   class="contact-link"
   aria-label="Trimite email către @PersonalData.Email_Personal"
   title="Deschide aplicația de email">
    <i class="fas fa-envelope" aria-hidden="true"></i>
    @PersonalData.Email_Personal
</a>
```

### High Contrast Support
```css
@media (prefers-contrast: high) {
    html body .view-personal-modal-content .detail-card {
        border: 3px solid #000 !important;
        background: #fff !important;
    }
    
    html body .view-personal-modal-content .field-value {
        color: #000 !important;
        background: #fff !important;
        border: 2px solid #000 !important;
    }
}
```

### Reduced Motion Support
```css
@media (prefers-reduced-motion: reduce) {
    html body .view-personal-modal-content .detail-card {
        transition: none !important;
    }
    
    html body .view-personal-modal-content .detail-card:hover {
        transform: none !important;
    }
    
    html body .view-personal-modal-content .card-header::before {
        transition: none !important;
    }
}
```

## 🚀 Performance Optimizations

### Lazy Loading Implementation
```csharp
protected override async Task OnInitializedAsync()
{
    try
    {
        isLoading = true;
        StateHasChanged();
        
        // Simulate data processing time for complex calculations
        await Task.Delay(100); // Allow UI to render loading state
        
        if (PersonalData != null)
        {
            Logger.LogInformation("📊 Loading personal details for {PersonalName}", 
                PersonalData.NumeComplet);
            
            // Pre-calculate expensive operations
            await PreCalculateDisplayData();
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error initializing personal details view");
        hasError = true;
        errorMessage = "Eroare la încărcarea detaliilor personalului";
    }
    finally
    {
        isLoading = false;
        StateHasChanged();
    }
}

private async Task PreCalculateDisplayData()
{
    // Pre-calculate age, validity status, formatted addresses etc.
    // This prevents recalculation on every render
    await Task.Run(() =>
    {
        var age = CalculateAge(PersonalData.Data_Nasterii);
        var validityStatus = GetValidityStatus(PersonalData.Valabil_CI_Pana);
        var formattedPhone = FormatPhoneNumber(PersonalData.Telefon_Personal);
        // Cache these values for use in rendering
    });
}
```

### Memoization for Expensive Operations
```csharp
private readonly Dictionary<string, object> _calculationCache = new();

private T GetOrCalculate<T>(string key, Func<T> calculation)
{
    if (_calculationCache.ContainsKey(key))
    {
        return (T)_calculationCache[key];
    }
    
    var result = calculation();
    _calculationCache[key] = result!;
    return result;
}

// Usage
private int GetAge() => GetOrCalculate("age", () => CalculateAge(PersonalData.Data_Nasterii));
```

## 🧪 Testing Strategy

### Component Unit Tests
```csharp
[TestFixture]
public class VizualizeazaPersonalTests
{
    [Test]
    public void GetValidityBadge_ExpiredDate_ReturnsExpiredBadge()
    {
        // Arrange
        var component = new VizualizeazaPersonal();
        var expiredDate = DateTime.Today.AddDays(-10);
        
        // Act
        var result = component.GetValidityBadge(expiredDate);
        
        // Assert
        Assert.That(result.Value, Contains.Substring("expired"));
    }
    
    [Test]
    public void FormatPhoneNumber_ValidPhone_ReturnsFormattedString()
    {
        // Arrange
        var component = new VizualizeazaPersonal();
        var phone = "0745123456";
        
        // Act
        var result = component.FormatPhoneNumber(phone);
        
        // Assert
        Assert.AreEqual("0745-123-456", result);
    }
}
```

### Visual Regression Tests
```javascript
// Using Playwright for visual testing
test('personal details modal visual consistency', async ({ page }) => {
    await page.goto('/administrare/personal');
    
    // Open modal with test data
    await page.click('[data-testid="view-personal-btn"]');
    
    // Wait for animations to complete
    await page.waitForTimeout(500);
    
    // Take screenshot and compare
    await expect(page.locator('.view-personal-modal-content')).toHaveScreenshot('personal-modal.png');
});
```

## 📊 Analytics and Monitoring

### User Interaction Tracking
```csharp
private async Task TrackUserInteraction(string action, string details = "")
{
    try
    {
        Logger.LogInformation("👤 User interaction - Action: {Action}, Details: {Details}, PersonalId: {PersonalId}", 
            action, details, PersonalData?.Id_Personal);
        
        // Optional: Send to analytics service
        await AnalyticsService.TrackEventAsync("PersonalModalInteraction", new
        {
            Action = action,
            Details = details,
            PersonalId = PersonalData?.Id_Personal,
            Timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        Logger.LogWarning(ex, "Failed to track user interaction");
    }
}

// Usage in event handlers
private async Task HandleEmailClick(string email)
{
    await TrackUserInteraction("EmailClick", email);
    // ... rest of implementation
}
```

### Performance Monitoring
```csharp
private readonly Stopwatch _renderStopwatch = new();

protected override void OnAfterRender(bool firstRender)
{
    if (firstRender)
    {
        _renderStopwatch.Stop();
        Logger.LogInformation("⏱️ Personal modal initial render completed in {ElapsedMs}ms", 
            _renderStopwatch.ElapsedMilliseconds);
    }
    
    base.OnAfterRender(firstRender);
}
```

## 🔧 Customization and Extension

### Custom Card Types
```csharp
public enum CardType
{
    General,
    Contact,
    Address,
    Professional,
    Identity,
    Observations,
    Custom
}

private string GetCardCssClass(CardType cardType)
{
    return cardType switch
    {
        CardType.General => "general-card",
        CardType.Contact => "contact-card",
        CardType.Address => "address-card",
        CardType.Professional => "professional-card",
        CardType.Identity => "identity-card",
        CardType.Observations => "observations-card",
        CardType.Custom => "custom-card",
        _ => "detail-card"
    };
}
```

### Custom Field Formatters
```csharp
private readonly Dictionary<string, Func<object?, string>> _fieldFormatters = new()
{
    ["phone"] = value => FormatPhoneNumber(value?.ToString() ?? ""),
    ["email"] = value => FormatEmail(value?.ToString() ?? ""),
    ["date"] = value => FormatDate(value as DateTime?),
    ["currency"] = value => FormatCurrency(value as decimal?),
    ["percentage"] = value => FormatPercentage(value as double?)
};

private string FormatFieldValue(string fieldType, object? value)
{
    return _fieldFormatters.ContainsKey(fieldType) 
        ? _fieldFormatters[fieldType](value)
        : value?.ToString() ?? "";
}
```

---

**🎯 Key Takeaways for Developers**:
1. **Card-based design** provides excellent information organization
2. **Premium CSS** with maximum specificity ensures consistent styling
3. **Interactive elements** (emails, phones) enhance user experience
4. **Accessibility features** make the component inclusive
5. **Performance optimizations** keep the modal responsive

**🔗 Related Components**:
- **AdministrarePersonal.razor** - Main management page
- **AdaugaEditezaPersonal.razor** - Form modal for editing
- **LocationDependentGridDropdowns.razor** - Lookup components

**📞 Technical Support**: development@valyanmed.ro  
**📖 UI/UX Guidelines**: Internal design system documentation  
**🎨 Figma Designs**: Available in internal design repository

**Document Version**: 2.0  
**Last Updated**: December 2024  
**Author**: ValyanMed UI/UX Team
