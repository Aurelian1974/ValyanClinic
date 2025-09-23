# Ghid Utilizator - Gestionarea Utilizatorilor

**Aplicatie:** ValyanMed - Sistem de Management Clinic  
**Modul:** Gestionare Utilizatori  
**Creat:** Septembrie 2025  
**Actualizat:** Septembrie 2025  
**Destinat pentru:** Administratori, Manageri, Personal cu drepturi de gestionare utilizatori  

---

## Prezentare Generala

Modulul de Gestionare Utilizatori va permite sa administrati conturile personalului din clinica. Puteti vizualiza, adauga, modifica si gestiona utilizatorii sistemului ValyanMed intr-un mod simplu si eficient.

### Ce puteti face in acest modul
- **Vizualizarea listei** complete de utilizatori
- **Cautarea si filtrarea** utilizatorilor dupa diverse criterii
- **Adaugarea** de utilizatori noi in sistem
- **Modificarea informatiilor** utilizatorilor existenti
- **Vizualizarea detaliilor** complete ale unui utilizator
- **Gestionarea rolurilor** si permisiunilor
- **Monitorizarea activitatii** utilizatorilor

---

## Accesarea Modulului

### Cum sa ajungeti la Gestionarea Utilizatorilor

1. **Conectati-va** in aplicatia ValyanMed
2. **Din meniul principal**, cautati sectiunea "Utilizatori"
3. **Apasati pe "Gestionare Utilizatori"**
4. Se va deschide pagina cu lista utilizatorilor

### Verificarea permisiunilor
Pentru a accesa acest modul, trebuie sa aveti unul dintre rolurile:
- ✅ **Administrator** - Acces complet la toate functiile
- ✅ **Manager** - Poate vizualiza si modifica utilizatorii din departamentul sau
- ❌ **Utilizator standard** - Nu are acces la acest modul

---

## intelegerea Interfetei

### Antetul paginii

#### Titlul si descrierea
- **"Gestionare Utilizatori"** - Titlul principal al paginii
- **Descrierea** explica scopul paginii: "Administreaza utilizatorii sistemului ValyanMed"

#### Butoanele de actiune
- **"Adauga Utilizator"** 🆕 - Pentru crearea unui utilizator nou
- **"Actualizeaza"** 🔄 - Pentru reincarcarea listei de utilizatori

### Statisticile utilizatorilor

in partea de sus a paginii veti vedea **8 carduri cu statistici**:

| Statistica | Ce inseamna |
|------------|-------------|
| **Total Utilizatori** | Numarul total de conturi din sistem |
| **Utilizatori Activi** | Conturi care sunt in prezent active |
| **Medici** | Numarul de utilizatori cu rolul de Medic |
| **Asistente Medicale** | Numarul de asistente inregistrate |
| **Administratori** | Numarul de administratori de sistem |
| **Personal Inactiv** | Conturi dezactivate temporar |
| **Conectari Astazi** | Cati utilizatori s-au conectat astazi |
| **Utilizatori Noi** | Conturi create in ultima saptamana |

---

## Sistemul de Filtrare Avansata

### Activarea filtrelor

1. **Cautati panoul "Filtrare Avansata"** sub statistici
2. **Apasati pe "Arata Filtrele"** pentru a deschide optiunile
3. **Panoul se va extinde** si veti vedea toate optiunile de filtrare

### Tipurile de filtre disponibile

#### 🔍 Cautare text globala
- **Cautati dupa**: Nume, prenume, email, username
- **Introduceti textul** in campul "Cauta in nume, email, username..."
- **Rezultatele** se actualizeaza automat in timp real

#### 👤 Filtru dupa rol
- **Alegeti din lista**: Toate rolurile, Administrator, Medic, Asistent Medical, etc.
- **Selectati "Toate rolurile"** pentru a elimina filtrul

#### ✅ Filtru dupa status
- **Optiuni disponibile**: Activ, Inactiv, Suspendat, Blocat
- **Selectati "Toate statusurile"** pentru a elimina filtrul

#### 🏢 Filtru dupa departament
- **Alegeti departamentul**: Cardiologie, Chirurgie, Radiologie, etc.
- **Lista se actualizeaza** cu departamentele active din clinica

#### ⏰ Filtru dupa perioada de activitate
- **Optiuni temporale**: Astazi, Saptamana trecuta, Luna trecuta, etc.
- **Filtreaza utilizatorii** dupa ultima lor conectare

### Aplicarea filtrelor

1. **Setati filtrele dorite** folosind dropdown-urile
2. **Apasati "Aplica Filtrele"** pentru a activa filtrarea
3. **Rezultatele** se vor actualiza in tabelul de utilizatori
4. **Veti vedea** "Rezultate: X din Y utilizatori" pentru feedback

### Curatarea filtrelor

- **Apasati "Curata Filtrele"** pentru a elimina toate filtrele
- **Sau selectati "Toate..."** in fiecare dropdown individual

---

## Lucrul cu Tabelul de Utilizatori

### Coloanele din tabel

| Coloana | Ce afiseaza | Functii speciale |
|---------|-------------|------------------|
| **ID** | Numarul unic al utilizatorului | Nu se poate modifica |
| **Nume** | Numele de familie | Se poate sorta si filtra |
| **Prenume** | Prenumele utilizatorului | Se poate sorta si filtra |
| **Email** | Adresa de email | Link pentru a trimite email |
| **Username** | Numele de utilizator pentru conectare | Se poate cauta |
| **Telefon** | Numarul de telefon | Format romanesc |
| **Rol** | Rolul in sistem | Cu badge colorat |
| **Departament** | Departamentul de apartenenta | Se poate grupa |
| **Status** | Starea contului | Cu indicator colorat |
| **Functia** | Postul ocupat in clinica | Text liber |
| **Data Crearii** | Cand a fost creat contul | Format dd.mm.yyyy |
| **Ultima Autent.** | Ultima conectare | Relativ (ex: "2 zile") |
| **Actiuni** | Butoane pentru actiuni | Nu se poate sorta |

### Functii avansate ale tabelului

#### 📊 Sortarea datelor
- **Apasati pe antetul coloanei** pentru sortare crescatoare
- **Apasati din nou** pentru sortare descrescatoare
- **A treia apasare** elimina sortarea

#### 🔍 Filtrarea coloanelor
- **Fiecare coloana** are propriul filtru in antet
- **Introduceti textul** pentru filtrare rapida
- **Filtrele Excel** sunt disponibile pentru filtrare avansata

#### 📑 Gruparea datelor
- **Trageti antetul coloanei** in zona "Grupeaza aici"
- **Datele se vor grupa** dupa coloana selectata
- **Puteti grupa dupa multiple coloane** simultan

#### 🔄 Reordonarea coloanelor
- **Trageti antetul coloanei** la pozitia dorita
- **Ordinea se va salva** pentru sesiunile viitoare

#### 📏 Redimensionarea coloanelor
- **Trageti marginea** coloanei pentru redimensionare
- **Dublu-click pe margine** pentru redimensionare automata

### Paginarea rezultatelor

- **Sus-dreapta tabelului**: Puteti alege numarul de inregistrari pe pagina
- **Optiuni disponibile**: 10, 20, 50, 100, Toate
- **Navigarea**: Folositi butoanele < > pentru schimbarea paginilor

---

## Vizualizarea Detaliilor unui Utilizator

### Deschiderea ferestrei de detalii

1. **in coloana "Actiuni"** cautati butonul albastru cu iconita ochiului 👁️
2. **Apasati pe acest buton** pentru utilizatorul dorit
3. **Se va deschide o fereastra modala** cu toate detaliile

### Ce veti vedea in fereastra de detalii

#### 📋 Card "Informatii Personale"
- **Nume si prenume** complet
- **Adresa de email** de contact
- **Numarul de telefon** (daca este specificat)

#### 👤 Card "Informatii Cont"
- **Username-ul** pentru conectare
- **ID-ul unic** in sistem
- **Rolul** in sistem cu badge colorat
- **Statusul** contului cu indicator vizual

#### 🏢 Card "Informatii Organizationale"
- **Departamentul** de apartenenta
- **Functia** ocupata in clinica

#### ⏰ Card "Informatii Temporale"
- **Data crearii** contului
- **Ultima autentificare** in sistem
- **Statutul activitatii** recente (ex: "Activ astazi")
- **Vechimea in sistem** calculata automat

#### 🛡️ Card "Permisiuni si Securitate"
- **Lista completa** a permisiunilor utilizatorului
- **Permisiuni universale**: Accesul de baza la sistem
- **Permisiuni pe rol**: Specifice functiei (ex: Administrare pentru Admin)
- **Permisiuni medicale**: Pentru personal medical

### inchiderea ferestrei

- **Apasati pe X** din coltul din dreapta-sus
- **Apasati "inchide"** din josul ferestrei
- **Apasati tasta Escape** de pe tastatura

---

## Adaugarea unui Utilizator Nou

### Pornirea procesului de adaugare

1. **in antetul paginii** apasati pe "Adauga Utilizator"
2. **Se va deschide** o fereastra pentru introducerea datelor
3. **Titlul ferestrei** va fi "Adauga Utilizator Nou"

### Completarea formularului

#### 📋 Sectiunea "Informatii Personale"

**Campurile obligatorii** (marcate cu *):
- **Nume*** - Numele de familie (ex: Popescu)
- **Prenume*** - Prenumele (ex: Maria)  
- **Email*** - Adresa de email (ex: maria.popescu@valyanmed.ro)

**Campurile optionale**:
- **Telefon** - Numarul de telefon (ex: 0723456789)

#### 👤 Sectiunea "Informatii Cont"

**Campurile obligatorii**:
- **Username*** - Numele pentru conectare (ex: maria.popescu)
- **Rol in Sistem*** - Alegeti din lista: Administrator, Medic, Asistent Medical, etc.

**Campurile optionale**:
- **Status** - in mod normal ramane "Activ"

#### 🏢 Sectiunea "Informatii Organizationale"

**Campurile optionale**:
- **Departament** - Alegeti din lista departamentelor active
- **Functia** - Descrierea postului (ex: "Medic Specialist Cardiologie")

### Validarea datelor

#### Mesaje de validare pe masura ce completati:
- **Nume si prenume**: Trebuie sa aiba intre 2-50 caractere
- **Email**: Trebuie sa aiba format valid (ceva@domeniu.ro)
- **Username**: Nu poate fi deja folosit de alt utilizator
- **Telefon**: Trebuie sa aiba format romanesc valid

#### Mesaje de eroare comune:
- ❌ **"Acest username este deja utilizat"**
- ❌ **"Acest email este deja inregistrat"**
- ❌ **"Format email invalid"**
- ❌ **"Numele este obligatoriu"**

### Salvarea utilizatorului

1. **Verificati** ca toate campurile obligatorii sunt completate
2. **Apasati "Creeaza Utilizatorul"** din josul ferestrei
3. **Asteptati** confirmarea "Se salveaza..."
4. **in caz de succes** utilizatorul va aparea in lista
5. **in caz de eroare** veti vedea mesajul explicativ

---

## Modificarea unui Utilizator Existent

### Pornirea procesului de modificare

1. **in coloana "Actiuni"** cautati butonul portocaliu cu iconita creion ✏️
2. **Apasati pe acest buton** pentru utilizatorul dorit
3. **Se va deschide** o fereastra cu datele precompletate

### Modificarea datelor

- **Toate campurile** vor fi completate cu informatiile curente
- **Modificati** doar campurile pe care doriti sa le schimbati
- **Validarea** se face la fel ca la adaugare

### Restrictii pentru modificare

#### Ce NU puteti modifica:
- **ID-ul utilizatorului** - Este fix si unic
- **Data crearii** - Este istorica si nu se schimba
- **Datele de audit** - Sunt generate automat

#### Ce puteti modifica cu restrictii:
- **Rolul** - Doar daca aveti permisiuni superioare
- **Statusul** - Doar administratorii pot suspenda/bloca
- **Departamentul** - Doar in departamentele pentru care aveti drepturi

### Salvarea modificarilor

1. **Faceti modificarile** necesare in formular
2. **Apasati "Actualizeaza Utilizatorul"** din josul ferestrei
3. **Confirmati modificarile** daca vi se cere
4. **Utilizatorul actualizat** va aparea in lista cu noile date

---

## Exportarea Datelor

### Exportul rezultatelor filtrate

1. **Aplicati filtrele** dupa criteriile dorite
2. **in panoul de filtrare** apasati "Exporta Rezultate"
3. **Alegeti formatul**: Excel, PDF, sau CSV
4. **Fisierul se va descarca** automat in browser

### Ce contine exportul

- **Toate coloanele vizibile** din tabel
- **Doar randurile filtrate** (nu toti utilizatorii)
- **Formatare pastrata** pentru roluri si statusuri
- **Date actualizate** la momentul exportului

### Utilizarea fisierului exportat

- **Excel**: Pentru analize si rapoarte avansate
- **PDF**: Pentru imprimare si arhivare
- **CSV**: Pentru import in alte sisteme

---

## Situatii Speciale si Erori

### Utilizatori cu probleme

#### 🔒 Utilizatori blocati
**De ce se intampla**: Prea multe incercari gresite de conectare
**Ce sa faceti**:
1. Editati utilizatorul
2. Schimbati statusul la "Activ"
3. Informati utilizatorul sa isi schimbe parola

#### ⏸️ Utilizatori suspendati
**De ce se intampla**: Suspendare administrativa temporara
**Ce sa faceti**:
1. Verificati motivul suspendarii
2. Editati utilizatorul daca este cazul
3. Schimbati statusul la "activ" dupa rezolvarea problemei

#### ❌ Utilizatori inactivi
**De ce se intampla**: Nu s-au mai conectat de mult timp
**Ce sa faceti**:
1. Verificati daca mai lucreaza in clinica
2. Contactati utilizatorul pentru clarificare
3. Dezactivati contul daca nu mai este necesar

### Mesaje de eroare in aplicatie

#### "Nu aveti permisiuni pentru aceasta actiune"
**Cauza**: Rolul dvs. nu permite aceasta operatiune
**Solutia**: Contactati administratorul pentru acordarea permisiunilor

#### "Utilizatorul nu poate fi sters"
**Cauza**: Utilizatorul are inregistrari asociate in sistem
**Solutia**: Dezactivati contul in loc sa-l stergeti

#### "Sesiunea a expirat"
**Cauza**: Ati stat prea mult timp inactiv
**Solutia**: Reconectati-va si reluati operatiunea

---

## Sfaturi si Bune Practici

### Pentru organizarea utilizatorilor

#### ✅ Bune practici:
- **Folositi conventii** pentru username-uri (ex: nume.prenume)
- **Asignati rolurile** cu atentie si doar personalului autorizat
- **Verificati periodic** utilizatorii inactivi
- **Mentineti datele** actualizate (telefon, departament, functie)
- **Folositi functia de export** pentru raportari regulate

#### ❌ Ce sa evitati:
- **Nu creati conturi** pentru personal temporar pe perioade scurte
- **Nu lasati conturi active** pentru persoane care nu mai lucreaza
- **Nu dati drepturi de administrator** decat unde este strict necesar
- **Nu folositi acelasi email** pentru mai multe conturi

### Pentru securitate

- **Verificati periodic** lista administratorilor
- **Monitorizati utilizatorii** care nu s-au mai conectat recent
- **Informati personalul** sa isi schimbe parolele regulat
- **Raportati imediat** activitatea suspecta

---

## intrebari Frecvente

### 1. De cati utilizatori am nevoie pentru clinica mea?
**Raspuns:** Creati cate un cont pentru fiecare persoana care va lucra cu sistemul. Nu impartiti conturile intre mai multe persoane.

### 2. Pot sa schimb rolul unui utilizator dupa ce l-am creat?
**Raspuns:** Da, puteti modifica rolul editand utilizatorul, dar verificati ca aveti permisiuni pentru rolul pe care doriti sa-l asignati.

### 3. Ce se intampla daca sterg din greseala un utilizator?
**Raspuns:** in majoritatea cazurilor nu puteti sterge, doar dezactiva. Contactati administratorul pentru recuperarea datelor.

### 4. Pot sa vad parola unui utilizator?
**Raspuns:** Nu, parolele sunt criptate si nu pot fi vizualizate. Puteti doar sa resetati parola unui utilizator.

### 5. De ce nu vad toti utilizatorii in lista?
**Raspuns:** Poate aveti filtre aplicate sau nu aveti permisiuni pentru anumite departamente. Verificati setarile de filtrare.

### 6. Cum stiu cand un utilizator s-a conectat ultima data?
**Raspuns:** Coloana "Ultima Autent." din tabel arata aceasta informatie. Pentru detalii, deschideti profilul utilizatorului.

---

## Contactarea Suportului

### Cand sa contactati suportul:
- **Nu puteti accesa** modulul de gestionare utilizatori
- **intampinati erori** la salvarea utilizatorilor
- **Aveti intrebari** despre permisiuni si roluri
- **Trebuie sa recuperati** date sterse accidental
- **Observati activitate neobisnuita** in lista utilizatorilor

### Informatii de furnizat:
1. **Numele dvs. si rolul** in sistem
2. **Actiunea** pe care incercati sa o faceti
3. **Mesajul de eroare** exact (faceti o poza)
4. **Utilizatorul** asupra caruia lucrati (daca e cazul)
5. **Browserul** si versiunea folosite

---

*Acest ghid va ajuta sa gestionati eficient utilizatorii din sistemul ValyanMed. Pentru intrebari specifice sau probleme tehnice, nu ezitati sa contactati echipa de suport.*

**Versiune document:** 1.0  
**Data actualizarii:** Septembrie 2025  
**Autor:** Echipa ValyanMed
