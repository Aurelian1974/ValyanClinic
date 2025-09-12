# ValyanMed CSS Architecture

## ?? Structura CSS

Aplica?ia ValyanMed folose?te o arhitectur? CSS modular? ?i organizat? pentru a asigura mentenabilitatea ?i scalabilitatea codului.

### Structura de Fi?iere

```
wwwroot/
??? css/
?   ??? global.css          # Stiluri globale, variabile CSS, reset
?   ??? components.css      # Componente reutilizabile (sidebar, header, buttons)
?   ??? utilities.css       # Clase utilitare (spacing, colors, display)
?   ??? app.css            # Fi?ier principal (identic cu cel din root)
?   ??? pages/
?       ??? home.css       # Stiluri pentru pagina de dashboard
?       ??? patients.css   # Stiluri pentru pagina de pacien?i
?       ??? simple.css     # Stiluri pentru pagini simple de con?inut
??? app.css                # Fi?ier principal care import? toate modulele
```

## ?? Paleta de Culori

Aplica?ia folose?te o palet? de culori albastru deschis pentru o experien?? vizual? calm? ?i profesional?:

### Culori Principale
- **Blue 50-900**: Gam? complet? de albastru (de la foarte deschis la închis)
- **Gray 50-900**: Culori neutre pentru text ?i background-uri
- **Success/Warning/Error**: Culori semantice pentru st?ri

### Gradient-uri Predefinite
- `--blue-gradient-light`: Pentru background-uri delicate
- `--blue-gradient-medium`: Pentru elemente interactive
- `--blue-gradient-header`: Pentru header-uri ?i sec?iuni importante
- `--blue-gradient-sidebar`: Pentru sidebar-ul aplica?iei

## ?? Componente CSS

### Layout Principal
- `.app-layout`: Container principal al aplica?iei
- `.sidebar`: Navigation lateral cu submeniuri
- `.main-content`: Zona principal? de con?inut
- `.main-header`: Header-ul aplica?iei

### Componente Interactive
- `.btn-*`: Familie de butoane cu multiple variante
- `.card`: Carduri pentru organizarea con?inutului
- `.alert-*`: Alerte ?i notific?ri
- `.table`: Tabele responsive

### Componente Specializate
- `.stat-card`: Carduri pentru statistici
- `.activity-item`: Elemente pentru activit??i recente
- `.filter-*`: Componente pentru filtrare ?i c?utare

## ?? Design Responsive

Aplica?ia este complet responsive cu breakpoint-uri definite:

- **Desktop**: > 1024px - Layout complet cu sidebar fix
- **Tablet**: 768px - 1024px - Sidebar colapsabil
- **Mobile**: < 768px - Sidebar overlay cu hamburger menu
- **Small Mobile**: < 480px - Layout optimizat pentru ecrane mici

## ?? Variabile CSS

Toate valorile importante sunt definite ca variabile CSS pentru consisten??:

```css
:root {
  /* Culori */
  --blue-500: #0ea5e9;
  --gray-800: #1f2937;
  
  /* Spacing */
  --border-radius-lg: 12px;
  --shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  
  /* Typography */
  --font-size-base: 1rem;
  --font-family: system-ui, -apple-system, 'Segoe UI', 'Roboto';
  
  /* Transitions */
  --transition-base: 0.2s ease;
}
```

## ?? Clase Utilitare

Sistemul include clase utilitare pentru dezvoltare rapid?:

### Spacing
- `.m-*`, `.mt-*`, `.mb-*`, etc. - Margin utilities
- `.p-*`, `.pt-*`, `.pb-*`, etc. - Padding utilities

### Display & Layout
- `.d-flex`, `.d-grid`, `.d-none` - Display utilities
- `.justify-*`, `.align-*` - Flexbox utilities

### Typography
- `.text-*` - Text sizes ?i colors
- `.font-*` - Font weights

### Colors
- `.text-blue-*`, `.bg-blue-*` - Color utilities
- `.text-success`, `.bg-error` - Semantic colors

## ?? Best Practices

1. **Modular?**: Fiecare tip de stiluri are propriul fi?ier
2. **Variabile CSS**: Toate valorile importante sunt centralizate
3. **Mobile First**: Design-ul porne?te de la mobile spre desktop
4. **Semantic**: Clasele au nume descriptive ?i clare
5. **Consistent**: Utilizarea unui sistem de design unificat

## ?? Ad?ugarea Stilurilor Noi

### Pentru o pagin? nou?:
1. Creeaz? `wwwroot/css/pages/numele-paginii.css`
2. Adaug? import în `app.css`: `@import url('css/pages/numele-paginii.css');`

### Pentru o component? nou?:
1. Adaug? stilurile în `components.css`
2. Folose?te variabilele CSS existente
3. Asigur?-te c? este responsive

### Pentru utilit??i noi:
1. Adaug? în `utilities.css`
2. Urmeaz? patternul existent de denumire

## ?? Debugging

Pentru debugging CSS:
1. Verific? c? toate import-urile sunt corecte în `app.css`
2. Folose?te dev tools pentru a vedea care stiluri se aplic?
3. Verific? specificitatea CSS în cazul conflictelor

---

**Autor**: Echipa de dezvoltare ValyanMed  
**Ultima actualizare**: 2024