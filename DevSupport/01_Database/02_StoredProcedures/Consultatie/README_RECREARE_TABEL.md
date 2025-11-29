# 🔧 Script Recreare Tabel Consultatii

## 📋 Problema Identificată

Tabelul `Consultatii` existent în baza de date **ValyanMed** are o structură VECHE cu doar **9 coloane**, incompatibilă cu noua funcționalitate de **Scrisoare Medicală Completă**.

### ⚠️ IMPORTANT - Foreign Keys Corecte!

**Structura PK-urilor:**
- **Pacienti**: PK = `Id` (NU `PacientID`!)
- **PersonalMedical**: PK = `PersonalID` (NU `PersonalMedicalID`!)
- **Programari**: PK = `ProgramareID` ✓

**Vezi detalii complete în:** `FK_MAPPING_CORRECT.md`

### Structură VECHE (existentă):
```sql
- ConsultatieID (UNIQUEIDENTIFIER)
- ProgramareID (UNIQUEIDENTIFIER) + FK
- PlangereaPrincipala (NVARCHAR(1000))
- IstoricBoalaActuala (NVARCHAR(2000))
- ExamenFizic (NVARCHAR(2000))
- Evaluare (NVARCHAR(1000))
- Plan (NVARCHAR(1000))
- DataConsultatie (DATETIME2)
- Durata (INT)
```

### Structură NOUĂ (necesară):
```sql
- 80+ coloane pentru Scrisoare Medicală Completă
- Secțiuni: Motive, Antecedente, Examen Obiectiv, Investigații, Diagnostic, Tratament
- NEWSEQUENTIALID() pentru Primary Key (performanță îmbunătățită)
- Foreign Keys CORECTE:
  ✓ Programari(ProgramareID)
  ✓ Pacienti(Id) ← ATENTIE: Id nu PacientID!
  ✓ PersonalMedical(PersonalID) ← ATENTIE: PersonalID nu PersonalMedicalID!
```

---

## 🚀 Pași de Execuție

### 1️⃣ Verificare Structură Existentă

Rulează scriptul PowerShell de verificare:

```powershell
cd D:\Lucru\CMS
.\DevSupport\Scripts\PowerShellScripts\Check-ConsultatiiTable.ps1
```

**Output așteptat:**
- ✅ Afișează structura actuală (9 coloane)
- ✅ Listează Foreign Keys existente
- ✅ Generează script DROP cu ștergere FK
- ✅ Salvează script în: `DevSupport\Database\Scripts\DROP_Consultatii_WithFK.sql`

---

### 2️⃣ Backup Date Existente (IMPORTANT!)

⚠️ **ATENȚIE:** Recrearea tabelului va **ȘTERGE TOATE DATELE**!

```sql
-- Conectare la ValyanMed
USE ValyanMed;
GO

-- Verificare date existente
SELECT COUNT(*) AS NumarConsultatii FROM Consultatii;

-- BACKUP date (dacă există)
SELECT * 
INTO Consultatii_Backup_20250108
FROM Consultatii;
GO

-- Verificare backup
SELECT COUNT(*) AS NumarBackup FROM Consultatii_Backup_20250108;
```

---

### 3️⃣ Recreare Tabel Consultatii

Rulează scriptul SQL principal:

**Fișier:** `DevSupport\Database\StoredProcedures\Consultatie\Consultatie_StoredProcedures.sql`  
**Versiune:** 1.3 (FINAL - cu FK-uri CORECTE)

**Ce face scriptul:**
1. ✅ Șterge automat toate Foreign Keys (cele care referă Consultatii)
2. ✅ Șterge tabelul Consultatii vechi
3. ✅ Creează tabelul nou cu structura completă
4. ✅ Adaugă Foreign Keys noi (CORECTE!):
   - `FK_Consultatii_Programari` → Programari(ProgramareID) ✓
   - `FK_Consultatii_Pacienti` → **Pacienti(Id)** ⚠️ Nu PacientID!
   - `FK_Consultatii_PersonalMedical` → **PersonalMedical(PersonalID)** ⚠️ Nu PersonalMedicalID!
5. ✅ Creează Stored Procedures (5 bucăți):
   - `sp_Consultatie_Create`
   - `sp_Consultatie_GetById`
   - `sp_Consultatie_GetByPacient`
   - `sp_Consultatie_GetByMedic`
   - `sp_Consultatie_GetByProgramare`

**Execuție în SSMS:**
```sql
-- Deschide fișierul în SSMS
-- File → Open → File...
-- Selectează: DevSupport\Database\StoredProcedures\Consultatie\Consultatie_StoredProcedures.sql

-- SAU copiază conținutul și rulează
USE ValyanMed;
GO
-- [paste script here]
```

**Output așteptat:**
```
========================================
Initiere recreare tabel Consultatii
Database: ValyanMed
========================================

1. Verificare si stergere Foreign Keys...
   ✓ Foreign Keys sterse cu succes (sau nu exista)

2. Stergere tabel Consultatii (daca exista)...
   ✓ Tabel Consultatii sters cu succes

3. Creare tabel Consultatii cu structura noua...
   ✓ Tabel Consultatii creat cu succes
   ✓ Coloane: 80+ (Scrisoare Medicala Completa)
   ✓ Primary Key: ConsultatieID (NEWSEQUENTIALID)
   ✓ Indexes: 4 (PacientID, MedicID, ProgramareID, DataConsultatie)

4. Adaugare Foreign Keys...
   ✓ FK_Consultatii_Programari adaugat
   ✓ FK_Consultatii_Pacienti adaugat (→ Pacienti.Id)
   ✓ FK_Consultatii_PersonalMedical adaugat (→ PersonalMedical.PersonalID)

5. Creare Stored Procedures...
   ✓ sp_Consultatie_Create creat
   ✓ sp_Consultatie_GetById creat
   ✓ sp_Consultatie_GetByPacient creat
   ✓ sp_Consultatie_GetByMedic creat
   ✓ sp_Consultatie_GetByProgramare creat

========================================
✓ Tabel Consultatii recreat cu succes!
✓ Foreign Keys TOATE CORECTE
✓ Stored Procedures create (5 buc)
========================================
```

---

### 4️⃣ Verificare Post-Deployment

Rulează query-uri de verificare:

```sql
-- 1. Verifică tabelul
SELECT 
    TABLE_NAME,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Consultatii') AS NumarColoane
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'Consultatii';
-- Expected: Consultatii | 85

-- 2. Verifică Primary Key și DEFAULT
SELECT 
    c.name AS ColumnName,
    dc.definition AS DefaultValue
FROM sys.columns c
LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
WHERE c.object_id = OBJECT_ID('Consultatii')
AND c.name = 'ConsultatieID';
-- Expected: ConsultatieID | (newsequentialid())

-- 3. Verifică Foreign Keys (ATENTIE LA REFERINTE!)
SELECT 
    fk.name AS FK_Name,
    OBJECT_NAME(fk.referenced_object_id) AS Referenced_Table,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS Referenced_Column
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fc ON fk.object_id = fc.constraint_object_id
WHERE fk.parent_object_id = OBJECT_ID('Consultatii')
ORDER BY fk.name;
-- Expected: 
--   FK_Consultatii_Pacienti        | Pacienti        | Id           ← IMPORTANT!
--   FK_Consultatii_PersonalMedical | PersonalMedical | PersonalID   ← IMPORTANT!
--   FK_Consultatii_Programari      | Programari      | ProgramareID

-- 4. Verifică Stored Procedures
SELECT name 
FROM sys.procedures 
WHERE name LIKE 'sp_Consultatie_%'
ORDER BY name;
-- Expected: 5 procedures

-- 5. Test inserare (opțional)
DECLARE @TestID UNIQUEIDENTIFIER = NEWID();
DECLARE @TestProgramareID UNIQUEIDENTIFIER = (SELECT TOP 1 ProgramareID FROM Programari);
DECLARE @TestPacientID UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Pacienti);  -- ATENTIE: Id!
DECLARE @TestMedicID UNIQUEIDENTIFIER = (SELECT TOP 1 PersonalID FROM PersonalMedical WHERE EsteActiv = 1);  -- ATENTIE: PersonalID!

IF @TestProgramareID IS NOT NULL AND @TestPacientID IS NOT NULL AND @TestMedicID IS NOT NULL
BEGIN
    EXEC sp_Consultatie_Create
        @ConsultatieID = @TestID OUTPUT,
        @ProgramareID = @TestProgramareID,
        @PacientID = @TestPacientID,
        @MedicID = @TestMedicID,
        @DataConsultatie = '2025-01-08',
        @OraConsultatie = '10:00',
        @TipConsultatie = 'Prima consultatie',
        @MotivPrezentare = 'Test consultatie',
        @CreatDe = @TestMedicID;

    SELECT @TestID AS ConsultatieID_Generat;
    -- Expected: GUID generat

    -- Cleanup test
    DELETE FROM Consultatii WHERE ConsultatieID = @TestID;
    PRINT 'Test de inserare executat cu succes!';
END
ELSE
BEGIN
    PRINT 'Nu exista date pentru test (Programari, Pacienti sau PersonalMedical goale)';
END
```

---

## 📊 Structură Detaliată Tabel Nou

### Secțiuni Scrisoare Medicală:

#### I. Motive Prezentare (2 coloane)
- MotivPrezentare
- IstoricBoalaActuala

#### II. Antecedente (22 coloane)
**A. Heredo-Colaterale (5)**
- AHC_Mama, AHC_Tata, AHC_Frati, AHC_Bunici, AHC_Altele

**B. Fiziologice (5)**
- AF_Nastere, AF_Dezvoltare, AF_Menstruatie, AF_Sarcini, AF_Alaptare

**C. Personale Patologice (7)**
- APP_BoliCopilarieAdolescenta, APP_BoliAdult, APP_Interventii
- APP_Traumatisme, APP_Transfuzii, APP_Alergii, APP_Medicatie

**D. Socio-Economice (5)**
- Profesie, ConditiiLocuinta, ConditiiMunca, ObiceiuriAlimentare, Toxice

#### III. Examen Obiectiv (26 coloane)
**A. Examen General (7)**
- StareGenerala, Constitutie, Atitudine, Facies
- Tegumente, Mucoase, GangliniLimfatici

**B. Semne Vitale (9)**
- Greutate, Inaltime, IMC, Temperatura, TensiuneArteriala
- Puls, FreccventaRespiratorie, SaturatieO2, Glicemie

**C. Examen pe Aparate (10)**
- ExamenCardiovascular, ExamenRespiratoriu, ExamenDigestiv
- ExamenUrinar, ExamenNervos, ExamenLocomotor
- ExamenEndocrin, ExamenORL, ExamenOftalmologic, ExamenDermatologic

#### IV. Investigații (4 coloane)
- InvestigatiiLaborator, InvestigatiiImagistice
- InvestigatiiEKG, AlteInvestigatii

#### V. Diagnostic (4 coloane)
- DiagnosticPozitiv, DiagnosticDiferential
- DiagnosticEtiologic, CoduriICD10

#### VI. Tratament (8 coloane)
- TratamentMedicamentos, TratamentNemedicamentos
- RecomandariDietetice, RecomandariRegimViata
- InvestigatiiRecomandate, ConsulturiSpecialitate
- DataUrmatoareiProgramari, RecomandariSupraveghere

#### VII. Concluzie (4 coloane)
- Prognostic, Concluzie
- ObservatiiMedic, NotePacient

#### Metadata (9 coloane)
- Status, DataFinalizare, DurataMinute
- DocumenteAtatate
- DataCreare, CreatDe, DataUltimeiModificari, ModificatDe

---

## ⚠️ Troubleshooting

### Eroare: "Foreign key 'FK_Consultatii_Pacienti' references invalid column 'PacientID'"
**Cauză:** Scriptul folosește FK greșit  
**Soluție:** Tabelul Pacienti are PK = **`Id`**, NU `PacientID`. Folosește scriptul v1.3 corectat.

```sql
-- CORECT (v1.3):
FOREIGN KEY (PacientID) REFERENCES dbo.Pacienti(Id);
```

### Eroare: "Foreign key references invalid column 'PersonalMedicalID'"
**Cauză:** Scriptul folosește FK greșit  
**Soluție:** Tabelul PersonalMedical are PK = **`PersonalID`**, NU `PersonalMedicalID`. Folosește scriptul v1.3 corectat.

```sql
-- CORECT (v1.3):
FOREIGN KEY (MedicID) REFERENCES dbo.PersonalMedical(PersonalID);
```

### Eroare: "Could not drop object because it is referenced by a FOREIGN KEY constraint"
**Soluție:** Scriptul nou șterge automat toate FK-urile. Dacă persistă:
```sql
-- Găsește toate FK-urile care referă Consultatii
SELECT 
    'ALTER TABLE [' + OBJECT_NAME(parent_object_id) + '] DROP CONSTRAINT [' + name + '];' AS DropCommand
FROM sys.foreign_keys
WHERE referenced_object_id = OBJECT_ID('Consultatii')
   OR parent_object_id = OBJECT_ID('Consultatii');
-- Copiază și rulează comenzile generate
```

### Eroare: "There is already an object named 'Consultatii'"
**Soluție:** Tabelul nu s-a șters complet. Rulează manual:
```sql
-- Mai intai sterge FK-urile (vezi mai sus)
-- Apoi:
DROP TABLE dbo.Consultatii;
```

### Eroare: "Invalid column name 'PacientID'"
**Cauză:** Stored procedure-ul vede structura veche.
**Soluție:** Reconectează SSMS sau rulează:
```sql
EXEC sp_refreshview 'Consultatii';
```

---

## 📝 Note Importante

1. **NEWSEQUENTIALID vs NEWID:**
   - `NEWSEQUENTIALID()` = folosit ca DEFAULT pe tabel (performanță mai bună)
   - `NEWID()` = folosit în stored procedures (NEWSEQUENTIALID nu merge în SP)

2. **Foreign Keys CORECTE:**
   - Verifică că tabelele `Programari`, `Pacienti`, `PersonalMedical` există
   - PK-urile trebuie să fie:
     - `Pacienti.Id` ← **NU PacientID!**
     - `PersonalMedical.PersonalID` ← **NU PersonalMedicalID!**
     - `Programari.ProgramareID` ✓

3. **Migrare Date Vechi:**
   - Dacă ai date în structura veche, trebuie mapare manuală
   - Vezi exemplu în secțiunea "Migrare Date"

---

## ✅ Checklist Final

- [ ] Backup date existente creat
- [ ] Script PowerShell de verificare rulat
- [ ] Script SQL v1.3 (FINAL) executat cu succes
- [ ] Tabel recreat cu 85 coloane
- [ ] Foreign Keys adăugate (3 bucăți - TOATE CORECTE)
- [ ] Stored Procedures create (5 bucăți)
- [ ] Teste de inserare executate
- [ ] Aplicația .NET rebuild-uită
- [ ] Modalul `ConsultatieModal.razor` testat în browser

---

## 📞 Contact & Support

Dacă întâmpini probleme:
1. Verifică log-urile din SSMS (Messages tab)
2. Rulează query-urile de verificare din Secțiunea 4
3. Consultă documentația în: `FK_MAPPING_CORRECT.md`
4. Consultă implementare: `DevSupport\Documentation\Modal-Consultatie-Implementation-Guide.md`

**Autor:** Copilot AI Assistant  
**Data:** 2025-01-08  
**Versiune Script:** 1.3 (FINAL - FK-uri CORECTE)
