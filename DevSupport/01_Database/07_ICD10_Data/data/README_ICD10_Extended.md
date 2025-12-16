# 📋 ICD-10 Extended Dataset - ValyanClinic

## Fișiere CSV Create

Am creat următoarele fișiere CSV cu coduri ICD-10 în format corect (cu punct) și în limba română:

| Fișier | Categorii | Coduri aproximative |
|--------|-----------|---------------------|
| `icd10_common_codes_ro.csv` | Toate (coduri frecvente) | ~110 |
| `icd10_extended_part1_ro.csv` | A-E (Infecțioase, Neoplasme, Sânge, Endocrin) | ~250 |
| `icd10_extended_part2_ro.csv` | F-H (Mental, Nervos, Ochi, Ureche) | ~200 |
| `icd10_extended_part3_cardiovascular_ro.csv` | I (Cardiovascular complet) | ~180 |

**Total: ~740 coduri ICD-10 în limba română**

## Structura CSV

```
Code,Category,ShortDescription,LongDescription,IsCommon,Severity,SearchTerms
I21.0,Cardiovascular,Infarct miocardic acut anterior transmural,Infarct miocardic acut transmural al peretelui anterior,1,Critical,stemi anterior infarct
```

## Categorii Acoperite

### ✅ Complete (detaliate)
- **A00-B99**: Boli infecțioase și parazitare
- **C00-D48**: Neoplasme
- **D50-D89**: Boli ale sângelui
- **E00-E90**: Boli endocrine, nutriție, metabolism
- **F00-F99**: Tulburări mentale
- **G00-G99**: Boli ale sistemului nervos
- **H00-H59**: Boli ale ochiului
- **H60-H95**: Boli ale urechii
- **I00-I99**: Boli ale aparatului circulator (FOARTE DETALIAT)

### ⏳ De adăugat (dacă e nevoie)
- **J00-J99**: Boli ale aparatului respirator (parțial în common)
- **K00-K93**: Boli ale aparatului digestiv (parțial în common)
- **L00-L99**: Boli ale pielii
- **M00-M99**: Boli ale sistemului osteo-articular
- **N00-N99**: Boli ale aparatului genito-urinar
- **O00-O99**: Sarcină, naștere
- **P00-P96**: Afecțiuni perinatale
- **Q00-Q99**: Malformații congenitale
- **R00-R99**: Simptome și semne (parțial în common)
- **S00-T98**: Traumatisme
- **V01-Y98**: Cauze externe
- **Z00-Z99**: Factori influențând starea de sănătate

## Import în Baza de Date

### Script SQL pentru import CSV:

```sql
-- Import din CSV în tabel temporar
CREATE TABLE #TempICD10 (
    Code NVARCHAR(10),
    Category NVARCHAR(50),
    ShortDescription NVARCHAR(200),
    LongDescription NVARCHAR(1000),
    IsCommon BIT,
    Severity NVARCHAR(20),
    SearchTerms NVARCHAR(MAX)
);

-- Import cu BULK INSERT (ajustează path-ul)
BULK INSERT #TempICD10
FROM 'D:\Lucru\CMS\DevSupport\01_Database\07_ICD10_Data\data\icd10_extended_part1_ro.csv'
WITH (
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2,
    CODEPAGE = '65001'
);

-- Insert în tabelul principal
INSERT INTO ICD10_Codes (Code, FullCode, Category, ShortDescription, LongDescription, IsCommon, Severity, SearchTerms)
SELECT 
    Code,
    Code AS FullCode,
    Category,
    ShortDescription,
    LongDescription,
    IsCommon,
    Severity,
    SearchTerms
FROM #TempICD10
WHERE NOT EXISTS (SELECT 1 FROM ICD10_Codes WHERE ICD10_Codes.Code = #TempICD10.Code);

DROP TABLE #TempICD10;
```

## Surse de Date

Aceste coduri au fost compilate din:
1. ICD-10 WHO (Organizația Mondială a Sănătății)
2. Ordin MS 1438/2009 (România)
3. Nomenclatoare CNAS

## Note

- Toate codurile sunt în format WHO ICD-10 (NU ICD-10-CM)
- Descrierile sunt în limba română
- SearchTerms conține sinonime și termeni de căutare în română
- IsCommon=1 pentru coduri frecvent folosite
- Severity: Mild, Moderate, Severe, Critical

---

**Ultima actualizare:** 2025-01-15
**Total coduri:** ~740
