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
- Page header, patient card, and 4-tab structure placeholders (blue theme) added
- Added exam grid in Tab 2 (Examen Clinic General) per template
- Added diagnosis cards (principal/secundar) in Tab 3 per template
- Scoped styles updated in `Consultatii.razor.css`

### ⬜ Step 3: Convert JavaScript to C# Code-Behind
Pending.

### ⬜ Step 4: Implement MediatR Commands/Queries
Pending integration in page.

### ✅ Step 5: Navigation Integration (PARTIAL - 2025-12-06)
- Updated `DashboardMedic.razor.cs` `StartConsultatie()` to navigate to route `/consultatii/{programareId}/{pacientId}` instead of opening modal.
- Removed modal include from `DashboardMedic.razor`.

### ✅ Step 6: Route Configuration (DONE - 2025-12-06)
- Added route: `@page "/consultatii/{ProgramareIdParam}/{PacientIdParam}"`
- Added authorization: `[Authorize(Roles = "Doctor,Medic")]`

### ⬜ Step 7: Testing Checklist
Pending after markup and logic implementation.

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

(Unchanged)

---

## 🚀 NEXT ACTIONS

1. Finish porting remaining sections (investigații breakdown, more exam items) with bindings.
2. Start Step 3: bind inputs to `CreateConsultatieCommand`, implement tab logic, IMC calculation, autosave in `.razor.cs`.
3. Wire save draft and finalize using existing Draft and CreateConsultatieCommand.
4. Test end-to-end from DashboardMedic.
