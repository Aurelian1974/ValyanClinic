# 🚀 Quick Start: Modale Departamente

## Utilizare Rapidă

### ➕ Adăugare Departament Nou

1. Navighează la **Administrare** → **Departamente**
2. Click pe butonul **"Adauga Departament"** (verde, sus-dreapta)
3. Completează formularul:
   - **Denumire Departament** ⭐ (obligatoriu)
   - **Tip Departament** (optional, dropdown)
   - **Descriere** (optional, max 500 caractere)
4. Click **"Salveaza"**
5. ✅ Departament creat și adăugat în listă

### ✏️ Editare Departament

1. **Selectează** un departament din grid (click pe rând)
2. Toolbar-ul devine activ
3. Click pe butonul **"Editeaza"**
4. Modifică datele dorite
5. Click **"Actualizeaza"**
6. ✅ Departament actualizat

### 🗑️ Ștergere Departament

1. **Selectează** departamentul dorit
2. Click pe butonul **"Sterge"**
3. Confirmă ștergerea în dialog
4. ✅ Departament șters

---

## Validări Importante

### ⚠️ Câmpuri Obligatorii
- **Denumire Departament** - Nu poate fi gol, max 200 caractere

### ✅ Validări Automate
- **Unicitate** - Nu pot exista două departamente cu aceeași denumire
- **Lungime** - Descrierea este limitată la 500 caractere

---

## Scurtături Tastatură

| Acțiune | Scurtătură |
|---------|------------|
| Închide modal | `ESC` |
| Salvează (când modal este deschis) | `Enter` în câmpuri |

---

## Tips & Tricks

### 💡 Dropdown Tip Departament
- Poți **căuta** direct în dropdown (filtrare automată)
- Click pe **X** pentru a șterge selecția
- Optional field - poți lăsa necompletat

### 💡 Descriere
- Folosește descrierea pentru:
  - Responsabilități departament
  - Locație fizică
  - Contact principal
  - Orice alte detalii relevante

### 💡 Grid Features
- **Sortare**: Click pe header-ul coloanei
- **Redimensionare**: Drag pe marginea header-ului
- **Reordonare**: Drag & drop coloane
- **Scroll virtual**: Pentru performanță la multe înregistrări

---

## Troubleshooting

### ❌ "Exista deja un departament cu aceasta denumire"
**Soluție:** Alege o denumire unică. Verifică în listă dacă nu există deja.

### ❌ Modal nu se deschide
**Soluție:** 
- Verifică că nu ai erori în consolă browser (F12)
- Reîmprospătează pagina (F5)

### ❌ Dropdown gol (Tip Departament)
**Soluție:**
- Verifică că există tipuri de departamente în baza de date
- Tabel: `TipDepartament`

---

## Pentru Dezvoltatori

### 🔧 Cum funcționează tehnic

**Add Flow:**
```
Click "Adauga" 
→ OpenForAdd() 
→ Empty form 
→ LoadTipDepartamente() 
→ Modal visible

Submit form 
→ HandleSubmit() 
→ CreateDepartamentCommand 
→ MediatR 
→ CreateDepartamentCommandHandler 
→ Check uniqueness 
→ Save to DB 
→ Return Result<Guid>
→ Success callback 
→ Reload grid 
→ Show toast
```

**Edit Flow:**
```
Select row + Click "Editeaza"
→ OpenForEdit(id)
→ GetDepartamentByIdQuery
→ MediatR
→ GetDepartamentByIdQueryHandler
→ Load from DB
→ Populate form
→ Modal visible

Submit form
→ HandleSubmit()
→ UpdateDepartamentCommand
→ MediatR
→ UpdateDepartamentCommandHandler
→ Check existence
→ Check uniqueness (exclude current)
→ Update in DB
→ Return Result<bool>
→ Success callback
→ Reload grid
→ Show toast
```

### 📦 Dependencies
- **MediatR** - CQRS pattern
- **Syncfusion.Blazor.Grids** - Grid component
- **Syncfusion.Blazor.DropDowns** - Dropdown component
- **Syncfusion.Blazor.Notifications** - Toast notifications

### 🎨 Styling
- **Tema:** Verde (#22c55e)
- **CSS:** `DepartamentFormModal.razor.css`
- **Bază:** `modal-base.css` (global)

---

## FAQ

### ❓ De ce "Tip Departament" este optional?
Unele departamente pot să nu aibă o categorie specifică. Este flexibil pentru diferite organizări.

### ❓ Pot avea mai multe departamente cu aceeași descriere?
Da! Doar **denumirea** trebuie să fie unică. Descrierea poate fi identică.

### ❓ Ce se întâmplă dacă șterg un departament?
⚠️ **Atenție:** Dacă departamentul este folosit în alte entități (Personal, etc.), ștergerea poate eșua.

### ❓ Cum adaug un nou "Tip Departament"?
Momentan, tipurile se adaugă direct în baza de date (tabel `TipDepartament`). Interfață admin va fi adăugată în viitor.

---

## 📞 Suport

Pentru probleme tehnice sau întrebări:
- Verifică console-ul browser (F12)
- Verifică log-urile aplicației (`Logs/valyan-clinic-*.log`)
- Contactează echipa de dezvoltare

---

**Happy managing! 🎉**
