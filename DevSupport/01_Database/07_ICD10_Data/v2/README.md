# 📋 ICD-10 Database Schema v2.0

## 🎯 Descriere

Schema nouă pentru codurile ICD-10 cu suport complet pentru:
- **Multilingv**: Română (principal) + Engleză (original din XML)
- **Ierarhie**: Capitole → Secțiuni → Coduri (cu self-reference)
- **Metadata**: Instrucțiuni codificare, excluderi, note

## 📊 Diagramă Relații

```
┌─────────────────────┐
│   ICD10_Chapters    │ (22 capitole)
│  ChapterId (PK, GUID)│
│  ChapterNumber      │
│  DescriptionRo/En   │
└─────────┬───────────┘
          │ 1:N
          ▼
┌─────────────────────┐
│   ICD10_Sections    │ (~200 secțiuni)
│  SectionId (PK, GUID)│
│  ChapterId (FK)     │
│  SectionCode        │
│  DescriptionRo/En   │
└─────────┬───────────┘
          │ 1:N
          ▼
┌─────────────────────────────────────────────┐
│              ICD10_Codes                     │ (~70,000+ coduri)
│  ICD10_ID (PK, GUID NEWSEQUENTIALID)        │
│  ChapterId (FK)                              │
│  SectionId (FK)                              │
│  ParentId (FK → Self)    ◄──┐               │
│  Code, FullCode              │               │
│  ShortDescriptionRo/En       │ Ierarhie     │
│  LongDescriptionRo/En        │ recursivă    │
│  ParentCode ─────────────────┘               │
│  HierarchyLevel (0-5)                        │
│  IsLeafNode, IsBillable                      │
│  Category, Severity, IsCommon                │
│  SearchTermsRo/En                            │
│  IsTranslated, TranslatedAt/By               │
└──────────────────┬──────────────────────────┘
                   │ 1:N
     ┌─────────────┼─────────────┬─────────────┐
     ▼             ▼             ▼             ▼
┌─────────┐  ┌─────────┐  ┌───────────┐  ┌───────┐
│Inclusion│  │Exclusions│  │Coding     │  │Notes  │
│Terms    │  │         │  │Instructions│  │       │
└─────────┘  └─────────┘  └───────────┘  └───────┘
```

## 📁 Fișiere SQL

| Fișier | Descriere |
|--------|-----------|
| `01_Create_ICD10_Schema_v2.sql` | Creează toate tabelele și indexurile |
| `02_Create_ICD10_Views_SPs_v2.sql` | Creează views și stored procedures |
| `03_Import_ICD10_FromXML.sql` | Script pentru import din XML (TODO) |

## 🗃️ Structura Tabelelor

### ICD10_Chapters (Capitole)
| Coloană | Tip | Descriere |
|---------|-----|-----------|
| ChapterId | UNIQUEIDENTIFIER | PK, NEWSEQUENTIALID() |
| ChapterNumber | INT | 1-22 |
| CodeRangeStart/End | NVARCHAR(10) | A00/B99, C00/D49, etc. |
| DescriptionRo | NVARCHAR(500) | Descriere în română |
| DescriptionEn | NVARCHAR(500) | Descriere în engleză |

### ICD10_Sections (Secțiuni)
| Coloană | Tip | Descriere |
|---------|-----|-----------|
| SectionId | UNIQUEIDENTIFIER | PK, NEWSEQUENTIALID() |
| ChapterId | UNIQUEIDENTIFIER | FK → Chapters |
| SectionCode | NVARCHAR(20) | A00-A09, C00-C14, etc. |
| DescriptionRo/En | NVARCHAR(500) | Descrieri bilingve |

### ICD10_Codes (Coduri - PRINCIPAL)
| Coloană | Tip | Descriere |
|---------|-----|-----------|
| ICD10_ID | UNIQUEIDENTIFIER | PK, NEWSEQUENTIALID() |
| ChapterId | UNIQUEIDENTIFIER | FK → Chapters |
| SectionId | UNIQUEIDENTIFIER | FK → Sections (nullable) |
| ParentId | UNIQUEIDENTIFIER | FK → Self (ierarhie) |
| Code | NVARCHAR(10) | Codul ICD-10 (ex: A00.0) |
| FullCode | NVARCHAR(15) | Pentru sortare |
| **ShortDescriptionRo** | NVARCHAR(250) | **Descriere română (UI)** |
| ShortDescriptionEn | NVARCHAR(250) | Descriere engleză (XML) |
| LongDescriptionRo/En | NVARCHAR(1000) | Descrieri detaliate |
| ParentCode | NVARCHAR(10) | Codul părinte |
| HierarchyLevel | INT | 0=Category, 1=Subcategory, etc. |
| IsLeafNode | BIT | True = cod final utilizabil |
| IsBillable | BIT | True = valid pentru facturare |
| Category | NVARCHAR(50) | Cardiovascular, Endocrin, etc. |
| IsCommon | BIT | Cod frecvent utilizat |
| Severity | NVARCHAR(20) | Mild, Moderate, Severe, Critical |
| SearchTermsRo/En | NVARCHAR(MAX) | Keywords pentru căutare |
| **IsTranslated** | BIT | **Are traducere în română** |
| TranslatedAt | DATETIME2 | Când a fost tradus |
| TranslatedBy | NVARCHAR(100) | Cine a tradus |

### ICD10_InclusionTerms
| Coloană | Tip | Descriere |
|---------|-----|-----------|
| InclusionId | UNIQUEIDENTIFIER | PK |
| ICD10_ID | UNIQUEIDENTIFIER | FK → Codes |
| TermType | NVARCHAR(20) | 'includes' sau 'inclusionTerm' |
| TermTextRo/En | NVARCHAR(1000) | Text bilingv |

### ICD10_Exclusions
| Coloană | Tip | Descriere |
|---------|-----|-----------|
| ExclusionId | UNIQUEIDENTIFIER | PK |
| ICD10_ID | UNIQUEIDENTIFIER | FK → Codes |
| ExclusionType | NVARCHAR(10) | 'excludes1' sau 'excludes2' |
| NoteTextRo/En | NVARCHAR(1000) | Text bilingv |
| ReferencedCode | NVARCHAR(20) | Codul referențiat |

### ICD10_CodingInstructions
| Coloană | Tip | Descriere |
|---------|-----|-----------|
| InstructionId | UNIQUEIDENTIFIER | PK |
| ICD10_ID | UNIQUEIDENTIFIER | FK → Codes |
| InstructionType | NVARCHAR(25) | 'codeFirst', 'useAdditionalCode', 'codeAlso' |
| InstructionTextRo/En | NVARCHAR(1000) | Text bilingv |

### ICD10_Notes
| Coloană | Tip | Descriere |
|---------|-----|-----------|
| NoteId | UNIQUEIDENTIFIER | PK |
| ICD10_ID | UNIQUEIDENTIFIER | FK → Codes |
| NoteType | NVARCHAR(20) | 'general', 'clinical', 'coding' |
| NoteTextRo/En | NVARCHAR(2000) | Text bilingv |

## 🔧 Views

| View | Descriere |
|------|-----------|
| `vw_ICD10_CodesComplete` | Toate codurile cu join-uri complete |
| `vw_ICD10_CommonCodes` | Doar codurile frecvent utilizate |
| `vw_ICD10_UntranslatedCodes` | Coduri fără traducere română |

## 📋 Stored Procedures

| Procedure | Parametri | Descriere |
|-----------|-----------|-----------|
| `sp_ICD10_Search` | @SearchTerm, @Category, @OnlyCommon, @Language | Căutare cu relevanță |
| `sp_ICD10_GetByCode` | @Code, @Language | Detalii complete + note |
| `sp_ICD10_GetHierarchy` | @Code, @Language | Ierarhia completă |
| `sp_ICD10_GetChildren` | @ParentCode, @Language | Copiii unui cod |
| `sp_ICD10_GetCommonCodes` | @Category, @MaxResults | Coduri frecvente |
| `sp_ICD10_UpdateTranslation` | @Code, @ShortDescriptionRo, ... | Actualizare traducere |
| `sp_ICD10_GetStatistics` | - | Statistici bază de date |

## 🚀 Deployment

```sql
-- În SSMS, rulează în ordine:
:r "01_Create_ICD10_Schema_v2.sql"
:r "02_Create_ICD10_Views_SPs_v2.sql"
:r "03_Import_ICD10_FromXML.sql"
```

## 🇷🇴 Workflow Traducere

1. **Import din XML** → Coduri cu `IsTranslated = 0`
2. **Traducere automată/manuală** → Actualizare `ShortDescriptionRo`
3. **Marcare ca tradus** → `IsTranslated = 1, TranslatedAt, TranslatedBy`

```sql
-- Exemplu actualizare traducere
EXEC sp_ICD10_UpdateTranslation 
    @Code = 'A00.0',
    @ShortDescriptionRo = 'Holeră cauzată de Vibrio cholerae 01, biovar cholerae',
    @TranslatedBy = 'Dr. Popescu'
```

## 📊 Statistici Estimate

| Metric | Valoare |
|--------|---------|
| Capitole | 22 |
| Secțiuni | ~200 |
| Coduri totale | ~70,000+ |
| Coduri facturabile | ~70,000 |
| Termeni includere | ~50,000 |
| Excluderi | ~30,000 |
| Instrucțiuni | ~15,000 |

---

**Versiune:** 2.0  
**Data:** 2025-01-15  
**Compatibil:** ICD-10-CM 2026
