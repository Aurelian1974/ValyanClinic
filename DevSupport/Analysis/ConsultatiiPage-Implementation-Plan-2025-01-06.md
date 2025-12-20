# Consultații Page - Implementation Plan

**Date:** 2025-01-06  
**Task:** Convert ConsultatiiPageV2.html template to Blazor full page  
**Status:** 🚧 IN PROGRESS

---

## 🎯 IMPLEMENTATION STEPS

### ✅ Step 1: Create File Structure (DONE - 2025-12-06)
Evidence:
- Created `ValyanClinic/Components/Pages/Consultatii/Consultatii.razor`
- Created `ValyanClinic/Components/Pages/Consultatii/Consultatii.razor.cs`
- Created `ValyanClinic/Components/Pages/Consultatii/Consultatii.razor.css`

### 🔄 Step 2: Extract & Adapt HTML Structure (IN PROGRESS - 2025-12-06)
Evidence:
- Page header, patient card, and 4-tab structure placeholders (blue theme)
- Exam grid in Tab 2 (Examen Clinic General)
- Diagnosis cards (principal/secundar) in Tab 3
- Investigații breakdown sections (Laborator/Imagistică/EKG/Alte)
- Scoped styles updated in `Consultatii.razor.css`

### 🔄 Step 3: Convert JavaScript to C# Code-Behind (IN PROGRESS - 2025-12-06)
Evidence:
- `Consultatii.razor.cs`: Draft autosave integrated via `DraftAutoSaveHelper` + `IDraftStorageService`
- IMC calculation support via `IIMCCalculatorService` and `_imcBadgeText`
- Draft load restores `Model` and vitals (`Greutate`, `Inaltime`)
- `MarkChanged()` helper added for change tracking

### ⬜ Step 4: Implement MediatR Commands/Queries
Pending integration in page (submit/save actions).

### ✅ Step 5: Navigation Integration (PARTIAL - 2025-12-06)
- `DashboardMedic.razor.cs` navigates to `/consultatii/{programareId}/{pacientId}`
- Removed modal include from `DashboardMedic.razor`

### ✅ Step 6: Route Configuration (DONE - 2025-12-06)
- Route: `@page "/consultatii/{ProgramareIdParam}/{PacientIdParam}"`
- Authorization: `[Authorize(Roles = "Doctor,Medic")]`

### ⬜ Step 7: Testing Checklist
Pending after bindings and actions are implemented.

---

## 📊 COMPLEXITY BREAKDOWN

| Task | Lines of Code | Est. Time |
|------|---------------|-----------|
| HTML extraction → Razor | ~800 lines | 2-3 hours |
| JavaScript → C# logic | ~400 lines | 2-3 hours |
| CSS adaptation (teal→blue) | ~1200 lines | 1-2 hours |
| Testing & fixes | - | 1-2 hours |
| **TOTAL** | ~2400 lines | **6-10 hours** |

---

## ⚠️ CRITICAL REQUIREMENTS

### Design System (NON-NEGOTIABLE)
✅ **Blue theme ONLY** - No teal/green/purple  
✅ **Scoped CSS** - No global pollution  
✅ **CSS variables** - Use `variables.css`  
✅ **Responsive** - Mobile/tablet/desktop breakpoints

### Code Quality (MANDATORY)
✅ **Clean separation** - Logic in `.razor.cs` only  
✅ **MediatR pattern** - All commands/queries  
✅ **Input validation** - FluentValidation  
✅ **Authorization** - `[Authorize]` attribute

### Performance
✅ **StateHasChanged()** - Call ONLY when needed  
✅ **Async/await** - All I/O operations  
✅ **Dispose** - Cleanup timers/subscriptions

---

## 🔄 IMPLEMENTATION ORDER (STRICT)

1. **Create files** (Step 1) - Empty shells
2. **Extract HTML** (Step 2) - Patient card + tabs
3. **Add routing** (Step 6) - Test page loads
4. **Convert JS** (Step 3) - Timer, tabs, IMC
5. **Integrate MediatR** (Step 4) - Load/save data
6. **Navigation link** (Step 5) - Add to NavMenu
7. **Testing** (Step 7) - Verify all features
8. **Documentation** - Update analysis file

---

## 📁 FILE ORGANIZATION

```
ValyanClinic/
├── Components/
│   ├── Pages/
│   │   └── Consultatii/
│   │       ├── Consultatii.razor          # Markup (~800 lines)
│   │       ├── Consultatii.razor.cs       # Logic (~400 lines)
│   │       └── Consultatii.razor.css      # Styles (~1200 lines)
│   └── Layout/
│       └── NavMenu.razor                  # Add link (5 lines)
└── DevSupport/
    └── Analysis/
        └── ConsultatiiPage-Implementation-Plan-2025-01-06.md  # This file
```

---

## ✅ DEPENDENCIES VERIFIED

**Existing Infrastructure (CAN REUSE):**
- ✅ `CreateConsultatieCommand` - MediatR command
- ✅ `GetPacientByIdQuery` - Load patient
- ✅ `IIMCCalculatorService` - Calculate IMC
- ✅ `DraftAutoSaveHelper` - Auto-save drafts
- ✅ `IDraftStorageService` - LocalStorage
- ✅ CSS `variables.css` - Blue theme vars

**No New Services Needed!** 🎉

---

## 🚀 NEXT ACTIONS

1. Bind inputs in `Consultatii.razor` to `Model` fields and call `MarkChanged()`.
2. Display IMC dynamically in UI using `_imcBadgeText` and add indicator class (normal/over/under).
3. Implement submit (final save) using `Mediator.Send(Model)` and clear draft on success.
4. Add footer buttons wiring (Back/Save Draft/Continue) to code-behind.
5. Test end-to-end from DashboardMedic.
