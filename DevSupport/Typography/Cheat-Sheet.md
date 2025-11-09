# 📝 Typography Cheat Sheet - ValyanClinic

> **Quick reference pentru uniformizarea fonturilor**

---

## 🎯 Font Family (Tipul de Font)

```css
/* Font Family Principal */
--font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, 
               "Helvetica Neue", Arial, sans-serif;

/* Font Family Monospace (pentru cod) */
--font-family-mono: 'Courier New', Courier, monospace;
```

### Aplicare:
```css
body, .modal-container, .page-content {
    font-family: var(--font-family);
}

code, pre, .code-block {
 font-family: var(--font-family-mono);
}
```

### Ordinea Font Stack:
1. **-apple-system** -Font nativ Apple (San Francisco pe macOS/iOS)
2. **BlinkMacSystemFont** - Backup pentru Chrome pe macOS
3. **"Segoe UI"** - Font nativ Windows 10/11
4. **Roboto** - Font nativ Android
5. **"Helvetica Neue"** - Fallback macOS mai vechi
6. **Arial** - Fallback universal
7. **sans-serif** - Generic fallback

**Rezultat vizual:** Font modern, curat, fără serife (sans-serif style)

---

## 🎯 CSS Variables - Font Sizes

```css
/* Core Sizes */
--font-size-xs: 11px   /* Badge small, micro text */
--modal-label: 13px         /* Labels (uppercase) */
--font-size-base: 14px      /* ⭐ STANDARD (body, buttons, tabs) */
--modal-value: 15px   /* Values, emphasized text */
--modal-card-title: 16.4px  /* Card titles */
--font-size-xl: 18px        /* Icons */
--modal-header-title: 22px  /* Modal headers */
--page-header-title: 28px/* Page headers */
```

## 🎨 Font Weights

```css
--font-weight-normal: 400      /* Text, values */
--font-weight-medium: 500      /* Tabs inactive */
--font-weight-semibold: 600    /* Labels, buttons, tabs active */
--font-weight-bold: 700   /* Page headers */
```

---

## 🔄 Quick Replace Guide

### Font Family
```css
/* Aplică font-family peste tot */
font-family: Arial, sans-serif → var(--font-family)
font-family: "Segoe UI" → var(--font-family)
font-family: sans-serif → var(--font-family)
```

### Headers
```css
font-size: 1.5rem → var(--modal-header-title)     /* 22px */
font-size: 2rem   → var(--page-header-title)  /* 28px */
```

### Tabs
```css
font-size: 0.95rem → var(--modal-tab-text)      /* 14px */
font-size: 1rem→ var(--modal-tab-icon)        /* 14px */
```

### Cards
```css
font-size: 1.1rem  → var(--modal-card-title)      /* 16.4px */
font-size: 1.25rem → var(--modal-card-title-icon) /* 18px */
```

### Labels & Values
```css
font-size: 0.85rem → var(--modal-label)           /* 13px */
font-size: 1rem → var(--modal-value)       /* 15px */
```

### Buttons & Badges
```css
font-size: 0.95rem → var(--button-text)         /* 14px */
font-size: 0.875rem → var(--modal-badge)        /* 14px */
font-size: 0.75rem → var(--modal-badge-small)     /* 11px */
```

### Weights
```css
font-weight: 400 → var(--font-weight-normal)
font-weight: 500 → var(--font-weight-medium)
font-weight: 600 → var(--font-weight-semibold)
font-weight: 700 → var(--font-weight-bold)
```

---

## 📋 Component Patterns

### Base Setup (in base.css)
```css
html, body {
    font-family: var(--font-family);
    font-size: var(--font-size-base);
    line-height: var(--line-height-base);
}
```

### Modal Header
```css
.modal-header h2 {
    font-family: var(--font-family); /* Usually inherited */
    font-size: var(--modal-header-title);
    font-weight: var(--font-weight-semibold);
}
```

### Tab Button
```css
.tab-button {
    font-family: var(--font-family);
    font-size: var(--modal-tab-text);
    font-weight: var(--font-weight-medium);
}

.tab-button.active {
    font-weight: var(--font-weight-semibold);
}

.tab-button i {
    font-size: var(--modal-tab-icon);
}
```

### Card Title
```css
.card-title {
    font-family: var(--font-family);
    font-size: var(--modal-card-title);
    font-weight: var(--font-weight-semibold);
}

.card-title i {
    font-size: var(--modal-card-title-icon);
}
```

### Label & Value
```css
.info-item label {
    font-family: var(--font-family);
    font-size: var(--modal-label);
    font-weight: var(--font-weight-semibold);
    text-transform: uppercase;
    letter-spacing: var(--letter-spacing-wide);
}

.info-value {
    font-family: var(--font-family);
    font-size: var(--modal-value);
    line-height: var(--line-height-base);
}
```

### Button
```css
.btn {
    font-family: var(--font-family);
    font-size: var(--button-text);
    font-weight: var(--font-weight-semibold);
}
```

### Badge
```css
.badge {
    font-family: var(--font-family);
    font-size: var(--modal-badge);
    font-weight: var(--font-weight-semibold);
}

.badge-small {
    font-size: var(--modal-badge-small);
}
```

---

## 📏 Line Heights & Letter Spacing

```css
/* Line Heights */
--line-height-tight: 1.25      /* Headings, compact text */
--line-height-base: 1.5      /* Body text (STANDARD) */
--line-height-relaxed: 1.75    /* Long-form content, observations */

/* Letter Spacing */
--letter-spacing-tight: -0.025em    /* Large headings */
--letter-spacing-normal: 0          /* Normal text */
--letter-spacing-wide: 0.025em      /* Small caps */
--letter-spacing-wider: 0.05em      /* Labels (uppercase) */
```

### Aplicare:
```css
h1, h2, h3 {
    line-height: var(--line-height-tight);
}

body, p, .info-value {
line-height: var(--line-height-base);
}

.observation-text, .description {
    line-height: var(--line-height-relaxed);
}

.info-item label {
 letter-spacing: var(--letter-spacing-wider);
}
```

---

## 📱 Responsive Pattern

```css
@media (max-width: 768px) {
    /* Form inputs - iOS zoom prevention */
    .form-control {
   font-size: var(--form-input-mobile); /* 16px */
    }
  
    /* Smaller headers on mobile */
    .modal-header h2 {
        font-size: var(--font-size-xl); /* 18px instead of 22px */
    }
    
    .page-header h1 {
font-size: var(--font-size-2xl); /* 22px instead of 28px */
    }
}
```

---

## 🎨 Bonus: Color Variables

```css
/* Text */
color: var(--text-color) /* #334155 */
color: var(--text-secondary)     /* #64748b */
color: var(--text-muted)       /* #94a3b8 */

/* Primary */
color: var(--primary-color)      /* #60a5fa */
color: var(--primary-dark)       /* #3b82f6 */

/* Backgrounds */
background: var(--background-color)      /* #f8fafc */
background: var(--background-secondary)  /* #f1f5f9 */
background: var(--background-light) /* #eff6ff */

/* Borders */
border-color: var(--border-color) /* #e2e8f0 */
```

---

## ✅ Verification Checklist

Per fișier CSS verifică:
- [ ] Font family → variable (sau inherited din body)
- [ ] Header title → variable
- [ ] Tab text & icons → variables
- [ ] Card titles & icons → variables
- [ ] Labels (uppercase) → variable + letter-spacing
- [ ] Values → variable + line-height
- [ ] Buttons → variable
- [ ] Badges → variables
- [ ] Font weights → variables
- [ ] Line heights → variables where needed
- [ ] No hardcoded rem/px (except special cases)
- [ ] Responsive adjustments present

---

## 🔍 Search Commands

```bash
# Find hardcoded font-family
rg "font-family:\s*['\"]?[^;]+" --type css

# Find hardcoded font-sizes
rg "font-size:\s*\d+\.?\d*rem" --type css
rg "font-size:\s*\d+px" --type css

# Find hardcoded font-weights
rg "font-weight:\s*\d{3}" --type css

# Find hardcoded line-heights
rg "line-height:\s*\d+\.?\d+" --type css
```

---

## 📁 Key Files

- **Variables:** `ValyanClinic\wwwroot\css\variables.css`
- **Base (font-family applied):** `ValyanClinic\wwwroot\css\base.css`
- **Modal Base:** `ValyanClinic\wwwroot\css\modal-base.css`
- **Template:** `PersonalViewModal.razor.css`

---

## 🎯 Remember

### The Golden Rules:

1. **Font Family:** System font stack (Segoe UI pe Windows, San Francisco pe Mac)
   ```css
   font-family: var(--font-family);
   ```

2. **Font Size:** 14px (--font-size-base) is the standard for body text, buttons, and tabs
   ```css
font-size: var(--font-size-base);
   ```

3. **Font Weight:** Use semantic variables (normal, medium, semibold, bold)
   ```css
   font-weight: var(--font-weight-semibold);
   ```

4. **Line Height:** 1.5 for body text, 1.25 for headings, 1.75 for long content
   ```css
   line-height: var(--line-height-base);
   ```

### The Complete Typography Stack:
```css
body {
    font-family: var(--font-family);        /* System fonts */
    font-size: var(--font-size-base);       /* 14px standard */
    font-weight: var(--font-weight-normal); /* 400 */
    line-height: var(--line-height-base); /* 1.5 */
    letter-spacing: var(--letter-spacing-normal); /* 0 */
    color: var(--text-color);  /* #334155 */
}
```

### The Hierarchy:
```
28px (Page Header)
  └─ 22px (Modal Header)
      └─ 16.4px (Card Title)
          └─ 15px (Values)
              └─ 14px (Body/Buttons) ⭐ STANDARD
     └─ 13px (Labels)
  └─ 11px (Badge Small)

All using: System Font Stack (Segoe UI/San Francisco/Roboto)
```

---

## 💡 Pro Tips

### Font Rendering Optimization
```css
body {
    font-family: var(--font-family);
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
    text-rendering: optimizeLegibility;
}
```

### Icon Font Compatibility
```css
/* Icons use their own font-family, dar size-ul e uniform */
.icon, i {
    font-size: inherit; /* Moștenește din părinte */
}

.card-title i {
    font-size: var(--modal-card-title-icon); /* 18px */
}
```

---

*Quick reference pentru uniformizarea fonturilor în ValyanClinic* 🎨

**Last Updated:** 2025-01-08  
**Version:** 1.1 (Added Font Family section)
