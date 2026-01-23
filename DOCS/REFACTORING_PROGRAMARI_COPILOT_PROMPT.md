# Refactorizare Modul Programări - Valyan Clinic

## Context Proiect
Aplicație medicală Blazor Server (.NET 8) pentru clinică privată.
**Stack:** Blazor Server, MudBlazor, Syncfusion, MediatR (CQRS), EF Core, SQL Server.

## Scope: Modulul Programări

### Inventar Actual (23 fișiere UI)
```
Components/Pages/Programari/
├── CalendarProgramari.razor (.cs, .css - 202 linii + 394 CSS)
├── ListaProgramari.razor (.cs, .css - 442 linii + 817 CSS)
└── Modals/
    ├── ProgramareViewModal.razor (.cs, .css)
    ├── ProgramareAddEditModal.razor (.cs, .css)
    ├── ConfirmCancelModal.razor (.cs, .css)
    ├── ProgramareStatisticsModal.razor (.cs, .css)
    ├── ProgramareSchedulerModal.razor (.cs, .css)
    └── SendDailyEmailModal.razor (.cs)

Total: 3445 linii CSS scoped - 15+ pattern-uri duplicate
```

### Backend (CQRS bine organizat - NU MODIFICA)
- ✅ 11 Queries, 3 Commands, 5 DTOs (Application Layer)
- ✅ Programare Entity cu 27 proprietăți
- ✅ Enums: TipProgramare, ProgramareStatus
- ⚠️ Posibilă necesitate servicii helper pentru UI logic

---

## ⚠️ IMPORTANT: Pattern Code-Behind OBLIGATORIU

**TOATĂ aplicația folosește separarea strictă:**
```
ComponentName.razor       → DOAR markup (HTML + Razor syntax)
ComponentName.razor.cs    → TOATĂ logica C# (proprietăți, metode, handlers)
~~ComponentName.razor.css~~ → va fi ELIMINAT și mutat în CSS global
```

### ❌ NU FOLOSI blocuri @code în .razor:
```razor
@* NU FACE ASTA! *@
@code {
    [Parameter] public string Status { get; set; }
    private void DoSomething() { }
}
```

### ✅ FOLOSEȘTE separation of concerns:

**StatusBadge.razor** (doar markup):
```razor
@* Components/Shared/Programari/StatusBadge.razor *@

<span class="programare-status-badge programare-status-badge--@StatusClass">
    @if (ShowIcon)
    {
        <i class="fas fa-@StatusIcon"></i>
    }
    @StatusDisplay
</span>
```

**StatusBadge.razor.cs** (toată logica):
```csharp
// Components/Shared/Programari/StatusBadge.razor.cs

using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Shared.Programari;

public partial class StatusBadge : ComponentBase
{
    [Parameter, EditorRequired]
    public string Status { get; set; } = "";

    [Parameter]
    public bool ShowIcon { get; set; } = true;

    // Computed properties pentru binding în .razor
    private string StatusClass => GetStatusClass();
    private string StatusDisplay => GetStatusDisplay();
    private string StatusIcon => GetStatusIcon();

    private string GetStatusClass() => Status?.ToLower() ?? "necunoscut";

    private string GetStatusDisplay() => Status?.ToLower() switch
    {
        "programata" => "Programată",
        "confirmata" => "Confirmată",
        "checkedin" => "Check-in",
        "inconsultatie" => "În consultație",
        "finalizata" => "Finalizată",
        "anulata" => "Anulată",
        "noshow" => "Nu s-a prezentat",
        _ => "Necunoscut"
    };

    private string GetStatusIcon() => Status?.ToLower() switch
    {
        "programata" => "calendar-plus",
        "confirmata" => "calendar-check",
        "checkedin" => "door-open",
        "inconsultatie" => "user-md",
        "finalizata" => "check-circle",
        "anulata" => "times-circle",
        "noshow" => "user-slash",
        _ => "question-circle"
    };
}
```

---

## 🎯 OBIECTIVE REFACTORIZARE

### 1. Migrare CSS Scoped → CSS Global (3445 linii)

**Problemă identificată:**
- **15+ pattern-uri de culori duplicate** (gradiente albastru, shadows, border-radius)
- **30+ apariții border-radius: 8px/12px** - fără consistență
- **25+ apariții box-shadow** - valori hardcodate
- **20+ padding/gap patterns** - valori magice

**Soluție - CSS Variables + BEM:**

#### A. Creează structura CSS globală
```
wwwroot/css/
├── programari/
│   ├── _variables.css          # CSS custom properties
│   ├── _calendar.css           # Syncfusion scheduler overrides
│   ├── _modals.css             # Modal layouts și styles
│   ├── _grid.css               # Lista programări grid/table
│   ├── _filters.css            # Advanced filters panel
│   ├── _badges-status.css      # Status badges și chips
│   └── programari.css          # Main import file
└── app.css                     # Import @import 'programari/programari.css';
```

#### B. Definește CSS Variables din pattern-uri găsite

**_variables.css:**
```css
:root {
  /* === THEME COLORS === */
  --programari-primary-blue: #3b82f6;
  --programari-primary-blue-dark: #2563eb;
  --programari-primary-blue-darker: #1e40af;

  /* Gradient Patterns (14+ apariții) */
  --gradient-blue-light: linear-gradient(135deg, #93c5fd 0%, #60a5fa 100%);
  --gradient-blue-medium: linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%);
  --gradient-blue-dark: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);

  /* Background Colors */
  --bg-blue-lightest: #eff6ff;
  --bg-blue-light: #dbeafe;
  --bg-blue-medium: #bfdbfe;

  /* Text Colors */
  --text-dark: #334155;
  --text-medium: #64748b;
  --text-light: #94a3b8;

  /* Status Colors */
  --status-success: #10b981;
  --status-danger: #ef4444;
  --status-warning: #f59e0b;

  /* === SPACING SYSTEM === */
  --spacing-xs: 4px;
  --spacing-sm: 8px;    /* gap: 8px - 30+ apariții */
  --spacing-md: 12px;   /* gap: 12px - 25+ apariții */
  --spacing-lg: 16px;
  --spacing-xl: 20px;

  /* Padding Patterns */
  --padding-button: 10px 20px;      /* 25+ apariții */
  --padding-label: 12px 16px;       /* 20+ apariții */
  --padding-small: 8px 12px;        /* 18+ apariții */

  /* === BORDERS === */
  --border-radius-sm: 6px;          /* 20+ apariții */
  --border-radius-md: 8px;          /* 30+ apariții */
  --border-radius-lg: 12px;         /* 25+ apariții */
  --border-radius-full: 20px;       /* badges/chips */

  /* === SHADOWS === */
  --shadow-sm: 0 1px 3px rgba(0, 0, 0, 0.1);        /* 12+ apariții */
  --shadow-md: 0 2px 8px rgba(0, 0, 0, 0.1);        /* 20+ apariții */
  --shadow-lg: 0 4px 12px rgba(0, 0, 0, 0.2);       /* 15+ apariții */
  --shadow-focus: 0 0 0 3px rgba(96, 165, 250, 0.1); /* 8+ apariții */

  /* === TRANSITIONS === */
  --transition-fast: 0.15s ease;
  --transition-normal: 0.3s ease;
}
```

**_badges-status.css (consolidează 5 componente):**
```css
/* Status Badge Pattern - prezent în 5 locuri */
.programare-status-badge {
  display: inline-flex;
  align-items: center;
  padding: var(--padding-small);
  border-radius: var(--border-radius-full);
  font-size: 0.875rem;
  font-weight: 600;
  gap: var(--spacing-xs);
}

.programare-status-badge--programata {
  background: var(--gradient-blue-light);
  color: var(--programari-primary-blue-dark);
}

.programare-status-badge--confirmata {
  background: linear-gradient(135deg, #86efac 0%, #22c55e 100%);
  color: #15803d;
}

.programare-status-badge--checkedin {
  background: linear-gradient(135deg, #fde68a 0%, #fbbf24 100%);
  color: #92400e;
}

.programare-status-badge--inconsultatie {
  background: linear-gradient(135deg, #c084fc 0%, #a855f7 100%);
  color: #6b21a8;
}

.programare-status-badge--finalizata {
  background: linear-gradient(135deg, #86efac 0%, #10b981 100%);
  color: #047857;
}

.programare-status-badge--anulata {
  background: linear-gradient(135deg, #fca5a5 0%, #ef4444 100%);
  color: #991b1b;
}

.programare-status-badge--noshow {
  background: linear-gradient(135deg, #d1d5db 0%, #9ca3af 100%);
  color: #374151;
}

.programare-status-badge--necunoscut {
  background: #f3f4f6;
  color: #6b7280;
}
```

**_modals.css (pattern din 3 modals):**
```css
/* Modal Header Pattern - repetitiv în 3 modals */
.programare-modal__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-lg);
  border-bottom: 1px solid var(--bg-blue-light);
  background: var(--bg-blue-lightest);
}

.programare-modal__title {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--text-dark);
}

.programare-modal__title i {
  color: var(--programari-primary-blue);
}

/* Modal Body */
.programare-modal__body {
  padding: var(--spacing-lg);
}

/* Modal Footer */
.programare-modal__footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-md);
  padding: var(--spacing-lg);
  border-top: 1px solid var(--bg-blue-light);
  background: var(--bg-blue-lightest);
}

/* Detail Section Pattern - repetitiv în 3 locuri */
.detail-section {
  margin-bottom: var(--spacing-lg);
}

.detail-section__title {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-md);
  font-size: 1rem;
  font-weight: 600;
  color: var(--programari-primary-blue);
}

.detail-section__grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  background: var(--bg-blue-lightest);
  border-radius: var(--border-radius-md);
}

.detail-item {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.detail-item__label {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-medium);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.detail-item__value {
  font-size: 0.875rem;
  color: var(--text-dark);
}
```

**_filters.css (advanced filter panel - 2 locuri):**
```css
.advanced-filter-panel {
  background: var(--bg-blue-lightest);
  border-radius: var(--border-radius-lg);
  padding: 0;
  margin-bottom: var(--spacing-lg);
  max-height: 0;
  overflow: hidden;
  transition: max-height var(--transition-normal);
  border: 1px solid var(--bg-blue-light);
}

.advanced-filter-panel.expanded {
  max-height: 600px;
  padding: var(--spacing-lg);
}

.filter-panel__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-md);
}

.filter-panel__toggle {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--padding-button);
  background: var(--gradient-blue-light);
  color: var(--programari-primary-blue-dark);
  border: none;
  border-radius: var(--border-radius-md);
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.filter-panel__toggle:hover {
  box-shadow: var(--shadow-md);
  transform: translateY(-1px);
}

.filters-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-md);
}

.filter-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-md);
  padding-top: var(--spacing-md);
  border-top: 1px solid var(--bg-blue-light);
}
```

**_calendar.css (Syncfusion overrides):**
```css
/* Calendar Container */
.calendar-container {
  background: white;
  border-radius: var(--border-radius-lg);
  box-shadow: var(--shadow-md);
  padding: var(--spacing-lg);
}

.calendar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-lg);
  padding: var(--padding-label);
  background: var(--gradient-blue-light);
  border-radius: var(--border-radius-lg);
}

/* Syncfusion Scheduler Overrides */
.e-schedule .e-timeline-view .e-resource-cells,
.e-schedule .e-timeline-month-view .e-resource-cells {
  background: var(--bg-blue-lightest);
}

.e-schedule .e-appointment {
  border-radius: var(--border-radius-sm);
  border-left-width: 4px;
}

.e-schedule .e-appointment.e-programata {
  background: var(--gradient-blue-medium);
  border-left-color: var(--programari-primary-blue-darker);
}

.e-schedule .e-appointment.e-confirmata {
  background: linear-gradient(135deg, #86efac 0%, #22c55e 100%);
  border-left-color: #15803d;
}

.e-schedule .e-appointment.e-anulata {
  background: linear-gradient(135deg, #fca5a5 0%, #ef4444 100%);
  border-left-color: #991b1b;
  opacity: 0.7;
}
```

**_grid.css (Lista programări table):**
```css
/* Grid Container */
.programari-grid-container {
  background: white;
  border-radius: var(--border-radius-lg);
  box-shadow: var(--shadow-md);
  padding: var(--spacing-lg);
}

.grid-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-lg);
}

.grid-header__title {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--text-dark);
}

.grid-header__actions {
  display: flex;
  gap: var(--spacing-md);
}

/* Search Bar */
.search-bar {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--padding-small);
  background: var(--bg-blue-lightest);
  border-radius: var(--border-radius-md);
  border: 1px solid var(--bg-blue-light);
  margin-bottom: var(--spacing-lg);
}

.search-bar input {
  flex: 1;
  border: none;
  background: transparent;
  outline: none;
}

/* Syncfusion Grid Overrides */
.e-grid .e-headercell {
  background: var(--gradient-blue-light);
  color: var(--programari-primary-blue-dark);
  font-weight: 600;
}

.e-grid .e-row:hover {
  background: var(--bg-blue-lightest);
}

.e-grid .e-gridheader {
  border-top-left-radius: var(--border-radius-md);
  border-top-right-radius: var(--border-radius-md);
}
```

**programari.css (main import file):**
```css
/* Main import file pentru modulul Programări */

@import '_variables.css';
@import '_badges-status.css';
@import '_modals.css';
@import '_filters.css';
@import '_calendar.css';
@import '_grid.css';

/* Loading Overlay - common pentru toate componentele */
.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.9);
  z-index: 1000;
  gap: var(--spacing-md);
}

.loading-overlay p {
  color: var(--text-medium);
  font-weight: 600;
}

/* Empty State - common pentru toate componentele */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--spacing-xl) var(--spacing-lg);
  text-align: center;
  color: var(--text-medium);
}

.empty-state i {
  color: var(--bg-blue-medium);
  margin-bottom: var(--spacing-lg);
}

.empty-state h5 {
  color: var(--text-dark);
  margin-bottom: var(--spacing-sm);
}

.empty-state p {
  color: var(--text-medium);
  margin-bottom: var(--spacing-lg);
}
```

#### C. Plan de migrare CSS

1. [ ] **Creează `wwwroot/css/programari/` folder**
2. [ ] **Creează `_variables.css`** cu toate custom properties
3. [ ] **Creează `_badges-status.css`** - status badge styles
4. [ ] **Creează `_modals.css`** - modal structure și detail sections
5. [ ] **Creează `_filters.css`** - advanced filter panel
6. [ ] **Creează `_calendar.css`** - migrează din CalendarProgramari.razor.css (394 linii)
7. [ ] **Creează `_grid.css`** - migrează din ListaProgramari.razor.css (817 linii)
8. [ ] **Creează `programari.css`** - main import file cu loading/empty state common
9. [ ] **Șterge toate `.razor.css` files** (6 fișiere)
10. [ ] **Update `wwwroot/css/app.css`** adaugă `@import 'programari/programari.css';`
11. [ ] **Test vizual complet** - toate paginile și modalele

**✅ Checkpoint:** CSS 100% global, zero scoped CSS, aspect vizual identic

---

### 2. Extragere Componente Refolosibile

#### A. Componente identificate pentru extragere

**PRIORITATE ÎNALTĂ (markup duplicat în 3+ locuri):**

##### 1. StatusBadge - Pattern în 5 componente

**StatusBadge.razor:**
```razor
@* Components/Shared/Programari/StatusBadge.razor *@

<span class="programare-status-badge programare-status-badge--@StatusClass">
    @if (ShowIcon)
    {
        <i class="fas fa-@StatusIcon"></i>
    }
    @StatusDisplay
</span>
```

**StatusBadge.razor.cs:**
```csharp
// Components/Shared/Programari/StatusBadge.razor.cs

using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Shared.Programari;

/// <summary>
/// Componentă pentru afișarea badge-ului de status programare.
/// Consolidează logica de display din 5 componente diferite.
/// </summary>
public partial class StatusBadge : ComponentBase
{
    /// <summary>
    /// Status-ul programării (ex: "programata", "confirmata", "anulata")
    /// </summary>
    [Parameter, EditorRequired]
    public string Status { get; set; } = "";

    /// <summary>
    /// Determină dacă se afișează iconul FontAwesome
    /// </summary>
    [Parameter]
    public bool ShowIcon { get; set; } = true;

    // Computed properties pentru binding în template
    private string StatusClass => GetStatusClass();
    private string StatusDisplay => GetStatusDisplay();
    private string StatusIcon => GetStatusIcon();

    private string GetStatusClass() => Status?.ToLower() ?? "necunoscut";

    private string GetStatusDisplay() => Status?.ToLower() switch
    {
        "programata" => "Programată",
        "confirmata" => "Confirmată",
        "checkedin" => "Check-in",
        "inconsultatie" => "În consultație",
        "finalizata" => "Finalizată",
        "anulata" => "Anulată",
        "noshow" => "Nu s-a prezentat",
        _ => "Necunoscut"
    };

    private string GetStatusIcon() => Status?.ToLower() switch
    {
        "programata" => "calendar-plus",
        "confirmata" => "calendar-check",
        "checkedin" => "door-open",
        "inconsultatie" => "user-md",
        "finalizata" => "check-circle",
        "anulata" => "times-circle",
        "noshow" => "user-slash",
        _ => "question-circle"
    };
}
```

**Utilizare:**
```razor
@* Înainte (ListaProgramari.razor): *@
<span class="badge badge-@GetStatusBadgeClass(programare.Status)">
    @GetStatusDisplay(programare.Status)
</span>

@* După (ListaProgramari.razor): *@
<StatusBadge Status="@programare.Status" />
```

---

##### 2. LoadingOverlay - Pattern în 4 modals

**LoadingOverlay.razor:**
```razor
@* Components/Shared/LoadingOverlay.razor *@

@if (IsLoading)
{
    <div class="loading-overlay">
        <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">@LoadingText</span>
        </div>
        <p>@LoadingText</p>
    </div>
}
```

**LoadingOverlay.razor.cs:**
```csharp
// Components/Shared/LoadingOverlay.razor.cs

using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Shared;

/// <summary>
/// Overlay de loading cu spinner pentru operațiuni asincrone.
/// </summary>
public partial class LoadingOverlay : ComponentBase
{
    /// <summary>
    /// Determină dacă overlay-ul este vizibil
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }

    /// <summary>
    /// Textul afișat sub spinner
    /// </summary>
    [Parameter]
    public string LoadingText { get; set; } = "Se încarcă...";
}
```

**Utilizare:**
```razor
@* Înainte (ProgramareViewModal.razor): *@
@if (IsLoading)
{
    <div class="loading-overlay">
        <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Se încarcă...</span>
        </div>
        <p>Se încarcă...</p>
    </div>
}

@* După (ProgramareViewModal.razor): *@
<LoadingOverlay IsLoading="@IsLoading" LoadingText="Se încarcă programarea..." />
```

---

##### 3. ModalHeader - Pattern în 3 modals

**ModalHeader.razor:**
```razor
@* Components/Shared/Modals/ModalHeader.razor *@

<div class="programare-modal__header">
    <h5 class="programare-modal__title">
        @if (!string.IsNullOrEmpty(Icon))
        {
            <i class="fas fa-@Icon"></i>
        }
        @Title
    </h5>
    <button type="button" class="btn-close" @onclick="HandleClose" aria-label="Închide"></button>
</div>
```

**ModalHeader.razor.cs:**
```csharp
// Components/Shared/Modals/ModalHeader.razor.cs

using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Shared.Modals;

/// <summary>
/// Header standard pentru modale cu titlu, icon și buton close.
/// </summary>
public partial class ModalHeader : ComponentBase
{
    /// <summary>
    /// Titlul modal-ului
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = "";

    /// <summary>
    /// Icon FontAwesome (fără prefixul "fa-")
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>
    /// Callback apelat la click pe butonul close
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    private async Task HandleClose()
    {
        await OnClose.InvokeAsync();
    }
}
```

**Utilizare:**
```razor
@* Înainte (ProgramareViewModal.razor): *@
<div class="modal-header">
    <h5 class="modal-title">
        <i class="fas fa-eye"></i>
        Vizualizare Programare
    </h5>
    <button type="button" class="btn-close" @onclick="CloseModal" />
</div>

@* După (ProgramareViewModal.razor): *@
<ModalHeader Title="Vizualizare Programare" Icon="eye" OnClose="CloseModal" />
```

---

##### 4. DetailSection - Pattern în 3 locuri

**DetailSection.razor:**
```razor
@* Components/Shared/Programari/DetailSection.razor *@

<div class="detail-section">
    <h6 class="detail-section__title">
        @if (!string.IsNullOrEmpty(Icon))
        {
            <i class="fas fa-@Icon"></i>
        }
        @Title
    </h6>
    <div class="detail-section__grid">
        @ChildContent
    </div>
</div>
```

**DetailSection.razor.cs:**
```csharp
// Components/Shared/Programari/DetailSection.razor.cs

using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Shared.Programari;

/// <summary>
/// Secțiune cu titlu și grid pentru afișare detalii programare.
/// </summary>
public partial class DetailSection : ComponentBase
{
    /// <summary>
    /// Titlul secțiunii
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = "";

    /// <summary>
    /// Icon FontAwesome (fără prefixul "fa-")
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>
    /// Conținutul secțiunii (detail items)
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**Utilizare:**
```razor
@* Înainte (ProgramareViewModal.razor): *@
<div class="detail-section">
    <h6 class="section-title">
        <i class="fas fa-user"></i>
        Informații Pacient
    </h6>
    <div class="detail-grid">
        <div class="detail-item">
            <span class="label">Nume:</span>
            <span class="value">@Programare.Pacient?.Nume</span>
        </div>
    </div>
</div>

@* După (ProgramareViewModal.razor): *@
<DetailSection Title="Informații Pacient" Icon="user">
    <div class="detail-item">
        <span class="detail-item__label">Nume:</span>
        <span class="detail-item__value">@Programare.Pacient?.Nume</span>
    </div>
</DetailSection>
```

---

##### 5. EmptyState - Pentru când nu există date

**EmptyState.razor:**
```razor
@* Components/Shared/EmptyState.razor *@

<div class="empty-state">
    <i class="fas fa-@Icon fa-3x"></i>
    <h5>@Title</h5>
    @if (!string.IsNullOrEmpty(Message))
    {
        <p>@Message</p>
    }
    @ActionButton
</div>
```

**EmptyState.razor.cs:**
```csharp
// Components/Shared/EmptyState.razor.cs

using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Shared;

/// <summary>
/// Componentă pentru afișarea stării goale (no data found).
/// </summary>
public partial class EmptyState : ComponentBase
{
    /// <summary>
    /// Icon FontAwesome (fără prefixul "fa-")
    /// </summary>
    [Parameter]
    public string Icon { get; set; } = "inbox";

    /// <summary>
    /// Titlul principal
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = "";

    /// <summary>
    /// Mesaj descriptiv opțional
    /// </summary>
    [Parameter]
    public string? Message { get; set; }

    /// <summary>
    /// Buton de acțiune opțional (ex: "Adaugă programare")
    /// </summary>
    [Parameter]
    public RenderFragment? ActionButton { get; set; }
}
```

**Utilizare:**
```razor
@* Înainte (ListaProgramari.razor): *@
@if (!Programari.Any())
{
    <div class="text-center p-5">
        <i class="fas fa-calendar-times fa-3x text-muted mb-3"></i>
        <h5>Nu există programări</h5>
        <p>Nu au fost găsite programări pentru criteriile selectate.</p>
    </div>
}

@* După (ListaProgramari.razor): *@
@if (!Programari.Any())
{
    <EmptyState
        Icon="calendar-times"
        Title="Nu există programări"
        Message="Nu au fost găsite programări pentru criteriile selectate.">
        <ActionButton>
            <button class="btn btn-primary" @onclick="OpenAddModal">
                <i class="fas fa-plus"></i> Adaugă programare
            </button>
        </ActionButton>
    </EmptyState>
}
```

---

**PRIORITATE MEDIE:**

##### 6. AdvancedFilterPanel - Pattern în 2 locuri

**AdvancedFilterPanel.razor:**
```razor
@* Components/Shared/Programari/AdvancedFilterPanel.razor *@

<div class="advanced-filter-panel @(IsExpanded ? "expanded" : "")">
    @if (ShowHeader)
    {
        <div class="filter-panel__header">
            <button class="filter-panel__toggle" @onclick="ToggleExpanded">
                <i class="fas fa-filter"></i>
                Filtre avansate
                <i class="fas fa-chevron-@(IsExpanded ? "up" : "down")"></i>
            </button>
        </div>
    }

    <div class="filters-grid">
        @ChildContent
    </div>

    @if (ShowActions)
    {
        <div class="filter-actions">
            <button class="btn btn-primary" @onclick="HandleApply">
                <i class="fas fa-check"></i> Aplică
            </button>
            <button class="btn btn-secondary" @onclick="HandleReset">
                <i class="fas fa-times"></i> Resetează
            </button>
        </div>
    }
</div>
```

**AdvancedFilterPanel.razor.cs:**
```csharp
// Components/Shared/Programari/AdvancedFilterPanel.razor.cs

using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Shared.Programari;

/// <summary>
/// Panel expandabil pentru filtre avansate.
/// </summary>
public partial class AdvancedFilterPanel : ComponentBase
{
    /// <summary>
    /// Determină dacă panelul este expandat
    /// </summary>
    [Parameter]
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Callback pentru two-way binding pe IsExpanded
    /// </summary>
    [Parameter]
    public EventCallback<bool> IsExpandedChanged { get; set; }

    /// <summary>
    /// Afișează header-ul cu buton de toggle
    /// </summary>
    [Parameter]
    public bool ShowHeader { get; set; } = true;

    /// <summary>
    /// Afișează butoanele de acțiune (Aplică/Resetează)
    /// </summary>
    [Parameter]
    public bool ShowActions { get; set; } = true;

    /// <summary>
    /// Conținutul panelului (controale de filtrare)
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Callback la click pe "Aplică"
    /// </summary>
    [Parameter]
    public EventCallback OnApply { get; set; }

    /// <summary>
    /// Callback la click pe "Resetează"
    /// </summary>
    [Parameter]
    public EventCallback OnReset { get; set; }

    private async Task ToggleExpanded()
    {
        IsExpanded = !IsExpanded;
        await IsExpandedChanged.InvokeAsync(IsExpanded);
    }

    private async Task HandleApply()
    {
        await OnApply.InvokeAsync();
    }

    private async Task HandleReset()
    {
        await OnReset.InvokeAsync();
    }
}
```

**Utilizare:**
```razor
@* Înainte (ListaProgramari.razor): *@
<div class="advanced-filter-panel @(IsAdvancedFilterExpanded ? "expanded" : "")">
    <button @onclick="() => IsAdvancedFilterExpanded = !IsAdvancedFilterExpanded">
        Filtre avansate
    </button>
    <div class="filters-grid">
        <SfDropDownList ...></SfDropDownList>
        <!-- Multiple filters -->
    </div>
    <button @onclick="ApplyFilters">Aplică</button>
    <button @onclick="ResetFilters">Resetează</button>
</div>

@* După (ListaProgramari.razor): *@
<AdvancedFilterPanel @bind-IsExpanded="IsAdvancedFilterExpanded"
                     OnApply="ApplyFilters"
                     OnReset="ResetFilters">
    <SfDropDownList ...></SfDropDownList>
    <!-- Multiple filters -->
</AdvancedFilterPanel>
```

---

#### B. Structură organizare componente noi

```
Components/
├── Shared/
│   ├── Modals/
│   │   ├── ModalHeader.razor
│   │   └── ModalHeader.razor.cs
│   ├── Programari/
│   │   ├── StatusBadge.razor
│   │   ├── StatusBadge.razor.cs
│   │   ├── DetailSection.razor
│   │   ├── DetailSection.razor.cs
│   │   ├── AdvancedFilterPanel.razor
│   │   └── AdvancedFilterPanel.razor.cs
│   ├── LoadingOverlay.razor
│   ├── LoadingOverlay.razor.cs
│   ├── EmptyState.razor
│   └── EmptyState.razor.cs
└── Pages/
    └── Programari/
        ├── CalendarProgramari.razor (REFACTORIZAT)
        ├── CalendarProgramari.razor.cs (REFACTORIZAT)
        ├── ListaProgramari.razor (REFACTORIZAT)
        ├── ListaProgramari.razor.cs (REFACTORIZAT)
        └── Modals/ (TOATE REFACTORIZATE)
```

---

### 3. Extragere Servicii pentru UI Logic

#### A. Servicii identificate pentru extragere

##### 1. ProgramariUIService - Consolidează display logic din 5 componente

**IProgramariUIService.cs:**
```csharp
// Services/Programari/IProgramariUIService.cs

namespace ValyanClinic.Services.Programari;

/// <summary>
/// Serviciu pentru logica UI a modulului Programări (display, formatting, color coding).
/// Consolidează logica duplicată din 5 componente diferite.
/// </summary>
public interface IProgramariUIService
{
    // Status Display
    string GetStatusDisplay(string status);
    string GetStatusBadgeClass(string status);
    string GetStatusIcon(string status);

    // Type Display
    string GetTipProgramareDisplay(string tip);
    string GetTipProgramareIcon(string tip);

    // Color Coding pentru calendar
    string GetProgramareColor(string status);

    // Formatting
    string FormatProgramareDateTime(DateTime? data, TimeSpan? ora);
    string GetDurationDisplay(TimeSpan? oraInceput, TimeSpan? oraSfarsit);
}
```

**ProgramariUIService.cs:**
```csharp
// Services/Programari/ProgramariUIService.cs

namespace ValyanClinic.Services.Programari;

/// <summary>
/// Implementare serviciu UI pentru modulul Programări.
/// </summary>
public class ProgramariUIService : IProgramariUIService
{
    public string GetStatusDisplay(string status) => status?.ToLower() switch
    {
        "programata" => "Programată",
        "confirmata" => "Confirmată",
        "checkedin" => "Check-in",
        "inconsultatie" => "În consultație",
        "finalizata" => "Finalizată",
        "anulata" => "Anulată",
        "noshow" => "Nu s-a prezentat",
        _ => status ?? "Necunoscut"
    };

    public string GetStatusBadgeClass(string status) => status?.ToLower() switch
    {
        "programata" => "programata",
        "confirmata" => "confirmata",
        "checkedin" => "checkedin",
        "inconsultatie" => "inconsultatie",
        "finalizata" => "finalizata",
        "anulata" => "anulata",
        "noshow" => "noshow",
        _ => "necunoscut"
    };

    public string GetStatusIcon(string status) => status?.ToLower() switch
    {
        "programata" => "calendar-plus",
        "confirmata" => "calendar-check",
        "checkedin" => "door-open",
        "inconsultatie" => "user-md",
        "finalizata" => "check-circle",
        "anulata" => "times-circle",
        "noshow" => "user-slash",
        _ => "question-circle"
    };

    public string GetTipProgramareDisplay(string tip) => tip?.ToLower() switch
    {
        "consultatie" => "Consultație",
        "control" => "Control",
        "investigatie" => "Investigație",
        "tratament" => "Tratament",
        "interventie" => "Intervenție",
        _ => tip ?? "Necunoscut"
    };

    public string GetTipProgramareIcon(string tip) => tip?.ToLower() switch
    {
        "consultatie" => "stethoscope",
        "control" => "clipboard-check",
        "investigatie" => "vial",
        "tratament" => "pills",
        "interventie" => "procedures",
        _ => "calendar"
    };

    public string GetProgramareColor(string status) => status?.ToLower() switch
    {
        "programata" => "#3b82f6",
        "confirmata" => "#22c55e",
        "checkedin" => "#fbbf24",
        "inconsultatie" => "#a855f7",
        "finalizata" => "#10b981",
        "anulata" => "#ef4444",
        "noshow" => "#9ca3af",
        _ => "#6b7280"
    };

    public string FormatProgramareDateTime(DateTime? data, TimeSpan? ora)
    {
        if (!data.HasValue) return "N/A";

        var result = data.Value.ToString("dd.MM.yyyy");

        if (ora.HasValue)
        {
            result += $" la {ora.Value:hh\\:mm}";
        }

        return result;
    }

    public string GetDurationDisplay(TimeSpan? oraInceput, TimeSpan? oraSfarsit)
    {
        if (!oraInceput.HasValue || !oraSfarsit.HasValue)
            return "N/A";

        var duration = oraSfarsit.Value - oraInceput.Value;

        if (duration.TotalHours >= 1)
        {
            return $"{duration.TotalHours:F1} ore";
        }

        return $"{duration.TotalMinutes:F0} minute";
    }
}
```

---

##### 2. DataLoadingService - Base pentru try-catch-finally pattern din 8 componente

**IDataLoadingService.cs:**
```csharp
// Services/Common/IDataLoadingService.cs

namespace ValyanClinic.Services.Common;

/// <summary>
/// Serviciu pentru pattern-ul de încărcare date cu error handling standardizat.
/// Elimină codul try-catch-finally duplicat din 8 componente.
/// </summary>
public interface IDataLoadingService
{
    /// <summary>
    /// Încarcă date cu error handling și loading state management automat.
    /// </summary>
    Task<TResult> LoadDataAsync<TResult>(
        Func<Task<TResult>> loadFunc,
        Action<bool>? setLoading = null,
        Action? onSuccess = null,
        Action<Exception>? onError = null);
}
```

**DataLoadingService.cs:**
```csharp
// Services/Common/DataLoadingService.cs

using Microsoft.Extensions.Logging;

namespace ValyanClinic.Services.Common;

/// <summary>
/// Implementare serviciu pentru încărcare date cu pattern standardizat.
/// </summary>
public class DataLoadingService : IDataLoadingService
{
    private readonly ILogger<DataLoadingService> _logger;

    public DataLoadingService(ILogger<DataLoadingService> logger)
    {
        _logger = logger;
    }

    public async Task<TResult> LoadDataAsync<TResult>(
        Func<Task<TResult>> loadFunc,
        Action<bool>? setLoading = null,
        Action? onSuccess = null,
        Action<Exception>? onError = null)
    {
        try
        {
            setLoading?.Invoke(true);

            var result = await loadFunc();

            onSuccess?.Invoke();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error loading data");
            onError?.Invoke(ex);
            throw;
        }
        finally
        {
            setLoading?.Invoke(false);
        }
    }
}
```

---

##### 3. SearchDebounceService - Debounce logic din 2 componente

**ISearchDebounceService.cs:**
```csharp
// Services/Common/ISearchDebounceService.cs

namespace ValyanClinic.Services.Common;

/// <summary>
/// Serviciu pentru debounce pe operațiuni de căutare.
/// Evită apeluri excesive la server în timpul tastării.
/// </summary>
public interface ISearchDebounceService : IDisposable
{
    /// <summary>
    /// Execută o acțiune cu debounce (delay înainte de execuție).
    /// </summary>
    void Debounce(Action action, int delayMs = 500);
}
```

**SearchDebounceService.cs:**
```csharp
// Services/Common/SearchDebounceService.cs

namespace ValyanClinic.Services.Common;

/// <summary>
/// Implementare serviciu debounce pentru căutare.
/// </summary>
public class SearchDebounceService : ISearchDebounceService
{
    private System.Threading.Timer? _debounceTimer;

    public void Debounce(Action action, int delayMs = 500)
    {
        _debounceTimer?.Dispose();
        _debounceTimer = new System.Threading.Timer(
            _ => action(),
            null,
            delayMs,
            System.Threading.Timeout.Infinite
        );
    }

    public void Dispose()
    {
        _debounceTimer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

---

##### 4. ModalStateService - Modal management din 5 modals (OPȚIONAL)

**IModalStateService.cs:**
```csharp
// Services/UI/IModalStateService.cs

namespace ValyanClinic.Services.UI;

/// <summary>
/// Serviciu pentru gestionarea state-ului modalelor.
/// OPȚIONAL - poate simplifica modal management, dar nu este critic.
/// </summary>
public interface IModalStateService<T>
{
    bool IsVisible { get; set; }
    T? CurrentData { get; set; }

    Task OpenAsync(T? data = default);
    Task CloseAsync();

    event Action? OnStateChanged;
}
```

**ModalStateService.cs:**
```csharp
// Services/UI/ModalStateService.cs

namespace ValyanClinic.Services.UI;

/// <summary>
/// Implementare serviciu state management pentru modale.
/// </summary>
public class ModalStateService<T> : IModalStateService<T>
{
    public bool IsVisible { get; set; }
    public T? CurrentData { get; set; }

    public event Action? OnStateChanged;

    public async Task OpenAsync(T? data = default)
    {
        CurrentData = data;
        IsVisible = true;
        OnStateChanged?.Invoke();
        await Task.CompletedTask;
    }

    public async Task CloseAsync()
    {
        IsVisible = false;
        CurrentData = default;
        OnStateChanged?.Invoke();
        await Task.CompletedTask;
    }
}
```

---

#### B. Înregistrare servicii în `Program.cs`

```csharp
// Program.cs - adaugă în secțiunea services

// === UI Services pentru modulul Programări ===
builder.Services.AddScoped<IProgramariUIService, ProgramariUIService>();

// === Common Services ===
builder.Services.AddScoped<IDataLoadingService, DataLoadingService>();
builder.Services.AddScoped<ISearchDebounceService, SearchDebounceService>();

// === Modal State Services (OPȚIONAL) ===
builder.Services.AddScoped<IModalStateService<Guid?>, ModalStateService<Guid?>>();
```

---

#### C. Exemple de utilizare servicii în componente

**Exemplu 1: ListaProgramari.razor.cs - DataLoadingService**

**Înainte:**
```csharp
private async Task LoadProgramariAsync()
{
    try
    {
        IsLoading = true;
        StateHasChanged();

        var query = new GetProgramareListQuery();
        var result = await Mediator.Send(query);

        if (result.IsSuccess && result.Value != null)
        {
            Programari = result.Value.ToList();
            Logger.LogInformation("✅ Loaded {Count} programari", Programari.Count);
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "❌ Error loading programari");
        await NotificationService.Error("Eroare la încărcarea programărilor");
    }
    finally
    {
        IsLoading = false;
        StateHasChanged();
    }
}
```

**După:**
```csharp
[Inject] private IDataLoadingService DataLoadingService { get; set; } = default!;

private async Task LoadProgramariAsync()
{
    await DataLoadingService.LoadDataAsync(
        loadFunc: async () =>
        {
            var query = new GetProgramareListQuery();
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                Programari = result.Value.ToList();
            }

            return result;
        },
        setLoading: loading =>
        {
            IsLoading = loading;
            StateHasChanged();
        },
        onSuccess: () => Logger.LogInformation("✅ Loaded {Count} programari", Programari.Count),
        onError: async ex => await NotificationService.Error("Eroare la încărcarea programărilor")
    );
}
```

**Exemplu 2: CalendarProgramari.razor.cs - ProgramariUIService**

**Înainte:**
```csharp
private string GetStatusColor(string status) => status?.ToLower() switch
{
    "programata" => "#3b82f6",
    "confirmata" => "#22c55e",
    // ... 7 linii duplicate
};
```

**După:**
```csharp
[Inject] private IProgramariUIService UIService { get; set; } = default!;

// În OnInitializedAsync sau în event handlers:
var color = UIService.GetProgramareColor(programare.Status);
```

**Exemplu 3: ListaProgramari.razor.cs - SearchDebounceService**

**Înainte:**
```csharp
private System.Threading.Timer? _debounceTimer;
private const int DEBOUNCE_DELAY_MS = 500;

private void HandleSearchKeyUp()
{
    _debounceTimer?.Dispose();
    _debounceTimer = new System.Threading.Timer(
        async _ => await ApplyFilters(),
        null,
        DEBOUNCE_DELAY_MS,
        System.Threading.Timeout.Infinite
    );
}

public void Dispose()
{
    _debounceTimer?.Dispose();
}
```

**După:**
```csharp
[Inject] private ISearchDebounceService DebounceService { get; set; } = default!;

private void HandleSearchKeyUp()
{
    DebounceService.Debounce(async () =>
    {
        await InvokeAsync(async () =>
        {
            await ApplyFilters();
            StateHasChanged();
        });
    }, 500);
}

// DebounceService se va dispose automat la destroy component
```

---

## 🚀 PLAN DE EXECUȚIE - ETAPE INCREMENTALE

### FAZA 1: Setup CSS Global (Estimat: 6+ fișiere CSS)
1. [ ] **Creează `wwwroot/css/programari/` folder**
2. [ ] **Creează `_variables.css`** cu toate custom properties (culori, spacing, shadows, etc.)
3. [ ] **Creează `_badges-status.css`** - consolidează status badges din 5 componente
4. [ ] **Creează `_modals.css`** - modal structure și detail sections din 3 modals
5. [ ] **Creează `_filters.css`** - advanced filter panel din 2 componente
6. [ ] **Creează `_calendar.css`** - migrează din CalendarProgramari.razor.css (394 linii)
7. [ ] **Creează `_grid.css`** - migrează din ListaProgramari.razor.css (817 linii)
8. [ ] **Creează `programari.css`** - main import cu loading/empty state common
9. [ ] **Șterge toate 6 fișiere `.razor.css`** din modulul Programări
10. [ ] **Update `wwwroot/css/app.css`** - adaugă `@import 'programari/programari.css';`
11. [ ] **Test vizual complet:**
    - [ ] CalendarProgramari (toate view-urile)
    - [ ] ListaProgramari (grid, filtre, badges)
    - [ ] Toate 5 modalele
12. [ ] **Commit:** "refactor(Programari): Migrare CSS scoped -> CSS global cu variables"

**✅ Checkpoint:** CSS 100% global, zero scoped CSS, aspect vizual identic

---

### FAZA 2: Extragere Componente UI (Estimat: 6 componente noi, 12 fișiere)
13. [ ] **Creează folder `Components/Shared/Programari/`**
14. [ ] **Creează folder `Components/Shared/Modals/`**
15. [ ] **Creează `StatusBadge` component (.razor + .razor.cs)**
16. [ ] **Refactorizează ListaProgramari** - folosește StatusBadge
17. [ ] **Test StatusBadge** în ListaProgramari
18. [ ] **Commit:** "refactor(Programari): Extract StatusBadge component"
19. [ ] **Creează `LoadingOverlay` component (.razor + .razor.cs)**
20. [ ] **Refactorizează 1 modal** (ex: ProgramareViewModal) - folosește LoadingOverlay
21. [ ] **Test LoadingOverlay**
22. [ ] **Commit:** "refactor(Programari): Extract LoadingOverlay component"
23. [ ] **Creează `ModalHeader` component (.razor + .razor.cs)**
24. [ ] **Refactorizează ProgramareViewModal** - folosește ModalHeader
25. [ ] **Test ModalHeader**
26. [ ] **Commit:** "refactor(Programari): Extract ModalHeader component"
27. [ ] **Creează `DetailSection` component (.razor + .razor.cs)**
28. [ ] **Refactorizează ProgramareViewModal** - folosește DetailSection
29. [ ] **Test DetailSection**
30. [ ] **Commit:** "refactor(Programari): Extract DetailSection component"
31. [ ] **Creează `EmptyState` component (.razor + .razor.cs)**
32. [ ] **Refactorizează ListaProgramari** - folosește EmptyState când nu există date
33. [ ] **Test EmptyState**
34. [ ] **Commit:** "refactor(Programari): Extract EmptyState component"
35. [ ] **Creează `AdvancedFilterPanel` component (.razor + .razor.cs)**
36. [ ] **Refactorizează ListaProgramari** - folosește AdvancedFilterPanel
37. [ ] **Test AdvancedFilterPanel**
38. [ ] **Commit:** "refactor(Programari): Extract AdvancedFilterPanel component"
39. [ ] **Refactorizează CalendarProgramari** - folosește StatusBadge, LoadingOverlay
40. [ ] **Refactorizează toate modalele rămase** - folosește componentele noi
41. [ ] **Test complet funcțional** - toate paginile și modalele
42. [ ] **Commit:** "refactor(Programari): Apply new components to all pages/modals"

**✅ Checkpoint:** Componente reutilizate în minim 3 locuri fiecare, markup DRY

---

### FAZA 3: Extragere Servicii UI Logic (Estimat: 4 servicii, 8 fișiere)
43. [ ] **Creează folder `Services/Programari/`**
44. [ ] **Creează folder `Services/Common/`**
45. [ ] **Creează folder `Services/UI/`** (dacă implementezi ModalStateService)
46. [ ] **Creează `IProgramariUIService.cs` + `ProgramariUIService.cs`**
47. [ ] **Înregistrează în `Program.cs`** - AddScoped<IProgramariUIService, ProgramariUIService>()
48. [ ] **Refactorizează StatusBadge.razor.cs** - folosește ProgramariUIService (dacă face sens)
49. [ ] **Refactorizează CalendarProgramari.razor.cs** - folosește ProgramariUIService
50. [ ] **Șterge metode duplicate GetStatusDisplay, GetStatusColor** din toate componentele
51. [ ] **Test funcțional** - verifică că status display funcționează corect
52. [ ] **Commit:** "refactor(Programari): Extract ProgramariUIService"
53. [ ] **Creează `IDataLoadingService.cs` + `DataLoadingService.cs`**
54. [ ] **Înregistrează în `Program.cs`** - AddScoped<IDataLoadingService, DataLoadingService>()
55. [ ] **Refactorizează ListaProgramari.razor.cs** - folosește DataLoadingService
56. [ ] **Refactorizează CalendarProgramari.razor.cs** - folosește DataLoadingService
57. [ ] **Refactorizează toate modalele** - folosește DataLoadingService
58. [ ] **Șterge try-catch-finally duplicate** din toate componentele
59. [ ] **Test funcțional** - verifică loading states și error handling
60. [ ] **Commit:** "refactor(Programari): Extract DataLoadingService"
61. [ ] **Creează `ISearchDebounceService.cs` + `SearchDebounceService.cs`**
62. [ ] **Înregistrează în `Program.cs`** - AddScoped<ISearchDebounceService, SearchDebounceService>()
63. [ ] **Refactorizează ListaProgramari.razor.cs** - folosește SearchDebounceService
64. [ ] **Șterge debounce timer manual** din ListaProgramari
65. [ ] **Test funcțional** - verifică că search debounce funcționează
66. [ ] **Commit:** "refactor(Programari): Extract SearchDebounceService"
67. [ ] **[OPȚIONAL] Creează `IModalStateService.cs` + `ModalStateService.cs`**
68. [ ] **[OPȚIONAL] Înregistrează în `Program.cs`**
69. [ ] **[OPȚIONAL] Refactorizează modale** - folosește ModalStateService
70. [ ] **[OPȚIONAL] Test funcțional**
71. [ ] **[OPȚIONAL] Commit:** "refactor(Programari): Extract ModalStateService"

**✅ Checkpoint:** Logica UI centralizată, fără duplicate, testată

---

### FAZA 4: Cleanup Final & Optimizări
72. [ ] **Review complet cod** - caută duplicate rămase (Grep, manual review)
73. [ ] **Verifică `using` statements** - elimină imports neutilizate
74. [ ] **Optimizează re-rendering** - verifică StateHasChanged() calls excesive
75. [ ] **Verifică memory leaks:**
    - [ ] Dispose() pentru servicii cu timers
    - [ ] Unsubscribe de la events
    - [ ] Event handlers cleanup
76. [ ] **Adaugă documentație XML** pentru toate serviciile și componentele noi
77. [ ] **Test complet funcțional:**
    - [ ] CalendarProgramari - view săptămână/lună, drag&drop
    - [ ] ListaProgramari - filtrare, sortare, paginare, search
    - [ ] ProgramareViewModal - vizualizare detalii
    - [ ] ProgramareAddEditModal - CRUD operations, conflict detection
    - [ ] ConfirmCancelModal - anulare programare
    - [ ] ProgramareStatisticsModal - statistici și KPIs
    - [ ] SendDailyEmailModal - trimitere email
    - [ ] Export Excel - funcționalitate export
78. [ ] **Performance testing:**
    - [ ] Măsoară timp încărcare CalendarProgramari
    - [ ] Măsoară timp încărcare ListaProgramari cu 100+ programări
    - [ ] Verifică que nu există warning-uri în browser console
79. [ ] **Cleanup final:**
    - [ ] Șterge comentarii vechi/TODO-uri rezolvate
    - [ ] Formatează cod consistent
    - [ ] Verifică că nu există fișiere .razor.css rămase
80. [ ] **Commit final:** "refactor(Programari): Final cleanup and optimizations"
81. [ ] **Creează PR** cu rezumat complet al refactorizării

**✅ Checkpoint:** Cod curat, performant, fără regresii funcționale, ready for review

---

## 📊 METRICI DE SUCCES

| Metrica | Before | Target After |
|---------|--------|--------------|
| **Fișiere CSS scoped** | 6 | 0 ✅ |
| **Linii CSS totale** | 3445 | ~2000 (reducere 40%) ✅ |
| **Pattern-uri CSS duplicate** | 15+ | 0 ✅ |
| **Fișiere CSS globale noi** | 0 | 7 (variables + 6 module) ✅ |
| **Componente refolosibile noi** | 0 | 6 ✅ |
| **Servicii UI helper noi** | 0 | 3-4 ✅ |
| **Duplicate status display logic** | 5 locuri | 0 (centralizat în ProgramariUIService) ✅ |
| **Duplicate modal management** | 5 modals | 0 (folosesc ModalHeader component) ✅ |
| **Duplicate data loading pattern** | 8 componente | 0 (folosesc DataLoadingService) ✅ |
| **Duplicate debounce logic** | 2 componente | 0 (folosesc SearchDebounceService) ✅ |
| **Lines of code saved** | N/A | ~500-800 linii eliminate ✅ |

---

## ⚠️ CONSTRAINTE ȘI REGULI

### ✅ DA
- **Pattern code-behind OBLIGATORIU** - .razor (markup) + .razor.cs (logic)
- Păstrează TOATĂ funcționalitatea existentă (zero regresii)
- Testează după fiecare componentă/serviciu nou
- Commit incremental cu mesaje clare (nu un commit uriaș)
- Reutilizează Syncfusion components (SfSchedule, SfGrid, SfDropDownList, etc.)
- Păstrează arhitectura CQRS (11 Queries, 3 Commands, 5 DTOs - NU MODIFICA)
- Respectă naming conventions existente (PascalCase pentru componente, camelCase pentru private members)
- Folosește `[Parameter, EditorRequired]` pentru parametri obligatorii
- Adaugă documentație XML (///) pentru clase, metode și parametri publici

### ❌ NU
- **NU folosi blocuri `@code {}` în fișiere .razor** - toată logica în .razor.cs
- **NU modifica Application Layer** (Queries/Commands/DTOs/Handlers) - este bine organizat
- **NU modifica Domain Layer** (Programare Entity, Enums, Repository)
- **NU adăuga funcționalități noi** - doar refactorizare DRY
- **NU schimba comportamentul** conflict detection, validare, business rules
- **NU optimizează prematur** - focus pe DRY și mentenabilitate mai întâi
- **NU crea abstractions excesive** - KISS principle (componente simple, clare)
- **NU introduce dependențe noi** (NuGet packages) fără aprobare
- **NU șterge comentarii utile** - păstrează documentația existentă

---

## 🎯 PRIORITIZARE

**MUST HAVE (Prioritate ÎNALTĂ):**
1. ✅ CSS variables pentru toate pattern-urile duplicate (15+) - **CRITIC**
2. ✅ StatusBadge component (5 apariții) - **IMPACT MARE**
3. ✅ LoadingOverlay component (4 apariții) - **IMPACT MARE**
4. ✅ ModalHeader component (3 apariții) - **IMPACT MEDIE**
5. ✅ ProgramariUIService pentru display logic (5 apariții) - **IMPACT MARE**

**SHOULD HAVE (Prioritate MEDIE):**
6. ✅ DetailSection component (3 apariții) - **IMPACT MEDIE**
7. ✅ AdvancedFilterPanel component (2 apariții) - **IMPACT MEDIE**
8. ✅ DataLoadingService pentru try-catch pattern (8 apariții) - **IMPACT MARE**
9. ✅ SearchDebounceService (2 apariții) - **IMPACT MICA**

**NICE TO HAVE (Prioritate JOASĂ - OPȚIONAL):**
10. ⚪ ModalStateService - ar simplifica, dar nu este critic
11. ⚪ EmptyState component - improve UX, nu este critic

---

## 📝 TEMPLATE PENTRU COMPONENTE NOI

### Template .razor (doar markup):
```razor
@* Components/Shared/[Folder]/[ComponentName].razor *@
@* Descriere scurtă a componentei *@

<div class="[component-class] @CssClass">
    @ChildContent
</div>
```

### Template .razor.cs (toată logica):
```csharp
// Components/Shared/[Folder]/[ComponentName].razor.cs

using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Shared.[Folder];

/// <summary>
/// [Descriere detaliată a componentei]
/// [Unde este folosită, ce pattern elimină]
/// </summary>
public partial class [ComponentName] : ComponentBase
{
    /// <summary>
    /// [Descriere parametru]
    /// </summary>
    [Parameter, EditorRequired]
    public string RequiredParam { get; set; } = "";

    /// <summary>
    /// [Descriere parametru opțional]
    /// </summary>
    [Parameter]
    public string? OptionalParam { get; set; }

    /// <summary>
    /// [Descriere callback]
    /// </summary>
    [Parameter]
    public EventCallback OnSomething { get; set; }

    /// <summary>
    /// Conținut custom al componentei
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    // Computed properties pentru binding în .razor
    private string ComputedValue => GetComputedValue();

    // Metode private pentru logică
    private string GetComputedValue()
    {
        // Logică aici
        return "";
    }

    private async Task HandleEvent()
    {
        await OnSomething.InvokeAsync();
    }

    // Lifecycle methods dacă e necesar
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        // Inițializare
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        // React la schimbări de parametri
    }
}
```

---

## 📦 DELIVERABLES FINALE

La sfârșitul refactorizării, vei avea:

1. ✅ **Zero CSS scoped** - tot în CSS global bine organizat (7 fișiere CSS modulare)
2. ✅ **6 componente noi reutilizabile** - folosite în 15+ locuri
3. ✅ **3-4 servicii UI helper** - elimină duplicate logic
4. ✅ **~40% reducere CSS** - de la 3445 la ~2000 linii
5. ✅ **~500-800 linii cod eliminate** - prin DRY
6. ✅ **Cod 100% code-behind** - zero blocuri @code în .razor
7. ✅ **Git history curat** - 20+ commits incrementale cu mesaje clare
8. ✅ **Documentație XML** - pentru toate API-urile publice
9. ✅ **Funcționalitate identică** - zero regresii, toate testele pass
10. ✅ **Performance menținut/îmbunătățit** - măsurat cu before/after

---

## 🤔 ÎNTREBĂRI ÎNAINTE DE START

1. **Prioritate utilizare**: Care pagină/modal este cel mai des folosit? (CalendarProgramari sau ListaProgramari?)
2. **Timeline**: Preferi refactoring complet dintr-o dată sau incremental FAZĂ cu FAZĂ? (recomandat incremental)
3. **Testing**: Există teste automatizate (unit/integration) sau doar manual testing?
4. **Backwards compatibility**: Trebuie să menținem CSS classes vechi pentru alte module care ar putea referenția?
5. **Syncfusion customization**: Există override-uri Syncfusion specifice critical care trebuie păstrate exact?
6. **Modal state service**: Vrei implementarea ModalStateService (opțional) sau este overkill?
7. **Breaking changes**: Este OK să schimbăm naming-ul unor CSS classes sau trebuie 100% backwards compatible?

---

## 🎁 COMENZI UTILE PENTRU MONITORING PROGRES

```bash
# === MONITORING CSS MIGRATION ===

# Verifică câte fișiere .razor.css mai există în modulul Programări
find Components/Pages/Programari -name "*.razor.css" | wc -l
# Target: 0

# Verifică că există toate fișierele CSS globale noi
ls -la wwwroot/css/programari/
# Expected: _variables.css, _badges-status.css, _modals.css, _filters.css, _calendar.css, _grid.css, programari.css

# Găsește usage de CSS variables (verifică migrarea)
grep -r "var(--" wwwroot/css/programari --include="*.css" | wc -l
# Expected: 100+ apariții

# Găsește pattern-uri CSS hardcoded care ar trebui înlocuite
grep -r "linear-gradient" Components/Pages/Programari --include="*.css"
# Expected: 0 rezultate după migrare

# === MONITORING COMPONENTE ===

# Verifică componente noi create
ls -la Components/Shared/Programari/
ls -la Components/Shared/Modals/
ls -la Components/Shared/ | grep -E "(Loading|Empty)"

# Găsește usage StatusBadge în codebase
grep -r "<StatusBadge" Components/Pages/Programari --include="*.razor" | wc -l
# Expected: 5+ locuri

# Găsește metode duplicate GetStatusDisplay care ar trebui eliminate
grep -r "GetStatusDisplay" Components/Pages/Programari --include="*.cs"
# Expected: 0 după refactorizare (toate folosesc ProgramariUIService)

# === MONITORING SERVICII ===

# Verifică servicii noi create
ls -la Services/Programari/
ls -la Services/Common/

# Găsește injection ProgramariUIService
grep -r "IProgramariUIService" Components/Pages/Programari --include="*.cs" | wc -l
# Expected: 5+ locuri

# Găsește pattern-uri try-catch-finally duplicate
grep -A5 "IsLoading = true" Components/Pages/Programari --include="*.cs"
# Expected: Puține rezultate după migrare la DataLoadingService

# === METRICS FINALE ===

# Număr total linii CSS în modulul Programări
find wwwroot/css/programari -name "*.css" -exec wc -l {} + | tail -1
# Expected: ~2000 linii (față de 3445 înainte)

# Număr total fișiere în modulul refactorizat
find Components/Pages/Programari -type f | wc -l
find Components/Shared/Programari -type f | wc -l
find Components/Shared/Modals -type f | wc -l

# Verifică că nu există blocuri @code în fișierele .razor
grep -r "@code" Components/Pages/Programari --include="*.razor"
grep -r "@code" Components/Shared/Programari --include="*.razor"
# Expected: 0 rezultate

# === GIT COMMITS TRACKING ===

# Verifică istoricul commits pentru refactorizare
git log --oneline --grep="refactor(Programari)" | wc -l
# Expected: 15-20+ commits incrementale

# Verifică diferențele înainte/după (run pe branch-ul tău)
git diff main --stat | grep -E "(Programari|Shared)"
```

---

**Notă finală**:

Abordare **incrementală** este ESENȚIALĂ. Fiecare FAZĂ trebuie:
1. Testată complet funcțional
2. Verificată vizual (aspect identic sau îmbunătățit)
3. Committată cu mesaj clar
4. Documentată (adaugă XML comments)

Nu trece la FAZA următoare până când FAZA curentă este 100% completă și testată.

**Pattern code-behind este NON-NEGOCIABIL** - toată logica în `.razor.cs`, markup curat în `.razor`.

Succes! 🚀
