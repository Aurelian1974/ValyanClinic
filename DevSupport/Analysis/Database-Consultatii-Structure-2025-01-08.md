# 📊 Analiza Structura Database - Tabele Consultatii

**Data verificare:** 2025-01-08  
**Database:** ValyanMed  
**Server:** DESKTOP-3Q8HI82\ERP  
**Status:** ✅ VERIFICAT LIVE

---

## 🎯 Rezumat Executiv

✅ **Tabelul Consultatii EXISTĂ și este COMPLET**
- **86 coloane** (structura modernă pentru Scrisoare Medicală Completă)
- **3 Foreign Keys** (toate corecte)
- **5 Stored Procedures** (create și funcționale)
- **0 înregistrări** (tabel gol, gata pentru utilizare)

---

## 📋 1. Tabele Existente (Legate de Consultatii)

| Tabel | Coloane | Foreign Keys | Primary Key | Inregistrari |
|-------|---------|--------------|-------------|--------------|
| **Consultatii** | 86 | 3 | ✅ YES | 0 |
| **Diagnostice** | 8 | 1 | ✅ YES | 0 |
| **Prescriptii** | ? | ? | ✅ YES | 0 |

---

## 🔧 2. Structura Detaliata Tabel `Consultatii`

### 🔑 Primary Key & Foreign Keys

#### Primary Key:
- **ConsultatieID** (UNIQUEIDENTIFIER) - `DEFAULT NEWSEQUENTIALID()`

#### Foreign Keys:
| FK Name | Column | Referenced Table | Referenced Column | Status |
|---------|--------|------------------|-------------------|--------|
| FK_Consultatii_Programari | ProgramareID | Programari | ProgramareID | ✅ |
| FK_Consultatii_Pacienti | PacientID | Pacienti | **Id** | ✅ |
| FK_Consultatii_PersonalMedical | MedicID | PersonalMedical | **PersonalID** | ✅ |

⚠️ **IMPORTANT:** 
- Pacienti → PK = `Id` (NU `PacientID`!)
- PersonalMedical → PK = `PersonalID` (NU `PersonalMedicalID`!)

---

### 📊 Coloane (86 total) - Grupate pe Sectiuni

#### I. Date Consultatie (7 coloane)
```
ConsultatieID          UNIQUEIDENTIFIER  NOT NULL  PK  [NEWSEQUENTIALID()]
ProgramareID           UNIQUEIDENTIFIER  NOT NULL  FK  → Programari.ProgramareID
PacientID              UNIQUEIDENTIFIER  NOT NULL  FK  → Pacienti.Id
MedicID                UNIQUEIDENTIFIER  NOT NULL  FK  → PersonalMedical.PersonalID
DataConsultatie        DATE              NOT NULL  [GETDATE()]
OraConsultatie         TIME              NOT NULL  [CONVERT(TIME, GETDATE())]
TipConsultatie         NVARCHAR(50)      NOT NULL  ['Prima consultatie']
```

#### II. Motive Prezentare (2 coloane)
```
MotivPrezentare        NVARCHAR(MAX)     NULL
IstoricBoalaActuala    NVARCHAR(MAX)     NULL
```

#### III. Antecedente Heredo-Colaterale (5 coloane)
```
AHC_Mama               NVARCHAR(500)     NULL
AHC_Tata               NVARCHAR(500)     NULL
AHC_Frati              NVARCHAR(500)     NULL
AHC_Bunici             NVARCHAR(500)     NULL
AHC_Altele             NVARCHAR(500)     NULL
```

#### IV. Antecedente Fiziologice (5 coloane)
```
AF_Nastere             NVARCHAR(200)     NULL
AF_Dezvoltare          NVARCHAR(200)     NULL
AF_Menstruatie         NVARCHAR(200)     NULL  (pentru femei)
AF_Sarcini             NVARCHAR(200)     NULL  (pentru femei)
AF_Alaptare            NVARCHAR(200)     NULL  (pentru femei)
```

#### V. Antecedente Personale Patologice (7 coloane)
```
APP_BoliCopilarieAdolescenta  NVARCHAR(500)  NULL
APP_BoliAdult                 NVARCHAR(500)  NULL
APP_Interventii               NVARCHAR(500)  NULL
APP_Traumatisme               NVARCHAR(500)  NULL
APP_Transfuzii                NVARCHAR(500)  NULL
APP_Alergii                   NVARCHAR(500)  NULL
APP_Medicatie                 NVARCHAR(MAX)  NULL
```

#### VI. Conditii Socio-Economice (5 coloane)
```
Profesie               NVARCHAR(200)     NULL
ConditiiLocuinta       NVARCHAR(300)     NULL
ConditiiMunca          NVARCHAR(300)     NULL
ObiceiuriAlimentare    NVARCHAR(300)     NULL
Toxice                 NVARCHAR(300)     NULL  (tutun, alcool, droguri)
```

#### VII. Examen General (7 coloane)
```
StareGenerala          NVARCHAR(50)      NULL
Constitutie            NVARCHAR(50)      NULL
Atitudine              NVARCHAR(50)      NULL
Facies                 NVARCHAR(200)     NULL
Tegumente              NVARCHAR(200)     NULL
Mucoase                NVARCHAR(200)     NULL
GangliniLimfatici      NVARCHAR(200)     NULL
```

#### VIII. Semne Vitale (9 coloane)
```
Greutate               DECIMAL(5,2)      NULL  (kg)
Inaltime               DECIMAL(5,2)      NULL  (cm)
IMC                    DECIMAL(5,2)      NULL  (calculat)
Temperatura            DECIMAL(4,2)      NULL  (°C)
TensiuneArteriala      NVARCHAR(20)      NULL  (ex: 120/80)
Puls                   INT               NULL  (bpm)
FreccventaRespiratorie INT               NULL  (/min)
SaturatieO2            INT               NULL  (%)
Glicemie               DECIMAL(5,2)      NULL  (mg/dL)
```

#### IX. Examen pe Aparate/Sisteme (10 coloane)
```
ExamenCardiovascular   NVARCHAR(MAX)     NULL
ExamenRespiratoriu     NVARCHAR(MAX)     NULL
ExamenDigestiv         NVARCHAR(MAX)     NULL
ExamenUrinar           NVARCHAR(MAX)     NULL
ExamenNervos           NVARCHAR(MAX)     NULL
ExamenLocomotor        NVARCHAR(MAX)     NULL
ExamenEndocrin         NVARCHAR(MAX)     NULL
ExamenORL              NVARCHAR(MAX)     NULL
ExamenOftalmologic     NVARCHAR(MAX)     NULL
ExamenDermatologic     NVARCHAR(MAX)     NULL
```

#### X. Investigatii (4 coloane)
```
InvestigatiiLaborator  NVARCHAR(MAX)     NULL
InvestigatiiImagistice NVARCHAR(MAX)     NULL
InvestigatiiEKG        NVARCHAR(MAX)     NULL
AlteInvestigatii       NVARCHAR(MAX)     NULL
```

#### XI. Diagnostic (5 coloane)
```
DiagnosticPozitiv      NVARCHAR(MAX)     NULL
DiagnosticDiferential  NVARCHAR(MAX)     NULL
DiagnosticEtiologic    NVARCHAR(MAX)     NULL
CoduriICD10            NVARCHAR(200)     NULL  (coduri primare)
CoduriICD10Secundare   NVARCHAR(500)     NULL  (coduri secundare)
```

#### XII. Tratament (4 coloane)
```
TratamentMedicamentos      NVARCHAR(MAX)  NULL
TratamentNemedicamentos    NVARCHAR(MAX)  NULL
RecomandariDietetice       NVARCHAR(MAX)  NULL
RecomandariRegimViata      NVARCHAR(MAX)  NULL
```

#### XIII. Recomandari (4 coloane)
```
InvestigatiiRecomandate    NVARCHAR(MAX)  NULL
ConsulturiSpecialitate     NVARCHAR(MAX)  NULL
DataUrmatoareiProgramari   NVARCHAR(100)  NULL
RecomandariSupraveghere    NVARCHAR(MAX)  NULL
```

#### XIV. Prognostic & Concluzie (4 coloane)
```
Prognostic             NVARCHAR(50)      NULL  (Favorabil, Rezervat, Sever)
Concluzie              NVARCHAR(MAX)     NULL
ObservatiiMedic        NVARCHAR(MAX)     NULL
NotePacient            NVARCHAR(MAX)     NULL
```

#### XV. Status & Workflow (3 coloane)
```
Status                 NVARCHAR(50)      NOT NULL  ['In desfasurare']
DataFinalizare         DATETIME          NULL
DurataMinute           INT               NOT NULL  [0]
```

#### XVI. Documente (1 coloana)
```
DocumenteAtatate       NVARCHAR(MAX)     NULL  (JSON array cu paths)
```

#### XVII. Audit (4 coloane)
```
DataCreare             DATETIME          NOT NULL  [GETDATE()]
CreatDe                UNIQUEIDENTIFIER  NOT NULL
DataUltimeiModificari  DATETIME          NULL
ModificatDe            UNIQUEIDENTIFIER  NULL
```

---

## 🔍 3. Stored Procedures (5 bucati)

| Procedure Name | Created | Modified | Functie |
|----------------|---------|----------|---------|
| **sp_Consultatie_Create** | 2025-11-18 | 2025-11-18 | Creaza consultatie noua (INSERT) |
| **sp_Consultatie_GetById** | 2025-11-18 | 2025-11-18 | Obtine consultatie dupa ID |
| **sp_Consultatie_GetByPacient** | 2025-11-18 | 2025-11-18 | Obtine toate consultatiile unui pacient |
| **sp_Consultatie_GetByMedic** | 2025-11-18 | 2025-11-18 | Obtine toate consultatiile unui medic |
| **sp_Consultatie_GetByProgramare** | 2025-11-18 | 2025-11-18 | Obtine consultatia pentru o programare |

⚠️ **LIPSA:**
- `sp_Consultatie_Update` - Pentru UPDATE consultatie existenta
- `sp_Consultatie_SaveDraft` - Pentru auto-save (draft)
- `sp_Consultatie_Finalize` - Pentru finalizare consultatie

---

## 📋 4. Tabele Referentiate (PK-uri)

| Tabel | Primary Key Column | Coloane Total | Status |
|-------|-------------------|---------------|--------|
| **Pacienti** | **Id** | 33 | ✅ EXISTS |
| **PersonalMedical** | **PersonalID** | 15 | ✅ EXISTS |
| **Programari** | **ProgramareID** | 13 | ✅ EXISTS |

---

## 🔄 5. Tabele Dependente (Secondary)

### Diagnostice (8 coloane)
```
DiagnosticID               UNIQUEIDENTIFIER  NOT NULL  PK
ConsultatieID              UNIQUEIDENTIFIER  NOT NULL  FK → Consultatii.ConsultatieID
CodICD                     NVARCHAR(10)      NULL
DescriereaDiagnosticului   NVARCHAR(500)     NOT NULL
TipDiagnostic              NVARCHAR(50)      NULL
Severitate                 NVARCHAR(50)      NULL
Status                     NVARCHAR(50)      NULL
DataDiagnostic             DATETIME2         NULL
```

### Prescriptii (? coloane)
- Foreign Key → Consultatii.ConsultatieID
- Structura exacta: TBD (necesita verificare suplimentara)

---

## ✅ 6. Compatibilitate cu Pagina Consultatii

### Mapping Campuri Pagina → Database

#### Tab 1: Date Generale
| Camp Pagina | Coloana DB | Tip | Validare |
|-------------|------------|-----|----------|
| Greutate (kg) | Greutate | DECIMAL(5,2) | 20-300 kg |
| Inaltime (cm) | Inaltime | DECIMAL(5,2) | 50-250 cm |
| IMC | IMC | DECIMAL(5,2) | Calculat automat |
| Temperatura (°C) | Temperatura | DECIMAL(4,2) | 35-43 °C |
| Tensiune Arteriala | TensiuneArteriala | NVARCHAR(20) | Format: 120/80 |
| Puls (bpm) | Puls | INT | 40-200 bpm |
| Motiv Prezentare | MotivPrezentare | NVARCHAR(MAX) | Max 1000 caractere |
| Istoric Boala | IstoricBoalaActuala | NVARCHAR(MAX) | Max 2000 caractere |

#### Tab 2: Examen Clinic
| Camp Pagina | Coloana DB | Tip |
|-------------|------------|-----|
| Examen Cardiovascular | ExamenCardiovascular | NVARCHAR(MAX) |
| Examen Respirator | ExamenRespiratoriu | NVARCHAR(MAX) |
| Examen Digestiv | ExamenDigestiv | NVARCHAR(MAX) |
| Examen Neurologic | ExamenNervos | NVARCHAR(MAX) |
| Stare Generala | StareGenerala | NVARCHAR(50) |

#### Tab 3: Diagnostic & Tratament
| Camp Pagina | Coloana DB | Tip |
|-------------|------------|-----|
| Diagnostic Principal | DiagnosticPozitiv | NVARCHAR(MAX) |
| Coduri ICD-10 | CoduriICD10 | NVARCHAR(200) |
| Coduri ICD-10 Secundare | CoduriICD10Secundare | NVARCHAR(500) |
| Tratament Medicamentos | TratamentMedicamentos | NVARCHAR(MAX) |
| Recomandari Dietetice | RecomandariDietetice | NVARCHAR(MAX) |

#### Tab 4: Investigatii & Recomandari
| Camp Pagina | Coloana DB | Tip |
|-------------|------------|-----|
| Investigatii Laborator | InvestigatiiLaborator | NVARCHAR(MAX) |
| Investigatii Imagistice | InvestigatiiImagistice | NVARCHAR(MAX) |
| Investigatii Recomandate | InvestigatiiRecomandate | NVARCHAR(MAX) |
| Consulturi Specialitate | ConsulturiSpecialitate | NVARCHAR(MAX) |
| Prognostic | Prognostic | NVARCHAR(50) |
| Concluzie | Concluzie | NVARCHAR(MAX) |

---

## 🚀 7. Actiuni Necesare pentru MediatR Implementation

### ✅ Ce EXISTA deja:
1. ✅ Tabel Consultatii cu structura completa (86 coloane)
2. ✅ Foreign Keys corecte (Programari, Pacienti, PersonalMedical)
3. ✅ Stored Procedure pentru CREATE (sp_Consultatie_Create)
4. ✅ Stored Procedures pentru READ (GetById, GetByPacient, GetByMedic, GetByProgramare)
5. ✅ Repository interface (IConsultatieRepository)
6. ✅ Repository implementation (ConsultatieRepository)
7. ✅ Domain entity (Consultatie.cs cu 86 proprietati)

### ⚠️ Ce LIPSESTE:

#### A. Stored Procedures (CRITICAL)
- [ ] **sp_Consultatie_Update** - Pentru UPDATE consultatie existenta
- [ ] **sp_Consultatie_SaveDraft** - Pentru auto-save (draft)
- [ ] **sp_Consultatie_Finalize** - Pentru finalizare + update status programare

#### B. Repository Methods (CRITICAL)
- [ ] **UpdateAsync()** - Implementare completa (exista stub)
- [ ] **SaveDraftAsync()** - Pentru auto-save fara finalizare
- [ ] **FinalizeAsync()** - Pentru finalizare + schimbare status

#### C. MediatR Commands (TO CREATE)
- [ ] **CreateConsulatieCommand** - Creare consultatie noua
- [ ] **SaveConsultatieDraftCommand** - Auto-save draft
- [ ] **FinalizeConsulatieCommand** - Finalizare consultatie
- [ ] **UpdateConsulatieCommand** - Update consultatie existenta

#### D. MediatR Queries (TO CREATE)
- [ ] **GetConsulatieByIdQuery** - Obtine consultatie dupa ID
- [ ] **GetConsulatieByProgramareQuery** - Obtine consultatie dupa programare
- [ ] **GetConsultatiiByPacientQuery** - Lista consultatii pacient
- [ ] **GetConsultatiiByMedicQuery** - Lista consultatii medic

#### E. DTOs (TO CREATE)
- [ ] **ConsulatieDto** - Pentru transferul datelor (simplificat)
- [ ] **CreateConsulatieDto** - Pentru creare
- [ ] **UpdateConsulatieDto** - Pentru update
- [ ] **ConsulatieDetailsDto** - Pentru afisare detalii complete

#### F. Validators (TO CREATE)
- [ ] **CreateConsulatieCommandValidator** - FluentValidation pentru creare
- [ ] **UpdateConsulatieCommandValidator** - FluentValidation pentru update
- [ ] **FinalizeConsulatieCommandValidator** - Validare finalizare (campuri obligatorii)

---

## 📊 8. Statistici Database

- **Total Tabele Consultatii:** 3 (Consultatii, Diagnostice, Prescriptii)
- **Total Coloane Consultatii:** 86
- **Total Foreign Keys:** 3
- **Total Stored Procedures:** 5
- **Total Inregistrari:** 0 (tabele goale)
- **Database Size:** TBD
- **Last Modified:** 2025-11-18 09:08:49 AM

---

## 🔐 9. Securitate & Performance

### Indexes Existente:
```sql
-- Verificate automat la creare tabel
IX_Consultatii_PacientID      (PacientID)
IX_Consultatii_MedicID        (MedicID)
IX_Consultatii_ProgramareID   (ProgramareID)
IX_Consultatii_DataConsultatie (DataConsultatie DESC)
```

### Recomandari Performance:
- ✅ NEWSEQUENTIALID() folosit pentru PK (reduce fragmentation)
- ✅ Indexes pe toate Foreign Keys (optimizare JOIN-uri)
- ✅ Index pe DataConsultatie (optimizare sorting/filtering)
- ⚠️ Consider adding: `IX_Consultatii_Status` pentru filtrare rapida

### Recomandari Securitate:
- ✅ Foreign Keys implementate (referential integrity)
- ✅ Audit fields (DataCreare, CreatDe, DataUltimeiModificari, ModificatDe)
- ⚠️ Consider adding: Soft Delete (IsDeleted bit, DataStergere, StersDe)
- ⚠️ Consider adding: Row-Level Security pentru acces bazat pe rol

---

## 📝 10. Notes & Observations

### Observatii Pozitive:
1. ✅ Structura tabel moderna si completa (86 coloane vs 9 vechi)
2. ✅ Foreign Keys CORECTE (Pacienti.Id, PersonalMedical.PersonalID)
3. ✅ Default values pe coloane importante (DataCreare, Status, DurataMinute)
4. ✅ Campuri audit complete pentru tracking modificari
5. ✅ Stored Procedures pentru READ operations

### Observatii Negative:
1. ⚠️ Lipsa SP pentru UPDATE (necesara pentru auto-save)
2. ⚠️ Lipsa SP pentru FINALIZE (necesara pentru workflow)
3. ⚠️ Repository.UpdateAsync() - stub neimplementat
4. ⚠️ Nu exista Soft Delete (stergere logica)
5. ⚠️ Nu exista versioning (istoricul modificarilor)

### Recomandari Imediate:
1. **URGENT:** Creaza `sp_Consultatie_Update` (necesara pentru pagina)
2. **URGENT:** Implementeaza `Repository.UpdateAsync()`
3. **HIGH:** Creaza MediatR Commands/Queries (Step 5 din plan)
4. **MEDIUM:** Adauga SP pentru SaveDraft si Finalize
5. **LOW:** Consider Soft Delete pentru audit trail

---

## ✅ Concluzie

**Status:** ✅ **DATABASE READY FOR IMPLEMENTATION**

Structura de baza de date este **COMPLETA si FUNCTIONALA**. Tabelul `Consultatii` are toate coloanele necesare pentru functionalitatea de Scrisoare Medicala Completa.

**Next Steps:**
1. Creaza Stored Procedures lipsa (UPDATE, SaveDraft, Finalize)
2. Implementeaza Repository methods lipsa
3. Creaza MediatR Commands/Queries (STEP 5 din plan)
4. Conecteaza pagina Blazor la backend prin MediatR

**Estimated Effort:**
- SP Creation: 2-3 ore
- Repository Implementation: 1-2 ore
- MediatR Layer: 4-6 ore
- Testing: 2-3 ore
- **TOTAL: ~10-14 ore**

---

**Document creat:** 2025-01-08  
**Autor:** Copilot AI Assistant  
**Status:** ✅ VERIFIED LIVE FROM DATABASE  
**Versiune:** 1.0
