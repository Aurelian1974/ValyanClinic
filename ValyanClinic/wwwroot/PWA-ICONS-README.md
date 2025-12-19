# 📱 PWA Icons Setup

Aplicația are suport PWA complet, dar ai nevoie de icon-uri pentru a finaliza instalarea.

## Icon-uri necesare

Trebuie să adaugi următoarele fișiere în `wwwroot/`:

### 1. **icon-192.png** (192x192 px)
   - Icon principal pentru PWA
   - Folosit pentru install prompt și shortcut-uri
   - Trebuie să fie în formatul PNG

### 2. **icon-512.png** (512x512 px)
   - Icon de înaltă rezoluție
   - Folosit pe ecrane mari și pentru splash screen
   - Trebuie să fie în formatul PNG

### 3. **favicon.png** (optional, dacă nu ai deja)
   - Icon pentru tab-ul browserului
   - Recomandat: 32x32 sau 48x48 px

## Cum să creezi icon-urile

### Opțiunea 1: Folosește logo-ul existent
```bash
# Dacă ai un logo SVG sau PNG mare, poți să-l redimensionezi cu ImageMagick
convert logo.png -resize 192x192 icon-192.png
convert logo.png -resize 512x512 icon-512.png
```

### Opțiunea 2: Generatoare online
- **Favicon.io**: https://favicon.io/
- **Real Favicon Generator**: https://realfavicongenerator.net/
- **PWA Builder**: https://www.pwabuilder.com/

### Opțiunea 3: Design custom
Creează în Figma/Photoshop/GIMP cu următoarele specificații:
- Background solid (evită transparență pentru iOS)
- Logo/text centrat cu padding (safe area)
- Culoare de brand: `#0066cc` (conform manifest.json)

## Exemple de design

### Icon simplu (text-based)
```
┌─────────────────┐
│                 │
│       VC        │  - Text bold "VC" (Valyan Clinic)
│                 │  - Background: #0066cc
│                 │  - Text: white
└─────────────────┘
```

### Icon cu logo medical
```
┌─────────────────┐
│       ⚕️        │  - Simbol medical
│   Valyan       │  - Nume aplicație
│                 │  - Background gradient
└─────────────────┘
```

## Verificare

După ce adaugi icon-urile, verifică:

1. **Chrome DevTools**:
   - F12 → Application → Manifest
   - Ar trebui să vezi toate icon-urile listate

2. **Lighthouse**:
   - F12 → Lighthouse → Run audit
   - PWA score ar trebui să fie 90+

3. **Test instalare**:
   - Desktop: Click "Install" în address bar
   - Mobile: "Add to Home Screen"

## Troubleshooting

### Icon-urile nu apar
- Verifică că fișierele sunt exact `icon-192.png` și `icon-512.png`
- Clear cache: Ctrl+Shift+R
- Verifică că path-ul e corect în `manifest.json`

### PWA nu se instalează
- Trebuie HTTPS (sau localhost pentru dev)
- Service Worker trebuie înregistrat corect
- Verifică consolă pentru erori

## Placeholder temporar

Dacă vrei să testezi rapid, poți crea icon-uri simple cu SVG → PNG:

```bash
# Creează un icon temporar roșu 192x192
convert -size 192x192 xc:#0066cc -pointsize 80 -fill white -gravity center -annotate +0+0 'VC' icon-192.png

# Creează un icon temporar roșu 512x512
convert -size 512x512 xc:#0066cc -pointsize 200 -fill white -gravity center -annotate +0+0 'VC' icon-512.png
```

Sau folosește favicon.png existent:
```bash
cp favicon.png icon-192.png
cp favicon.png icon-512.png
```

---

**IMPORTANT**: După ce adaugi icon-urile, rebuild aplicația și clear cache pentru ca PWA să le recunoască!
