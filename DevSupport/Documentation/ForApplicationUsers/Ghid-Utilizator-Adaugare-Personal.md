# Ghid Utilizator - Adaugare Personal

## 🎯 Prezentare Generala

Acest ghid va indruma pas cu pas prin procesul de adaugare a unui nou membru al personalului non-medical in sistemul ValyanClinic. Veti invata sa completati toate informatiile necesare si sa validati datele inainte de salvare.

## 📍 Accesul la Functia de Adaugare

### Cum sa ajungeti la formularul de adaugare:
1. **Navigati** la pagina `Administrare` → `Administrare Personal`
2. **Faceti click** pe butonul verde **"Adauga Personal"** din partea dreapta sus
3. **Se va deschide** modalul "Adaugare Personal Nou"

### Permisiuni necesare:
- **Administrator sistem**: Acces complet
- **Manager HR**: Poate adauga personal nou
- **Asistent administrativ**: Poate adauga cu aprobare
- **Utilizatori standard**: Fara acces

## 📝 Formularul de Adaugare - Sectiuni Complete

### 🆔 Sectiunea "Informatii Generale"

#### Cod Angajat (Read-only)
- **Format**: EMP + Data + Numar secvential (ex: EMP20241201001)
- **Generare**: Se genereaza automat la deschiderea formularului
- **Nu se modifica**: Acest camp este protejat si nu poate fi editat manual
- **💡 Sfat**: Daca codul pare incorect, inchideti si redeschideti modalul

#### CNP (Obligatoriu) ⭐
- **Format**: Exact 13 cifre
- **Validare**: Se valideaza in timp real cu algoritmul oficial romanesc
- **Auto-calculare**: Data nasterii se calculeaza automat din CNP

**Cum sa introduceti CNP-ul corect**:
1. Introduceti cele 13 cifre fara spatii sau separatori
2. Urmariti indicatoarele vizuale:
   - 🟢 **Verde** = CNP valid
   - 🟡 **Galben** = CNP incomplet
   - 🔴 **Rosu** = CNP invalid
3. Daca apare eroare, verificati din nou cifrele

**Erori frecvente CNP**:
- **"Cifra de control incorecta"**: Verificati ultima cifra
- **"Anul nu este valid"**: Prima cifra determina secolul
- **"Data nasterii in viitor"**: Verificati luna si ziua

#### Nume si Prenume (Obligatorii) ⭐
- **Nume**: Numele de familie al angajatului
- **Prenume**: Prenumele complet
- **Formatare**: Prima litera mare, restul mici
- **Caractere speciale**: Sunt acceptate diacriticele romanesti

#### Nume Anterior (Optional)
- **Pentru**: Persoane care si-au schimbat numele (casatorie, divort)
- **Folosire**: Helps pentru identificarea in documentele vechi

### 👤 Sectiunea "Informatii Personale"

#### Data Nasterii
- **Auto-completare**: Se calculeaza din CNP
- **Format**: dd.mm.yyyy
- **Validare**: Varsta trebuie sa fie intre 16-80 ani pentru angajati
- **Modificare**: Se poate modifica manual daca e necesar

#### Locul Nasterii
- **Format**: Localitatea de nastere
- **Exemplu**: "Bucuresti", "Cluj-Napoca", "Iasi"
- **Nu este obligatoriu**: Dar recomandam completarea

#### Starea Civila
- **Optiuni disponibile**:
  - Celibatar(a)
  - Casatorit(a)
  - Divortat(a)
  - Vaduv(a)

#### Nationalitatea si Cetatenia
- **Implicit**: "Romana" pentru ambele
- **Modificare**: Se pot modifica daca angajatul nu este roman
- **Formatul**: Prima litera mare (ex: "Germana", "Italiana")

### 📞 Sectiunea "Informatii de Contact"

#### Telefon Personal (Recomandat) ⭐
- **Format**: 07XX-XXX-XXX sau 02XX-XXX-XXX
- **Validare**: Verifica formatul romanesc
- **Folosire**: Pentru contactarea urgenta a angajatului

#### Telefon Serviciu (Optional)
- **Pentru**: Numarul de la birou sau telefon de serviciu
- **Acelasi format** ca telefonul personal

#### Email Personal (Recomandat) ⭐
- **Format**: adresa@domeniu.com
- **Validare**: Verifica formatul standard email
- **Unicitate**: Nu pot exista 2 angajati cu acelasi email
- **Folosire**: Comunicari oficiale si notificari

#### Email Serviciu (Optional)  
- **Pentru**: Email-ul corporate de la clinica
- **Format**: nume.prenume@valyanmed.ro

### 🏠 Sectiunea "Adresa Domiciliu" (Obligatorie)

#### Adresa Completa ⭐
- **Format**: Strada, numarul, apartamentul
- **Exemplu**: "Strada Victoriei, Nr. 15, Ap. 23"
- **Detalii**: Includeti toate informatiile pentru corespondenta

#### Judetul ⭐ 
- **Dropdown**: Lista tuturor judetelor Romaniei
- **Cautare**: Puteti tasta pentru a cauta rapid
- **Obligatoriu**: Trebuie selectat pentru a continua

#### Orasul/Comuna ⭐
- **Dependent**: Se incarca automat dupa selectarea judetului
- **Auto-update**: Lista se actualizeaza cand schimbati judetul
- **Cautare**: Tastati pentru cautare rapida in lista

#### Cod Postal
- **Format**: 6 cifre (ex: 010101)
- **Optional**: Dar recomandam completarea
- **Validare**: Verifica ca sunt doar cifre

### 🏘️ Sectiunea "Adresa Resedinta" (Optionala)

#### Checkbox de Activare
- **Text**: "Adresa de resedinta difera de cea de domiciliu"
- **Implicit**: Nebifat (resedinta = domiciliu)
- **La bifarea**: Se deschid campurile pentru resedinta

#### Campurile Resedinta
- **Acelasi format** ca pentru domiciliu
- **Se completeaza** doar daca difera de domiciliu
- **La debifarea checkbox-ului**: Datele se sterg automat

### 💼 Sectiunea "Informatii Profesionale"

#### Functia (Obligatorie) ⭐
- **Descriere**: Functia ocupata in clinica
- **Exemple**: 
  - "Administrator"
  - "Contabil sef"
  - "Specialist IT"
  - "Agent de curatenie"
  - "Receptioner"

#### Departamentul (Obligatoriu) ⭐
- **Optiuni disponibile**:
  - Administratie
  - Financiar  
  - IT
  - intretinere
  - Logistica
  - Marketing
  - Receptie
  - Resurse Umane
  - Securitate
  - Transport
  - Juridic
  - Relatii Clienti
  - Calitate
  - Call Center

#### Status Angajat
- **Implicit**: Activ
- **Optiuni**: Activ, Inactiv
- **Pentru noi angajati**: Lasati pe "Activ"

### 🆔 Sectiunea "Acte de Identitate"

#### Serie CI (Carte Identitate)
- **Format**: 2 litere (ex: AB, IF, B)
- **Exemplu**: "AB" pentru Alba, "B" pentru Bucuresti

#### Numar CI
- **Format**: 6 cifre
- **Exemplu**: "123456"

#### Eliberat de
- **Format**: Institutia care a eliberat CI
- **Exemplu**: "SPCLEP Alba Iulia", "SPCLEP Sector 1 Bucuresti"

#### Data Eliberarii
- **Format**: dd.mm.yyyy
- **Folosire**: Pentru a verifica validitatea documentului

#### Valabil pana la
- **Format**: dd.mm.yyyy
- **Important**: Pentru urmarirea expirarii documentelor
- **Alerta**: Sistemul va alerta cand CI expira in 30 de zile

### 📝 Sectiunea "Observatii" (Optionala)

#### Text Liber
- **Maxim**: 1000 de caractere
- **Folosire**: Note suplimentare despre angajat
- **Exemple**:
  - "Are experienta in contabilitate"
  - "Disponibil pentru programul de noapte"
  - "Cunostinte limbi straine: engleza, franceza"

## ✅ Procesul de Salvare

### Pasul 1: Validare Automata
1. **Sistemul verifica** toate campurile obligatorii
2. **Valideaza formatul** CNP, email, telefon
3. **Afiseaza erori** daca ceva nu este corect

### Pasul 2: Confirmarea Salvarii
1. **Faceti click** pe butonul "Adauga Personal"
2. **Asteptati** confirmarea (loading indicator)
3. **Vedeti notificarea** de succes sau eroare

### Pasul 3: Dupa Salvare
1. **Modalul se inchide** automat
2. **Grid-ul se actualizeaza** cu noul angajat  
3. **Primiti notificare** de confirmare

## ⚠️ Erori Comune si Solutii

### Probleme cu CNP-ul
**Eroare**: "CNP invalid - cifra de control"
**Solutie**: 
1. Verificati toate cifrele din nou
2. Folositi un calculator CNP online pentru verificare
3. Contactati persoana pentru confirmarea CNP-ului

**Eroare**: "CNP-ul exista deja in sistem"
**Solutie**:
1. Verificati daca persoana nu este deja inregistrata
2. Cautati in grid-ul principal dupa nume
3. Daca e eroare de tastare, corectati CNP-ul

### Probleme cu Email-ul
**Eroare**: "Email-ul exista deja"
**Solutie**:
1. Verificati daca email-ul nu este folosit de altcineva
2. Adaugati un numar la sfarsitul adresei (ex: ion.popescu2@gmail.com)
3. Folositi adresa de serviciu in locul celei personale

### Probleme cu Judet/Localitate
**Eroare**: "Nu s-au putut incarca localitatile"
**Solutie**:
1. Reimprospatati pagina (F5)
2. Selectati din nou judetul
3. Asteptati 2-3 secunde pentru incarcarea listei
4. Daca persista, contactati suportul tehnic

### Probleme de Salvare
**Eroare**: "Eroare la salvarea datelor"
**Solutii**:
1. Verificati conexiunea la internet
2. Reimprospatati pagina si incercati din nou
3. Verificati ca toate campurile obligatorii sunt completate
4. Contactati administratorul sistem

## 💡 Sfaturi si Trucuri

### Pentru Completare Rapida
1. **Pregatiti datele** inainte sa deschideti formularul
2. **Folositi Tab** pentru navigarea intre campuri  
3. **Salvati frecvent** daca completarea dureaza mult
4. **Verificati CNP-ul** cu persoana inainte de introducere

### Pentru Evitarea Erorilor
1. **Copiati/Lipiti** CNP-ul din document oficial
2. **Double-check** email-urile pentru greseli de tastare
3. **Verificati diacriticele** in nume si prenume
4. **Completati telefon** cu format standard romanesc

### Pentru Eficienta
1. **Folositi template-uri** pentru functii similare
2. **Salvati adresele** frecvent folosite intr-un document
3. **Verificati duplicatele** inainte de salvare
4. **Grupati adaugarea** mai multor angajati intr-o sesiune

## 🔍 Verificari Finale inainte de Salvare

### Checklist Obligatoriu ✅
- [ ] CNP valid (13 cifre, validat cu verde)
- [ ] Nume si prenume complete
- [ ] Data nasterii calculata corect din CNP
- [ ] Telefon cu format romanesc
- [ ] Email cu format valid
- [ ] Adresa completa cu judet si localitate
- [ ] Functia si departamentul selectate

### Checklist Recomandat ✅
- [ ] Email personal functional
- [ ] Telefon personal activ
- [ ] Acte de identitate complete
- [ ] Observatii relevante
- [ ] Verificat duplicatele in sistem

## 📞 Suport si Asistenta

### in caz de probleme:
- **Suport telefonic**: 0373-XXX-XXX (L-V, 08:00-18:00)
- **Email suport**: suport.personal@valyanmed.ro
- **Chat live**: Butonul "Ajutor" din aplicatie
- **Manual utilizator**: Documentatia completa din aplicatie

### Pentru instruire:
- **Sesiuni de training**: Programate lunar
- **Video tutorial**: Disponibil in aplicatie  
- **FAQ online**: help.valyanmed.ro
- **Training personalizat**: La cerere pentru echipe

---

**💼 Acest ghid face parte din documentatia oficiala ValyanMed si se actualizeaza constant pentru a reflecta cele mai recente imbunatatiri ale sistemului.**

**Versiune ghid**: 2.0  
**Data actualizarii**: Decembrie 2024  
**Autor**: Echipa ValyanMed Training & Documentation
