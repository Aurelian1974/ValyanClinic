# 🚀 Quick Implementation Guide - Font Unification
**DOAR Fonturi, NU Culori!**

---

## 🎯 Regex Find & Replace Patterns

### Pattern 1: font-size Values
```regex
Find: font-size:\s*(0\.\d+rem|1\.\d+rem|\d+px)
Replace: Lookup in table below
```

### Pattern 2: font-weight Values
```regex
Find: font-weight:\s*(400|500|600|700)
Replace with variables:
400 → var(--font-weight-normal)
500 → var(--font-weight-medium)
600 → var(--font-weight-semibold)
700 → var(--font-weight-bold)
```

---

## 📊 Font-Size Conversion Table (Quick Reference)

| Hardcoded Value | CSS Variable | Context |
|-----------------|--------------|---------|
| `0.6875rem`, `0.75rem`, `11px`, `12px` | `var(--font-size-xs)` | Badge small |
| `0.8rem`, `0.8125rem`, `0.85rem`, `13px` | `var(--modal-label)` | Labels (uppercase) |
| `0.875rem`, `0.9rem`, `0.95rem`, `14px` | `var(--modal-tab-text)` or `var(--font-size-base)` | **STANDARD** tabs, buttons, body |
| `0.9375rem`, `1rem`, `15px`, `16px` | `var(--modal-value)` | Values, emphasized text |
| `1.025rem`, `1.0625rem`, `1.1rem`, `1.125rem`, `16-18px` | `var(--modal-card-title)` | Card titles |
| `1.125rem`, `1.25rem`, `18px`, `20px` | `var(--modal-card-title-icon)` or `var(--font-size-xl)` | Icons in titles |
| `1.375rem`, `1.5rem`, `22px` | `var(--modal-header-title)` | Modal headers |
| `1.75rem`, `2rem`, `28px`, `32px` | `var(--page-header-title)` | Page headers |

---

## ✅ Step-by-Step per File

### Pentru fiecare fișier `.razor.css`:

#### Step 1: Replace font-size (Tab Text)
```css
/* BEFORE */
.tab-button {
    font-size: 0.875rem; /* or 0.95rem, 14px */
}

/* AFTER */
.tab-button {
    font-size: var(--modal-tab-text);
}
```

#### Step 2: Replace font-size (Tab Icon)
```css
/* BEFORE */
.tab-button i {
    font-size: 0.875rem; /* or 1rem */
}

/* AFTER */
.tab-button i {
    font-size: var(--modal-tab-icon);
}
```

#### Step 3: Replace font-weight (Tabs)
```css
/* BEFORE */
.tab-button {
    font-weight: 500;
}

.tab-button.active {
    font-weight: 600;
}

/* AFTER */
.tab-button {
font-weight: var(--font-weight-medium);
}

.tab-button.active {
    font-weight: var(--font-weight-semibold);
}
```

#### Step 4: Replace font-size (Card Title)
```css
/* BEFORE */
.card-title {
    font-size: 1.025rem; /* or 1.1rem, 16.4px */
    font-weight: 600;
}

/* AFTER */
.card-title {
    font-size: var(--modal-card-title);
    font-weight: var(--font-weight-semibold);
}
```

#### Step 5: Replace font-size (Card Title Icon)
```css
/* BEFORE */
.card-title i {
    font-size: 1.125rem; /* or 1.25rem, 18px */
}

/* AFTER */
.card-title i {
    font-size: var(--modal-card-title-icon);
}
```

#### Step 6: Replace font-size (Labels)
```css
/* BEFORE */
.info-item label {
    font-size: 0.8rem; /* or 0.85rem, 0.75rem, 13px */
    font-weight: 600;
}

/* AFTER */
.info-item label {
 font-size: var(--modal-label);
    font-weight: var(--font-weight-semibold);
}
```

#### Step 7: Replace font-size (Values)
```css
/* BEFORE */
.info-value {
    font-size: 0.9375rem; /* or 1rem, 15px, 16px */
}

/* AFTER */
.info-value {
    font-size: var(--modal-value);
}
```

#### Step 8: Replace font-size (Badges)
```css
/* BEFORE */
.badge {
    font-size: 0.875rem; /* or 0.95rem, 14px */
    font-weight: 600;
}

/* AFTER */
.badge {
    font-size: var(--modal-badge);
    font-weight: var(--font-weight-semibold);
}
```

#### Step 9: Replace font-size (Modal Header - if present)
```css
/* BEFORE */
.modal-header h2 {
    font-size: 1.5rem; /* or 1.375rem, 22px */
    font-weight: 600;
}

/* AFTER */
.modal-header h2 {
    font-size: var(--modal-header-title);
    font-weight: var(--font-weight-semibold);
}
```

#### Step 10: Replace font-size (Page Header - if present)
```css
/* BEFORE */
h1, .page-header h1 {
    font-size: 2rem; /* or 1.75rem, 28px, 32px */
    font-weight: 700;
}

/* AFTER */
h1, .page-header h1 {
    font-size: var(--page-header-title);
    font-weight: var(--font-weight-bold);
}
```

#### Step 11: Replace font-size (Loading/Error text)
```css
/* BEFORE */
.loading-container p {
    font-size: 0.9rem; /* or 15px */
}

/* AFTER */
.loading-container p {
    font-size: var(--modal-value);
}
```

#### Step 12: Replace font-size (Contact Links)
```css
/* BEFORE */
.contact-link i {
    font-size: 0.8125rem; /* or 13px */
}

/* AFTER */
.contact-link i {
    font-size: var(--modal-label);
}
```

#### Step 13: Replace padding with variables (Optional)
```css
/* IF you want to unify padding too */
.tab-button {
    padding: var(--tab-padding); /* 0.625rem 1rem */
}

.badge {
    padding: var(--badge-padding); /* 0.375rem 0.875rem */
}
```

---

## ❌ DON'T TOUCH (Leave Colors Unchanged!)

```css
/* ❌ DON'T CHANGE THESE */
background: rgba(30, 58, 138, 0.3);
background: #f8fafc;
color: #64748b;
border-color: #e2e8f0;
box-shadow: 0 2px 8px rgba(96, 165, 250, 0.08);

/* ✅ ONLY CHANGE font-size and font-weight */
```

---

## 🔍 VS Code Multi-Cursor Trick

1. **Select all instances of a value:**
   - Ctrl+H (Find & Replace)
   - Find: `font-size: 0.875rem`
   - Replace: `font-size: var(--modal-tab-text)`
   - Replace All

2. **Multi-cursor editing:**
   - Ctrl+D to select next occurrence
   - Alt+Click for multiple cursors
   - Edit all simultaneously

---

## ✅ Testing Checklist (per file)

After updating each file, verify:
- [ ] Tabs have 14px font-size
- [ ] Tab icons have 14px font-size
- [ ] Card titles have 16.4px font-size
- [ ] Card icons have 18px font-size
- [ ] Labels have 13px font-size (uppercase)
- [ ] Values have 15px font-size
- [ ] Badges have 14px font-size
- [ ] All font-weights use variables
- [ ] **Colors are UNCHANGED**
- [ ] Build succeeds (no errors)

---

## 📁 Priority Order (Process in this order)

### Batch 1: View Modals (7 files) - START HERE
1. ✅ PersonalMedicalViewModal.razor.css (DONE)
2. PacientViewModal.razor.css
3. DepartamentViewModal.razor.css
4. PozitieViewModal.razor.css
5. SpecializareViewModal.razor.css
6. UtilizatorViewModal.razor.css
7. ProgramareViewModal.razor.css

### Batch 2: Form Modals (8 files)
1. PersonalFormModal.razor.css
2. PersonalMedicalFormModal.razor.css
3. PacientAddEditModal.razor.css
4. DepartamentFormModal.razor.css
5. PozitieFormModal.razor.css
6. SpecializareFormModal.razor.css
7. UtilizatorFormModal.razor.css
8. ProgramareAddEditModal.razor.css

### Batch 3: Specialized Modals (6 files)
1. PacientHistoryModal.razor.css
2. PacientDocumentsModal.razor.css
3. PacientDoctoriModal.razor.css
4. AddDoctorToPacientModal.razor.css
5. ProgramareStatisticsModal.razor.css
6. SettingEditModal.razor.css

### Batch 4: Confirm Modals (3 files)
1. Shared/ConfirmDeleteModal.razor.css
2. Pacienti/Modals/ConfirmDeleteModal.razor.css
3. Programari/Modals/ConfirmCancelModal.razor.css

### Batch 5: Pages (15 files)
1. AdministrarePersonal.razor.css
2. AdministrarePersonalMedical.razor.css
3. AdministrarePacienti.razor.css
4. VizualizarePacienti.razor.css
5. AdministrareDepartamente.razor.css
6. AdministrarePozitii.razor.css
7. AdministrareSpecializari.razor.css
8. AdministrareUtilizatori.razor.css
9. SetariAutentificare.razor.css
10. CalendarProgramari.razor.css
11. ListaProgramari.razor.css
12. AuditLog.razor.css
13. AdministrareSesiuniActive.razor.css
14. Home.razor.css
15. (other pages if any)

### Batch 6: Layout (3 files)
1. MainLayout.razor.css
2. Header.razor.css
3. NavMenu.razor.css

### Batch 7: Global (1 file)
1. app.css

---

## 🚀 Automated Script Option (PowerShell)

```powershell
# Run this in ValyanClinic\Components folder
Get-ChildItem -Recurse -Filter "*.razor.css" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    
  # Replace font-sizes
    $content = $content -replace 'font-size:\s*0\.875rem', 'font-size: var(--modal-tab-text)'
    $content = $content -replace 'font-size:\s*0\.95rem', 'font-size: var(--modal-tab-text)'
    $content = $content -replace 'font-size:\s*14px', 'font-size: var(--modal-tab-text)'
    
    $content = $content -replace 'font-size:\s*0\.8rem', 'font-size: var(--modal-label)'
    $content = $content -replace 'font-size:\s*0\.85rem', 'font-size: var(--modal-label)'
    $content = $content -replace 'font-size:\s*13px', 'font-size: var(--modal-label)'
    
    $content = $content -replace 'font-size:\s*0\.9375rem', 'font-size: var(--modal-value)'
    $content = $content -replace 'font-size:\s*1rem', 'font-size: var(--modal-value)'
    $content = $content -replace 'font-size:\s*15px', 'font-size: var(--modal-value)'
    
    $content = $content -replace 'font-size:\s*1\.025rem', 'font-size: var(--modal-card-title)'
    $content = $content -replace 'font-size:\s*1\.1rem', 'font-size: var(--modal-card-title)'
    
    $content = $content -replace 'font-size:\s*1\.125rem', 'font-size: var(--modal-card-title-icon)'
    $content = $content -replace 'font-size:\s*1\.25rem', 'font-size: var(--modal-card-title-icon)'
    
    # Replace font-weights
    $content = $content -replace 'font-weight:\s*400', 'font-weight: var(--font-weight-normal)'
$content = $content -replace 'font-weight:\s*500', 'font-weight: var(--font-weight-medium)'
    $content = $content -replace 'font-weight:\s*600', 'font-weight: var(--font-weight-semibold)'
    $content = $content -replace 'font-weight:\s*700', 'font-weight: var(--font-weight-bold)'
    
    # Save back
  Set-Content -Path $_.FullName -Value $content
    
    Write-Host "✅ Updated: $($_.Name)"
}

Write-Host "`n🎉 Font unification complete!"
```

**⚠️ IMPORTANT:** Testează script-ul pe 1-2 fișiere mai întâi înainte de a rula pe toate!

---

## 📝 Manual vs Automated

### Manual (Recommended):
- ✅ Mai sigur
- ✅ Control total
- ✅ Verifici fiecare fișier
- ❌ Mai lent (2-3 minute per fișier)
- **Timp estimat:** ~2-3 ore pentru toate

### Automated (Script):
- ✅ Foarte rapid (< 1 minut pentru toate)
- ❌ Risk de greșeli
- ❌ Trebuie verificat după
- **Recomandare:** Rulează doar pe batch-uri mici (5-10 fișiere) și verifică

---

## 🎯 Quick Win Strategy

1. **Procesează manual** primele 5-10 fișiere pentru a te familiariza
2. După ce vezi pattern-ul, **folosește script-ul** pentru restul
3. **Verifică build-ul** după fiecare batch
4. **Test vizual** pe 2-3 modale reprezentative

**Timp estimat total:** 1-2 ore (în loc de 8-10 ore manual complet)

---

*🚀 Let's unify those fonts - DOAR font-size și font-weight, culorile rămân neschimbate!*

**Created:** 2025-01-08  
**Version:** 1.0 - Colors Preserved Edition
