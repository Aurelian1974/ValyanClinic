# 📖 Ghid de Utilizare - Componente Consultație

## Quick Start

### 1. Import Namespaces

```razor
@using ValyanClinic.Components.Shared.Medical
@using ValyanClinic.Components.Shared.Consultatie
@using ValyanClinic.Components.Shared.Consultatie.Tabs
```

---

## 🧩 Componente Disponibile

### IMCCalculator

**Componentă medicală reutilizabilă** pentru calcul IMC conform standardelor OMS.

#### Usage

```razor
<IMCCalculator @bind-Greutate="Model.Greutate"
               @bind-Inaltime="Model.Inaltime"
               ShowDetails="true" />
```

#### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Greutate` | `decimal?` | Yes | `null` | Greutatea în kg |
| `Inaltime` | `decimal?` | Yes | `null` | Înălțimea în cm |
| `ShowDetails` | `bool` | No | `true` | Afișează risc și recomandări |

#### Two-Way Binding

Componenta suportă `@bind` pentru sincronizare automată:

```razor
@* Modificările în input-uri updatează automat Model *@
<IMCCalculator @bind-Greutate="Model.Greutate"
               @bind-Inaltime="Model.Inaltime" />

@* Acum poți accesa valorile *@
<p>Greutate: @Model.Greutate kg</p>
<p>Înălțime: @Model.Inaltime cm</p>
```

---

### ConsultatieHeader

**Header component** pentru modal cu informații pacient.

#### Usage

```razor
<ConsultatieHeader PacientInfo="@PacientInfo"
                   IsLoading="@IsLoadingPacient"
                   LastSaveTime="@LastSaveTime"
                   ShowDraftInfo="true"
                   OnClose="HandleClose" />
```

#### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `PacientInfo` | `PacientDetailDto?` | Yes | DTO cu informații pacient |
| `IsLoading` | `bool` | No | Afișează skeleton loading |
| `LastSaveTime` | `DateTime?` | No | Timpul ultimei salvări draft |
| `ShowDraftInfo` | `bool` | No | Afișează info draft |
| `OnClose` | `EventCallback` | Yes | Event când se închide |

#### Example

```csharp
// În code-behind
private async Task HandleClose()
{
    // Logic înainte de închidere
    if (HasUnsavedChanges)
    {
        await SaveDraft();
    }
    
    // Închide modal
    IsVisible = false;
}
```

---

### ConsultatieFooter

**Footer component** cu butoane de acțiune.

#### Usage

```razor
<ConsultatieFooter IsSaving="@IsSaving"
                   IsSavingDraft="@IsSavingDraft"
                   ShowDraftButton="true"
                   ShowPreviewButton="true"
                   SaveButtonText="Salvează Consultație"
                   OnSaveDraft="HandleSaveDraft"
                   OnPreview="HandlePreview"
                   OnCancel="HandleCancel" />
```

#### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `IsSaving` | `bool` | No | Loading state pentru submit |
| `IsSavingDraft` | `bool` | No | Loading state pentru draft |
| `ShowDraftButton` | `bool` | No | Afișează buton draft |
| `ShowPreviewButton` | `bool` | No | Afișează buton preview |
| `SaveButtonText` | `string` | No | Text buton principal |
| `OnSaveDraft` | `EventCallback` | Yes | Event salvare draft |
| `OnPreview` | `EventCallback` | No | Event preview PDF |
| `OnCancel` | `EventCallback` | Yes | Event anulare |

---

### ConsultatieProgress

**Progress indicator** pentru completion tracking.

#### Usage

```razor
<ConsultatieProgress Sections="@Sections"
                     CompletedSections="@CompletedSections"
                     ActiveSection="@ActiveSection" />
```

#### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `Sections` | `List<string>` | Yes | Lista secțiunilor |
| `CompletedSections` | `HashSet<string>` | Yes | Secțiuni completate |
| `ActiveSection` | `string` | Yes | Secțiunea curentă |

#### Example

```csharp
private List<string> Sections = new()
{
    "motive",
    "antecedente",
    "examen",
    "investigatii",
    "diagnostic",
    "tratament",
    "concluzie"
};

private HashSet<string> CompletedSections = new();

private void MarkSectionCompleted(string section)
{
    CompletedSections.Add(section);
    StateHasChanged();
}
```

---

### ConsultatieTabs

**Tab navigation** component.

#### Usage

```razor
<ConsultatieTabs Tabs="@Tabs"
                 ActiveTab="@ActiveTab"
                 CompletedTabs="@CompletedTabs"
                 OnTabChanged="HandleTabChanged"
                 IsDisabled="@IsSaving"
                 TabBadges="@BadgesDict" />
```

#### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `Tabs` | `List<string>` | Yes | Lista tab-urilor |
| `ActiveTab` | `string` | Yes | Tab-ul activ |
| `CompletedTabs` | `HashSet<string>` | Yes | Tab-uri completate |
| `OnTabChanged` | `EventCallback<string>` | Yes | Event la schimbare tab |
| `IsDisabled` | `bool` | No | Disable toate tab-urile |
| `TabBadges` | `Dictionary<string, int>?` | No | Badge-uri per tab |

#### Example

```csharp
private async Task HandleTabChanged(string newTab)
{
    // Save current tab state
    await SaveCurrentTab();
    
    // Change tab
    ActiveTab = newTab;
    
    // Load new tab
    await LoadTab(newTab);
    
    StateHasChanged();
}
```

---

## 🗂️ Tab Components

### MotivePrezentareTab

**Primul tab** - Motive prezentare și istoric.

#### Usage

```razor
<MotivePrezentareTab Model="@Model"
                     OnChanged="HandleFieldChanged"
                     OnSectionCompleted="HandleSectionCompleted"
                     ShowValidation="@ShowValidation" />
```

### ExamenTab

**Tab examen obiectiv** - Include IMCCalculator.

#### Usage

```razor
<ExamenTab Model="@Model"
           OnChanged="HandleFieldChanged"
           OnSectionCompleted="HandleSectionCompleted"
           ShowValidation="@ShowValidation" />
```

### DiagnosticTab

**Tab diagnostic** - Include management ICD-10.

#### Usage

```razor
<DiagnosticTab Model="@Model"
               OnChanged="HandleFieldChanged"
               OnSectionCompleted="HandleSectionCompleted"
               ShowValidation="@ShowValidation" />
```

---

## 🔄 Common Patterns

### Pattern 1: Tab Content Switching

```razor
@switch (ActiveTab)
{
    case "motive":
        <MotivePrezentareTab Model="@Model" OnChanged="HandleChanged" />
        break;
    case "examen":
        <ExamenTab Model="@Model" OnChanged="HandleChanged" />
        break;
    case "diagnostic":
        <DiagnosticTab Model="@Model" OnChanged="HandleChanged" />
        break;
}
```

### Pattern 2: Event Handling

```csharp
// În code-behind
private async Task HandleFieldChanged()
{
    // Mark as changed
    HasUnsavedChanges = true;
    
    // Trigger auto-save after delay
    _autoSaveDebounce?.Dispose();
    _autoSaveDebounce = new Timer(async _ =>
    {
        await InvokeAsync(async () =>
        {
            await SaveDraft();
        });
    }, null, 5000, Timeout.Infinite);
    
    StateHasChanged();
}

private async Task HandleSectionCompleted()
{
    // Mark section as completed
    CompletedSections.Add(ActiveSection);
    
    // Update progress
    StateHasChanged();
    
    // Optional: Auto-advance to next section
    if (ShouldAutoAdvance)
    {
        await AdvanceToNextSection();
    }
}
```

### Pattern 3: Validation

```csharp
private bool ShowValidation = false;

private async Task HandleSubmit()
{
    ShowValidation = true;
    
    if (!IsFormValid())
    {
        // Show error toast
        await ShowToast("Completați câmpurile obligatorii");
        return;
    }
    
    // Submit form
    await SubmitConsultatie();
}

private bool IsFormValid()
{
    return !string.IsNullOrWhiteSpace(Model.MotivPrezentare) &&
           !string.IsNullOrWhiteSpace(Model.DiagnosticPozitiv);
}
```

---

## 🎨 Customization

### Styling

Toate componentele folosesc **scoped CSS**. Pentru override:

```css
/* În parent component CSS */
::deep .imc-calculator-card {
    background: linear-gradient(135deg, #your-color-1, #your-color-2);
}

::deep .consultatie-header {
    background: your-custom-gradient;
}
```

### Theme Variables

```css
/* Definește în root CSS */
:root {
    --consultatie-primary: #667eea;
    --consultatie-secondary: #764ba2;
    --consultatie-success: #34d399;
    --consultatie-error: #dc2626;
}
```

---

## ⚠️ Important Notes

### 1. Two-Way Binding

```razor
@* ✅ GOOD - Two-way binding *@
<IMCCalculator @bind-Greutate="Model.Greutate" />

@* ❌ BAD - Manual binding *@
<IMCCalculator Greutate="@Model.Greutate"
               GreutateChanged="@((value) => Model.Greutate = value)" />
```

### 2. EventCallback vs Action

```csharp
// ✅ GOOD - Use EventCallback
[Parameter] public EventCallback OnChanged { get; set; }

// ❌ BAD - Use Action (nu funcționează în Blazor)
[Parameter] public Action OnChanged { get; set; }
```

### 3. StateHasChanged

```csharp
// După modificări în event handlers
private async Task HandleChanged()
{
    // Make changes
    Model.Field = "new value";
    
    // Notify UI
    await InvokeAsync(StateHasChanged);
}
```

---

## 🐛 Troubleshooting

### Component nu se afișează

**Problem:** Componenta nu apare în UI  
**Solution:** Verifică că ai importat namespace-ul corect

```razor
@using ValyanClinic.Components.Shared.Consultatie
```

### EventCallback nu se declanșează

**Problem:** Event-ul nu ajunge la parent  
**Solution:** Verifică că ai făcut `await InvokeAsync()`

```csharp
await OnChanged.InvokeAsync();
```

### Styling nu se aplică

**Problem:** CSS-ul nu se aplică corect  
**Solution:** Verifică că `.razor.css` e în același folder cu `.razor`

---

## 📚 Additional Resources

- [Blazor Component Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/)
- [EventCallback Best Practices](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/event-handling)
- [Scoped CSS in Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/css-isolation)

---

**Autor:** AI Assistant (Claude)  
**Data:** 19 decembrie 2024  
**Versiune:** 1.0
