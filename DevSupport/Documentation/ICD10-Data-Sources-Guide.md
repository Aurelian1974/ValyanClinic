# Ghid Surse Date ICD-10 pentru ValyanClinic

**Data:** 2025-01-14  
**Status:** ✅ INFRASTRUCTURĂ COMPLETĂ + DATE INCLUSE

---

## ✅ **CE EXISTĂ DEJA**

Infrastructura ICD-10 este **complet implementată**:

| Component | Fișier | Status |
|-----------|--------|--------|
| **Entitate** | `ValyanClinic.Domain\Entities\ICD10Code.cs` | ✅ Complet |
| **Repository** | `ValyanClinic.Infrastructure\Repositories\ICD10Repository.cs` | ✅ Complet |
| **DTOs** | `ValyanClinic.Application\Features\ICD10Management\DTOs\ICD10Dtos.cs` | ✅ Complet |
| **DI Registration** | `Program.cs` - `IICD10Repository` | ✅ Complet |
| **Tabel SQL** | `ICD10_Codes` (15 coloane) | ✅ Script creat |
| **Stored Procedures** | `sp_ICD10_Search`, etc. | ✅ Scripturi create |
| **Date inițiale** | ~150 coduri (Cardiovascular, Endocrin, Respirator) | ✅ Scripturi SQL |
| **Date suplimentare** | ~200 coduri comune (toate categoriile) | ✅ Script SQL nou |
| **CSV inclus** | ~120 coduri comune în română | ✅ Fișier CSV local |

### 📁 Locație Scripturi SQL
```
DevSupport\01_Database\07_ICD10_Data\
├── 01_Create_ICD10_Table.sql
├── 02_Insert_ICD10_Cardiovascular.sql (~30 coduri)
├── 03_Insert_ICD10_Endocrin.sql (~26 coduri)
├── 04_Insert_ICD10_Respirator.sql (~25 coduri)
├── 05_Create_SP_SearchICD10.sql
├── 06_Update_Common_Codes_RO.sql
├── 07_Create_ICD10_StoredProcedures.sql
├── 09_Insert_ICD10_Additional_Common.sql  ← NOU! (~200 coduri)
├── data\
│   └── icd10_common_codes_ro.csv          ← NOU! (~120 coduri CSV)
├── Deploy-ICD10.ps1
├── Import-ICD10-FromCSV.ps1
├── Download-ICD10-All.ps1
└── README.md
```

---

## 🚀 **PAȘI PENTRU DEPLOYMENT RAPID**

### Opțiunea 1: Rulare scripturi SQL manual în SSMS

1. **Deschide SQL Server Management Studio (SSMS)**
2. **Conectează-te la server:** `DESKTOP-3Q8HI82\ERP` (sau serverul tău)
3. **Selectează database:** `ValyanMed`
4. **Rulează în ordine:**

```sql
-- Pasul 1: Creează tabela
:r "D:\Lucru\CMS\DevSupport\01_Database\07_ICD10_Data\01_Create_ICD10_Table.sql"

-- Pasul 2: Inserează coduri cardiovasculare (~30)
:r "D:\Lucru\CMS\DevSupport\01_Database\07_ICD10_Data\02_Insert_ICD10_Cardiovascular.sql"

-- Pasul 3: Inserează coduri endocrine (~26)
:r "D:\Lucru\CMS\DevSupport\01_Database\07_ICD10_Data\03_Insert_ICD10_Endocrin.sql"

-- Pasul 4: Inserează coduri respiratorii (~25)
:r "D:\Lucru\CMS\DevSupport\01_Database\07_ICD10_Data\04_Insert_ICD10_Respirator.sql"

-- Pasul 5: Inserează coduri suplimentare (~200)
:r "D:\Lucru\CMS\DevSupport\01_Database\07_ICD10_Data\09_Insert_ICD10_Additional_Common.sql"

-- Pasul 6: Creează stored procedures
:r "D:\Lucru\CMS\DevSupport\01_Database\07_ICD10_Data\07_Create_ICD10_StoredProcedures.sql"
```

### Opțiunea 2: Copy-Paste direct

Deschide fiecare fișier SQL și fă Copy-Paste în SSMS, apoi execută (F5).

---

## 📊 **STATISTICI CODURI INCLUSE**

### Total: ~350+ coduri ICD-10 în limba română

| Categorie | Coduri | Exemple |
|-----------|--------|---------|
| Cardiovascular | ~35 | I10 HTA, I21 Infarct, I48 FA, I50 IC |
| Endocrin | ~30 | E10/E11 Diabet, E66 Obezitate, E78 Dislipidemie |
| Respirator | ~30 | J00 Răceală, J45 Astm, J44 BPOC |
| Digestiv | ~25 | K21 GERD, K29 Gastrită, K76.0 Steatoză |
| Genito-urinar | ~25 | N39.0 ITU, N18 BCR, N20 Litiază |
| Nervos | ~20 | G43 Migrenă, G20 Parkinson, G40 Epilepsie |
| Musculo-scheletic | ~20 | M54.5 Lombalgie, M17 Gonartroza |
| Mental | ~20 | F32 Depresie, F41 Anxietate |
| Simptome | ~20 | R51 Cefalee, R42 Vertij, R05 Tuse |
| Infecțioase | ~15 | A09 Gastroenterită, U07.1 COVID-19 |
| Alte | ~10 | Z00 Control, C50 Cancer mamar |

---

## 🔍 **VERIFICARE DUPĂ DEPLOYMENT**

```sql
-- Verifică numărul total de coduri
SELECT COUNT(*) AS TotalCoduri FROM ICD10_Codes

-- Statistici pe categorii
SELECT 
    Category,
    COUNT(*) AS NumarCoduri,
    SUM(CASE WHEN IsCommon = 1 THEN 1 ELSE 0 END) AS CoduriComune
FROM ICD10_Codes
GROUP BY Category
ORDER BY NumarCoduri DESC

-- Test căutare
EXEC sp_ICD10_Search @SearchTerm = 'diabet', @MaxResults = 10
EXEC sp_ICD10_Search @SearchTerm = 'hipertensiune', @MaxResults = 5
EXEC sp_ICD10_Search @SearchTerm = 'I10', @MaxResults = 5
```

---

## 📦 **SURSE PENTRU DESCĂRCARE CODURI SUPLIMENTARE**

Dacă ai nevoie de mai multe coduri (~14,000+):

### 1. **GitHub - kamillamagna/ICD-10-CSV** (~14,400 coduri EN)
```
https://raw.githubusercontent.com/kamillamagna/ICD-10-CSV/master/codes.csv
```

### 2. **CMS.gov (US)** - ICD-10-CM Complet (~72,000 coduri EN)
```
https://www.cms.gov/medicare/coding-billing/icd-10-codes/icd-10-cm-files
```

### 3. **WHO ICD-10 API** - Multilingv (necesită înregistrare)
```
https://icd.who.int/icdapi
```

---

## ✅ **CHECKLIST IMPLEMENTARE**

- [x] ✅ Entitate `ICD10Code` în Domain
- [x] ✅ Repository `ICD10Repository` cu Dapper
- [x] ✅ DTOs pentru căutare
- [x] ✅ Înregistrare în DI Container
- [x] ✅ Script creare tabel SQL
- [x] ✅ Stored procedures pentru căutare
- [x] ✅ ~350 coduri comune în română (scripturi SQL)
- [x] ✅ CSV cu coduri comune inclus local
- [x] ✅ Script import CSV

### Ce mai trebuie:
- [ ] ⏳ **Rulare scripturi SQL pe baza de date** ← URMĂTORUL PAS
- [ ] ⏳ Integrare autocomplete în UI Consultatii
- [ ] ⏳ (Opțional) Import coduri suplimentare din CSV extern

---

## 🗄️ **STRUCTURA TABELULUI**

```sql
CREATE TABLE ICD10_Codes (
    ICD10_ID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    Code NVARCHAR(10) NOT NULL UNIQUE,       -- I20.0, E11.9
    FullCode NVARCHAR(20) NOT NULL,          -- Pentru sortare
    Category NVARCHAR(50) NOT NULL,          -- Cardiovascular, Endocrin
    ShortDescription NVARCHAR(200) NOT NULL, -- Descriere scurtă (RO)
    LongDescription NVARCHAR(1000) NULL,     -- Descriere detaliată
    EnglishDescription NVARCHAR(500) NULL,   -- Descriere EN
    ParentCode NVARCHAR(10) NULL,            -- Codul părinte
    IsLeafNode BIT NOT NULL DEFAULT 1,       -- 1 = cod final
    IsCommon BIT NOT NULL DEFAULT 0,         -- 1 = cod frecvent
    Severity NVARCHAR(20) NULL,              -- Mild, Moderate, Severe
    SearchTerms NVARCHAR(MAX) NULL,          -- Keywords RO
    Notes NVARCHAR(MAX) NULL,
    DataCreare DATETIME NOT NULL DEFAULT GETDATE(),
    DataModificare DATETIME NULL
)
```

---

**Ultima actualizare:** 2025-01-14
