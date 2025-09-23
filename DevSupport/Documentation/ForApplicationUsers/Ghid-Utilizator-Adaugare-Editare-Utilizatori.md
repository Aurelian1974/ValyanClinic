# Ghid Utilizator - Adaugarea si Editarea Utilizatorilor

**Aplicatie:** ValyanMed - Sistem de Management Clinic  
**Functie:** Formulare de adaugare si editare utilizatori  
**Creat:** Septembrie 2025  
**Actualizat:** Septembrie 2025  
**Destinat pentru:** Administratori, Manageri cu drepturi de gestionare personal  

---

## Prezentare Generala

Formularele de adaugare si editare utilizatori va permit sa introduceti sau sa modificati informatiile despre personalul clinicii intr-un mod structurat si securizat. Acelasi formular este folosit atat pentru crearea utilizatorilor noi, cat si pentru modificarea celor existenti.

### Ce puteti face cu aceste formulare
- **Adaugarea** de utilizatori noi in sistem
- **Modificarea** informatiilor utilizatorilor existenti  
- **Asignarea rolurilor** si permisiunilor
- **Setarea informatiilor** organizationale
- **Validarea datelor** in timp real
- **Salvarea securizata** a informatiilor

---

## Accesarea Formularelor

### Pentru adaugarea unui utilizator nou

1. **Accesati** modulul "Gestionare Utilizatori"
2. **Apasati** butonul "Adauga Utilizator" din antetul paginii
3. **Se va deschide** formularul de adaugare cu campuri goale

### Pentru editarea unui utilizator existent

1. **in lista utilizatorilor** gasiti utilizatorul dorit
2. **in coloana "Actiuni"** apasati butonul portocaliu cu creionul ✏️
3. **Se va deschide** formularul cu datele precompletate

---

## Structura Formularului

Formularul este organizat in **3 sectiuni principale**, fiecare cu un titlu colorat si o iconita specifica:

### 📋 1. Informatii Personale
- **Iconita**: Card de identitate
- **Culoarea**: Gradient albastru-verde
- **Continut**: Datele de baza ale persoanei

### 👤 2. Informatii Cont  
- **Iconita**: Roata de setari utilizator
- **Culoarea**: Gradient verde-albastru
- **Continut**: Datele pentru conectarea in sistem

### 🏢 3. Informatii Organizationale
- **Iconita**: Cladire
- **Culoarea**: Gradient albastru-violet
- **Continut**: Informatii despre rolul in clinica

---

## Sectiunea "Informatii Personale"

### Campurile disponibile

#### 📝 Nume (Obligatoriu)
- **Ce sa introduceti**: Numele de familie al persoanei
- **Exemple**: Popescu, Ionescu, Marinescu
- **Restrictii**: Minimum 2 caractere, maximum 50
- **Validare**: Se verifica automat ca nu este gol

#### 📝 Prenume (Obligatoriu)  
- **Ce sa introduceti**: Prenumele persoanei
- **Exemple**: Maria, Alexandru, Elena
- **Restrictii**: Minimum 2 caractere, maximum 50
- **Validare**: Se verifica automat ca nu este gol

#### 📧 Email (Obligatoriu)
- **Ce sa introduceti**: Adresa de email profesionala
- **Exemple**: maria.popescu@valyanmed.ro, doctor.ionescu@gmail.com
- **Restrictii**: Trebuie sa aiba format valid de email
- **Validare**: Se verifica ca nu este deja folosita de altcineva

#### 📱 Telefon (Optional)
- **Ce sa introduceti**: Numarul de telefon pentru contact
- **Exemple**: 0723456789, +40723456789, 0373123456
- **Restrictii**: Format romanesc valid
- **Validare**: Se verifica formatul daca este completat

### Sfaturi pentru completare

#### ✅ Bune practici:
- **Folositi** nume complete si corecte din punct de vedere ortografic
- **Verificati** ca email-ul este corect inainte de salvare
- **Folositi email-uri profesionale** cand este posibil
- **Introduceti** numarul de telefon pentru situatii urgente

#### ❌ Ce sa evitati:
- **Nu folositi** prescurtari pentru nume (ex: "Alex" in loc de "Alexandru")
- **Nu folositi** email-uri personale pentru posturi importante
- **Nu lasati** campurile obligatorii goale
- **Nu introduceti** date false pentru testare

---

## Sectiunea "Informatii Cont"

### Campurile disponibile

#### 👤 Username (Obligatoriu)
- **Ce sa introduceti**: Numele de utilizator pentru conectare
- **Exemple**: maria.popescu, doctor.ionescu, asistent.elena
- **Restrictii**: Trebuie sa fie unic in sistem
- **Conventii**: Recomandam formatul nume.prenume

#### 🎭 Rol in Sistem (Obligatoriu)
- **Ce sa alegeti**: Rolul principal al utilizatorului
- **Optiuni disponibile**:
  - **Administrator** - Control total asupra sistemului
  - **Medic** - Acces la functii medicale si pacienti  
  - **Asistent Medical** - Asistenta medicala si inregistrari
  - **Receptioner** - Gestionarea programarilor si primirii pacientilor
  - **Manager** - Supraveghere si raportari departamentale
  - **Operator** - Utilizator cu acces limitat

#### ✅ Status (Optional)
- **Valoarea implicita**: Activ
- **Optiuni disponibile**:
  - **Activ** - Utilizatorul se poate conecta normal
  - **Inactiv** - Contul este temporar dezactivat
  - **Suspendat** - Contul este blocat temporar
  - **Blocat** - Contul este blocat din motive de securitate

### intelegerea rolurilor

#### 👑 Administrator
- **Pentru cine**: Personal IT, manageri superiori
- **Ce poate face**: Tot ce este disponibil in sistem
- **Responsabilitati**: Gestionarea completa a sistemului
- **Atentie**: Acordati cu grija, doar personalului de incredere

#### 👨‍⚕️ Medic  
- **Pentru cine**: Medici de toate specialitatile
- **Ce poate face**: Consultatii, prescriptii, fise medicale
- **Responsabilitati**: ingrijirea pacientilor si documentatia medicala
- **Permisiuni**: Acces la toate datele medicale

#### 👩‍⚕️ Asistent Medical
- **Pentru cine**: Asistente medicale, infirmiere
- **Ce poate face**: Asistenta medicala, inregistrari vitale
- **Responsabilitati**: Suportul in activitatea medicala
- **Restrictii**: Nu poate prescrie medicamente

#### 📞 Receptioner
- **Pentru cine**: Personalul de la receptie
- **Ce poate face**: Programari, inregistrarea pacientilor
- **Responsabilitati**: Primul contact cu pacientii
- **Restrictii**: Nu are acces la datele medicale

---

## Sectiunea "Informatii Organizationale"

### Campurile disponibile

#### 🏢 Departament (Optional)
- **Ce sa alegeti**: Departamentul unde lucreaza persoana
- **Exemple**: Cardiologie, Chirurgie, Radiologie, Administratie
- **Functionalitate**: Lista se incarca dinamic cu departamentele active
- **Cautare**: Puteti cauta rapid in lista departamentelor

#### 💼 Functia (Optional)
- **Ce sa introduceti**: Postul específic ocupat
- **Exemple**: "Medic Specialist Cardiologie", "Asistent Medical Chirurgie"
- **Format liber**: Puteti introduce orice descriere relevanta
- **Sfat**: Fiti cat mai specifici pentru claritate

### Importanta informatiilor organizationale

#### De ce sunt importante:
- **Organizarea** personalului pe departamente
- **Raportarile** pe structura organizationala  
- **Permisiunile** bazate pe departament
- **Comunicarea** interna eficienta

#### Cum sa le completati corect:
- **Verificati** ca departamentul este correct
- **Consultati** organigramele existente
- **Folositi** denumiri oficiale pentru functii
- **Actualizati** la schimbari organizationale

---

## Validarea si Mesajele de Eroare

### Validarea in timp real

Pe masura ce completati formularul, sistemul verifica datele si afiseaza:
- **Bife verzi** ✅ pentru campurile corecte
- **X-uri rosii** ❌ pentru campurile cu probleme
- **Mesaje explicative** sub fiecare camp cu probleme

### Mesajele de eroare comune

#### Pentru campurile obligatorii:
- ❌ **"Numele este obligatoriu"** - Nu ati completat numele
- ❌ **"Prenumele este obligatoriu"** - Nu ati completat prenumele  
- ❌ **"Email-ul este obligatoriu"** - Nu ati completat email-ul
- ❌ **"Username-ul este obligatoriu"** - Nu ati completat username-ul

#### Pentru formatul datelor:
- ❌ **"Format email invalid"** - Email-ul nu are formatul corect
- ❌ **"Format telefon invalid"** - Telefonul nu are format romanesc
- ❌ **"Numele nu poate depasi 50 de caractere"** - Textul este prea lung

#### Pentru unicitate:
- ❌ **"Acest username este deja utilizat"** - Alt utilizator foloseste acelasi username
- ❌ **"Acest email este deja inregistrat"** - Alt utilizator foloseste acelasi email

### Rezolvarea erorilor

#### Pasi pentru corectare:
1. **Cititi cu atentie** mesajul de eroare
2. **Corectati** datele in campul indicat
3. **Asteptati** sa dispara mesajul de eroare
4. **Continuati** cu completarea formularului
5. **Salvati** doar cand toate erorile sunt rezolvate

---

## Salvarea si Anularea

### Butoanele din josul formularului

#### 💾 Butonul de salvare
**Pentru utilizatori noi**:
- **Textul**: "Creeaza Utilizatorul"  
- **Culoarea**: Albastru (buton principal)
- **Functia**: Salveaza utilizatorul nou in sistem

**Pentru utilizatori existenti**:
- **Textul**: "Actualizeaza Utilizatorul"
- **Culoarea**: Albastru (buton principal)  
- **Functia**: Salveaza modificarile facute

#### ❌ Butonul de anulare
- **Textul**: "Anuleaza"
- **Culoarea**: Gri (buton secundar)
- **Functia**: inchide formularul fara salvare

### Procesul de salvare

#### Ce se intampla cand apasati "Salveaza":
1. **Validarea finala** - Sistemul verifica inca o data toate datele
2. **Afisarea "Se salveaza..."** - Indicatorul de progres apare
3. **Trimiterea datelor** - Informatiile sunt trimise la server
4. **Confirmarea** - Primiti un mesaj de succes sau eroare
5. **inchiderea automata** - Formularul se inchide la succes
6. **Actualizarea listei** - Lista de utilizatori se reincarca

#### in caz de eroare la salvare:
- **Mesajul de eroare** va aparea in partea de sus a formularului
- **Formularul ramane deschis** pentru corectari
- **Datele introduse** nu se pierd
- **Corectati problemele** si incercati din nou

### Anularea modificarilor

#### Cand sa folositi "Anuleaza":
- **Nu mai doriti** sa faceti modificari
- **Ati facut greseli** si vreti sa reluati
- **V-ati razgandit** privind crearea utilizatorului
- **Formularul nu functioneaza** corect

#### Ce se intampla la anulare:
- **Toate modificarile** se pierd (nu se salveaza)
- **Formularul se inchide** imediat  
- **Reveniti** la lista de utilizatori
- **Datele originale** raman neschimbate (la editare)

---

## Scenarii Speciale

### Editarea propriului cont

#### Ce puteti modifica:
- ✅ **Informatiile de contact** (telefon, email)
- ✅ **Informatiile organizationale** (daca aveti drepturi)

#### Ce NU puteti modifica:
- ❌ **Propriul rol** in sistem
- ❌ **Propriul status** (activ/inactiv)
- ❌ **Username-ul** (in majoritatea cazurilor)

### Editarea utilizatorilor cu rol superior

#### Restrictii pentru administratori:
- **Alti administratori** pot fi editati doar de super-administratori
- **Propriile permisiuni** nu pot fi reduse
- **Conturile de sistem** pot avea restrictii speciale

#### Mesaje de avertizare:
- ⚠️ **"Nu aveti permisiuni pentru a modifica acest rol"**
- ⚠️ **"Nu puteti modifica un utilizator cu drepturi superioare"**

### Editarea utilizatorilor inactivi

#### Pentru utilizatori inactivi:
- **Toate campurile** pot fi editare in mod normal
- **Statusul** poate fi schimbat la "Activ" pentru reactivare
- **Validarile** sunt identice cu cele pentru utilizatori activi

---

## Sfaturi pentru Eficienta

### Pentru crearea in masa

#### Daca aveti multi utilizatori de adaugat:
1. **Pregatiti o lista** cu toate datele necesare
2. **Folositi conventii** consistente pentru username-uri
3. **Verificati email-urile** inainte sa incepeti
4. **Lucrati** departament cu departament
5. **Testati primul utilizator** complet inainte sa continuati

### Pentru modificari in grup

#### Daca trebuie sa schimbati acelasi lucru la mai multi utilizatori:
1. **Notati utilizatorii** care au nevoie de modificare
2. **Faceti modificarile** una cate una
3. **Verificati rezultatele** dupa fiecare modificare
4. **Documentati schimbarile** importante

### Pentru organizare

#### Pastrarea unei organizari coerente:
- **Username-uri**: Folositi acelasi format (nume.prenume)
- **Email-uri**: Preferati adresele profesionale
- **Functii**: Folositi denumiri oficiale din organigrame
- **Departamente**: Mentineti lista actualizata

---

## Depanarea Problemelor

### Probleme frecvente si solutii

#### "Formularul nu se incarca"
**Cauze posibile**:
- Conexiune slaba la internet
- Probleme de permisiuni
- Eroare temporara de server

**Solutii**:
1. Reimprospatati pagina (F5)
2. Verificati conexiunea la internet
3. Deconectati-va si conectati-va din nou
4. Contactati suportul tehnic

#### "Nu pot sa salvez datele"
**Cauze posibile**:
- Campuri obligatorii necompletate
- Date in format gresit
- Username sau email deja folosite
- Probleme de permisiuni

**Solutii**:
1. Verificati toate mesajele de eroare rosii
2. Corectati campurile marcate cu erori  
3. incercati username sau email diferite
4. Verificati ca aveti dreptul sa creati/modificati utilizatori

#### "Formularele merg incet"
**Cauze posibile**:
- Multe aplicatii deschise simultan
- Browser vechi sau cu multe tab-uri
- Cache plin

**Solutii**:
1. inchideti tab-urile si aplicatiile neufolositoare
2. Curatati cache-ul browserului
3. Folositi un browser mai nou
4. Restartati browserul

---

## intrebari Frecvente

### 1. Pot sa creez utilizatori fara email?
**Raspuns:** Nu, email-ul este obligatoriu pentru toate conturile. Este folosit pentru comunicari si recuperarea parolei.

### 2. Ce se intampla cu parola pentru utilizatorii noi?
**Raspuns:** Sistemul genereaza automat o parola temporara care este trimisa pe email. Utilizatorul va trebui sa o schimbe la prima conectare.

### 3. Pot sa modific rolul unui utilizator in orice moment?
**Raspuns:** Da, daca aveti permisiuni pentru ambele roluri (cel vechi si cel nou). Modificarea este imediata.

### 4. Ce fac daca introduc din greseala un email gresit?
**Raspuns:** Editati utilizatorul si corectati email-ul. Daca utilizatorul nou nu si-a setat inca parola, nu va fi afectat.

### 5. Pot sa creez utilizatori cu acelasi nume si prenume?
**Raspuns:** Da, dar username-ul si email-ul trebuie sa fie diferite. Sistemul distinge utilizatorii dupa username.

### 6. De ce nu vad toate departamentele in lista?
**Raspuns:** Sunt afisate doar departamentele active si pentru care aveti permisiuni de asignare a personalului.

---

## Lista de Verificare

### inainte de a crea un utilizator nou:

- [ ] **Am toate informatiile** necesare despre persoana
- [ ] **Am verificat** ca persoana chiar lucreaza in clinica
- [ ] **Am stabilit rolul corect** pentru functia lor
- [ ] **Am verificat** ca email-ul si username-ul sunt unice
- [ ] **Am ales departamentul** si functia corecte

### inainte de a edita un utilizator:

- [ ] **Am confirmat** ca modificarile sunt autorizate
- [ ] **Am verificat** impactul schimbarii de rol (daca aplicabil)
- [ ] **Am informat utilizatorul** despre modificari (daca e relevant)
- [ ] **Am backup** la informatiile importante (daca e necesar)

### Dupa salvare:

- [ ] **Am verificat** ca utilizatorul apare corect in lista
- [ ] **Am testat** conectarea (pentru utilizatori noi)
- [ ] **Am informat utilizatorul** despre cont si date de conectare
- [ ] **Am documentat** modificarile importante

---

*Acest ghid va ajuta sa utilizati eficient formularele de adaugare si editare a utilizatorilor. Pentru probleme specifice sau intrebari tehnice, contactati echipa de suport.*

**Versiune document:** 1.0  
**Data actualizarii:** Septembrie 2025  
**Autor:** Echipa ValyanMed
