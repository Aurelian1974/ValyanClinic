# ✅ **CORECȚII APLICATE - SCRIPT PROGRAMARI**

**Data:** 2025-01-15  
**Fișier actualizat:** `DevSupport/Database/TableStructure/Programari_Complete.sql`  
**Status:** ✅ **READY TO RUN**

---

## 🔧 **CORECȚII ARHITECTURALE APLICATE**

### **1. NEWSEQUENTIALID() pentru Primary Key** ✅

**ÎNAINTE:**
```sql
[ProgramareID] UNIQUEIDENTIFIER NOT NULL,
CONSTRAINT [PK_Programari] PRIMARY KEY ([ProgramareID])
-- Cu DEFAULT NEWID() sau fără default
```

**DUPĂ (CORECTAT):**
```sql
[ProgramareID] UNIQUEIDENTIFIER NOT NULL 
  CONSTRAINT PK_Programari PRIMARY KEY DEFAULT NEWSEQUENTIALID()
```

**Beneficii:**
- ✅ **Performance:** Reduce fragmentation în clustered index
- ✅ **Sequential:** GUIDs generate în ordine
- ✅ **Best Practice:** Recomandat pentru SQL Server

---

### **2. Data și Ora SEPARATE** ✅

**ÎNAINTE (Script vechi):**
```sql
[DataProgramare] DATETIME2 NOT NULL,
[TipProgramare] NVARCHAR(100) NULL,
[Status] NVARCHAR(50) NULL
-- Fără OraInceput/OraSfarsit separate
```

**DUPĂ (CORECTAT - Opțiunea B):**
```sql
[DataProgramare] DATE NOT NULL,
[OraInceput] TIME NOT NULL,
[OraSfarsit] TIME NOT NULL,
[TipProgramare] NVARCHAR(100) NULL,
[Status] NVARCHAR(50) NOT NULL DEFAULT 'Programata'
```

**Beneficii:**
- ✅ **Flexibilitate:** Validări separate pentru dată și oră
- ✅ **Query Performance:** Filtrare ușoară după dată
- ✅ **UI Friendly:** Mai ușor de afișat în calendar
- ✅ **Validări:** Check constraint `OraInceput < OraSfarsit`

---

### **3. Foreign Key către Pacienti.Id (NU PacientID)** ✅

**VERIFICARE EFECTUATĂ:**
```sql
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Pacienti' AND COLUMN_NAME IN ('Id', 'PacientID')
```

**REZULTAT:** ✅ Tabelul `Pacienti` folosește coloana `Id`

**ÎNAINTE (Script vechi - GREȘIT):**
```sql
ALTER TABLE dbo.Programari
ADD CONSTRAINT FK__Programar__Pacie__370627FE FOREIGN KEY ([PacientID]) 
    REFERENCES dbo.[Pacienti] ([PacientID])  -- GREȘIT!
```

**DUPĂ (CORECTAT):**
```sql
ALTER TABLE dbo.Programari
ADD CONSTRAINT FK_Programari_Pacienti FOREIGN KEY (PacientID) 
    REFERENCES dbo.Pacienti(Id)  -- CORECT: Id nu PacientID!
```

**Verificare FK către PersonalMedical:**
```sql
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PersonalMedical' AND COLUMN_NAME = 'PersonalID'
```
**REZULTAT:** ✅ Coloana `PersonalID` există - FK corect!

---

## 📊 **STRUCTURĂ FINALĂ TABEL**

```sql
CREATE TABLE dbo.Programari (
    -- Primary Key cu NEWSEQUENTIALID
    [ProgramareID] UNIQUEIDENTIFIER NOT NULL 
        CONSTRAINT PK_Programari PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    
    -- Foreign Keys (VERIFICATE ȘI CORECTATE!)
    [PacientID] UNIQUEIDENTIFIER NOT NULL,-- FK → Pacienti.Id ✅
    [DoctorID] UNIQUEIDENTIFIER NOT NULL,    -- FK → PersonalMedical.PersonalID ✅
    
    -- Data și Ora SEPARATE (Opțiunea B)
    [DataProgramare] DATE NOT NULL,
    [OraInceput] TIME NOT NULL,
    [OraSfarsit] TIME NOT NULL,
  
    -- Detalii
    [TipProgramare] NVARCHAR(100) NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Programata',
    [Observatii] NVARCHAR(1000) NULL,
    
    -- Audit
    [DataCreare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [CreatDe] UNIQUEIDENTIFIER NOT NULL,       -- FK → PersonalMedical.PersonalID ✅
  [DataUltimeiModificari] DATETIME2 NULL,
    [ModificatDe] UNIQUEIDENTIFIER NULL,
    
    -- Constraints
    CONSTRAINT FK_Programari_Pacienti FOREIGN KEY (PacientID) 
        REFERENCES dbo.Pacienti(Id),
    CONSTRAINT FK_Programari_Doctor FOREIGN KEY (DoctorID) 
        REFERENCES dbo.PersonalMedical(PersonalID),
    CONSTRAINT FK_Programari_CreatDe FOREIGN KEY (CreatDe) 
        REFERENCES dbo.PersonalMedical(PersonalID),
    CONSTRAINT CK_Programari_OraValida CHECK (OraInceput < OraSfarsit)
);

-- 3 Indexes pentru performance
CREATE NONCLUSTERED INDEX IX_Programari_Data_Doctor 
    ON dbo.Programari (DataProgramare ASC, DoctorID ASC);
    
CREATE NONCLUSTERED INDEX IX_Programari_Pacient_Status 
    ON dbo.Programari (PacientID ASC, Status ASC);
    
CREATE NONCLUSTERED INDEX IX_Programari_DataOraInceput
    ON dbo.Programari (DataProgramare ASC, OraInceput ASC);
```

---

## ✅ **VERIFICARE FINALĂ**

### **Checklist Corecții:**

- [x] ✅ **NEWSEQUENTIALID()** în loc de NEWID()
- [x] ✅ **Data și ora separate** (DATE + TIME + TIME)
- [x] ✅ **FK către Pacienti.Id** (verificat în DB: coloana `Id` există)
- [x] ✅ **FK către PersonalMedical.PersonalID** (verificat în DB: coloana există)
- [x] ✅ **Check Constraint** pentru validare oră (OraInceput < OraSfarsit)
- [x] ✅ **3 Indexes** pentru queries frecvente
- [x] ✅ **Status DEFAULT** 'Programata'
- [x] ✅ **DROP statements** pentru FK constraints înaintea drop table

---

## 🚀 **GATA DE RULARE!**

### **Pași Următori:**

1. **ACUM - Rulează scriptul actualizat:**
   ```powershell
   # În SQL Server Management Studio (SSMS)
   # Deschide: DevSupport/Database/TableStructure/Programari_Complete.sql
   # Execută (F5)
   ```

2. **Verifică rezultatul:**
   ```powershell
   cd D:\Lucru\CMS\DevSupport\Scripts\PowerShellScripts
   .\Quick-CheckProgramari.ps1
   ```

   **Rezultat așteptat:**
   ```
   ✓ Tabelul Programari EXISTA
   ✓ Gasite 3 foreign keys
   ✓ Pacienti
   ✓ PersonalMedical
   ```

3. **NEXT: Creează Stored Procedures**
   - Fișier: `DevSupport/Database/StoredProcedures/sp_Programari.sql`
   - Template: `sp_Pacienti.sql` sau `sp_Specializari.sql`

---

## 📋 **EXEMPLU DATE TEST**

După creare tabel, poți testa cu:

```sql
-- INSERT test (după ce tabelul e creat)
DECLARE @TestPacientID UNIQUEIDENTIFIER;
DECLARE @TestDoctorID UNIQUEIDENTIFIER;
DECLARE @TestUserID UNIQUEIDENTIFIER;

-- Obține ID-uri reale din DB
SELECT TOP 1 @TestPacientID = Id FROM Pacienti WHERE Activ = 1;
SELECT TOP 1 @TestDoctorID = PersonalID FROM PersonalMedical WHERE EsteActiv = 1;
SELECT TOP 1 @TestUserID = PersonalID FROM PersonalMedical WHERE EsteActiv = 1;

-- Inserare programare test
INSERT INTO Programari (
    PacientID, DoctorID, DataProgramare, OraInceput, OraSfarsit,
    TipProgramare, Status, Observatii, CreatDe
)
VALUES (
    @TestPacientID,
    @TestDoctorID,
    CAST(DATEADD(DAY, 1, GETDATE()) AS DATE),  -- Mâine
'09:00',  -- 9:00 AM
    '09:30',  -- 9:30 AM
    'Consultatie Generala',
    'Programata',
  'Programare test creată automat',
    @TestUserID
);

-- Verificare
SELECT * FROM Programari;
```

---

## 🎉 **READY TO GO!**

**Script:** ✅ Corectat complet  
**FK-uri:** ✅ Verificate în DB  
**Structure:** ✅ Conform best practices  
**Performance:** ✅ NEWSEQUENTIALID + Indexes  

**🚀 Poți rula scriptul ACUM în SSMS!**

---

*Document creat: 2025-01-15*  
*Script verificat și corectat: ✅ READY*  
*Următorul pas: Rulează `Programari_Complete.sql`*
