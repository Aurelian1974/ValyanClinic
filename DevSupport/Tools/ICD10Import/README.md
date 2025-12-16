# 🛠️ ICD-10 XML Import Tool

## Descriere

Tool pentru importarea codurilor ICD-10 din fișierul XML oficial (ICD-10-CM 2026) în baza de date ValyanMed.

## Caracteristici

- ✅ Parsează complet structura ierarhică din XML
- ✅ Importă capitole, secțiuni și coduri
- ✅ Extrage termeni de includere (inclusionTerm, includes)
- ✅ Extrage excluderi (excludes1, excludes2)
- ✅ Extrage instrucțiuni de codificare (codeFirst, useAdditionalCode, codeAlso)
- ✅ Determină automat categoria medicală
- ✅ Construiește ierarhia părinte-copil
- ✅ Suport pentru traduceri bilingve (EN + RO)

## Utilizare

### Opțiunea 1: Rulare din Visual Studio

1. Setează `DevSupport` ca proiect de pornire
2. Rulează cu F5 sau Ctrl+F5
3. Confirmă operațiunea tastând "da"

### Opțiunea 2: Rulare din linie de comandă

```powershell
cd D:\Lucru\CMS\DevSupport
dotnet run
```

### Opțiunea 3: Cu parametri personalizați

```powershell
dotnet run -- "cale/catre/icd10.xml" "Server=...;Database=...;..."
```

## Cerințe

1. **Schema SQL creată** - Rulează mai întâi:
   ```sql
   :r "01_Create_ICD10_Schema_v2.sql"
   :r "02_Create_ICD10_Views_SPs_v2.sql"
   ```

2. **Fișier XML** - `icd10cm_tabular_2026.xml` în:
   ```
   D:\Lucru\CMS\DevSupport\01_Database\07_ICD10_Data\data\
   ```

3. **Conexiune la baza de date** - SQL Server cu database `ValyanMed`

## Output

### Statistici Import (2026)

| Metric | Valoare |
|--------|---------|
| Capitole | 22 |
| Secțiuni | 297 |
| Coduri | 46,881 |
| Termeni includere | 13,492 |
| Excluderi | 7,433 |
| Instrucțiuni | 1,500 |
| Note | 101 |
| **Durată** | ~2-3 minute |

### Coduri per Categorie

| Categorie | Coduri |
|-----------|--------|
| Traumatisme | 13,333 |
| Musculo-scheletic | 7,100 |
| Cauze externe | 3,636 |
| Ochi | 3,216 |
| Neoplasme | 2,178 |
| Cardiovascular | 1,798 |
| Obstetric | 1,791 |
| Infectioase | 1,309 |
| ... | ... |

## Structura Fișierelor

```
DevSupport\
├── Tools\
│   └── ICD10Import\
│       ├── Program.cs           # Entry point
│       └── ICD10XmlImporter.cs  # Logica de import
├── 01_Database\
│   └── 07_ICD10_Data\
│       ├── data\
│       │   └── icd10cm_tabular_2026.xml  # XML sursă
│       └── v2\
│           ├── 01_Create_ICD10_Schema_v2.sql
│           ├── 02_Create_ICD10_Views_SPs_v2.sql
│           └── README.md
└── DevSupport.csproj
```

## Testare După Import

```sql
-- Statistici generale
EXEC sp_ICD10_GetStatistics

-- Căutare cod
EXEC sp_ICD10_Search @SearchTerm = 'diabetes', @MaxResults = 10

-- Detalii cod specific
EXEC sp_ICD10_GetByCode @Code = 'E11.9'

-- Ierarhie cod
EXEC sp_ICD10_GetHierarchy @Code = 'E11.65'

-- Coduri comune (când sunt marcate)
EXEC sp_ICD10_GetCommonCodes @MaxResults = 50
```

## Pașii Următori

1. **Marcare coduri comune** - Identifică și marchează cele ~200-500 coduri frecvent utilizate:
   ```sql
   UPDATE ICD10_Codes SET IsCommon = 1 WHERE Code IN ('I10', 'E11.9', 'J06.9', ...)
   ```

2. **Traducere în română** - Folosește procedura:
   ```sql
   EXEC sp_ICD10_UpdateTranslation 
       @Code = 'I10',
       @ShortDescriptionRo = 'Hipertensiune arterială esențială',
       @TranslatedBy = 'Admin'
   ```

3. **Import traduceri din CSV-urile existente** - Mapează codurile din fișierele CSV create anterior

---

**Versiune:** 2.0  
**Data:** 2025-01-15  
**Autor:** ValyanClinic DevTeam
