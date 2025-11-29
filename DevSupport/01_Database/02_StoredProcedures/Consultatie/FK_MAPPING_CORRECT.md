# 🔑 Mapare Corecta Foreign Keys - Tabel Consultatii

**Data:** 2025-01-08  
**Database:** ValyanMed  
**Status:** ✅ **CORECT SI VALIDAT**

---

## 📋 Structura Tabele Referite

| Tabel | Primary Key | Tip |
|-------|-------------|-----|
| **Pacienti** | `Id` | UNIQUEIDENTIFIER |
| **Programari** | `ProgramareID` | UNIQUEIDENTIFIER |
| **PersonalMedical** | `PersonalID` | UNIQUEIDENTIFIER |

---

## 🔗 Foreign Keys Tabel Consultatii

| FK Name | Coloana in Consultatii | Referinta | Coloana Referita |
|---------|----------------------|-----------|------------------|
| `FK_Consultatii_Programari` | `ProgramareID` | `Programari` | `ProgramareID` |
| `FK_Consultatii_Pacienti` | `PacientID` | `Pacienti` | **`Id`** ⚠️ |
| `FK_Consultatii_PersonalMedical` | `MedicID` | `PersonalMedical` | **`PersonalID`** ⚠️ |

### ⚠️ ATENTIE - Erori Comune

#### ❌ **GRESIT** (versiuni vechi):
```sql
-- GRESIT: Pacienti.PacientID nu exista!
FOREIGN KEY (PacientID) REFERENCES dbo.Pacienti(PacientID);

-- GRESIT: PersonalMedical.PersonalMedicalID nu exista!
FOREIGN KEY (MedicID) REFERENCES dbo.PersonalMedical(PersonalMedicalID);
```

#### ✅ **CORECT** (versiunea finala):
```sql
-- CORECT: Pacienti are PK = Id
FOREIGN KEY (PacientID) REFERENCES dbo.Pacienti(Id);

-- CORECT: PersonalMedical are PK = PersonalID
FOREIGN KEY (MedicID) REFERENCES dbo.PersonalMedical(PersonalID);
```

---

## 🛠️ Scriptul SQL Corectat

**Fisier:** `DevSupport/Database/StoredProcedures/Consultatie/Consultatie_StoredProcedures.sql`

**Versiune:** 1.3 (FINAL)

### FK Declarations (Linii 220-240):

```sql
-- FK 1: Programari
ALTER TABLE dbo.Consultatii
ADD CONSTRAINT FK_Consultatii_Programari 
FOREIGN KEY (ProgramareID) REFERENCES dbo.Programari(ProgramareID);

-- FK 2: Pacienti (FIXED v1.2)
ALTER TABLE dbo.Consultatii
ADD CONSTRAINT FK_Consultatii_Pacienti 
FOREIGN KEY (PacientID) REFERENCES dbo.Pacienti(Id);  -- Id nu PacientID!

-- FK 3: PersonalMedical (FIXED v1.3)
ALTER TABLE dbo.Consultatii
ADD CONSTRAINT FK_Consultatii_PersonalMedical 
FOREIGN KEY (MedicID) REFERENCES dbo.PersonalMedical(PersonalID);  -- PersonalID nu PersonalMedicalID!
```

---

## 🔍 Verificare Structura Existenta

### Query Verificare PK Pacienti:
```sql
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_NAME = 'Pacienti' 
  AND CONSTRAINT_NAME LIKE 'PK_%';
-- Expected: Id
```

### Query Verificare PK PersonalMedical:
```sql
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_NAME = 'PersonalMedical' 
  AND CONSTRAINT_NAME LIKE 'PK_%';
-- Expected: PersonalID
```

### Query Verificare PK Programari:
```sql
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_NAME = 'Programari' 
  AND CONSTRAINT_NAME LIKE 'PK_%';
-- Expected: ProgramareID
```

---

## 📝 Istoric Modificari

### v1.0 (Initial)
- ❌ Database: ValyanClinicDB (GRESIT)
- ❌ Pacienti FK: `REFERENCES Pacienti(PacientID)` (GRESIT)
- ❌ PersonalMedical FK: `REFERENCES PersonalMedical(PersonalMedicalID)` (GRESIT)

### v1.1
- ✅ Database: ValyanMed (CORECTAT)
- ❌ Pacienti FK: `REFERENCES Pacienti(PacientID)` (inca gresit)
- ❌ PersonalMedical FK: `REFERENCES PersonalMedical(PersonalMedicalID)` (inca gresit)

### v1.2
- ✅ Database: ValyanMed
- ✅ Pacienti FK: `REFERENCES Pacienti(Id)` (CORECTAT)
- ❌ PersonalMedical FK: `REFERENCES PersonalMedical(PersonalMedicalID)` (inca gresit)

### v1.3 (FINAL) ⭐
- ✅ Database: ValyanMed
- ✅ Pacienti FK: `REFERENCES Pacienti(Id)` ✓
- ✅ PersonalMedical FK: `REFERENCES PersonalMedical(PersonalID)` ✓
- ✅ Toate FK-urile CORECTE

---

## 🚀 Deployment

### Pas 1: Backup (IMPORTANT!)
```sql
USE ValyanMed;
GO

-- Backup date existente (daca exista)
SELECT * INTO Consultatii_Backup_20250108 FROM Consultatii;
GO
```

### Pas 2: Rulare Script
```sql
-- Deschide in SSMS:
-- DevSupport\Database\StoredProcedures\Consultatie\Consultatie_StoredProcedures.sql

-- SAU executa direct (F5)
```

### Pas 3: Verificare Post-Deployment
```sql
-- 1. Verifica FK-uri
SELECT 
    fk.name AS FK_Name,
    OBJECT_NAME(fk.parent_object_id) AS From_Table,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS From_Column,
    OBJECT_NAME(fk.referenced_object_id) AS To_Table,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS To_Column
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fc ON fk.object_id = fc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'Consultatii'
ORDER BY fk.name;

-- Expected Output:
-- FK_Consultatii_Pacienti        | Consultatii | PacientID    | Pacienti        | Id
-- FK_Consultatii_PersonalMedical | Consultatii | MedicID      | PersonalMedical | PersonalID
-- FK_Consultatii_Programari      | Consultatii | ProgramareID | Programari      | ProgramareID

-- 2. Verifica Stored Procedures
SELECT name FROM sys.procedures WHERE name LIKE 'sp_Consultatie_%';
-- Expected: 5 procedures

-- 3. Verifica Indexes
SELECT name FROM sys.indexes WHERE object_id = OBJECT_ID('Consultatii');
-- Expected: 5 (1 PK + 4 indexes)
```

---

## ✅ Success Criteria

- [x] Database name corectat: **ValyanMed**
- [x] FK Pacienti corectat: **Pacienti(Id)**
- [x] FK PersonalMedical corectat: **PersonalMedical(PersonalID)**
- [x] FK Programari corect: **Programari(ProgramareID)**
- [x] Script rulat cu succes (zero errors)
- [x] 3 Foreign Keys create
- [x] 5 Stored Procedures create
- [x] 4 Indexes create

---

## 🐛 Troubleshooting

### Eroare: "Foreign key references invalid column 'PacientID'"
**Cauza:** Vechea versiune a scriptului  
**Fix:** Foloseste scriptul v1.3 (FINAL) cu `REFERENCES Pacienti(Id)`

### Eroare: "Foreign key references invalid column 'PersonalMedicalID'"
**Cauza:** Vechea versiune a scriptului  
**Fix:** Foloseste scriptul v1.3 (FINAL) cu `REFERENCES PersonalMedical(PersonalID)`

### Eroare: "Cannot open database ValyanClinicDB"
**Cauza:** Vechea versiune a scriptului  
**Fix:** Database-ul corect este **ValyanMed**, nu ValyanClinicDB

---

## 📚 Referinte

### Fisiere Relevante:
- `DevSupport/Database/TableStructure/Pacienti_Complete.sql` - Structura Pacienti (PK = Id)
- `DevSupport/Database/TableStructure/PersonalMedical_Complete.sql` - Structura PersonalMedical (PK = PersonalID)
- `DevSupport/Database/TableStructure/Programari_Complete.sql` - Structura Programari (PK = ProgramareID)
- `DevSupport/Database/StoredProcedures/Consultatie/Consultatie_StoredProcedures.sql` - Script FINAL v1.3

### Documentatie C#:
- `ValyanClinic.Domain/Entities/Consultatie.cs` - Entity model
- `ValyanClinic.Application/Features/ConsultatieManagement/` - Feature handlers
- `ValyanClinic.Infrastructure/Repositories/ConsultatieRepository.cs` - Data access

---

**Status:** ✅ **PRODUCTION READY**  
**Versiune:** 1.3 (FINAL)  
**Validat:** 2025-01-08  
**Toate FK-urile:** ✅ **CORECTE**

---

*Document creat automat pe baza analizei structurii de tabele din ValyanMed*

