# Ghid Utilizator - Gestionarea Utilizatorilor

**Aplicație:** ValyanMed - Sistem de Management Clinic  
**Modul:** Gestionare Utilizatori  
**Creat:** Septembrie 2025  
**Actualizat:** Septembrie 2025  
**Destinat pentru:** Administratori, Manageri, Personal cu drepturi de gestionare utilizatori  

---

## Prezentare Generală

Modulul de Gestionare Utilizatori vă permite să administrați conturile personalului din clinică. Puteți vizualiza, adăuga, modifica și gestiona utilizatorii sistemului ValyanMed într-un mod simplu și eficient.

### Ce puteți face în acest modul
- **Vizualizarea listei** complete de utilizatori
- **Căutarea și filtrarea** utilizatorilor după diverse criterii
- **Adăugarea** de utilizatori noi în sistem
- **Modificarea informațiilor** utilizatorilor existenți
- **Vizualizarea detaliilor** complete ale unui utilizator
- **Gestionarea rolurilor** și permisiunilor
- **Monitorizarea activității** utilizatorilor

---

## Accesarea Modulului

### Cum să ajungeți la Gestionarea Utilizatorilor

1. **Conectați-vă** în aplicația ValyanMed
2. **Din meniul principal**, căutați secțiunea "Utilizatori"
3. **Apăsați pe "Gestionare Utilizatori"**
4. Se va deschide pagina cu lista utilizatorilor

### Verificarea permisiunilor
Pentru a accesa acest modul, trebuie să aveți unul dintre rolurile:
- ✅ **Administrator** - Acces complet la toate funcțiile
- ✅ **Manager** - Poate vizualiza și modifica utilizatorii din departamentul său
- ❌ **Utilizator standard** - Nu are acces la acest modul

---

## Înțelegerea Interfeței

### Antetul paginii

#### Titlul și descrierea
- **"Gestionare Utilizatori"** - Titlul principal al paginii
- **Descrierea** explică scopul paginii: "Administrează utilizatorii sistemului ValyanMed"

#### Butoanele de acțiune
- **"Adaugă Utilizator"** 🆕 - Pentru crearea unui utilizator nou
- **"Actualizează"** 🔄 - Pentru reîncărcarea listei de utilizatori

### Statisticile utilizatorilor

În partea de sus a paginii veți vedea **8 carduri cu statistici**:

| Statistica | Ce înseamnă |
|------------|-------------|
| **Total Utilizatori** | Numărul total de conturi din sistem |
| **Utilizatori Activi** | Conturi care sunt în prezent active |
| **Medici** | Numărul de utilizatori cu rolul de Medic |
| **Asistente Medicale** | Numărul de asistente înregistrate |
| **Administratori** | Numărul de administratori de sistem |
| **Personal Inactiv** | Conturi dezactivate temporar |
| **Conectări Astăzi** | Câți utilizatori s-au conectat astăzi |
| **Utilizatori Noi** | Conturi create în ultima săptămână |

---

## Sistemul de Filtrare Avansată

### Activarea filtrelor

1. **Căutați panoul "Filtrare Avansată"** sub statistici
2. **Apăsați pe "Arată Filtrele"** pentru a deschide opțiunile
3. **Panoul se va extinde** și veți vedea toate opțiunile de filtrare

### Tipurile de filtre disponibile

#### 🔍 Căutare text globală
- **Căutați după**: Nume, prenume, email, username
- **Introduceți textul** în câmpul "Caută în nume, email, username..."
- **Rezultatele** se actualizează automat în timp real

#### 👤 Filtru după rol
- **Alegeți din listă**: Toate rolurile, Administrator, Medic, Asistent Medical, etc.
- **Selectați "Toate rolurile"** pentru a elimina filtrul

#### ✅ Filtru după status
- **Opțiuni disponibile**: Activ, Inactiv, Suspendat, Blocat
- **Selectați "Toate statusurile"** pentru a elimina filtrul

#### 🏢 Filtru după departament
- **Alegeți departamentul**: Cardiologie, Chirurgie, Radiologie, etc.
- **Lista se actualizează** cu departamentele active din clinică

#### ⏰ Filtru după perioada de activitate
- **Opțiuni temporale**: Astăzi, Săptămâna trecută, Luna trecută, etc.
- **Filtrează utilizatorii** după ultima lor conectare

### Aplicarea filtrelor

1. **Setați filtrele dorite** folosind dropdown-urile
2. **Apăsați "Aplică Filtrele"** pentru a activa filtrarea
3. **Rezultatele** se vor actualiza în tabelul de utilizatori
4. **Veți vedea** "Rezultate: X din Y utilizatori" pentru feedback

### Curățarea filtrelor

- **Apăsați "Curăță Filtrele"** pentru a elimina toate filtrele
- **Sau selectați "Toate..."** în fiecare dropdown individual

---

## Lucrul cu Tabelul de Utilizatori

### Coloanele din tabel

| Coloană | Ce afișează | Funcții speciale |
|---------|-------------|------------------|
| **ID** | Numărul unic al utilizatorului | Nu se poate modifica |
| **Nume** | Numele de familie | Se poate sorta și filtra |
| **Prenume** | Prenumele utilizatorului | Se poate sorta și filtra |
| **Email** | Adresa de email | Link pentru a trimite email |
| **Username** | Numele de utilizator pentru conectare | Se poate căuta |
| **Telefon** | Numărul de telefon | Format românesc |
| **Rol** | Rolul în sistem | Cu badge colorat |
| **Departament** | Departamentul de apartenență | Se poate grupa |
| **Status** | Starea contului | Cu indicator colorat |
| **Funcția** | Postul ocupat în clinică | Text liber |
| **Data Creării** | Când a fost creat contul | Format dd.mm.yyyy |
| **Ultima Autent.** | Ultima conectare | Relativ (ex: "2 zile") |
| **Acțiuni** | Butoane pentru acțiuni | Nu se poate sorta |

### Funcții avansate ale tabelului

#### 📊 Sortarea datelor
- **Apăsați pe antetul coloanei** pentru sortare crescătoare
- **Apăsați din nou** pentru sortare descrescătoare
- **A treia apăsare** elimină sortarea

#### 🔍 Filtrarea coloanelor
- **Fiecare coloană** are propriul filtru în antet
- **Introduceți textul** pentru filtrare rapidă
- **Filtrele Excel** sunt disponibile pentru filtrare avansată

#### 📑 Gruparea datelor
- **Trageți antetul coloanei** în zona "Grupează aici"
- **Datele se vor grupa** după coloana selectată
- **Puteți grupa după multiple coloane** simultan

#### 🔄 Reordonarea coloanelor
- **Trageți antetul coloanei** la poziția dorită
- **Ordinea se va salva** pentru sesiunile viitoare

#### 📏 Redimensionarea coloanelor
- **Trageți marginea** coloanei pentru redimensionare
- **Dublu-click pe margine** pentru redimensionare automată

### Paginarea rezultatelor

- **Sus-dreapta tabelului**: Puteți alege numărul de înregistrări pe pagină
- **Opțiuni disponibile**: 10, 20, 50, 100, Toate
- **Navigarea**: Folosiți butoanele < > pentru schimbarea paginilor

---

## Vizualizarea Detaliilor unui Utilizator

### Deschiderea ferestrei de detalii

1. **În coloana "Acțiuni"** căutați butonul albastru cu iconița ochiului 👁️
2. **Apăsați pe acest buton** pentru utilizatorul dorit
3. **Se va deschide o fereastră modală** cu toate detaliile

### Ce veți vedea în fereastra de detalii

#### 📋 Card "Informații Personale"
- **Nume și prenume** complet
- **Adresa de email** de contact
- **Numărul de telefon** (dacă este specificat)

#### 👤 Card "Informații Cont"
- **Username-ul** pentru conectare
- **ID-ul unic** în sistem
- **Rolul** în sistem cu badge colorat
- **Statusul** contului cu indicator vizual

#### 🏢 Card "Informații Organizaționale"
- **Departamentul** de apartenență
- **Funcția** ocupată în clinică

#### ⏰ Card "Informații Temporale"
- **Data creării** contului
- **Ultima autentificare** în sistem
- **Statutul activității** recente (ex: "Activ astăzi")
- **Vechimea în sistem** calculată automat

#### 🛡️ Card "Permisiuni și Securitate"
- **Lista completă** a permisiunilor utilizatorului
- **Permisiuni universale**: Accesul de bază la sistem
- **Permisiuni pe rol**: Specifice funcției (ex: Administrare pentru Admin)
- **Permisiuni medicale**: Pentru personal medical

### Închiderea ferestrei

- **Apăsați pe X** din colțul din dreapta-sus
- **Apăsați "Închide"** din josul ferestrei
- **Apăsați tasta Escape** de pe tastatură

---

## Adăugarea unui Utilizator Nou

### Pornirea procesului de adăugare

1. **În antetul paginii** apăsați pe "Adaugă Utilizator"
2. **Se va deschide** o fereastră pentru introducerea datelor
3. **Titlul ferestrei** va fi "Adaugă Utilizator Nou"

### Completarea formularului

#### 📋 Secțiunea "Informații Personale"

**Câmpurile obligatorii** (marcate cu *):
- **Nume*** - Numele de familie (ex: Popescu)
- **Prenume*** - Prenumele (ex: Maria)  
- **Email*** - Adresa de email (ex: maria.popescu@valyanmed.ro)

**Câmpurile opționale**:
- **Telefon** - Numărul de telefon (ex: 0723456789)

#### 👤 Secțiunea "Informații Cont"

**Câmpurile obligatorii**:
- **Username*** - Numele pentru conectare (ex: maria.popescu)
- **Rol în Sistem*** - Alegeți din listă: Administrator, Medic, Asistent Medical, etc.

**Câmpurile opționale**:
- **Status** - În mod normal rămâne "Activ"

#### 🏢 Secțiunea "Informații Organizaționale"

**Câmpurile opționale**:
- **Departament** - Alegeți din lista departamentelor active
- **Funcția** - Descrierea postului (ex: "Medic Specialist Cardiologie")

### Validarea datelor

#### Mesaje de validare pe măsură ce completați:
- **Nume și prenume**: Trebuie să aibă între 2-50 caractere
- **Email**: Trebuie să aibă format valid (ceva@domeniu.ro)
- **Username**: Nu poate fi deja folosit de alt utilizator
- **Telefon**: Trebuie să aibă format românesc valid

#### Mesaje de eroare comune:
- ❌ **"Acest username este deja utilizat"**
- ❌ **"Acest email este deja înregistrat"**
- ❌ **"Format email invalid"**
- ❌ **"Numele este obligatoriu"**

### Salvarea utilizatorului

1. **Verificați** că toate câmpurile obligatorii sunt completate
2. **Apăsați "Creează Utilizatorul"** din josul ferestrei
3. **Așteptați** confirmarea "Se salvează..."
4. **În caz de succes** utilizatorul va apărea în listă
5. **În caz de eroare** veți vedea mesajul explicativ

---

## Modificarea unui Utilizator Existent

### Pornirea procesului de modificare

1. **În coloana "Acțiuni"** căutați butonul portocaliu cu iconița creion ✏️
2. **Apăsați pe acest buton** pentru utilizatorul dorit
3. **Se va deschide** o fereastră cu datele precompletate

### Modificarea datelor

- **Toate câmpurile** vor fi completate cu informațiile curente
- **Modificați** doar câmpurile pe care doriți să le schimbați
- **Validarea** se face la fel ca la adăugare

### Restricții pentru modificare

#### Ce NU puteți modifica:
- **ID-ul utilizatorului** - Este fix și unic
- **Data creării** - Este istorică și nu se schimbă
- **Datele de audit** - Sunt generate automat

#### Ce puteți modifica cu restricții:
- **Rolul** - Doar dacă aveți permisiuni superioare
- **Statusul** - Doar administratorii pot suspenda/bloca
- **Departamentul** - Doar în departamentele pentru care aveți drepturi

### Salvarea modificărilor

1. **Faceți modificările** necesare în formular
2. **Apăsați "Actualizează Utilizatorul"** din josul ferestrei
3. **Confirmați modificările** dacă vi se cere
4. **Utilizatorul actualizat** va apărea în listă cu noile date

---

## Exportarea Datelor

### Exportul rezultatelor filtrate

1. **Aplicați filtrele** după criteriile dorite
2. **În panoul de filtrare** apăsați "Exportă Rezultate"
3. **Alegeți formatul**: Excel, PDF, sau CSV
4. **Fișierul se va descărca** automat în browser

### Ce conține exportul

- **Toate coloanele vizibile** din tabel
- **Doar rândurile filtrate** (nu toți utilizatorii)
- **Formatare păstrată** pentru roluri și statusuri
- **Date actualizate** la momentul exportului

### Utilizarea fișierului exportat

- **Excel**: Pentru analize și rapoarte avansate
- **PDF**: Pentru imprimare și arhivare
- **CSV**: Pentru import în alte sisteme

---

## Situații Speciale și Erori

### Utilizatori cu probleme

#### 🔒 Utilizatori blocați
**De ce se întâmplă**: Prea multe încercări greșite de conectare
**Ce să faceți**:
1. Editați utilizatorul
2. Schimbați statusul la "Activ"
3. Informați utilizatorul să își schimbe parola

#### ⏸️ Utilizatori suspendați
**De ce se întâmplă**: Suspendare administrativă temporară
**Ce să faceți**:
1. Verificați motivul suspendării
2. Editați utilizatorul dacă este cazul
3. Schimbați statusul la "activ" după rezolvarea problemei

#### ❌ Utilizatori inactivi
**De ce se întâmplă**: Nu s-au mai conectat de mult timp
**Ce să faceți**:
1. Verificați dacă mai lucrează în clinică
2. Contactați utilizatorul pentru clarificare
3. Dezactivați contul dacă nu mai este necesar

### Mesaje de eroare în aplicație

#### "Nu aveți permisiuni pentru această acțiune"
**Cauza**: Rolul dvs. nu permite această operațiune
**Soluția**: Contactați administratorul pentru acordarea permisiunilor

#### "Utilizatorul nu poate fi șters"
**Cauza**: Utilizatorul are înregistrări asociate în sistem
**Soluția**: Dezactivați contul în loc să-l ștergeți

#### "Sesiunea a expirat"
**Cauza**: Ați stat prea mult timp inactiv
**Soluția**: Reconectați-vă și reluați operațiunea

---

## Sfaturi și Bune Practici

### Pentru organizarea utilizatorilor

#### ✅ Bune practici:
- **Folosiți convenții** pentru username-uri (ex: nume.prenume)
- **Asignați rolurile** cu atenție și doar personalului autorizat
- **Verificați periodic** utilizatorii inactivi
- **Mențineți datele** actualizate (telefon, departament, funcție)
- **Folosiți funcția de export** pentru raportări regulate

#### ❌ Ce să evitați:
- **Nu creați conturi** pentru personal temporar pe perioade scurte
- **Nu lăsați conturi active** pentru persoane care nu mai lucrează
- **Nu dați drepturi de administrator** decât unde este strict necesar
- **Nu folosiți același email** pentru mai multe conturi

### Pentru securitate

- **Verificați periodic** lista administratorilor
- **Monitorizați utilizatorii** care nu s-au mai conectat recent
- **Informați personalul** să își schimbe parolele regulat
- **Raportați imediat** activitatea suspectă

---

## Întrebări Frecvente

### 1. De câți utilizatori am nevoie pentru clinica mea?
**Răspuns:** Creați câte un cont pentru fiecare persoană care va lucra cu sistemul. Nu împărțiți conturile între mai multe persoane.

### 2. Pot să schimb rolul unui utilizator după ce l-am creat?
**Răspuns:** Da, puteți modifica rolul editând utilizatorul, dar verificați că aveți permisiuni pentru rolul pe care doriți să-l asignați.

### 3. Ce se întâmplă dacă șterg din greșeală un utilizator?
**Răspuns:** În majoritatea cazurilor nu puteți șterge, doar dezactiva. Contactați administratorul pentru recuperarea datelor.

### 4. Pot să văd parola unui utilizator?
**Răspuns:** Nu, parolele sunt criptate și nu pot fi vizualizate. Puteți doar să resetați parola unui utilizator.

### 5. De ce nu văd toți utilizatorii în listă?
**Răspuns:** Poate aveți filtre aplicate sau nu aveți permisiuni pentru anumite departamente. Verificați setările de filtrare.

### 6. Cum știu când un utilizator s-a conectat ultima dată?
**Răspuns:** Coloana "Ultima Autent." din tabel arată această informație. Pentru detalii, deschideți profilul utilizatorului.

---

## Contactarea Suportului

### Când să contactați suportul:
- **Nu puteți accesa** modulul de gestionare utilizatori
- **Întâmpinați erori** la salvarea utilizatorilor
- **Aveți întrebări** despre permisiuni și roluri
- **Trebuie să recuperați** date șterse accidental
- **Observați activitate neobișnuită** în lista utilizatorilor

### Informații de furnizat:
1. **Numele dvs. și rolul** în sistem
2. **Acțiunea** pe care încercați să o faceți
3. **Mesajul de eroare** exact (faceți o poză)
4. **Utilizatorul** asupra căruia lucrați (dacă e cazul)
5. **Browserul** și versiunea folosite

---

*Acest ghid vă ajută să gestionați eficient utilizatorii din sistemul ValyanMed. Pentru întrebări specifice sau probleme tehnice, nu ezitați să contactați echipa de suport.*

**Versiune document:** 1.0  
**Data actualizării:** Septembrie 2025  
**Autor:** Echipa ValyanMed
