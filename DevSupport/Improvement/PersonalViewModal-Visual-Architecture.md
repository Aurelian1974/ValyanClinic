# PersonalViewModal - Arhitectură Vizuală

## 📐 Structura Fișierelor

```
ValyanClinic/
├── Components/
│   └── Pages/
│       └── Administrare/
│           └── Personal/
│               ├── AdministrarePersonal.razor ────────────┐
│               ├── AdministrarePersonal.razor.cs          │ Parent Component
│               ├── AdministrarePersonal.razor.css         │
│               └── Modals/                                 │
│                   ├── PersonalViewModal.razor ───────────┤ Child Component
│                   ├── PersonalViewModal.razor.cs         │ (Integration)
│                   └── PersonalViewModal.razor.css        │
│                                                           │
└── Application/                                            │
    └── Features/                                           │
        └── PersonalManagement/                             │
            └── Queries/                                    │
                └── GetPersonalById/                        │
                    ├── GetPersonalByIdQuery.cs ────────────┤ Backend Query
                    ├── GetPersonalByIdQueryHandler.cs      │ (via MediatR)
                    └── PersonalDetailDto.cs ───────────────┘
```

---

## 🔄 Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    AdministrarePersonal Page                     │
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                    Syncfusion Grid                         │  │
│  │  ┌─────┬─────────┬────────┬─────┬──────┬────────────┐    │  │
│  │  │ Cod │  Nume   │Prenume │ CNP │ Tel  │   Status   │    │  │
│  │  ├─────┼─────────┼────────┼─────┼──────┼────────────┤    │  │
│  │  │ A001│ Popescu │ Ion    │ ... │ ...  │ [✓ Activ]  │◄───┼──┼─── Row Selection
│  │  └─────┴─────────┴────────┴─────┴──────┴────────────┘    │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                   Action Toolbar                           │  │
│  │  [ 👁 Vizualizează ]  [ ✏ Editează ]  [ 🗑 Șterge ]      │  │
│  └───────────────────────────────────────────────────────────┘  │
│                            │                                      │
│                            │ Click "Vizualizează"                │
│                            ▼                                      │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  personalViewModal.Open(selectedId)                        │  │
│  └───────────────────────────────────────────────────────────┘  │
└────────────────────────────────┬────────────────────────────────┘
                                 │
                                 │ Event Trigger
                                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                     PersonalViewModal                            │
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │ 1. IsVisible = true                                        │  │
│  │ 2. IsLoading = true                                        │  │
│  │ 3. StateHasChanged()  ────────────► Show Loading Spinner   │  │
│  └───────────────────────────────────────────────────────────┘  │
│                            │                                      │
│                            │ LoadPersonalData(id)                │
│                            ▼                                      │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  var query = new GetPersonalByIdQuery(id);                 │  │
│  │  var result = await Mediator.Send(query);                  │  │
│  └───────────────────────────────────────────────────────────┘  │
└────────────────────────────────┬────────────────────────────────┘
                                 │
                                 │ MediatR Send
                                 ▼
┌─────────────────────────────────────────────────────────────────┐
│              GetPersonalByIdQueryHandler                         │
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  var personal = await _personalRepository                  │  │
│  │                       .GetByIdAsync(id, ct);               │  │
│  └───────────────────────────────────────────────────────────┘  │
│                            │                                      │
│                            │ Map to DTO                           │
│                            ▼                                      │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  return Result<PersonalDetailDto>.Success(dto);            │  │
│  └───────────────────────────────────────────────────────────┘  │
└────────────────────────────────┬────────────────────────────────┘
                                 │
                                 │ Return Result
                                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                     PersonalViewModal                            │
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  if (result.IsSuccess)                                     │  │
│  │  {                                                          │  │
│  │     PersonalData = result.Value;                           │  │
│  │     IsLoading = false;                                     │  │
│  │     StateHasChanged(); ──────► Render Modal Content        │  │
│  │  }                                                          │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  ╔═══════════════════════════════════════════════════╗  │    │
│  │  ║  👤 Detalii Personal                          [X] ║  │    │
│  │  ╠═══════════════════════════════════════════════════╣  │    │
│  │  ║ [Personal] [Contact] [Adresă] [Poziție] [Doc]    ║  │    │
│  │  ╠═══════════════════════════════════════════════════╣  │    │
│  │  ║                                                   ║  │    │
│  │  ║  📋 Informații Generale                          ║  │    │
│  │  ║  ┌─────────────────────────────────────────────┐ ║  │    │
│  │  ║  │ Cod Angajat:  [A001]                        │ ║  │    │
│  │  ║  │ Status:       [✓ Activ]                     │ ║  │    │
│  │  ║  │ Nume Complet: Popescu Ion                   │ ║  │    │
│  │  ║  │ CNP:          1234567890123                 │ ║  │    │
│  │  ║  │ Data Nașterii: 01.01.1980                   │ ║  │    │
│  │  ║  │ Vârstă:       [44 ani]                      │ ║  │    │
│  │  ║  └─────────────────────────────────────────────┘ ║  │    │
│  │  ║                                                   ║  │    │
│  │  ╠═══════════════════════════════════════════════════╣  │    │
│  │  ║      [Închide]  [Editează]  [Șterge]             ║  │    │
│  │  ╚═══════════════════════════════════════════════════╝  │    │
│  └─────────────────────────────────────────────────────────┘    │
│                            │                                      │
│                            │ User Actions                         │
│                            ▼                                      │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  [Editează] ───► OnEditRequested.Invoke(personalId)       │  │
│  │  [Șterge]   ───► OnDeleteRequested.Invoke(personalId)     │  │
│  │  [Închide]  ───► Close()                                  │  │
│  └───────────────────────────────────────────────────────────┘  │
└────────────────────────────────┬────────────────────────────────┘
                                 │
                                 │ Event Callbacks
                                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                    AdministrarePersonal Page                     │
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  HandleEditFromModal(personalId)                           │  │
│  │  ───► NavigationManager.NavigateTo("/editeaza/{id}")      │  │
│  │                                                             │  │
│  │  HandleDeleteFromModal(personalId)                         │  │
│  │  ───► Show Toast Warning (placeholder)                    │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🎨 UI Component Layout

```
┌──────────────────────────────────────────────────────────────┐
│                      MODAL OVERLAY                            │
│                   (backdrop blur effect)                      │
│                                                               │
│    ┌──────────────────────────────────────────────────┐     │
│    │  ╔════════════════════════════════════════════╗  │     │
│    │  ║  MODAL HEADER (gradient blue-purple)     ║  │     │
│    │  ║  👤 Detalii Personal              [X]     ║  │     │
│    │  ╚════════════════════════════════════════════╝  │     │
│    │  ┌──────────────────────────────────────────┐   │     │
│    │  │         MODAL BODY                        │   │     │
│    │  │  ┌────────────────────────────────────┐  │   │     │
│    │  │  │  TAB BUTTONS                       │  │   │     │
│    │  │  │  [Personal] [Contact] [Adresă]...  │  │   │     │
│    │  │  └────────────────────────────────────┘  │   │     │
│    │  │                                           │   │     │
│    │  │  ┌────────────────────────────────────┐  │   │     │
│    │  │  │  TAB CONTENT                       │  │   │     │
│    │  │  │                                     │  │   │     │
│    │  │  │  ┌──────────────────────────────┐ │  │   │     │
│    │  │  │  │  INFO CARD                   │ │  │   │     │
│    │  │  │  │  📋 Informații Generale      │ │  │   │     │
│    │  │  │  │  ┌────────────┬────────────┐ │ │  │   │     │
│    │  │  │  │  │ Label      │ Value      │ │ │  │   │     │
│    │  │  │  │  ├────────────┼────────────┤ │ │  │   │     │
│    │  │  │  │  │ Cod:       │ [A001]     │ │ │  │   │     │
│    │  │  │  │  │ Status:    │ [✓ Activ]  │ │ │  │   │     │
│    │  │  │  │  └────────────┴────────────┘ │ │  │   │     │
│    │  │  │  └──────────────────────────────┘ │  │   │     │
│    │  │  │                                     │  │   │     │
│    │  │  │  ┌──────────────────────────────┐ │  │   │     │
│    │  │  │  │  INFO CARD                   │ │  │   │     │
│    │  │  │  │  📞 Contact                  │ │  │   │     │
│    │  │  │  │  ...                         │ │  │   │     │
│    │  │  │  └──────────────────────────────┘ │  │   │     │
│    │  │  │                                     │  │   │     │
│    │  │  └────────────────────────────────────┘  │   │     │
│    │  │  (scrollable content)                    │   │     │
│    │  └──────────────────────────────────────────┘   │     │
│    │  ╔════════════════════════════════════════════╗ │     │
│    │  ║  MODAL FOOTER                             ║ │     │
│    │  ║  [Închide] [Editează] [Șterge]            ║ │     │
│    │  ╚════════════════════════════════════════════╝ │     │
│    └──────────────────────────────────────────────────┘     │
│                                                               │
└──────────────────────────────────────────────────────────────┘
```

---

## 📊 State Diagram

```
┌─────────────┐
│   CLOSED    │ ◄──────────────────────────────┐
└──────┬──────┘                                 │
       │                                        │
       │ Open(id) called                       │
       ▼                                        │
┌─────────────┐                                │
│  OPENING    │                                │
│ IsVisible=T │                                │
│ IsLoading=T │                                │
└──────┬──────┘                                │
       │                                        │
       │ LoadPersonalData()                    │
       ▼                                        │
┌─────────────┐                                │
│  LOADING    │──┐                             │
│ Show Spinner│  │ Error occurs                │
└──────┬──────┘  │                             │
       │         │                              │
       │         ▼                              │
       │    ┌─────────────┐                    │
       │    │    ERROR    │                    │
       │    │ Show Alert  │                    │
       │    └──────┬──────┘                    │
       │           │ Retry or Close            │
       │           │                            │
       │ Success   ▼                            │
       ▼    ┌─────────────┐                    │
┌─────────────┐   LOADED   │                   │
│   LOADED    │◄───────────┘                   │
│ Show Content│                                │
└──────┬──────┘                                │
       │                                        │
       │ Tab switching                          │
       ├──► [Personal]  ──┐                    │
       ├──► [Contact]    ├─► Stay in LOADED    │
       ├──► [Adresă]     │                     │
       ├──► [Poziție]    │                     │
       ├──► [Documente] ──┘                    │
       └──► [Audit]                            │
       │                                        │
       │ User Actions                           │
       ├──► [Editează] ──┐                     │
       ├──► [Șterge]    ├─► Trigger Event      │
       └──► [Închide]  ──┘    & Close()        │
                              │                 │
                              └─────────────────┘
```

---

## 🔐 Security Flow

```
┌──────────────────────────────────────────────────────────┐
│                    Client Browser                         │
│  ┌────────────────────────────────────────────────────┐  │
│  │  PersonalViewModal                                  │  │
│  │  - Can only READ data                               │  │
│  │  - All fields disabled                              │  │
│  │  - No direct data mutation                          │  │
│  └────────────────────────────────────────────────────┘  │
└────────────────────────┬─────────────────────────────────┘
                         │
                         │ HTTPS Request
                         │ GetPersonalByIdQuery
                         ▼
┌──────────────────────────────────────────────────────────┐
│                    Application Layer                      │
│  ┌────────────────────────────────────────────────────┐  │
│  │  GetPersonalByIdQueryHandler                        │  │
│  │  - Validates request                                │  │
│  │  - Checks authorization (future)                    │  │
│  │  - Logs access attempt                              │  │
│  └────────────────────────────────────────────────────┘  │
└────────────────────────┬─────────────────────────────────┘
                         │
                         │ Repository call
                         ▼
┌──────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                     │
│  ┌────────────────────────────────────────────────────┐  │
│  │  PersonalRepository                                 │  │
│  │  - Executes stored procedure                        │  │
│  │  - sp_Personal_GetById                              │  │
│  │  - Returns entity                                   │  │
│  └────────────────────────────────────────────────────┘  │
└────────────────────────┬─────────────────────────────────┘
                         │
                         │ Database query
                         ▼
┌──────────────────────────────────────────────────────────┐
│                       Database                            │
│  ┌────────────────────────────────────────────────────┐  │
│  │  SELECT * FROM Personal WHERE Id_Personal = @Id    │  │
│  │  - Row-level security (future)                      │  │
│  │  - Audit log (existing)                             │  │
│  └────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────┘

                    ┌────────────────────┐
                    │   Security Layers  │
                    ├────────────────────┤
                    │ 1. HTTPS           │
                    │ 2. Authentication  │
                    │ 3. Authorization   │
                    │ 4. Validation      │
                    │ 5. Audit Logging   │
                    │ 6. Read-Only UI    │
                    └────────────────────┘
```

---

## 🎯 Tab Navigation Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    Tab Button Click                          │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
                   SetActiveTab(tabName)
                          │
                          ├──► ActiveTab = tabName
                          ├──► Logger.LogDebug(...)
                          └──► StateHasChanged()
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                  Conditional Rendering                        │
│                                                               │
│  @if (ActiveTab == "personal") {                             │
│      <div class="tab-pane active">                           │
│          <!-- Date Personale Content -->                     │
│      </div>                                                   │
│  }                                                            │
│  else if (ActiveTab == "contact") {                          │
│      <div class="tab-pane active">                           │
│          <!-- Contact Content -->                            │
│      </div>                                                   │
│  }                                                            │
│  // ... other tabs                                           │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
                  CSS Animation fadeIn
                  (300ms duration)
```

---

## 🎨 CSS Architecture

```
┌──────────────────────────────────────────────────────────────┐
│              PersonalViewModal.razor.css                      │
│                  (Scoped Styles)                              │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Layout Components                                      │ │
│  │  ├─ .modal-overlay (position: fixed, z-index: 9999)    │ │
│  │  ├─ .modal-container (max-width: 900px, flex column)   │ │
│  │  ├─ .modal-header (gradient background)                │ │
│  │  ├─ .modal-body (flex: 1, overflow-y: auto)            │ │
│  │  └─ .modal-footer (flex-shrink: 0)                     │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                               │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Interactive Components                                 │ │
│  │  ├─ .tab-buttons (flex, gap: 0.5rem)                   │ │
│  │  ├─ .tab-button (transitions, hover effects)           │ │
│  │  ├─ .btn (gradient backgrounds)                        │ │
│  │  └─ .contact-link (color transitions)                  │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                               │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Data Display Components                                │ │
│  │  ├─ .info-card (background, border-radius, padding)    │ │
│  │  ├─ .info-grid (CSS Grid, auto-fit)                    │ │
│  │  ├─ .info-item (flex column, gap)                      │ │
│  │  └─ .info-value (padding, border, background)          │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                               │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Status & Badge Components                              │ │
│  │  ├─ .status-badge (inline-flex, border-radius: 20px)   │ │
│  │  ├─ .badge-primary (gradient blue-purple)              │ │
│  │  ├─ .badge-secondary (gradient gray)                   │ │
│  │  ├─ .badge-warning (gradient orange)                   │ │
│  │  └─ .badge-danger (gradient red)                       │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                               │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Animations                                             │ │
│  │  ├─ @keyframes fadeIn (opacity + translateY)           │ │
│  │  ├─ @keyframes spinner-border (rotate 360deg)          │ │
│  │  └─ transition: all 0.3s ease (buttons, links)         │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                               │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Responsive Design                                      │ │
│  │  @media (max-width: 768px) {                           │ │
│  │    - modal-container: width 95%, max-height 95vh       │ │
│  │    - tab-buttons: overflow-x auto, no wrap             │ │
│  │    - info-grid: grid-template-columns 1fr              │ │
│  │    - modal-footer: flex-direction column, full-width   │ │
│  │  }                                                      │ │
│  └────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────┘
```

---

## 📱 Responsive Breakpoints

```
┌──────────────────────────────────────────────────────────────┐
│                   Desktop (>768px)                            │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  Modal: 900px width, 90vh height                       │  │
│  │  Grid: 2 columns (auto-fit, minmax(250px, 1fr))       │  │
│  │  Tabs: horizontal row, all visible                     │  │
│  │  Footer: horizontal buttons                            │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                   Mobile (<768px)                             │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  Modal: 95% width, 95vh height                         │  │
│  │  Grid: 1 column (all fields full width)               │  │
│  │  Tabs: horizontal scroll, compact                      │  │
│  │  Footer: vertical stack, full-width buttons            │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

---

*Documentație vizuală generată: 2025-01-08*  
*Component: PersonalViewModal*  
*Format: ASCII Diagrams*
