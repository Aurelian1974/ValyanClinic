# Ghid Utilizator - Administrare Personal

## 🏢 Prezentare Generala

Pagina **Administrare Personal** va permite sa gestionati intregul personal non-medical al clinicii intr-un mod eficient si organizat. De aici puteti adauga, modifica, vizualiza si cauta informatii despre angajatii din departamentele administrative, financiare, IT, intretinere si altele.

## 📍 Acces la Pagina

### Cum sa ajungeti la pagina Administrare Personal:
1. **Din meniul principal**: Navigati la `Administrare` → `Administrare Personal`
2. **URL direct**: `https://valyanmed.ro/administrare/personal`
3. **Shortcut keyboard**: `Alt + A` → `P` (daca sunt activate)

### Permisiuni necesare:
- **Administrator sistem**: Acces complet (CRUD)
- **Manager HR**: Acces la vizualizare si editare
- **Asistent administrativ**: Doar vizualizare si adaugare
- **Utilizator standard**: Fara acces

## 🖥️ Interfata Principala

### Header Pagina
Partea superioara contine:
- **Titlul paginii**: "Administrare Personal" cu iconita 👥
- **Subtitle**: Descrierea scurta a functionalitatii
- **Butoane de actiune principala**:
  - 🟢 **"Adauga Personal"** - Pentru adaugarea unui nou angajat
  - 🔄 **"Actualizeaza"** - Pentru reincarcarea datelor
  - ⋮ **Meniu kebab** - Pentru optiuni suplimentare

### Meniul Kebab (⋮)
Cand faceti click pe cele trei puncte din dreapta, veti vedea:
- 📊 **Statistici** - Afiseaza/ascunde cardurile cu statistici
- 🔍 **Filtrare Avansata** - Afiseaza/ascunde panoul de filtrare

**💡 Tip**: Meniul se inchide automat cand faceti click in afara lui sau apasati tasta `Esc`.

## 📊 Statistici (Optional)

Cand activati statisticile din meniul kebab, veti vedea carduri colorate cu informatii importante:

### Cardurile de Statistici
- **Total Personal** - Numarul total de angajati inregistrati
- **Personal Activ** - Angajatii cu status activ
- **Personal Inactiv** - Angajatii cu status inactiv  
- **Departamente** - Numarul de departamente cu personal
- **Adaugari Recente** - Personal adaugat in ultima luna

**🎨 Design**: Fiecare card are o culoare specifica si se animeaza la hover.

## 🔍 Filtrare Avansata (Optional)

### Activarea Filtrarii
1. Faceti click pe meniul kebab (⋮)
2. Selectati "Filtrare Avansata"
3. Se va deschide panoul cu optiuni de filtrare

### Optiuni de Filtrare

#### Prima linie de filtre:
- **🏢 Departament**: 
  - Dropdown cu toate departamentele
  - Optiuni: Administratie, Financiar, IT, intretinere, etc.
  - Implicit: "Toate departamentele"

- **📋 Status**: 
  - Dropdown cu statusurile disponibile
  - Optiuni: Activ, Inactiv
  - Implicit: "Toate statusurile"

- **🔎 Cautare text**:
  - Cautare libera in nume, prenume, email
  - Placeholder: "Cauta in nume, prenume, email..."
  - Are buton de stergere (X)

#### A doua linie de filtre:
- **📅 Perioada activitate**:
  - Dropdown cu optiuni predefinite
  - Optiuni: "Ultima luna", "Ultimele 3 luni", "Ultimul an"
  - Implicit: "Orice perioada"

### Butoane de Actiune Filtre
- **✅ Aplica Filtrele** - Activeaza filtrarea cu criteriile selectate
- **❌ Curata Filtrele** - Reseteaza toate filtrele la valorile implicite
- **💾 Exporta Rezultate** - Descarca rezultatele filtrate in Excel

### Rezumatul Rezultatelor
Sub filtre vedeti:
- **Numarul rezultatelor**: "Rezultate: **15** din **89** angajati"
- **Indicator filtrare activa**: Apare cand sunt aplicate filtre

## 📋 Tabelul de Date (DataGrid)

### Structura Coloanelor

| Coloana | Descriere | Latime | Functii |
|---------|-----------|--------|---------|
| **Cod** | Codul unic al angajatului | 80px | Sortare, Filtrare |
| **Nume** | Numele de familie | 100px | Sortare, Filtrare |
| **Prenume** | Prenumele | 100px | Sortare, Filtrare |
| **Email** | Adresa de email personala | 160px | Sortare, Filtrare, Click pentru email |
| **Telefon** | Numarul de telefon | 90px | Sortare, Filtrare |
| **Functia** | Functia ocupata | 100px | Sortare, Filtrare |
| **Departament** | Departamentul de apartenenta | 100px | Sortare, Filtrare, Grupare |
| **Status** | Status activ/inactiv cu badge colorat | 70px | Sortare, Filtrare |
| **Data Crearii** | Cand a fost adaugat in sistem | 90px | Sortare, Filtrare |
| **Actiuni** | Butoane pentru operatii | 120px | Vezi, Editeaza, sterge |

### Functionalitati Grid

#### 🔄 Sortare
- **Click pe header coloana** - Sortare ascendenta
- **Al doilea click** - Sortare descendenta  
- **Al treilea click** - Elimina sortarea
- **Multiple column sort** - tine `Ctrl` si click pe mai multe coloane

#### 🔍 Filtrare Excel
- **Click pe iconita filter** din header
- **Filtrare automata** pe masura ce tastati
- **Checkbox selection** pentru valori multiple
- **Custom filters** pentru conditii complexe

#### 📊 Grupare
- **Drag & drop** header de coloana in zona de grupare
- **Implicit grupat** dupa Departament
- **Expand/collapse** grupuri
- **Multi-level grouping** posibil

#### 📄 Paginare
- **Pagini**: 10, 20, 50, 100 inregistrari per pagina
- **Navigare**: Prima, Anterioara, Urmatoarea, Ultima
- **Jump to page**: Mergi direct la o pagina specifica

### Actiuni pe Randuri

in coloana "Actiuni" gasiti 3 butoane colorate:

#### 👁️ Vizualizeaza (Albastru)
- **Functie**: Deschide modalul cu detaliile complete ale personalului
- **Shortcut**: `Enter` pe randul selectat
- **Permisiuni**: Toate rolurile

#### ✏️ Editeaza (Portocaliu)  
- **Functie**: Deschide modalul pentru editarea informatiilor
- **Shortcut**: `F2` pe randul selectat
- **Permisiuni**: Administrator, Manager HR

#### 🗑️ sterge (Rosu)
- **Functie**: sterge inregistrarea dupa confirmare
- **Shortcut**: `Delete` pe randul selectat  
- **Permisiuni**: Doar Administrator sistem
- **⚠️ Atentie**: Operatie ireversibila!

## ➕ Adaugarea Personalului Nou

### Pasul 1: Deschiderea Modalului
1. Faceti click pe butonul verde **"Adauga Personal"**
2. Se deschide modalul "Adaugare Personal Nou"
3. **Codul angajat** se genereaza automat (ex: EMP202412001)

### Pasul 2: Completarea Informatiilor

#### 📋 Sectiunea "Informatii Generale"
- **Cod Angajat** (readonly) - Generat automat
- **CNP** - 13 cifre, cu validare in timp real
- **Nume** - Numele de familie (obligatoriu)
- **Prenume** - Prenumele (obligatoriu)
- **Nume Anterior** - Pentru persoanele casatorite (optional)

#### 👤 Sectiunea "Informatii Personale"  
- **Data Nasterii** - Se calculeaza automat din CNP
- **Locul Nasterii** - Localitatea de nastere
- **Starea Civila** - Celibatar(a), Casatorit(a), Divortat(a), Vaduv(a)
- **Nationalitatea** - Implicit "Romana"
- **Cetatenia** - Implicit "Romana"

#### 📞 Sectiunea "Contact"
- **Telefon Personal** - Format: 0XXX-XXX-XXX
- **Telefon Serviciu** - Numarul de la birou (optional)
- **Email Personal** - Cu validare format email
- **Email Serviciu** - Email-ul de la clinica (optional)

#### 🏠 Sectiunea "Adresa Domiciliu"
- **Adresa** - Strada, numarul, apartamentul
- **Judetul** - Dropdown cu judetele Romaniei
- **Orasul/Comuna** - Se incarca automat dupa selectarea judetului
- **Codul Postal** - 6 cifre

#### 🏢 Sectiunea "Adresa Resedinta" (Optional)
- **Checkbox** "Adresa de resedinta difera de cea de domiciliu"
- **Campurile** se activeaza doar daca este bifat
- **Acelasi format** ca pentru domiciliu

#### 💼 Sectiunea "Informatii Profesionale"
- **Functia** - Functia ocupata in clinica (obligatoriu)
- **Departamentul** - Dropdown cu departamentele disponibile
- **Status Angajat** - Activ/Inactiv (implicit Activ)

#### 🆔 Sectiunea "Acte de Identitate"
- **Serie CI** - Seria cartii de identitate
- **Numar CI** - Numarul cartii de identitate  
- **Eliberat de** - Institutia care a eliberat actul
- **Data Eliberarii** - Cand a fost eliberata CI
- **Valabil pana la** - Data expirarii

#### 📝 Sectiunea "Observatii"
- **Text liber** pentru note suplimentare
- **Maxim 1000 caractere**

### Pasul 3: Validarea si Salvarea
1. Completati campurile obligatorii (marcate cu *)
2. Sistemul valideaza automat in timp real
3. Click pe **"Adauga Personal"** pentru salvare
4. Sau **"Anuleaza"** pentru a inchide fara salvare

### 🎯 Validari Importante

#### CNP Validation
- **Format**: Exact 13 cifre
- **Cifra de control**: Validare cu algoritmul oficial romanesc
- **Data nasterii**: Se calculeaza si valideaza automat
- **Varsta**: Trebuie sa fie intre 16-80 ani pentru angajati
- **Feedback vizual**: Verde pentru valid, rosu pentru invalid

#### Email Validation
- **Format standard**: nume@domeniu.com
- **Unicitate**: Nu poate exista acelasi email pentru 2 persoane
- **Validare server**: Verificare suplimentara pe server

#### Telefon Validation  
- **Format romanesc**: 07XX-XXX-XXX sau 02XX-XXX-XXX
- **Lungime**: Exact 10 cifre
- **Prefix valid**: Prefixe romanesti acceptate

## ✏️ Editarea Personalului

### Accesul la Editare
1. **Din grid**: Click pe butonul portocaliu ✏️ "Editeaza"
2. **Din modal vizualizare**: Click pe "Editeaza Personal"
3. Se deschide acelasi modal ca la adaugare, dar pre-populat

### Diferente fata de Adaugare
- **Codul angajat** ramane readonly (nu se poate modifica)
- **CNP-ul** se poate modifica doar de Admin sistem
- **Data crearii** se pastreaza din inregistrarea originala
- **Audit trail** - se inregistreaza cine si cand a modificat

### Salvarea Modificarilor
1. Modificati campurile dorite
2. Click pe **"Actualizeaza Personal"**
3. Se salveaza cu versioning pentru audit
4. Notificare de succes prin toast

## 👁️ Vizualizarea Detaliilor

### Accesul la Vizualizare
1. Click pe butonul albastru 👁️ "Vizualizeaza" din grid
2. Se deschide un modal cu dashboard profesional

### Layout-ul Dashboard-ului

#### Header Modal
- **Iconita utilizator** 👤
- **Numele complet** ca titlu principal
- **"Detalii personal"** ca subtitlu

#### Organizarea in Carduri

##### 📋 Card "Informatii Generale"
- Cod angajat, CNP, Nume complet
- Data nasterii cu varsta calculata
- Locul nasterii, starea civila
- **Design**: Gradient albastru in header

##### 📞 Card "Informatii Contact" 
- Telefon personal (cu link pentru apelare)
- Telefon serviciu
- Email personal (cu link pentru trimitere email)
- Email serviciu
- **Design**: Gradient verde in header

##### 🏠 Card "Adresa si Locuire"
- Adresa completa de domiciliu
- Judet, oras, cod postal
- **Separator vizual** daca exista si adresa de resedinta
- **Design**: Gradient portocaliu in header

##### 💼 Card "Informatii Profesionale"
- Functia ocupata
- Departamentul de apartenenta  
- Status angajat (cu badge colorat)
- Data crearii inregistrarii
- **Design**: Gradient purple in header

##### 🆔 Card "Acte de Identitate"
- Informatii complete carte identitate
- **Badge-uri de validitate**:
  - 🟢 Valid - CI in regula
  - 🟡 Expira in curand - Sub 30 zile
  - 🔴 Expirat - Trebuie reinnoit
- **Design**: Gradient teal in header

##### 📝 Card "Observatii" (daca exista)
- Text formatat pentru observatii
- **Design**: Gradient roz in header
- **Full width** - ocupa toata latimea

### Actiuni din Modal Vizualizare
- **Editeaza Personal** - Deschide direct modalul de editare
- **inchide** - inchide modalul si revine la grid

### 🎨 Functii Vizuale
- **Animatii hover** pe carduri
- **Loading states** pentru incarcare
- **Error states** pentru erori
- **Responsive design** - se adapteaza pe mobile
- **Print friendly** - poate fi printat

## 🔄 Actualizarea Datelor

### Butonul "Actualizeaza"
- **Locatie**: in header-ul paginii  
- **Functie**: Reincarca toate datele din baza de date
- **Cuando usar**: Cand suspectati ca datele au fost modificate de altii
- **Feedback**: Toast notification cu rezultatul

### Auto-refresh
- **La 5 minute**: Verificare automata pentru modificari
- **La focus**: Cand reveniti la pagina dupa ce ati fost pe alta
- **Dupa salvare**: Automat dupa operatii CRUD

## 🗑️ stergerea Personalului

### ⚠️ Atentie Importanta
stergerea personalului este o operatie **ireversibila** si trebuie facuta cu mare atentie!

### Procesul de stergere
1. Click pe butonul rosu 🗑️ "sterge" din grid
2. Apare dialog de confirmare JavaScript:
   ```
   Sigur doriti sa stergeti personalul [Nume Prenume]?
   ```
3. **Da** - Confirma stergerea
4. **Nu/Anulare** - Anuleaza operatia

### Restrictii stergere
- **Permisiuni**: Doar Administrator sistem
- **Validari business**: 
  - Nu se poate sterge personal cu dosare active
  - Nu se poate sterge personal cu contracte in derulare
  - Nu se poate sterge personal cu tranzactii financiare

### Ce se intampla la stergere
1. **Verificare dependinte** - Se verifica daca exista legaturi cu alte entitati
2. **Soft delete** - inregistrarea se marcheaza ca stearsa, nu se elimina fizic
3. **Audit logging** - Se inregistreaza cine, cand si de ce a sters
4. **Notificare** - Toast cu confirmarea stergerii

## 📱 Responsive Design

### 🖥️ Desktop (1200px+)
- **Layout complet** cu toate coloanele vizibile
- **Grid mare** cu 10+ inregistrari per pagina
- **Toate functiile** disponibile
- **Modale mari** (900px latime)

### 💻 Tablet (768px - 1199px)  
- **Coloane optimizate** - unele se ascund pe latimi mici
- **Touch-friendly buttons** - butoane mai mari
- **Modale medii** (80% din ecran)
- **Grid responsive** cu scroll orizontal daca e necesar

### 📱 Mobile (320px - 767px)
- **Layout vertical** pentru header
- **Coloane minimale** in grid
- **Modale full-screen** pentru formulare
- **Touch gestures** pentru navigare
- **Meniul kebab** se adapteaza pentru touch

## 🚀 Performance si Optimizari

### incarcare Rapida
- **Lazy loading** pentru imagini si componente mari
- **Pagination** pentru volume mari de date  
- **Caching** pentru dropdown-uri si lookup-uri
- **Compression** pentru datele transferate

### Memory Management
- **Proper disposal** pentru toate componentele
- **Event listener cleanup** pentru preventDefault memory leaks
- **State clearing** la navigare intre pagini

### User Experience
- **Loading indicators** pentru operatii lungi
- **Progress bars** pentru upload/download
- **Debounced search** pentru cautarea in timp real
- **Keyboard shortcuts** pentru utilizatori avansati

## 🆘 Depanarea Problemelor

### Probleme Frecvente

#### Nu se incarca datele
**Cauze posibile**:
- Probleme de conectivitate la server
- Session expirat
- Permisiuni insuficiente

**Solutii**:
1. Reimprospatati pagina (`Ctrl + F5`)
2. Verificati conexiunea la internet
3. Reconectati-va in aplicatie
4. Contactati administratorul pentru permisiuni

#### CNP-ul nu se valideaza corect
**Cauze posibile**:
- CNP invalid sau cu greseli de tiparire
- Algoritmul de validare nu recunoaste formatul
- Date de nastere inconsistente

**Solutii**:
1. Verificati din nou cifrele CNP-ului
2. Contactati persoana pentru confirmarea CNP-ului
3. Folositi un validator extern pentru verificare
4. Raportati problema la echipa tehnica

#### Modalul nu se deschide
**Cauze posibile**:
- JavaScript dezactivat in browser
- Pop-up blocker activ
- Probleme cu componentele Syncfusion

**Solutii**:
1. Activati JavaScript in browser
2. Dezactivati pop-up blocker pentru site
3. stergeti cache-ul browserului
4. incercati un browser diferit

#### Filtrarea nu functioneaza
**Cauze posibile**:
- Date corupte in cache
- Probleme cu query-urile pe server
- Volume foarte mari de date

**Solutii**:
1. Curatati toate filtrele si aplicati din nou
2. Reimprospatati pagina
3. Reduceti numarul de criterii de filtrare
4. Contactati suportul tehnic

### Contacte Suport

#### 🔧 Suport Tehnic
- **Email**: suport@valyanmed.ro
- **Telefon**: +40 373 XXX XXX
- **Program**: Luni-Vineri, 08:00-18:00
- **Chat**: Butonul "Ajutor" din aplicatie

#### 📚 Resurse Suplimentare
- **Video tutorials**: https://help.valyanmed.ro/videos
- **FAQ**: https://help.valyanmed.ro/faq  
- **Knowledge base**: https://help.valyanmed.ro/kb
- **Community forum**: https://community.valyanmed.ro

---

**💡 Sfat Final**: Pentru o experienta optima, recomandam folosirea browserelor moderne (Chrome 100+, Firefox 95+, Edge 90+) si o conexiune stabila la internet.

**📖 Documentatia** se actualizeaza constant. Pentru cea mai recenta versiune, consultati help-ul online din aplicatie.

**Versiune ghid**: 2.0  
**Data actualizarii**: Decembrie 2024  
**Autori**: Echipa ValyanMed UX/UI
