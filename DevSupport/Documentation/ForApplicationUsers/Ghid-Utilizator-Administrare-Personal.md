# Ghid Utilizator - Administrare Personal

## 🏢 Prezentare Generală

Pagina **Administrare Personal** vă permite să gestionați întregul personal non-medical al clinicii într-un mod eficient și organizat. De aici puteți adăuga, modifica, vizualiza și căuta informații despre angajații din departamentele administrative, financiare, IT, întreținere și altele.

## 📍 Acces la Pagină

### Cum să ajungeți la pagina Administrare Personal:
1. **Din meniul principal**: Navigați la `Administrare` → `Administrare Personal`
2. **URL direct**: `https://valyanmed.ro/administrare/personal`
3. **Shortcut keyboard**: `Alt + A` → `P` (dacă sunt activate)

### Permisiuni necesare:
- **Administrator sistem**: Acces complet (CRUD)
- **Manager HR**: Acces la vizualizare și editare
- **Asistent administrativ**: Doar vizualizare și adăugare
- **Utilizator standard**: Fără acces

## 🖥️ Interfața Principală

### Header Pagină
Partea superioară conține:
- **Titlul paginii**: "Administrare Personal" cu iconița 👥
- **Subtitle**: Descrierea scurtă a funcționalității
- **Butoane de acțiune principală**:
  - 🟢 **"Adaugă Personal"** - Pentru adăugarea unui nou angajat
  - 🔄 **"Actualizează"** - Pentru reîncărcarea datelor
  - ⋮ **Meniu kebab** - Pentru opțiuni suplimentare

### Meniul Kebab (⋮)
Când faceți click pe cele trei puncte din dreapta, veți vedea:
- 📊 **Statistici** - Afișează/ascunde cardurile cu statistici
- 🔍 **Filtrare Avansată** - Afișează/ascunde panoul de filtrare

**💡 Tip**: Meniul se închide automat când faceți click în afara lui sau apăsați tasta `Esc`.

## 📊 Statistici (Opțional)

Când activați statisticile din meniul kebab, veți vedea carduri colorate cu informații importante:

### Cardurile de Statistici
- **Total Personal** - Numărul total de angajați înregistrați
- **Personal Activ** - Angajații cu status activ
- **Personal Inactiv** - Angajații cu status inactiv  
- **Departamente** - Numărul de departamente cu personal
- **Adăugări Recente** - Personal adăugat în ultima lună

**🎨 Design**: Fiecare card are o culoare specifică și se animează la hover.

## 🔍 Filtrare Avansată (Opțional)

### Activarea Filtrării
1. Faceți click pe meniul kebab (⋮)
2. Selectați "Filtrare Avansată"
3. Se va deschide panoul cu opțiuni de filtrare

### Opțiuni de Filtrare

#### Prima linie de filtre:
- **🏢 Departament**: 
  - Dropdown cu toate departamentele
  - Opțiuni: Administratie, Financiar, IT, Întreținere, etc.
  - Implicit: "Toate departamentele"

- **📋 Status**: 
  - Dropdown cu statusurile disponibile
  - Opțiuni: Activ, Inactiv
  - Implicit: "Toate statusurile"

- **🔎 Căutare text**:
  - Căutare liberă în nume, prenume, email
  - Placeholder: "Caută în nume, prenume, email..."
  - Are buton de ștergere (X)

#### A doua linie de filtre:
- **📅 Perioada activitate**:
  - Dropdown cu opțiuni predefinite
  - Opțiuni: "Ultima lună", "Ultimele 3 luni", "Ultimul an"
  - Implicit: "Orice perioadă"

### Butoane de Acțiune Filtre
- **✅ Aplică Filtrele** - Activează filtrarea cu criteriile selectate
- **❌ Curăță Filtrele** - Resetează toate filtrele la valorile implicite
- **💾 Exportă Rezultate** - Descarcă rezultatele filtrate în Excel

### Rezumatul Rezultatelor
Sub filtre vedeți:
- **Numărul rezultatelor**: "Rezultate: **15** din **89** angajați"
- **Indicator filtrare activă**: Apare când sunt aplicate filtre

## 📋 Tabelul de Date (DataGrid)

### Structura Coloanelor

| Coloană | Descriere | Lățime | Funcții |
|---------|-----------|--------|---------|
| **Cod** | Codul unic al angajatului | 80px | Sortare, Filtrare |
| **Nume** | Numele de familie | 100px | Sortare, Filtrare |
| **Prenume** | Prenumele | 100px | Sortare, Filtrare |
| **Email** | Adresa de email personală | 160px | Sortare, Filtrare, Click pentru email |
| **Telefon** | Numărul de telefon | 90px | Sortare, Filtrare |
| **Funcția** | Funcția ocupată | 100px | Sortare, Filtrare |
| **Departament** | Departamentul de apartenență | 100px | Sortare, Filtrare, Grupare |
| **Status** | Status activ/inactiv cu badge colorat | 70px | Sortare, Filtrare |
| **Data Creării** | Când a fost adăugat în sistem | 90px | Sortare, Filtrare |
| **Acțiuni** | Butoane pentru operații | 120px | Vezi, Editează, Șterge |

### Funcționalități Grid

#### 🔄 Sortare
- **Click pe header coloană** - Sortare ascendentă
- **Al doilea click** - Sortare descendentă  
- **Al treilea click** - Elimină sortarea
- **Multiple column sort** - Ține `Ctrl` și click pe mai multe coloane

#### 🔍 Filtrare Excel
- **Click pe iconița filter** din header
- **Filtrare automată** pe măsură ce tastați
- **Checkbox selection** pentru valori multiple
- **Custom filters** pentru condiții complexe

#### 📊 Grupare
- **Drag & drop** header de coloană în zona de grupare
- **Implicit grupat** după Departament
- **Expand/collapse** grupuri
- **Multi-level grouping** posibil

#### 📄 Paginare
- **Pagini**: 10, 20, 50, 100 înregistrări per pagină
- **Navigare**: Prima, Anterioară, Următoarea, Ultima
- **Jump to page**: Mergi direct la o pagină specifică

### Acțiuni pe Rânduri

În coloana "Acțiuni" găsiți 3 butoane colorate:

#### 👁️ Vizualizează (Albastru)
- **Funcție**: Deschide modalul cu detaliile complete ale personalului
- **Shortcut**: `Enter` pe rândul selectat
- **Permisiuni**: Toate rolurile

#### ✏️ Editează (Portocaliu)  
- **Funcție**: Deschide modalul pentru editarea informațiilor
- **Shortcut**: `F2` pe rândul selectat
- **Permisiuni**: Administrator, Manager HR

#### 🗑️ Șterge (Roșu)
- **Funcție**: Șterge înregistrarea după confirmare
- **Shortcut**: `Delete` pe rândul selectat  
- **Permisiuni**: Doar Administrator sistem
- **⚠️ Atenție**: Operație ireversibilă!

## ➕ Adăugarea Personalului Nou

### Pasul 1: Deschiderea Modalului
1. Faceți click pe butonul verde **"Adaugă Personal"**
2. Se deschide modalul "Adăugare Personal Nou"
3. **Codul angajat** se generează automat (ex: EMP202412001)

### Pasul 2: Completarea Informațiilor

#### 📋 Secțiunea "Informații Generale"
- **Cod Angajat** (readonly) - Generat automat
- **CNP** - 13 cifre, cu validare în timp real
- **Nume** - Numele de familie (obligatoriu)
- **Prenume** - Prenumele (obligatoriu)
- **Nume Anterior** - Pentru persoanele căsătorite (opțional)

#### 👤 Secțiunea "Informații Personale"  
- **Data Nașterii** - Se calculează automat din CNP
- **Locul Nașterii** - Localitatea de naștere
- **Starea Civilă** - Celibatar(ă), Căsătorit(ă), Divorțat(ă), Văduv(ă)
- **Naționalitatea** - Implicit "Română"
- **Cetățenia** - Implicit "Română"

#### 📞 Secțiunea "Contact"
- **Telefon Personal** - Format: 0XXX-XXX-XXX
- **Telefon Serviciu** - Numărul de la birou (opțional)
- **Email Personal** - Cu validare format email
- **Email Serviciu** - Email-ul de la clinică (opțional)

#### 🏠 Secțiunea "Adresă Domiciliu"
- **Adresa** - Strada, numărul, apartamentul
- **Județul** - Dropdown cu județele României
- **Orașul/Comuna** - Se încarcă automat după selectarea județului
- **Codul Poștal** - 6 cifre

#### 🏢 Secțiunea "Adresă Reședință" (Opțional)
- **Checkbox** "Adresa de reședință diferă de cea de domiciliu"
- **Câmpurile** se activează doar dacă este bifat
- **Același format** ca pentru domiciliu

#### 💼 Secțiunea "Informații Profesionale"
- **Funcția** - Funcția ocupată în clinică (obligatoriu)
- **Departamentul** - Dropdown cu departamentele disponibile
- **Status Angajat** - Activ/Inactiv (implicit Activ)

#### 🆔 Secțiunea "Acte de Identitate"
- **Serie CI** - Seria cărții de identitate
- **Număr CI** - Numărul cărții de identitate  
- **Eliberat de** - Instituția care a eliberat actul
- **Data Eliberării** - Când a fost eliberată CI
- **Valabil până la** - Data expirării

#### 📝 Secțiunea "Observații"
- **Text liber** pentru note suplimentare
- **Maxim 1000 caractere**

### Pasul 3: Validarea și Salvarea
1. Completați câmpurile obligatorii (marcate cu *)
2. Sistemul validează automat în timp real
3. Click pe **"Adaugă Personal"** pentru salvare
4. Sau **"Anulează"** pentru a închide fără salvare

### 🎯 Validări Importante

#### CNP Validation
- **Format**: Exact 13 cifre
- **Cifra de control**: Validare cu algoritmul oficial românesc
- **Data nașterii**: Se calculează și validează automat
- **Vârstă**: Trebuie să fie între 16-80 ani pentru angajați
- **Feedback vizual**: Verde pentru valid, roșu pentru invalid

#### Email Validation
- **Format standard**: nume@domeniu.com
- **Unicitate**: Nu poate exista același email pentru 2 persoane
- **Validare server**: Verificare suplimentară pe server

#### Telefon Validation  
- **Format românesc**: 07XX-XXX-XXX sau 02XX-XXX-XXX
- **Lungime**: Exact 10 cifre
- **Prefix valid**: Prefixe românești acceptate

## ✏️ Editarea Personalului

### Accesul la Editare
1. **Din grid**: Click pe butonul portocaliu ✏️ "Editează"
2. **Din modal vizualizare**: Click pe "Editează Personal"
3. Se deschide același modal ca la adăugare, dar pre-populat

### Diferențe față de Adăugare
- **Codul angajat** rămâne readonly (nu se poate modifica)
- **CNP-ul** se poate modifica doar de Admin sistem
- **Data creării** se păstrează din înregistrarea originală
- **Audit trail** - se înregistrează cine și când a modificat

### Salvarea Modificărilor
1. Modificați câmpurile dorite
2. Click pe **"Actualizează Personal"**
3. Se salvează cu versioning pentru audit
4. Notificare de succes prin toast

## 👁️ Vizualizarea Detaliilor

### Accesul la Vizualizare
1. Click pe butonul albastru 👁️ "Vizualizează" din grid
2. Se deschide un modal cu dashboard profesional

### Layout-ul Dashboard-ului

#### Header Modal
- **Iconița utilizator** 👤
- **Numele complet** ca titlu principal
- **"Detalii personal"** ca subtitlu

#### Organizarea în Carduri

##### 📋 Card "Informații Generale"
- Cod angajat, CNP, Nume complet
- Data nașterii cu vârsta calculată
- Locul nașterii, starea civilă
- **Design**: Gradient albastru în header

##### 📞 Card "Informații Contact" 
- Telefon personal (cu link pentru apelare)
- Telefon serviciu
- Email personal (cu link pentru trimitere email)
- Email serviciu
- **Design**: Gradient verde în header

##### 🏠 Card "Adresă și Locuire"
- Adresa completă de domiciliu
- Județ, oraș, cod poștal
- **Separator vizual** dacă există și adresa de reședință
- **Design**: Gradient portocaliu în header

##### 💼 Card "Informații Profesionale"
- Funcția ocupată
- Departamentul de apartenență  
- Status angajat (cu badge colorat)
- Data creării înregistrării
- **Design**: Gradient purple în header

##### 🆔 Card "Acte de Identitate"
- Informații complete carte identitate
- **Badge-uri de validitate**:
  - 🟢 Valid - CI în regulă
  - 🟡 Expiră în curând - Sub 30 zile
  - 🔴 Expirat - Trebuie reînnoit
- **Design**: Gradient teal în header

##### 📝 Card "Observații" (dacă există)
- Text formatat pentru observații
- **Design**: Gradient roz în header
- **Full width** - ocupă toată lățimea

### Acțiuni din Modal Vizualizare
- **Editează Personal** - Deschide direct modalul de editare
- **Închide** - Închide modalul și revine la grid

### 🎨 Funcții Vizuale
- **Animații hover** pe carduri
- **Loading states** pentru încărcare
- **Error states** pentru erori
- **Responsive design** - se adaptează pe mobile
- **Print friendly** - poate fi printat

## 🔄 Actualizarea Datelor

### Butonul "Actualizează"
- **Locație**: În header-ul paginii  
- **Funcție**: Reîncarcă toate datele din baza de date
- **Cuando usar**: Când suspectați că datele au fost modificate de alții
- **Feedback**: Toast notification cu rezultatul

### Auto-refresh
- **La 5 minute**: Verificare automată pentru modificări
- **La focus**: Când reveniți la pagină după ce ați fost pe alta
- **După salvare**: Automat după operații CRUD

## 🗑️ Ștergerea Personalului

### ⚠️ Atenție Importantă
Ștergerea personalului este o operație **ireversibilă** și trebuie făcută cu mare atenție!

### Procesul de Ștergere
1. Click pe butonul roșu 🗑️ "Șterge" din grid
2. Apare dialog de confirmare JavaScript:
   ```
   Sigur doriți să ștergeți personalul [Nume Prenume]?
   ```
3. **Da** - Confirmă ștergerea
4. **Nu/Anulare** - Anulează operația

### Restricții Ștergere
- **Permisiuni**: Doar Administrator sistem
- **Validări business**: 
  - Nu se poate șterge personal cu dosare active
  - Nu se poate șterge personal cu contracte în derulare
  - Nu se poate șterge personal cu tranzacții financiare

### Ce se întâmplă la ștergere
1. **Verificare dependințe** - Se verifică dacă există legături cu alte entități
2. **Soft delete** - Înregistrarea se marchează ca ștearsă, nu se elimină fizic
3. **Audit logging** - Se înregistrează cine, când și de ce a șters
4. **Notificare** - Toast cu confirmarea ștergerii

## 📱 Responsive Design

### 🖥️ Desktop (1200px+)
- **Layout complet** cu toate coloanele vizibile
- **Grid mare** cu 10+ înregistrări per pagină
- **Toate funcțiile** disponibile
- **Modale mari** (900px lățime)

### 💻 Tablet (768px - 1199px)  
- **Coloane optimizate** - unele se ascund pe lățimi mici
- **Touch-friendly buttons** - butoane mai mari
- **Modale medii** (80% din ecran)
- **Grid responsive** cu scroll orizontal dacă e necesar

### 📱 Mobile (320px - 767px)
- **Layout vertical** pentru header
- **Coloane minimale** în grid
- **Modale full-screen** pentru formulare
- **Touch gestures** pentru navigare
- **Meniul kebab** se adaptează pentru touch

## 🚀 Performance și Optimizări

### Încărcare Rapidă
- **Lazy loading** pentru imagini și componente mari
- **Pagination** pentru volume mari de date  
- **Caching** pentru dropdown-uri și lookup-uri
- **Compression** pentru datele transferate

### Memory Management
- **Proper disposal** pentru toate componentele
- **Event listener cleanup** pentru preventDefault memory leaks
- **State clearing** la navigare între pagini

### User Experience
- **Loading indicators** pentru operații lungi
- **Progress bars** pentru upload/download
- **Debounced search** pentru căutarea în timp real
- **Keyboard shortcuts** pentru utilizatori avansați

## 🆘 Depanarea Problemelor

### Probleme Frecvente

#### Nu se încarcă datele
**Cauze posibile**:
- Probleme de conectivitate la server
- Session expirat
- Permisiuni insuficiente

**Soluții**:
1. Reîmprospătați pagina (`Ctrl + F5`)
2. Verificați conexiunea la internet
3. Reconectați-vă în aplicație
4. Contactați administratorul pentru permisiuni

#### CNP-ul nu se validează corect
**Cauze posibile**:
- CNP invalid sau cu greșeli de tipărire
- Algoritmul de validare nu recunoaște formatul
- Date de naștere inconsistente

**Soluții**:
1. Verificați din nou cifrele CNP-ului
2. Contactați persoana pentru confirmarea CNP-ului
3. Folosiți un validator extern pentru verificare
4. Raportați problema la echipa tehnică

#### Modalul nu se deschide
**Cauze posibile**:
- JavaScript dezactivat în browser
- Pop-up blocker activ
- Probleme cu componentele Syncfusion

**Soluții**:
1. Activați JavaScript în browser
2. Dezactivați pop-up blocker pentru site
3. Ștergeți cache-ul browserului
4. Încercați un browser diferit

#### Filtrarea nu funcționează
**Cauze posibile**:
- Date corupte în cache
- Probleme cu query-urile pe server
- Volume foarte mari de date

**Soluții**:
1. Curățați toate filtrele și aplicați din nou
2. Reîmprospătați pagina
3. Reduceți numărul de criterii de filtrare
4. Contactați suportul tehnic

### Contacte Suport

#### 🔧 Suport Tehnic
- **Email**: suport@valyanmed.ro
- **Telefon**: +40 373 XXX XXX
- **Program**: Luni-Vineri, 08:00-18:00
- **Chat**: Butonul "Ajutor" din aplicație

#### 📚 Resurse Suplimentare
- **Video tutorials**: https://help.valyanmed.ro/videos
- **FAQ**: https://help.valyanmed.ro/faq  
- **Knowledge base**: https://help.valyanmed.ro/kb
- **Community forum**: https://community.valyanmed.ro

---

**💡 Sfat Final**: Pentru o experiență optimă, recomandăm folosirea browserelor moderne (Chrome 100+, Firefox 95+, Edge 90+) și o conexiune stabilă la internet.

**📖 Documentația** se actualizează constant. Pentru cea mai recentă versiune, consultați help-ul online din aplicație.

**Versiune ghid**: 2.0  
**Data actualizării**: Decembrie 2024  
**Autori**: Echipa ValyanMed UX/UI
