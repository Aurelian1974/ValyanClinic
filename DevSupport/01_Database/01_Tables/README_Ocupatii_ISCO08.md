# Tabel Ocupatii_ISCO08 - Documentație Completă

## Descriere Generală

Tabelul `Ocupatii_ISCO08` implementează clasificarea internațională standard a ocupațiilor conform standardului ISCO-08 (International Standard Classification of Occupations, Revision 4). Acest tabel permite stocarea structurii ierarhice complete a ocupațiilor și poate fi utilizat pentru:

- Clasificarea personalului medical și non-medical
- Raportări statistice conform standardelor internaționale  
- Validarea și standardizarea funcțiilor din resurse umane
- Integrarea cu sisteme externe (ANOFM, INS, UE)

## Sursa Datelor

**URL Oficial:** https://data.gov.ro/dataset/695974d3-4be3-4bbe-a56a-bb639ad908e2/resource/cc7db3b5-da8a-4eaa-afcc-514dd373eac6/download/isco-08-lista-alfabetica-ocupatii-2024.xml

**Autoritate:** Guvernul României - data.gov.ro  
**Standard:** ISCO-08 (ILO - International Labour Organization)  
**Versiune:** 2024  

## Structura Ierarhică ISCO-08

Clasificarea ISCO-08 are 4 niveluri ierarhice:

```
├── Nivel 1: Grupe Majore (1 cifră) - 10 grupe
│   ├── Nivel 2: Subgrupe (2 cifre) - cca. 43 subgrupe  
│   │   ├── Nivel 3: Grupe Minore (3 cifre) - cca. 130 grupe
│   │   │   └── Nivel 4: Ocupații (4 cifre) - cca. 436 ocupații
```

### Exemplu Structură:
```
2 - Profesioniști
├── 22 - Profesioniști în sănătate
│   ├── 221 - Medici
│   │   ├── 2211 - Medici generaliști
│   │   ├── 2212 - Medici specialiști
│   │   └── 2213 - Medici veterinari
│   └── 222 - Profesioniști din domeniul îngrijirii medicale
└── 25 - Profesioniști în tehnologia informației
```

## Schema Tabelului

### Coloane Principale

| Coloană | Tip | Descriere |
|---------|-----|-----------|
| `Id` | `INT IDENTITY` | Cheie primară auto-incrementată |
| `Cod_ISCO` | `NVARCHAR(10)` | Codul ISCO unic (1-4 cifre) |
| `Denumire_Ocupatie` | `NVARCHAR(500)` | Denumirea în română |
| `Denumire_Ocupatie_EN` | `NVARCHAR(500)` | Denumirea în engleză |
| `Nivel_Ierarhic` | `TINYINT` | Nivelul: 1-4 |

### Coloane Ierarhice

| Coloană | Tip | Descriere |
|---------|-----|-----------|
| `Cod_Parinte` | `NVARCHAR(10)` | Referința la codul părinte |
| `Grupa_Majora` | `NVARCHAR(10)` | Cod grupa majoră (1 cifră) |
| `Grupa_Majora_Denumire` | `NVARCHAR(300)` | Denumire grupa majoră |
| `Subgrupa` | `NVARCHAR(10)` | Cod subgrupa (2 cifre) |
| `Grupa_Minora` | `NVARCHAR(10)` | Cod grupa minoră (3 cifre) |

### Coloane Audit

| Coloană | Tip | Descriere |
|---------|-----|-----------|
| `Este_Activ` | `BIT` | Status activ/inactiv |
| `Data_Crearii` | `DATETIME2(7)` | Timestamp creare |
| `Data_Ultimei_Modificari` | `DATETIME2(7)` | Timestamp ultima modificare |
| `Creat_De` | `NVARCHAR(100)` | Utilizator creator |
| `Modificat_De` | `NVARCHAR(100)` | Utilizator modificator |

## Constrangeri și Indexuri

### Constrangeri
- **Primary Key:** `PK_Ocupatii_ISCO08` pe `Id`
- **Unique Key:** `UK_Ocupatii_ISCO08_Cod` pe `Cod_ISCO`
- **Check:** Nivel ierarhic 1-4
- **Check:** Lungimea codului să corespundă nivelului
- **Foreign Key:** Relația ierarhică părinte-copil

### Indexuri
- `IX_Ocupatii_ISCO08_Cod_ISCO` - Căutare după cod
- `IX_Ocupatii_ISCO08_Nivel_Ierarhic` - Filtrare pe nivel  
- `IX_Ocupatii_ISCO08_Grupa_Majora` - Filtrare pe grupa majoră
- `IX_Ocupatii_ISCO08_Parinte` - Navigare ierarhică
- `IX_Ocupatii_ISCO08_Denumire` - Căutare text
- `IX_Ocupatii_ISCO08_Activ` - Filtrare active/inactive

## Stored Procedures

### Consultare
- `sp_Ocupatii_ISCO08_GetAll` - Listare cu paginare și filtrare
- `sp_Ocupatii_ISCO08_GetById` - Căutare după ID
- `sp_Ocupatii_ISCO08_GetByCod` - Căutare după cod ISCO
- `sp_Ocupatii_ISCO08_GetCopii` - Obținere copii ierarhici
- `sp_Ocupatii_ISCO08_GetGrupeMajore` - Lista grupelor majore

### Căutare și Raportare  
- `sp_Ocupatii_ISCO08_Search` - Căutare avansată cu scoring
- `sp_Ocupatii_ISCO08_GetStatistics` - Statistici generale
- `sp_Ocupatii_ISCO08_GetDropdownOptions` - Opțiuni pentru dropdown-uri

## Utilizare în Aplicația ValyanClinic

### Scenarii de Utilizare

1. **Înregistrarea Personalului Medical**
   ```sql
   -- Selectarea ocupației pentru un medic cardiolog
   EXEC sp_Ocupatii_ISCO08_Search @SearchText = 'cardiolog', @NivelIerarhic = 4
   ```

2. **Raportări HR**
   ```sql
   -- Distribuția personalului pe grupe majore
   SELECT 
       o.Grupa_Majora_Denumire,
       COUNT(p.Id) as NumarAngajati
   FROM Personal p
   INNER JOIN Ocupatii_ISCO08 o ON p.Cod_ISCO = o.Cod_ISCO
   GROUP BY o.Grupa_Majora_Denumire
   ```

3. **Formulare Blazor**
   ```csharp
   // În componentele Blazor pentru selectarea ocupației
   var ocupatii = await ocupatiiService.GetDropdownOptionsAsync(nivelIerarhic: 4);
   ```

### Integrare cu Entity Framework

Entitatea `OcupatieISCO` este definită în `ValyanClinic.Domain.Entities` și include:
- Mapare completă a coloanelor
- Navigation properties pentru ierarhie
- Computed properties pentru UI (CodSiDenumire, NumeNivelIerarhic)
- Validări și constrangeri

## Import și Mentenanță

### Import Inițial
```powershell
# Din directorul DevSupport\Scripts\PowerShellScripts
.\Import-OcupatiiISCO.ps1 -XmlFilePath "isco-08-ocupatii-2024.xml"
```

### Populare cu Date Test
```sql
-- Rulare în SQL Server Management Studio
:r Ocupatii_ISCO08_SampleData.sql
```

### Actualizări Periodice
Datele ISCO-08 se actualizează anual. Pentru actualizări:

1. Descarcă noul fișier XML de la data.gov.ro
2. Rulează scriptul de import cu opțiunea de ștergere
3. Verifică integritatea datelor cu `sp_Ocupatii_ISCO08_GetStatistics`

## Performanță și Optimizare

### Recomandări
- Utilizează indexurile create pentru căutări rapide
- Pentru căutări text, folosește `sp_Ocupatii_ISCO08_Search` cu scoring
- Cache-ează grupele majore în aplicație (se schimbă rar)
- Utilizează paginarea pentru listări mari

### Statistici Estimative
- Grupe majore: 10
- Subgrupe: ~43  
- Grupe minore: ~130
- Ocupații detaliate: ~436
- **Total înregistrări: ~619**

## Conformitate și Standardizare

### Standarde Respectate
- **ILO ISCO-08** - Standard internațional oficial
- **ISO 3166** - Pentru codurile de țară (dacă aplicabil)
- **UTF-8** - Encoding pentru caractere românești

### Compatibilitate
- **ANOFM** - Compatibil cu sistemele Agenției Naționale pentru Ocuparea Forței de Muncă
- **INS** - Compatibil cu raportările către Institutul Național de Statistică  
- **UE** - Respectă cerințele de raportare europeană
- **OMS** - Pentru raportări în domeniul sănătății

## Securitate și Acces

### Permissions SQL Server
```sql
-- Doar citire pentru utilizatori normali
GRANT SELECT ON dbo.Ocupatii_ISCO08 TO [ValyanClinic_Users]

-- Acces complet pentru administratori
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.Ocupatii_ISCO08 TO [ValyanClinic_Admins]
```

### Audit Trail
- Toate modificările sunt înregistrate cu timestamp și utilizator
- Trigger automat pentru actualizarea `Data_Ultimei_Modificari`
- Istoric complet pentru compliance

---

**Ultima actualizare:** 2024-10-08  
**Versiune documentație:** 1.0  
**Contact:** echipa-dezvoltare@valyanmed.ro
