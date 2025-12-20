# Consultații Page - Analiză Concisă

**Data:** 2025-01-06  
**Task:** Convertire template HTML ConsultatiiPageV2.html → Blazor full page  
**Status:** 🎯 READY TO IMPLEMENT

---

## 📋 SCOPE

### Ce se implementează
- **Pagină nouă** `/consultatii/{programareId}/{pacientId}` (NU modal)
- **4 tab-uri:** Motiv+Antecedente | Examen+Investigații | Diagnostic+Tratament | Concluzii
- **Features:** Timer, IMC calculator, allergy alerts, auto-save, responsive

### Ce se refolosește (EXISTĂ DEJA)
✅ `CreateConsultatieCommand` - salvare consultație  
✅ `GetPacientByIdQuery` - load pacient  
✅ `IIMCCalculatorService` - calcul IMC  
✅ `DraftAutoSaveHelper` - auto-save drafts  
✅ CSS `variables.css` - blue theme

**→ ZERO servicii noi needed!**

---

## 🎨 DESIGN CHANGES (CRITICAL)

**Template folosește TEAL → Schimbăm la BLUE:**
```css
--primary: #0A6E7C → #60a5fa
--accent: #00C9A7 → #3b82f6
```

**Sidebar:** REMOVED (folosim NavMenu existent)

---

## 📁 FILES TO CREATE

```
ValyanClinic/Components/Pages/Consultatii/
├── Consultatii.razor          # Markup (~600 linii)
├── Consultatii.razor.cs       # Logic (~300 linii)
└── Consultatii.razor.css      # Styles (~800 linii)
```

**Total:** ~1,700 linii cod

---

## 🔄 IMPLEMENTATION STEPS

**1. Create files** → Empty shells  
**2. Extract HTML** → Patient card + 4 tabs  
**3. Add code-behind** → Timer, tabs, IMC, form handling  
**4. Add scoped CSS** → Blue theme, responsive  
**5. Add navigation** → Link în NavMenu.razor  
**6. Test** → Load, tabs, save, responsive

**Estimated Time:** 3-4 ore (nu 6-10!)

---

## ⚠️ CRITICAL RULES

✅ **Blue theme ONLY** - No teal  
✅ **Scoped CSS** - `.razor.css` only  
✅ **Logic in code-behind** - NO logic in `.razor`  
✅ **MediatR** - All commands/queries  
✅ **[Authorize]** - Doctor/Medic only

---

## 🚀 START NOW

**Next:** Create `Consultatii.razor` cu structură de bază

**Status:** ✅ ANALYSIS COMPLETE - IMPLEMENTING NOW  
**Lines:** 96 (concis!)
