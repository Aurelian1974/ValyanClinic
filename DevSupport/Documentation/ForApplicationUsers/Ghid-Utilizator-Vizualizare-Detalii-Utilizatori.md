# Ghid Utilizator - Vizualizarea Detaliilor Utilizatorilor

**Aplicatie:** ValyanMed - Sistem de Management Clinic  
**Functie:** Vizualizarea informatiilor detaliate ale utilizatorilor  
**Creat:** Septembrie 2025  
**Actualizat:** Septembrie 2025  
**Destinat pentru:** Administratori, Manageri, Personal cu acces la gestionarea utilizatorilor  

---

## Prezentare Generala

Fereastra de vizualizare a detaliilor utilizatorilor va ofera o imagine completa si organizata asupra informatiilor unui utilizator din sistem. Este conceputa ca un dashboard elegant care prezinta toate datele relevante intr-un format usor de citit si professional.

### Ce puteti vedea in aceasta fereastra
- **Informatii personale** complete ale utilizatorului
- **Detalii ale contului** si statusul acestuia
- **Informatii organizationale** si rolul in clinica
- **Istoricul activitatii** si statistici temporale
- **Permisiunile si drepturile** de acces in sistem
- **Indicatori vizuali** pentru status si rol

---

## Deschiderea Ferestrei de Detalii

### Cum sa accesati informatiile unui utilizator

1. **Navigati** la modulul "Gestionare Utilizatori"
2. **Gasiti utilizatorul** in lista afisata
3. **in coloana "Actiuni"** cautati butonul albastru cu iconita ochiului 👁️
4. **Apasati pe acest buton** - se va deschide fereastra de detalii

### Ce veti vedea la deschidere

- **O fereastra modala** se va deschide peste pagina curenta
- **Antetul ferestrei** va afisa numele complet al utilizatorului
- **Continutul** va fi organizat in carduri colorate si usor de citit
- **Butoanele de actiune** vor fi disponibile in partea de jos

---

## Structura Informatiilor

Informatiile sunt prezentate in **5 carduri principale**, fiecare cu o culoare si o iconita specifica:

### 📋 Card "Informatii Personale" 
- **Culoarea**: Gradient rosu-turquoise-albastru
- **Iconita**: Card de identitate
- **Continutul**: Date personale de baza

### 👤 Card "Informatii Cont"
- **Culoarea**: Gradient turquoise-albastru-violet  
- **Iconita**: Roata de setari utilizator
- **Continutul**: Date de conectare si status cont

### 🏢 Card "Informatii Organizationale"
- **Culoarea**: Gradient violet-roz-rosu
- **Iconita**: Cladire
- **Continutul**: Rolul in organizatia clinicii

### ⏰ Card "Informatii Temporale"
- **Culoarea**: Gradient albastru-portocaliu-verde
- **Iconita**: Calendar
- **Continutul**: Istoricul si activitatea in timp

### 🛡️ Card "Permisiuni si Securitate" (Latime completa)
- **Culoarea**: Gradient verde
- **Iconita**: Scut de securitate
- **Continutul**: Toate permisiunile si drepturile de acces

---

## Cardul "Informatii Personale"

### Ce veti gasi aici

#### 👤 Nume
- **Afiseaza**: Numele de familie al utilizatorului
- **Format**: Text simplu, clar si lizibil
- **Exemple**: Popescu, Marinescu, Ionescu

#### 👤 Prenume  
- **Afiseaza**: Prenumele utilizatorului
- **Format**: Text simplu
- **Exemple**: Maria, Alexandru, Elena

#### 📧 Email
- **Afiseaza**: Adresa de email completa
- **Format**: Link activ pentru trimiterea de email-uri
- **Functionalitate**: Puteti apasa pe email pentru a deschide clientul de email

#### 📱 Telefon
- **Afiseaza**: Numarul de telefon de contact
- **Format**: Format romanesc standard
- **Cazuri speciale**: Daca nu este specificat, va afisa "Nu este specificat"

### Cum sa interpretati informatiile

- **Toate campurile** sunt doar pentru vizualizare (nu pot fi editate direct)
- **Email-ul** este link activ - puteti apasa pe el pentru contact rapid
- **Datele lipsa** sunt afisate cu mesajul "Nu este specificat"
- **Informatiile** sunt actualizate in timp real

---

## Cardul "Informatii Cont"

### Ce veti gasi aici

#### 👤 Username
- **Afiseaza**: Numele de utilizator pentru conectare
- **Format**: Text simplu, de obicei format nume.prenume
- **Importanta**: Este ceea ce utilizatorul foloseste pentru a se conecta

#### 🔢 ID Utilizator
- **Afiseaza**: Numarul unic de identificare in sistem
- **Format**: #123 (cu simbolul diez inaintea numarului)
- **Utilitate**: Pentru referinte tehnice si suport

#### 🎭 Rol in Sistem
- **Afiseaza**: Rolul principal al utilizatorului
- **Format**: Badge colorat cu numele rolului in romana
- **Culori**:
  - **Albastru**: Administrator
  - **Verde**: Medic  
  - **Turquoise**: Asistent Medical
  - **Portocaliu**: Receptioner
  - **Violet**: Manager
  - **Gri**: Operator

#### ✅ Status
- **Afiseaza**: Starea curenta a contului
- **Format**: Badge colorat cu statusul in romana
- **Culori**:
  - **Verde**: Activ (utilizatorul se poate conecta)
  - **Rosu**: Inactiv (contul este dezactivat)
  - **Portocaliu**: Suspendat (blocat temporar)
  - **Albastru**: Blocat (blocat din motive de securitate)

### intelegerea rolurilor afisate

#### 👑 Administrator
- **inseamna**: Acces complet la toate functiile sistemului
- **Responsabilitati**: Gestionarea intregului sistem
- **Atentie**: Rol cu putere mare, acordat cu grija

#### 👨‍⚕️ Medic
- **inseamna**: Personal medical cu drepturi de consultatie
- **Responsabilitati**: ingrijirea pacientilor, prescriptii
- **Acces**: La toate datele medicale ale pacientilor

#### 👩‍⚕️ Asistent Medical
- **inseamna**: Personal de asistenta medicala
- **Responsabilitati**: Suportul in activitatea medicala
- **Acces**: La datele pacientilor pentru asistenta

---

## Cardul "Informatii Organizationale"

### Ce veti gasi aici

#### 🏢 Departament
- **Afiseaza**: Departamentul de apartenenta
- **Format**: Numele complet al departamentului
- **Exemple**: Cardiologie, Chirurgie, Radiologie, Administratie
- **Cazuri speciale**: "Nu este specificat" daca nu e asignat

#### 💼 Functia
- **Afiseaza**: Postul ocupat in clinica
- **Format**: Descriere libera a functiei
- **Exemple**: "Medic Specialist Cardiologie", "Asistent Medical Chirurgie"
- **Cazuri speciale**: "Nu este specificata" daca nu e completata

### Importanta informatiilor organizationale

#### Pentru ce sunt utile:
- **intelegerea structurii** organizationale
- **Identificarea responsabilitatilor** fiecarui utilizator
- **Organizarea echipelor** pe departamente
- **Raportarile** ierarhice si functionale

---

## Cardul "Informatii Temporale"

### Ce veti gasi aici

#### 📅 Data crearii
- **Afiseaza**: Cand a fost creat contul in sistem
- **Format**: dd.MM.yyyy HH:mm (ex: 15.03.2025 14:30)
- **Utilitate**: Pentru a stii de cand exista contul

#### 🔑 Ultima autentificare
- **Afiseaza**: Cand s-a conectat utilizatorul ultima data
- **Format**: dd.MM.yyyy HH:mm
- **Cazuri speciale**: "Niciodata" daca nu s-a conectat inca niciodata

#### 🟢 Activitate recenta
- **Afiseaza**: Un text descriptiv despre activitatea recenta
- **Formate posibile**:
  - "Online acum" - este conectat in acest moment
  - "Activ astazi" - s-a conectat azi
  - "Activ ieri" - s-a conectat ieri
  - "Activ acum 3 zile" - ultima conectare acum cateva zile
  - "Activ acum 2 saptamani" - pentru perioade mai mari
  - "Inactiv de mult timp" - nu s-a mai conectat de foarte mult timp

#### ⏳ Vechime in sistem
- **Afiseaza**: Cat timp a trecut de la crearea contului
- **Formate posibile**:
  - "5 zile" - pentru conturi noi
  - "2 luni" - pentru conturi de cateva luni
  - "1 an si 3 luni" - pentru conturi mai vechi

### Interpretarea informatiilor temporale

#### Indicatori de activitate:
- **"Online acum"** 🟢 = Utilizatorul este foarte activ
- **"Activ astazi"** 🟢 = Activitate regulata
- **"Activ acum 2-3 zile"** 🟡 = Activitate normala  
- **"Inactiv de mult timp"** 🔴 = Posibila problema sau plecare

#### Indicatori pentru conturi noi:
- **Creat recent + Niciodata conectat** = Utilizatorul nu si-a activat inca contul
- **Creat recent + Activ** = Utilizatorul nou si activ

---

## Cardul "Permisiuni si Securitate"

Acest card ocupa **intreaga latime** a ferestrei si afiseaza toate permisiunile utilizatorului sub forma unor butoane colorate.

### Tipuri de permisiuni

#### 🟢 Permisiuni Universale (Verde)
Acestea sunt acordate tuturor utilizatorilor:

- **"Acces Modul Utilizatori"** - Poate vedea lista utilizatorilor
- **"Acces Rapoarte"** - Poate consulta rapoartele de baza

#### 🔵 Permisiuni Administrative (Albastru)
Acestea apar doar pentru Administratori:

- **"Administrare Sistem"** - Control complet asupra sistemului
- **"Gestionare Utilizatori"** - Poate crea, modifica si sterge utilizatori

#### 🟣 Permisiuni Medicale (Violet)
Acestea apar doar pentru Medici:

- **"Fise Medicale"** - Acces complet la fisele pacientilor  
- **"Prescriere Medicamente"** - Poate prescrie tratamente

#### 🟡 Permisiuni Departamentale (Portocaliu)
Acestea apar pentru Manageri si alte roluri specifice:

- **"Management Departament"** - Poate gestiona propriul departament
- **"Rapoarte Avansate"** - Acces la rapoarte detaliate

### Cum sa interpretati permisiunile

#### Numarul de butoane:
- **Multe butoane** = Utilizator cu multe drepturi
- **Putine butoane** = Utilizator cu acces limitat
- **Butoane diferite** = Permisiuni specifice rolului

#### Culorile butoanelor:
- **Verde** = Permisiuni de baza, sigure
- **Albastru** = Permisiuni administrative, importante
- **Violet** = Permisiuni medicale, sensibile
- **Portocaliu** = Permisiuni departamentale, locale

---

## Navigarea in Fereastra

### Butoanele din antet

#### ❌ Butonul X (inchidere)
- **Locatia**: Coltul din dreapta-sus
- **Functia**: inchide fereastra fara alte actiuni
- **Scurtatura**: Tasta Escape

#### 📖 Titlul ferestrei
- **Afiseaza**: Numele complet al utilizatorului
- **Format**: "Prenume Nume" (ex: "Maria Popescu")
- **Subtitlu**: "Detalii utilizator"

### Butoanele din josul ferestrei

#### ✏️ "Editeaza Utilizatorul"
- **Culoarea**: Albastru (actiune principala)
- **Functia**: Deschide formularul de editare pentru acest utilizator
- **Disponibilitate**: Doar daca aveti drepturi de editare

#### ❌ "inchide"
- **Culoarea**: Gri (actiune secundara)  
- **Functia**: inchide fereastra si reveniti la lista utilizatorilor
- **Disponibilitate**: intotdeauna disponibil

### Scroll si navigare

#### Daca informatiile nu incap pe ecran:
- **Scroll vertical** este disponibil in interiorul ferestrei
- **Fereastra pastreaza** antetul si butoanele vizibile
- **Scroll-ul este smooth** si optimizat pentru citire

---

## Situatii Speciale de Afisare

### Utilizatori fara anumite informatii

#### Pentru campurile optionale:
- **Telefon lipsa**: "Nu este specificat"
- **Departament lipsa**: "Nu este specificat"  
- **Functia lipsa**: "Nu este specificata"

#### Pentru informatii de activitate:
- **Niciodata conectat**: "Niciodata autentificat"
- **Ultima conectare**: "Niciodata" in loc de data

### Utilizatori cu roluri speciale

#### Pentru super-administratori:
- **Permisiuni suplimentare** pot aparea
- **Butoane speciale** pentru functii avansate
- **Indicatori vizuali** pentru statusul privilegiat

#### Pentru utilizatori inactivi:
- **Informatiile** sunt afisate normal
- **Statusul** va arata "Inactiv" cu badge rosu
- **Activitatea recenta** va reflecta inactivitatea

---

## Sfaturi pentru Interpretare

### Identificarea problemelor

#### 🔴 Semnale de alerta:
- **Status "Blocat" sau "Suspendat"** - Utilizatorul are probleme
- **"Inactiv de mult timp"** - Posibil nu mai lucreaza
- **"Niciodata autentificat"** - Contul nu a fost activat
- **Lipsa departament** pentru personal medical - Informatii incomplete

#### 🟡 Semnale de atentie:
- **Permisiuni neobisnuit de multe** - Verificati necesitatea
- **Roluri incompatibile** cu functia - Posibila greseala
- **Informatii de contact incomplete** - Ar trebui actualizate

### Verificarea consecventei

#### Verificati ca:
- **Rolul** corespunde cu functia din organizatie
- **Departamentul** este corect pentru specialitatea medicala
- **Permisiunile** sunt adecvate pentru responsabilitati
- **Statusul** reflecta situatia reala a angajatului

---

## Actiuni Rapide din Fereastra

### Contactarea utilizatorului

#### Prin email:
1. **Apasati pe adresa de email** din cardul "Informatii Personale"
2. **Se va deschide** clientul de email implicit
3. **Email-ul** va fi pre-completat cu adresa utilizatorului

#### Prin telefon:
- **Notati numarul** din cardul "Informatii Personale"
- **Apelati** folosind telefonul mobil sau fix
- **Salvati** numarul in agenda telefonului daca e necesar

### Editarea rapida

1. **Apasati "Editeaza Utilizatorul"** din josul ferestrei
2. **Se va inchide** fereastra de detalii
3. **Se va deschide** formularul de editare cu datele precompletate
4. **Faceti modificarile** necesare si salvati

---

## intelegerea Contextului

### Pentru administratori

#### Ce sa urmariti:
- **Distributia rolurilor** - nu prea multi administratori
- **Utilizatori inactivi** - posibil de curatat
- **Permisiuni excesive** - reducere pentru securitate
- **Informatii incomplete** - completare necesara

### Pentru manageri departamentali

#### Ce sa urmariti:
- **Personalul din departament** are rolurile corecte
- **Activitatea recenta** a echipei dvs.
- **Informatiile de contact** sunt actualizate
- **Functiile** reflecta realitatea organizationala

---

## intrebari Frecvente

### 1. De ce nu pot sa editez informatiile direct din aceasta fereastra?
**Raspuns:** Aceasta fereastra este doar pentru vizualizare. Pentru editare, folositi butonul "Editeaza Utilizatorul" sau butonul de editare din lista principala.

### 2. Ce inseamna cand vad "Niciodata autentificat"?
**Raspuns:** Utilizatorul nu s-a conectat inca niciodata in sistem. Este normal pentru conturi nou create.

### 3. De ce nu vad toate permisiunile pentru un utilizator?
**Raspuns:** Se afiseaza doar permisiunile relevante pentru rolul utilizatorului. Permisiunile tehnice interne nu sunt afisate.

### 4. Pot sa printez aceste informatii?
**Raspuns:** Da, folositi functia de printare a browserului (Ctrl+P) cand fereastra este deschisa. Se va printa frumos formatat.

### 5. De ce unele informatii lipsesc?
**Raspuns:** Informatiile optionale (telefon, departament, functia) pot sa nu fi fost completate la crearea contului.

### 6. Cum stiu daca un utilizator este conectat acum?
**Raspuns:** Daca in "Activitate recenta" scrie "Online acum", inseamna ca utilizatorul este conectat in acest moment.

---

## Depanarea Problemelor

### Fereastra nu se deschide

#### Cauze posibile:
- Browser blocat sau incarcat
- Probleme de conectivitate
- Eroare temporara de server

#### Solutii:
1. **Reimprospatati pagina** cu F5
2. **incercati cu alt browser**
3. **Verificati conexiunea** la internet
4. **Contactati suportul** daca problema persista

### Informatiile nu se incarca complet

#### Cauze posibile:
- incarcare lenta de date
- Utilizator cu multe informatii
- Probleme de retea

#### Solutii:
1. **Asteptati** cateva secunde suplimentare
2. **inchideti si redeschideti** fereastra
3. **Verificati conexiunea** la internet

---

*Aceasta fereastra va ofera o imagine completa asupra utilizatorilor din sistemul ValyanMed. Folositi informatiile pentru a intelege mai bine organizatia si a gestiona eficient personalul clinicii.*

**Versiune document:** 1.0  
**Data actualizarii:** Septembrie 2025  
**Autor:** Echipa ValyanMed
