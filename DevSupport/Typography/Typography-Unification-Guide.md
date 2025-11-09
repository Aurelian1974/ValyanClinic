# Ghid de Uniformizare Tipografie - ValyanClinic
**Data creare:** 2025-01-08  
**Scop:** Standardizare fonturi și dimensiuni în toate paginile și modalele  
**Status:** 🚀 **IN IMPLEMENTARE**

---

## 📋 Rezumat

Acest document definește **sistemul unificat de tipografie** pentru întreaga aplicație ValyanClinic, asigurând consistență vizuală și experiență de utilizare uniformă.

---

## 🎯 Obiective

✅ **Consistență vizuală** - Toate componentele folosesc aceleași dimensiuni de font  
✅ **Ierarhie clară** - Relații vizuale clare între titluri, text și detalii  
✅ **Accesibilitate** - Dimensiuni minime pentru lizibilitate optimă  
✅ **Responsive** - Ajustări automate pentru mobile  

---

## 📏 Scala de Fonturi Unificată

### Scară Generală

| CSS Variable | Dimensiune | Pixeli | Utilizare |
|--------------|------------|--------|-----------|
| `--font-size-xs` | 0.6875rem | 11px | Badge labels, micro-text |
| `--font-size-sm` | 0.8125rem | 13px | Labels, captions, help text |
| `--font-size-base` | 0.875rem | **14px** | **TEXT STANDARD** (body, buttons, tabs) |
| `--font-size-md` | 0.9375rem | 15px | Emphasized values, input text |
| `--font-size-lg` | 1.025rem | 16.4px | Card titles, section headers |
| `--font-size-xl` | 1.125rem | 18px | Icons în titluri |
| `--font-size-2xl` | 1.375rem | 22px | Modal headers |
| `--font-size-3xl` | 1.75rem | 28px | Page headers |
| `--font-size-4xl` | 2rem | 32px | Hero headers |

---

## 🎨 Font Mapping pentru Componente

### 📦 MODALE (All Modals)

| Element | CSS Variable | Dimensiune | Greutate |
|---------|--------------|------------|----------|
| **Header Title** | `--modal-header-title` | 22px | 600 (semibold) |
| **Close Icon** | `--modal-close-icon` | 22px | - |
| **Tab Text** | `--modal-tab-text` | 14px | 500 (normal) / 600 (active) |
| **Tab Icon** | `--modal-tab-icon` | 14px | - |
| **Card Title** | `--modal-card-title` | 16.4px | 600 |
| **Card Title Icon** | `--modal-card-title-icon` | 18px | - |
| **Label (uppercase)** | `--modal-label` | 13px | 600 |
| **Value (data)** | `--modal-value` | 15px | 400 |
| **Badge** | `--modal-badge` | 14px | 600 |
| **Badge Small** | `--modal-badge-small` | 11px | 600 |
| **Button** | `--modal-button` | 14px | 600 |

#### Exemplu de Utilizare în Modal CSS:
```css
.modal-header h2 {
    font-size: var(--modal-header-title);
    font-weight: var(--font-weight-semibold);
}

.tab-button {
    font-size: var(--modal-tab-text);
    font-weight: var(--font-weight-medium);
}

.tab-button.active {
    font-weight: var(--font-weight-semibold);
}

.card-title {
    font-size: var(--modal-card-title);
    font-weight: var(--font-weight-semibold);
}

.info-item label {
    font-size: var(--modal-label);
    font-weight: var(--font-weight-semibold);
    text-transform: uppercase;
}

.info-value {
    font-size: var(--modal-value);
}

.badge {
    font-size: var(--modal-badge);
    font-weight: var(--font-weight-semibold);
}
```

---

### 📄 PAGINI / LISTE (Pages & Lists)

| Element | CSS Variable | Dimensiune | Greutate |
|---------|--------------|------------|----------|
| **Page Title** | `--page-header-title` | 28px | 700 (bold) |
| **Page Subtitle** | `--page-subtitle` | 16.4px | 500 |
| **Table Header** | `--table-header` | 14px | 600 |
| **Table Cell** | `--table-cell` | 14px | 400 |
| **Filter Label** | `--filter-label` | 14px | 600 |
| **Button Text** | `--button-text` | 14px | 600 |

---

### 📝 FORMULARE (Forms)

| Element | CSS Variable | Dimensiune | Greutate |
|---------|--------------|------------|----------|
| **Label** | `--form-label` | 14px | 600 |
| **Input Text** | `--form-input` | 15px | 400 |
| **Input Mobile** | `--form-input-mobile` | 16px | 400 (prevent zoom) |
| **Help Text** | `--form-help` | 13px | 400 |
| **Error Text** | `--form-error` | 13px | 500 |

#### Exemplu Form:
```css
.form-group label {
    font-size: var(--form-label);
    font-weight: var(--font-weight-semibold);
}

.form-control {
    font-size: var(--form-input);
}

@media (max-width: 768px) {
    .form-control {
        font-size: var(--form-input-mobile); /* Prevent iOS zoom */
    }
}

.form-text {
    font-size: var(--form-help);
}

.invalid-feedback {
    font-size: var(--form-error);
    font-weight: var(--font-weight-medium);
}
```

---

### 🧭 NAVIGARE (Navigation)

| Element | CSS Variable | Dimensiune | Greutate |
|---------|--------------|------------|----------|
| **Nav Item** | `--nav-item` | 14px | 500 / 600 (active) |
| **Nav Icon** | `--nav-icon` | 18px | - |
| **Breadcrumb** | `--breadcrumb` | 13px | 400 |

---

## 🎨 Font Weights Standard

```css
--font-weight-normal: 400;    /* Body text, values */
--font-weight-medium: 500;    /* Nav items, subtle emphasis */
--font-weight-semibold: 600;  /* Labels, buttons, active states */
--font-weight-bold: 700;      /* Page titles, strong emphasis */
```

### Reguli de Utilizare:
- **400** - Text normal, valori din modal
- **500** - Tab-uri inactive, nav items
- **600** - Labels, butoane, tab-uri active, titluri carduri
- **700** - Titluri pagini principale

---

## 📐 Line Heights

```css
--line-height-tight: 1.25;    /* Headings, titluri */
--line-height-base: 1.5;      /* Body text standard */
--line-height-relaxed: 1.75;  /* Long-form content, observații */
```

### Utilizare:
```css
h1, h2, h3, h4, h5, h6 {
    line-height: var(--line-height-tight);
}

body, p, .info-value {
    line-height: var(--line-height-base);
}

.observation-text, .description {
    line-height: var(--line-height-relaxed);
}
```

---

## 🎯 Ierarhie Vizuală

### Niveluri de Importanță (de sus în jos):

```
1. PAGE TITLE (28px, bold 700)
   └─ Titlu principal pagină
   
2. MODAL HEADER (22px, semibold 600)
   └─ Titlu modal
   
3. CARD TITLE (16.4px, semibold 600)
   └─ Titlu secțiuni din modal/pagină
   
4. EMPHASIZED VALUE (15px, normal 400)
   └─ Valori importante din modal
   
5. BODY TEXT (14px, normal 400)
   └─ Text standard, butoane, tab-uri
   
6. LABEL / CAPTION (13px, semibold 600 uppercase)
   └─ Labels pentru câmpuri
   
7. BADGE / MICRO TEXT (11px, semibold 600)
   └─ Badge-uri mici, indicators
```

---

## 📱 Responsive Adjustments

### Mobile (<768px)

```css
@media (max-width: 768px) {
    :root {
        --page-header-title: 1.5rem;        /* 28px → 24px */
        --modal-header-title: 1.25rem;      /* 22px → 20px */
     --modal-card-title: 0.9375rem;    /* 16.4px → 15px */
    }
    
    .form-control {
        font-size: 16px !important; /* Prevent iOS zoom */
    }
}
```

---

## ✅ Checklist Implementare per Componentă

### Pentru fiecare Modal/Pagină:

- [ ] **Header Title:** `font-size: var(--modal-header-title)`
- [ ] **Close Icon:** `font-size: var(--modal-close-icon)`
- [ ] **Tab Text:** `font-size: var(--modal-tab-text)`
- [ ] **Tab Icon:** `font-size: var(--modal-tab-icon)`
- [ ] **Card Title:** `font-size: var(--modal-card-title)`
- [ ] **Card Title Icon:** `font-size: var(--modal-card-title-icon)`
- [ ] **Labels:** `font-size: var(--modal-label)` + uppercase
- [ ] **Values:** `font-size: var(--modal-value)`
- [ ] **Badges:** `font-size: var(--modal-badge)`
- [ ] **Buttons:** `font-size: var(--modal-button)`

### Înlocuiri Necesare:

```css
/* ❌ BEFORE - Hardcoded values */
.modal-header h2 { font-size: 1.5rem; }
.tab-button { font-size: 0.95rem; }
.card-title { font-size: 1.1rem; }
.info-item label { font-size: 0.85rem; }
.info-value { font-size: 1rem; }
.badge { font-size: 0.95rem; }

/* ✅ AFTER - Using CSS variables */
.modal-header h2 { font-size: var(--modal-header-title); }
.tab-button { font-size: var(--modal-tab-text); }
.card-title { font-size: var(--modal-card-title); }
.info-item label { font-size: var(--modal-label); }
.info-value { font-size: var(--modal-value); }
.badge { font-size: var(--modal-badge); }
```

---

## 🔍 Exemple Practice

### Exemplu 1: PersonalViewModal ✅ (Already Updated)

```css
/* Header */
.modal-header h2 {
    font-size: var(--modal-header-title); /* 22px */
    font-weight: var(--font-weight-semibold);
}

/* Tabs */
.tab-button {
    font-size: var(--modal-tab-text); /* 14px */
    font-weight: var(--font-weight-medium);
}

.tab-button.active {
    font-weight: var(--font-weight-semibold);
}

/* Card */
.card-title {
    font-size: var(--modal-card-title); /* 16.4px */
    font-weight: var(--font-weight-semibold);
}

/* Labels & Values */
.info-item label {
    font-size: var(--modal-label); /* 13px */
font-weight: var(--font-weight-semibold);
    text-transform: uppercase;
}

.info-value {
    font-size: var(--modal-value); /* 15px */
}
```

### Exemplu 2: Login Page

```css
.login-header h1 {
    font-size: var(--page-header-title); /* 28px */
    font-weight: var(--font-weight-bold);
}

.subtitle {
    font-size: var(--modal-value); /* 15px */
}

.form-group label {
  font-size: var(--form-label); /* 14px */
    font-weight: var(--font-weight-semibold);
}

.form-control {
    font-size: var(--form-input); /* 15px */
}

.btn-login {
    font-size: var(--button-text); /* 14px */
    font-weight: var(--font-weight-semibold);
}
```

---

## 📊 Tabel Complet de Conversie

| Hardcoded Value | CSS Variable | Pixeli | Context |
|-----------------|--------------|--------|---------|
| `0.75rem` / `12px` | `--font-size-xs` | 11px | Badge small |
| `0.85rem` / `0.8rem` / `13px` | `--modal-label` | 13px | Labels |
| `0.875rem` / `14px` | `--font-size-base` | 14px | **Standard** |
| `0.95rem` / `15px` | `--modal-value` | 15px | Values |
| `1rem` / `16px` | - | 16px | Folosim 15px (--modal-value) |
| `1.1rem` / `1.025rem` | `--modal-card-title` | 16.4px | Card titles |
| `1.25rem` / `1.125rem` | `--font-size-xl` | 18px | Icons |
| `1.5rem` | `--modal-header-title` | 22px | Modal headers |
| `2rem` / `32px` | `--page-header-title` | 28px (mobile 24px) | Page headers |

---

## 🎯 Plan de Implementare

### Faza 1: Core Files ✅ COMPLETAT
- [x] `variables.css` - Sistem tipografie definit
- [x] `base.css` - Font-family aplicat
- [x] `PersonalViewModal.razor.css` - Template de referință

### Faza 2: Modale (Priority 1)
- [ ] `PersonalMedicalViewModal.razor.css`
- [ ] `PersonalFormModal.razor.css`
- [ ] `PersonalMedicalFormModal.razor.css`
- [ ] `PacientViewModal.razor.css`
- [ ] `PacientAddEditModal.razor.css`
- [ ] `PacientHistoryModal.razor.css`
- [ ] `PacientDocumentsModal.razor.css`
- [ ] `PacientDoctoriModal.razor.css`
- [ ] `DepartamentViewModal.razor.css`
- [ ] `DepartamentFormModal.razor.css`
- [ ] `PozitieViewModal.razor.css`
- [ ] `PozitieFormModal.razor.css`
- [ ] `SpecializareViewModal.razor.css`
- [ ] `SpecializareFormModal.razor.css`
- [ ] `UtilizatorViewModal.razor.css`
- [ ] `UtilizatorFormModal.razor.css`
- [ ] `ProgramareViewModal.razor.css`
- [ ] `ProgramareAddEditModal.razor.css`
- [ ] `ProgramareStatisticsModal.razor.css`
- [ ] `SettingEditModal.razor.css`
- [ ] `ConfirmDeleteModal.razor.css` (toate instanțele)
- [ ] `ConfirmCancelModal.razor.css`
- [ ] `AddDoctorToPacientModal.razor.css`

### Faza 3: Pagini (Priority 2)
- [ ] `Login.razor.css`
- [ ] `Home.razor.css`
- [ ] `AdministrarePersonal.razor.css`
- [ ] `AdministrarePersonalMedical.razor.css`
- [ ] `AdministrarePacienti.razor.css`
- [ ] `VizualizarePacienti.razor.css`
- [ ] `AdministrareDepartamente.razor.css`
- [ ] `AdministrarePozitii.razor.css`
- [ ] `AdministrareSpecializari.razor.css`
- [ ] `AdministrareUtilizatori.razor.css`
- [ ] `CalendarProgramari.razor.css`
- [ ] `ListaProgramari.razor.css`
- [ ] `AuditLog.razor.css`
- [ ] `AdministrareSesiuniActive.razor.css`
- [ ] `SetariAutentificare.razor.css`

### Faza 4: Layout (Priority 3)
- [ ] `MainLayout.razor.css`
- [ ] `Header.razor.css`
- [ ] `NavMenu.razor.css`

### Faza 5: Global Styles (Priority 4)
- [ ] `modal-base.css`
- [ ] `app.css`

---

## 🧪 Testing Checklist

### Visual Consistency Tests

Pentru fiecare componentă updatată:

- [ ] **Header Title** - 22px, semibold
- [ ] **Tab Text** - 14px, medium (inactive) / semibold (active)
- [ ] **Card Title** - 16.4px, semibold
- [ ] **Labels** - 13px, semibold, uppercase
- [ ] **Values** - 15px, normal
- [ ] **Badges** - 14px, semibold
- [ ] **Buttons** - 14px, semibold

### Cross-Component Tests

- [ ] Toate modalele au același header font size
- [ ] Toate tab-urile au același font size
- [ ] Toate label-urile au același font size
- [ ] Toate valorile au același font size
- [ ] Toate butoanele au același font size

### Responsive Tests

- [ ] Desktop (1920px) - Full sizes
- [ ] Laptop (1366px) - Full sizes
- [ ] Tablet (768px) - Adjusted sizes
- [ ] Mobile (375px) - Mobile optimizations + iOS zoom prevention

---

## 📝 Best Practices

### ✅ DO:
- Folosește **întotdeauna** CSS variables pentru font-size
- Respectă **ierarhia vizuală** stabilită
- Folosește **font-weight-uri consistente** pentru același tip de element
- Adaugă **line-height** pentru lizibilitate
- Testează pe **mobile** pentru iOS zoom prevention

### ❌ DON'T:
- Nu folosi valori hardcoded pentru font-size
- Nu inventa dimensiuni noi în afara scalei
- Nu folosi font-weight random (respectă 400, 500, 600, 700)
- Nu uita de responsive adjustments
- Nu amesteca unitele (folosește rem peste tot)

---

## 🚀 Beneficii

### Consistență
✅ Toate componentele arată uniform  
✅ Experiență de utilizare predictibilă  
✅ Profesionalism crescut  

### Mentenabilitate
✅ Modificări globale dintr-un singur loc (`variables.css`)  
✅ Cod mai curat și mai ușor de citit  
✅ Onboarding mai rapid pentru developeri noi  

### Performance
✅ CSS mai mic (variabile refolosite)  
✅ Browser caching mai eficient  
✅ Render performance optimizat  

### Accesibilitate
✅ Dimensiuni minime respectate  
✅ Contrast ratios optime  
✅ Lizibilitate îmbunătățită  

---

## 📞 Support

**Pentru întrebări despre tipografie:**
- **Document:** Typography-Unification-Guide.md
- **Variables:** `ValyanClinic\wwwroot\css\variables.css`
- **Template:** `PersonalViewModal.razor.css` (referință completă)

---

## 🎓 Quick Reference Card

```
MEMORIZE THIS:
- Modal Header: 22px (--modal-header-title)
- Card Title: 16.4px (--modal-card-title)
- Body/Buttons: 14px (--font-size-base)
- Values: 15px (--modal-value)
- Labels: 13px (--modal-label, uppercase)
- Badge Small: 11px (--font-size-xs)

WEIGHTS:
- Normal text: 400
- Tabs inactive: 500
- Labels/Buttons/Tabs active: 600
- Page titles: 700
```

---

*🎨 Uniformizare completă a tipografiei în ValyanClinic pentru o experiență vizuală superioară! 🎨*

**Status:** 🚀 **IN IMPLEMENTARE**  
**Created:** 2025-01-08  
**Version:** 1.0
