# 📦 Componente Reutilizabile - Consultație

Acest director conține componentele Blazor reutilizabile pentru modulul de consultații medicale.

## 📂 Structură

```
Components/Shared/
├── Medical/
│   └── IMCCalculator.razor        # Widget calcul IMC
│       └── IMCCalculator.razor.css
└── Consultatie/
    ├── ConsultatieHeader.razor    # Header modal
    │   └── ConsultatieHeader.razor.css
    └── ConsultatieFooter.razor    # Footer cu acțiuni
        └── ConsultatieFooter.razor.css
```

---

## 🧩 Componente

### 1. IMCCalculator

**Componentă medicală** pentru calculul și afișarea Indicelui de Masă Corporală conform standardelor OMS.

#### Usage

```razor
@* Import component *@
@using ValyanClinic.Components.Shared.Medical

@* Utilizare cu two-way binding *@
<IMCCalculator @bind-Greutate="Model.Greutate" 
               @bind-Inaltime="Model.Inaltime"
               ShowDetails="true" />
```

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Greutate` | `decimal?` | `null` | Greutatea în kg (two-way binding) |
| `Inaltime` | `decimal?` | `null` | Înălțimea în cm (two-way binding) |
| `ShowDetails` | `bool` | `true` | Afișează detalii (risc, recomandări) |

#### Features

- ✅ Input validat pentru greutate (1-500 kg)
- ✅ Input validat pentru înălțime (30-300 cm)
- ✅ Calcul automat la schimbare
- ✅ 6 categorii IMC cu badge-uri colorate
- ✅ Iconițe vizuale per categorie
- ✅ Risc sănătate (Low/Medium/High/Critical)
- ✅ Recomandări medicale personalizate
- ✅ Responsive design

#### Dependencies

- `IIMCCalculatorService` - injectat automat
- FontAwesome pentru iconițe

---

### 2. ConsultatieHeader

**Header component** pentru modal-ul de consultație. Afișează informații despre pacient și controale.

#### Usage

```razor
<ConsultatieHeader PacientInfo="@PacientInfo"
                   IsLoading="@IsLoadingPacient"
                   LastSaveTime="@LastSaveTime"
                   ShowDraftInfo="true"
                   OnClose="HandleClose" />
```

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `PacientInfo` | `PacientDetailDto?` | `null` | Informații pacient |
| `IsLoading` | `bool` | `false` | Loading state pentru skeleton |
| `LastSaveTime` | `DateTime?` | `null` | Când a fost salvat draft-ul |
| `ShowDraftInfo` | `bool` | `true` | Afișează info draft |
| `OnClose` | `EventCallback` | - | Event când se închide modal-ul |

#### Features

- ✅ Display nume complet, CNP, vârstă
- ✅ Date contact (telefon, email)
- ✅ Calcul automat vârstă din data nașterii
- ✅ Loading skeleton animat
- ✅ Info "Salvat acum X minute"
- ✅ Buton închidere cu animație
- ✅ Gradient background purple

---

### 3. ConsultatieFooter

**Footer component** cu butoanele de acțiune pentru formular.

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

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `IsSaving` | `bool` | `false` | Loading pentru salvare finală |
| `IsSavingDraft` | `bool` | `false` | Loading pentru draft |
| `ShowDraftButton` | `bool` | `true` | Afișează buton draft |
| `ShowPreviewButton` | `bool` | `true` | Afișează buton preview |
| `SaveButtonText` | `string` | `"Salvează Consultație"` | Text buton principal |
| `OnSaveDraft` | `EventCallback` | - | Event salvare draft |
| `OnPreview` | `EventCallback` | - | Event preview PDF |
| `OnCancel` | `EventCallback` | - | Event anulare |

#### Features

- ✅ 4 butoane cu funcții distincte
- ✅ Loading spinners pentru feedback
- ✅ Auto-disable în timpul salvării
- ✅ Responsive layout (mobile-first)
- ✅ Stilizare consistentă

---

## 🎨 Design System

### Culori

```css
/* Primary Gradient */
background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);

/* IMC Categories */
--subponderal: #fbbf24;  /* Yellow */
--normal: #34d399;       /* Green */
--supraponderal: #fb923c;/* Orange */
--obezitate1: #f87171;   /* Red */
--obezitate2: #dc2626;   /* Dark Red */
--obezitate-morbida: #991b1b; /* Very Dark Red */
```

### Typography

```css
--heading-size: 1.5rem;
--body-size: 0.95rem;
--small-size: 0.85rem;
--font-weight-normal: 500;
--font-weight-bold: 600;
```

### Spacing

```css
--padding-card: 1.5rem 2rem;
--gap-small: 0.5rem;
--gap-medium: 1rem;
--gap-large: 1.5rem;
--border-radius: 12px;
--border-radius-small: 8px;
```

---

## 📱 Responsive Breakpoints

```css
/* Mobile */
@media (max-width: 576px) { ... }

/* Tablet */
@media (max-width: 768px) { ... }

/* Desktop */
@media (min-width: 769px) { ... }
```

---

## 🔧 Best Practices

### 1. Two-Way Binding

```razor
@* Good ✅ *@
<IMCCalculator @bind-Greutate="Model.Greutate" />

@* Bad ❌ *@
<IMCCalculator Greutate="@Model.Greutate" 
               GreutateChanged="@((value) => Model.Greutate = value)" />
```

### 2. Event Callbacks

```csharp
// Component definition
[Parameter] public EventCallback OnClose { get; set; }

// Invoke event
await OnClose.InvokeAsync();
```

### 3. Scoped CSS

Toate componentele folosesc **scoped CSS** pentru izolare:

```css
/* IMCCalculator.razor.css */
.imc-calculator-card { ... }
```

---

## 🧪 Testing

### Unit Testing

Componentele sunt testabile prin:
- Mock-uri pentru servicii (`IIMCCalculatorService`)
- bUnit pentru component testing
- EventCallback testing

### Manual Testing

```bash
# Run app
dotnet run --project ValyanClinic

# Navigate to
https://localhost:5001/dashboard
```

---

## 📚 Dependencies

### NuGet Packages
- `Microsoft.AspNetCore.Components` - Blazor framework
- FontAwesome CSS - Iconițe

### Internal Services
- `IIMCCalculatorService` - Calcul IMC
- `IDraftStorageService<T>` - Management draft-uri

---

## 🚀 Roadmap

### Faza 2.1 (Current) ✅
- [x] IMCCalculator
- [x] ConsultatieHeader
- [x] ConsultatieFooter

### Faza 2.2 (Next)
- [ ] ConsultatieProgress
- [ ] ConsultatieTabs

### Faza 2.3 (Future)
- [ ] Tab components (7 total)
- [ ] ICD10Autocomplete
- [ ] MedicationSelector

---

## 📖 Documentație Suplimentară

- [Phase 1 Changelog](../../DevSupport/Refactoring/CHANGELOG_ConsultatieModal_Phase1.md)
- [Phase 2 Progress](../../DevSupport/Refactoring/PROGRESS_ConsultatieModal_Phase2.md)
- [Architecture Decision Records](../../DevSupport/ADR/)

---

**Autor:** AI Assistant (Claude)  
**Data:** 19 decembrie 2024  
**Versiune:** 1.0
