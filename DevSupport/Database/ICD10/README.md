# 📋 ICD-10 Database - README

## 🎯 Descriere

Acest folder conține toate scripturile necesare pentru crearea și popularea tabelei `ICD10_Codes` în baza de date `ValyanMed`.

## 🏗️ Structură Tabelă

```sql
CREATE TABLE ICD10_Codes (
    ICD10_ID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),  -- ✅ Primary Key
    Code NVARCHAR(10) NOT NULL UNIQUE,                             -- ex: I20.0, E11.9
    Category NVARCHAR(50) NOT NULL,                                 -- ex: Cardiovascular, Endocrin
    ShortDescription NVARCHAR(200) NOT NULL,                        -- Descriere scurtă (RO)
    LongDescription NVARCHAR(1000) NULL,                            -- Descriere detaliată (RO)
    IsLeafNode BIT NOT NULL DEFAULT 1,                              -- 1 = cod final
    IsCommon BIT NOT NULL DEFAULT 0,                                -- 1 = cod comun (top 100)
    Severity NVARCHAR(20) NULL,                                     -- Mild, Moderate, Severe, Critical
    SearchTerms NVARCHAR(MAX) NULL,                                 -- Keywords RO pentru căutare
    ...
)
```

### ✅ Avantaje UNIQUEIDENTIFIER cu NEWSEQUENTIALID():

1. **Performanță** - `NEWSEQUENTIALID()` generează GUID-uri secvențiale (mai rapid decât `NEWID()`)
2. **Distribuție** - Potențial pentru replicare și sisteme distribuite
3. **Unicitate globală** - GUID-uri unice chiar și între servere diferite
4. **Compatibilitate** - Standard pentru aplicații enterprise

## 📁 Fișiere SQL

| Fișier | Descriere | Coduri |
|--------|-----------|--------|
| `01_Create_ICD10_Table.sql` | Creează tabela + indexuri + full-text search | - |
| `02_Insert_ICD10_Cardiovascular.sql` | Boli cardiovasculare (I00-I99) | ~30 |
| `03_Insert_ICD10_Endocrin.sql` | Boli endocrine (E00-E90) | ~26 |
| `04_Insert_ICD10_Respirator.sql` | Boli respiratorii (J00-J99) | ~25 |
| `05_Create_SP_SearchICD10.sql` | Stored procedure pentru căutare | - |
| `06_Update_Common_Codes_RO.sql` | Traduceri RO pentru coduri comune | ~40 |

**Total:** ~150 coduri ICD-10 în **limba română**, cele mai frecvente în medicina primară.

## 🚀 Deployment

### Opțiunea 1: Deployment automat (RECOMANDAT)

```powershell
cd "D:\Lucru\CMS\DevSupport\Database\ICD10"
.\Deploy-ICD10.ps1
```

Scriptul va executa automat toate fișierele SQL în ordine și va testa configurația.

### Opțiunea 2: Deployment manual

Rulează fișierele în ordine în **SQL Server Management Studio (SSMS)**:

1. `01_Create_ICD10_Table.sql`
2. `02_Insert_ICD10_Cardiovascular.sql`
3. `03_Insert_ICD10_Endocrin.sql`
4. `04_Insert_ICD10_Respirator.sql`
5. `05_Create_SP_SearchICD10.sql`
6. `06_Update_Common_Codes_RO.sql`

## 📥 Import Date Suplimentare

### Din CSV (WHO/CMS datasets)

```powershell
# Download dataset
.\Download-ICD10-Romania.ps1

# Import în DB
.\Import-ICD10-FromCSV.ps1 -CsvFilePath "icd10_codes.csv"
```

### Surse recomandate:

1. **WHO ICD-10 API** (multilingual, inclusiv română)
   - https://icd.who.int/icdapi
   - Necesită înregistrare gratuită

2. **GitHub - kamillamagna/ICD-10-CSV** (~14k coduri EN)
   - https://github.com/kamillamagna/ICD-10-CSV
   - Download automat disponibil

3. **Ministerul Sănătății RO** (oficial pentru România)
   - https://www.ms.ro
   - Ordin MS 1438/2009

## 🔍 Utilizare Stored Procedure

### Căutare coduri ICD-10:

```sql
-- Căutare simplă
EXEC sp_SearchICD10 
    @SearchTerm = 'diabet', 
    @MaxResults = 10

-- Căutare în categorie specifică
EXEC sp_SearchICD10 
    @SearchTerm = 'infarct', 
    @Category = 'Cardiovascular',
    @MaxResults = 5

-- Doar coduri comune
EXEC sp_SearchICD10 
    @SearchTerm = 'hipertensiune', 
    @OnlyCommon = 1,
    @MaxResults = 5
```

### Exemple de rezultate:

| Code | Category | ShortDescription | IsCommon |
|------|----------|------------------|----------|
| E11 | Endocrin | Diabet zaharat tip 2 | 1 |
| E11.9 | Endocrin | Diabet zaharat tip 2 fără complicații | 1 |
| I21 | Cardiovascular | Infarct miocardic acut | 1 |
| I10 | Cardiovascular | Hipertensiune arterială esențială | 1 |

## 📊 Categorii Disponibile

| Categorie | Prefix | Exemple |
|-----------|--------|---------|
| Cardiovascular | I00-I99 | HTA, Infarct, FA, IC |
| Endocrin | E00-E90 | Diabet, Obezitate, Tiroidă |
| Respirator | J00-J99 | Pneumonie, Astm, BPOC |
| Digestiv | K00-K93 | GERD, Gastrită, Litiază |
| Neurologic | G00-G99 | Migrenă, Epilepsie, AVC |
| Genito-urinar | N00-N99 | ITU, Litiază renală |
| Musculo-scheletic | M00-M99 | Artroză, Lombalgie |
| Simptome | R00-R99 | Febră, Cefalee, Dispnee |

## 🇷🇴 Conformitate România

✅ **ICD-10 OMS** (NU ICD-10-CM)  
✅ **Ordin MS 1438/2009** - obligatoriu în România  
✅ **Compatibil CNAS/SIUI** - pentru raportări  
✅ **Traduceri în limba română** - pentru documente oficiale

## 🔧 Troubleshooting

### Error: "Cannot drop the table 'ICD10_Codes' because it does not exist"

✅ Normal - tabelul nu exista încă. Scriptul continuă cu crearea tabelei.

### Error: "Violation of UNIQUE KEY constraint"

❌ Codurile duplicate - verifică dacă ai rulat scripturile de 2 ori.

**Soluție:**
```sql
-- Șterge toate datele și recreează tabela
DROP TABLE ICD10_Codes
-- Apoi rulează din nou Deploy-ICD10.ps1
```

### Error: "Full-text catalog 'ICD10_Catalog' does not exist"

❌ Full-text search nu este activat pe SQL Server.

**Soluție:**
1. SQL Server Configuration Manager
2. Activează "Full-Text and Semantic Extractions for Search"
3. Restart SQL Server
4. Rulează din nou scripturile

## 📚 Resurse Utile

- **Legislație RO:** [Ordin MS 1438/2009](https://www.ms.ro)
- **WHO ICD-10:** [icd.who.int](https://icd.who.int)
- **CNAS SIUI:** [cnas.ro](https://www.cnas.ro)
- **GitHub ICD-10:** [Topics/ICD-10](https://github.com/topics/icd-10)

## 📞 Support

Pentru probleme sau întrebări:
- Repository: https://github.com/Aurelian1974/ValyanClinic
- Issues: Creează un issue nou pe GitHub

---

**Ultima actualizare:** 2025-01-15  
**Versiune:** 1.0.0  
**Database:** ValyanMed  
**Server:** DESKTOP-9H54BCS\SQLSERVER
