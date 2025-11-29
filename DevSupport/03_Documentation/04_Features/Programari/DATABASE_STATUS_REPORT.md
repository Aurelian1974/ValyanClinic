# 📊 **RAPORT STATUS BAZA DE DATE - MODUL PROGRAMĂRI**

**Data:** 2025-01-15  
**Database:** ValyanMed  
**Server:** DESKTOP-3Q8HI82\ERP  
**Status:** ❌ **INCOMPLETE** - Necesită implementare

---

## 🔍 **VERIFICARE EFECTUATĂ**

### ✅ **Conexiune Bază de Date**
```
Server: DESKTOP-3Q8HI82\ERP
Database: ValyanMed
Status: ✅ CONECTAT CU SUCCES
```

### 📋 **Tabele Verificate**

| Tabel | Status | Observații |
|-------|--------|------------|
| **Programari** | ❌ **NU EXISTĂ** | **CRITICAL** - Tabel principal lipsește! |
| Pacienti | ✅ EXISTĂ | OK - Necesare pentru FK (folosește `Id` nu `PacientID`) |
| PersonalMedical | ✅ EXISTĂ | OK - Necesare pentru FK (folosește `PersonalID`) |
| Consultatii | ✅ EXISTĂ | ⚠️ Are FK către Programari (care lipsește!) |
| Departamente | ✅ EXISTĂ | OK |
| Specializari | ✅ EXISTĂ | OK |
| Pozitii | ✅ EXISTĂ | OK |

### 📦 **Stored Procedures Verificate**

```sql
SELECT name FROM sys.procedures WHERE name LIKE 'sp_Programari_%'
```

**Rezultat:** ❌ **ZERO Stored Procedures găsite!**

**SP-uri necesare (lipsă):**
- ❌ `sp_Programari_GetAll` - Listă paginată cu filtrare
- ❌ `sp_Programari_GetCount` - Total pentru paginare
- ❌ `sp_Programari_GetById` - Detalii programare
- ❌ `sp_Programari_GetByDoctor` - Programări medic (interval)
- ❌ `sp_Programari_GetByDate` - Programări pe dată
- ❌ `sp_Programari_GetByPacient` - Programări pacient
- ❌ `sp_Programari_Create` - Creare programare nouă
- ❌ `sp_Programari_Update` - Modificare programare
- ❌ `sp_Programari_Delete` - Soft delete programare
- ❌ `sp_Programari_CheckConflict` - Verificare conflict orar

---

## 🎯 **ACȚIUNI NECESARE - PRIORITATE ÎNALTĂ**

## ✅ **DECIZII ARHITECTURALE FINALE**

### **Decizie 1: Structură Tabel - OPȚIUNEA B ALEASĂ** ✅
**Alegere:** `DataProgramare DATE + OraInceput TIME + OraSfarsit TIME` (mai flexibil)

**Motivație:**
- ✅ Mai flexibil pentru validări separate (dată vs. oră)
- ✅ Permite filtrare ușoară după dată
- ✅ Constraints separate pentru validare oră
- ✅ Mai ușor de afișat în UI (calendar)

### **Decizie 2: Primary Key Generation - NEWSEQUENTIALID()** ✅
**Alegere:** `NEWSEQUENTIALID()` în loc de `NEWID()`

**Motivație:**
- ✅ Performanță mai bună (reduce fragmentation index clustered)
- ✅ Sequential GUIDs sunt mai eficiente pentru INSERT
- ✅ Best practice pentru SQL Server

### **Decizie 3: Foreign Keys - Verificate** ✅
**Corecții necesare:**
- ✅ `Pacienti` folosește coloana `Id` (NU `PacientID`)
- ✅ `PersonalMedical` folosește coloana `PersonalID`

---

### **FAZA 1: Database Schema (URGENT)**

#### **1.1 Creare Tabel Programari - STRUCTURĂ FINALĂ**

**Script actualizat:** `DevSupport/Database/TableStructure/Programari_Complete.sql`

**Structură FINALĂ (cu corecțiile aplicate):**
```sql
CREATE TABLE dbo.Programari (
    -- Primary Key cu NEWSEQUENTIALID pentru performance
    [ProgramareID] UNIQUEIDENTIFIER NOT NULL 
        CONSTRAINT PK_Programari PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    
    -- Foreign Keys (CORECTATE!)
    [PacientID] UNIQUEIDENTIFIER NOT NULL,  -- FK către Pacienti.Id (NU PacientID!)
    [DoctorID] UNIQUEIDENTIFIER NOT NULL,   -- FK către PersonalMedical.PersonalID
    
    -- Data și Ora SEPARATE (Opțiunea B)
    [DataProgramare] DATE NOT NULL,
    [OraInceput] TIME NOT NULL,
    [OraSfarsit] TIME NOT NULL,
  
  -- Detalii programare
    [TipProgramare] NVARCHAR(100) NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Programata',
    [Observatii] NVARCHAR(1000) NULL,
    
    -- Audit fields
    [DataCreare] DATETIME2 NOT NULL DEFAULT GETDATE(),
  [CreatDe] UNIQUEIDENTIFIER NOT NULL,
    [DataUltimeiModificari] DATETIME2 NULL,
    [ModificatDe] UNIQUEIDENTIFIER NULL,
    
    -- Foreign Key Constraints (CORECTATE!)
    CONSTRAINT FK_Programari_Pacienti FOREIGN KEY (PacientID) 
        REFERENCES dbo.Pacienti(Id),  -- CORECTAT: Id nu PacientID!
    CONSTRAINT FK_Programari_Doctor FOREIGN KEY (DoctorID) 
     REFERENCES dbo.PersonalMedical(PersonalID),
    CONSTRAINT FK_Programari_CreatDe FOREIGN KEY (CreatDe) 
        REFERENCES dbo.PersonalMedical(PersonalID),
    
    -- Check Constraint pentru validare oră
    CONSTRAINT CK_Programari_OraValida CHECK (OraInceput < OraSfarsit)
);

-- Indexes pentru performance
CREATE NONCLUSTERED INDEX IX_Programari_Data_Doctor 
    ON dbo.Programari (DataProgramare ASC, DoctorID ASC);
    
CREATE NONCLUSTERED INDEX IX_Programari_Pacient_Status 
    ON dbo.Programari (PacientID ASC, Status ASC);
    
CREATE NONCLUSTERED INDEX IX_Programari_DataOraInceput
    ON dbo.Programari (DataProgramare ASC, OraInceput ASC);
```

**✅ CORECȚII APLICATE:**
1. ✅ `NEWSEQUENTIALID()` în loc de `NEWID()` pentru PK
2. ✅ Data și oră separate (`DATE + TIME + TIME`)
3. ✅ FK către `Pacienti.Id` (NU `PacientID`)
4. ✅ Index suplimentar pentru `DataProgramare + OraInceput`

#### **1.2 Verificare Foreign Keys - REZOLVATĂ** ✅

**FK către Pacienti - CORECTAT:**
```sql
-- Verificare coloana corectă (Id nu PacientID)
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Pacienti' AND COLUMN_NAME = 'Id'
-- REZULTAT AȘTEPTAT: Id
```

**FK către PersonalMedical - OK:**
```sql
-- Verificare coloana PersonalID
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PersonalMedical' AND COLUMN_NAME = 'PersonalID'
-- REZULTAT AȘTEPTAT: PersonalID
```

---

### **FAZA 2: Stored Procedures (URGENT)**

**Fișier necesar:** `DevSupport/Database/StoredProcedures/sp_Programari.sql`

**SP-uri de creat (conform altor module existente - Pacienti, Specializari, etc.):**

```sql
-- 1. sp_Programari_GetAll - Listă paginată cu filtrare
CREATE PROCEDURE sp_Programari_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @SearchText NVARCHAR(255) = NULL,
    @DoctorID UNIQUEIDENTIFIER = NULL,
    @PacientID UNIQUEIDENTIFIER = NULL,
    @DataStart DATE = NULL,
    @DataEnd DATE = NULL,
    @Status NVARCHAR(50) = NULL,
    @SortColumn NVARCHAR(50) = 'DataProgramare',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    -- Implementare cu paginare și filtrare
END
GO

-- 2. sp_Programari_GetCount - Total pentru paginare
CREATE PROCEDURE sp_Programari_GetCount
    @SearchText NVARCHAR(255) = NULL,
    @DoctorID UNIQUEIDENTIFIER = NULL,
    @PacientID UNIQUEIDENTIFIER = NULL,
    @DataStart DATE = NULL,
  @DataEnd DATE = NULL,
  @Status NVARCHAR(50) = NULL
AS
BEGIN
    SELECT COUNT(*) AS TotalCount FROM Programari WHERE 1=1 -- + filtre
END
GO

-- 3. sp_Programari_GetById
-- 4. sp_Programari_GetByDoctor
-- 5. sp_Programari_GetByDate
-- 6. sp_Programari_Create
-- 7. sp_Programari_Update
-- 8. sp_Programari_Delete
-- 9. sp_Programari_CheckConflict - IMPORTANT pentru validare
```

**Pattern de urmat:** Similar cu `sp_Pacienti.sql` și `sp_Specializari.sql` (deja existente)

---

### **FAZA 3: Application Layer**

**După ce DB este gata, trebuie creat:**

```
ValyanClinic.Application/Features/ProgramariManagement/
├── Commands/
│   ├── CreateProgramare/
│   │   ├── CreateProgramareCommand.cs
│   │   ├── CreateProgramareCommandHandler.cs
│   │   └── CreateProgramareValidator.cs (FluentValidation)
│   ├── UpdateProgramare/
│   └── DeleteProgramare/
├── Queries/
│   ├── GetProgramariList/
│   ├── GetProgramareById/
│   └── GetProgramariByDoctor/
└── DTOs/
    ├── ProgramareDTO.cs
    ├── ProgramareListItemDTO.cs
    ├── CreateProgramareRequest.cs
    └── UpdateProgramareRequest.cs
```

**Repository:**
```
ValyanClinic.Infrastructure/Repositories/
├── IProgramareRepository.cs
└── ProgramareRepository.cs (Dapper)
```

**Blazor UI:**
```
ValyanClinic/Components/Pages/Programari/
├── Calendar.razor (SfScheduler)
├── Calendar.razor.cs
├── ListaProgramari.razor (SfGrid)
├── ListaProgramari.razor.cs
└── Components/
    └── ProgramareDialog.razor (CRUD modal)
```

---

## 🚦 **ROADMAP IMPLEMENTARE**

### **Sprint 1: Database Foundation (1-2 zile)**
- [ ] **Step 1.1:** Decide structura finală tabel (DataProgramare vs. DataProgramare + Ora)
- [ ] **Step 1.2:** Modifică `Programari_Complete.sql` dacă e nevoie
- [ ] **Step 1.3:** Rulează script creare tabel
- [ ] **Step 1.4:** Verifică FK-uri funcționează
- [ ] **Step 1.5:** Crează SP-uri (minimum 8-10 SP-uri)
- [ ] **Step 1.6:** Testează SP-uri manual

### **Sprint 2: Application Layer (2-3 zile)**
- [ ] **Step 2.1:** Creare entitate `Programare.cs` (Domain)
- [ ] **Step 2.2:** Creare repository cu Dapper
- [ ] **Step 2.3:** Creare DTOs
- [ ] **Step 2.4:** Creare Commands + Handlers (MediatR)
- [ ] **Step 2.5:** Creare Queries + Handlers
- [ ] **Step 2.6:** FluentValidation validators
- [ ] **Step 2.7:** Unit tests (optional)

### **Sprint 3: UI Layer (3-4 zile)**
- [ ] **Step 3.1:** Calendar page cu Syncfusion SfScheduler
- [ ] **Step 3.2:** Lista Programari cu SfGrid
- [ ] **Step 3.3:** CRUD Dialog (Syncfusion SfDialog)
- [ ] **Step 3.4:** Validare conflict orar (client + server)
- [ ] **Step 3.5:** Integration testing
- [ ] **Step 3.6:** Manual UAT testing

**Total estimat:** **6-9 zile** pentru MVP complet

---

## 📝 **NEXT STEPS IMMEDIATE**

### **TODAY:**
1. ✅ **Decizie arhitecturală:** Structură tabel Programari
   - Opțiunea A: `DataProgramare DATETIME2` (mai simplu)
   - Opțiunea B: `DataProgramare DATE + OraInceput/OraSfarsit TIME` (mai flexibil)

2. 🔧 **Actualizare script SQL** (dacă alegem Opțiunea B)

3. ▶️ **Rulare script:** `Programari_Complete.sql` în SQL Server Management Studio

4. 📝 **Creare:** `sp_Programari.sql` cu toate SP-urile necesare

### **ЗАВТРА (Tomorrow):**
5. ✅ **Testare SP-uri** cu scriptul `Test-Programari.ps1` (de creat)

6. 🏗️ **Start Application Layer:** Domain entities + Repository

---

## 📊 **METRICS STATUS**

| Categorie | Status | Progres |
|-----------|--------|---------|
| **Database Schema** | ❌ Not Started | 0% |
| **Stored Procedures** | ❌ Not Started | 0% |
| **Application Layer** | ❌ Not Started | 0% |
| **UI Components** | ❌ Not Started | 0% |
| **Testing** | ❌ Not Started | 0% |

**Overall Progress:** **0%** ⚠️

---

## 🔗 **LINKURI UTILE**

### **Scripturi Existente (Template-uri):**
- ✅ `DevSupport/Database/TableStructure/Programari_Complete.sql` - Tabel (VERIFICAT - EXISTS!)
- ✅ `DevSupport/Database/StoredProcedures/sp_Pacienti.sql` - Template SP-uri
- ✅ `DevSupport/Database/StoredProcedures/sp_Specializari.sql` - Template SP-uri
- ✅ `DevSupport/Database/StoredProcedures/sp_Utilizatori.sql` - Template SP-uri

### **Planuri Implementare:**
- ✅ `DevSupport/Documentation/Programari/PLAN_IMPLEMENTARE_PROGRAMARI.md`
- ✅ `DevSupport/Documentation/Settings/SETTINGS_REQUIREMENTS.md` (Secțiunea Programări)

### **Scripturi Verificare:**
- ✅ `DevSupport/Scripts/PowerShellScripts/Check-ProgramariDatabase.ps1` (NOU - CREAT ACUM)
- ✅ `DevSupport/Scripts/PowerShellScripts/Quick-CheckProgramari.ps1` (NOU - CREAT ACUM)
- ✅ `DevSupport/Scripts/PowerShellScripts/Query-ValyanMedDatabase.ps1` (EXISTENT - FUNCȚIONAL)

---

## ✅ **CHECKLIST RAPID**

**Înainte de a începe implementarea:**

- [x] ✅ Verificare conexiune DB
- [x] ✅ Verificare tabele existente (Pacienti, PersonalMedical)
- [ ] ❌ Creare tabel Programari
- [ ] ❌ Creare stored procedures
- [ ] ❌ Testare manual SP-uri
- [ ] ❌ Implementare Application Layer
- [ ] ❌ Implementare UI Layer
- [ ] ❌ Testing complet

---

**Status:** ❌ **DATABASE INCOMPLETE** - Start cu FAZA 1  
**Next Action:** Rulează `Programari_Complete.sql` sau modifică-l conform MVP  
**Blocker:** NICIO implementare database pentru Programări  

**🎯 TARGET:** Finalizare MVP Programări în **6-9 zile** (cu toate cele 3 faze)

---

*Raport generat automat de: Check-ProgramariDatabase.ps1*  
*Data: 2025-01-15*  
*Versiune: 1.0*
