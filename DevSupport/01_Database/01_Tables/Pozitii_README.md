# Tabela Pozitii - Documentație

## Descriere
Tabela `Pozitii` stochează pozițiile/funcțiile pentru personalul medical și non-medical din cadrul clinicii ValyanMed.

## Structură Tabel

### Coloane

| Coloană | Tip | Descriere | Constrângeri |
|---------|-----|-----------|--------------|
| `Id` | UNIQUEIDENTIFIER | Identificator unic (PK) | NOT NULL, DEFAULT NEWSEQUENTIALID() |
| `Denumire` | NVARCHAR(200) | Denumirea poziției | NOT NULL, UNIQUE |
| `Descriere` | NVARCHAR(MAX) | Descriere detaliată (opțional) | NULL |
| `Este_Activ` | BIT | Indicator de activitate | NOT NULL, DEFAULT 1 |
| `Data_Crearii` | DATETIME2(7) | Data creării înregistrării | NOT NULL, DEFAULT GETDATE() |
| `Data_Ultimei_Modificari` | DATETIME2(7) | Data ultimei modificări | NOT NULL, DEFAULT GETDATE() |
| `Creat_De` | NVARCHAR(100) | Utilizator care a creat înregistrarea | NULL, DEFAULT SYSTEM_USER |
| `Modificat_De` | NVARCHAR(100) | Utilizator care a modificat ultima dată | NULL, DEFAULT SYSTEM_USER |

### Constrângeri
- **Primary Key**: `PK_Pozitii` pe coloana `Id`
- **Unique Key**: `UK_Pozitii_Denumire` pe coloana `Denumire`

### Indexuri
- `IX_Pozitii_Denumire` - Index pe coloana `Denumire`
- `IX_Pozitii_Activ` - Index pe coloana `Este_Activ`

### Trigger-e
- `TR_Pozitii_UpdateTimestamp` - Actualizează automat `Data_Ultimei_Modificari` și `Modificat_De` la UPDATE

## Date Inițiale

Tabela este populată inițial cu următoarele poziții:

### Personal Medical Superior
- Medic primar
- Medic specialist
- Medic rezident
- Medic stomatolog

### Personal Medical Specializat
- Farmacist
- Biolog
- Biochimist
- Chimist

### Poziții de Conducere
- Șef de secție
- Șef de laborator
- Șef de compartiment
- Farmacist-șef

### Personal Medical Asistent
- Asistent medical generalist
- Asistent medical cu studii superioare specialitatea medicina generală
- Asistent medical cu studii postliceale medicina generală
- Moașă

### Personal de Suport
- Infirmieră (debutantă și cu vechime)
- Îngrijitoare
- Brancardier
- Kinetoterapeut

## Stored Procedures

### 1. sp_Pozitii_GetAll
Obține lista paginată de poziții cu filtrare și sortare.

**Parametri:**
- `@PageNumber` (INT, default 1) - Numărul paginii
- `@PageSize` (INT, default 50) - Dimensiunea paginii
- `@SearchText` (NVARCHAR(255), opțional) - Text pentru căutare în denumire
- `@EsteActiv` (BIT, opțional) - Filtru pentru poziții active/inactive
- `@SortColumn` (NVARCHAR(50), default 'Denumire') - Coloana pentru sortare
- `@SortDirection` (NVARCHAR(4), default 'ASC') - Direcția sortării

**Exemplu:**
```sql
EXEC sp_Pozitii_GetAll 
    @PageNumber = 1, 
    @PageSize = 20, 
    @SearchText = 'medic',
    @EsteActiv = 1;
```

### 2. sp_Pozitii_GetCount
Returnează numărul total de poziții cu filtrare.

**Parametri:**
- `@SearchText` (NVARCHAR(255), opțional)
- `@EsteActiv` (BIT, opțional)

**Exemplu:**
```sql
EXEC sp_Pozitii_GetCount @EsteActiv = 1;
```

### 3. sp_Pozitii_GetById
Obține o poziție după ID.

**Parametri:**
- `@Id` (UNIQUEIDENTIFIER) - ID-ul poziției

**Exemplu:**
```sql
EXEC sp_Pozitii_GetById @Id = '12345678-1234-1234-1234-123456789012';
```

### 4. sp_Pozitii_GetByDenumire
Obține o poziție după denumire exactă.

**Parametri:**
- `@Denumire` (NVARCHAR(200)) - Denumirea poziției

**Exemplu:**
```sql
EXEC sp_Pozitii_GetByDenumire @Denumire = N'Medic primar';
```

### 5. sp_Pozitii_GetDropdownOptions
Returnează opțiuni pentru dropdown-uri (Value/Text pairs).

**Parametri:**
- `@EsteActiv` (BIT, default 1) - Doar poziții active

**Exemplu:**
```sql
EXEC sp_Pozitii_GetDropdownOptions @EsteActiv = 1;
```

### 6. sp_Pozitii_Create
Creează o poziție nouă.

**Parametri:**
- `@Denumire` (NVARCHAR(200)) - Denumirea poziției **(obligatoriu)**
- `@Descriere` (NVARCHAR(MAX), opțional) - Descriere detaliată
- `@EsteActiv` (BIT, default 1) - Indicator de activitate
- `@CreatDe` (NVARCHAR(100)) - Utilizator care creează **(obligatoriu)**

**Exemplu:**
```sql
EXEC sp_Pozitii_Create 
    @Denumire = N'Asistent medical debutant',
    @Descriere = N'Asistent medical cu experiență sub 2 ani',
    @EsteActiv = 1,
    @CreatDe = N'admin@valyanclinic.ro';
```

**Excepții:**
- `50001` - Denumirea există deja

### 7. sp_Pozitii_Update
Actualizează o poziție existentă.

**Parametri:**
- `@Id` (UNIQUEIDENTIFIER) - ID-ul poziției **(obligatoriu)**
- `@Denumire` (NVARCHAR(200)) - Noua denumire **(obligatoriu)**
- `@Descriere` (NVARCHAR(MAX), opțional) - Noua descriere
- `@EsteActiv` (BIT) - Starea de activitate **(obligatoriu)**
- `@ModificatDe` (NVARCHAR(100)) - Utilizator care modifică **(obligatoriu)**

**Exemplu:**
```sql
EXEC sp_Pozitii_Update 
    @Id = '12345678-1234-1234-1234-123456789012',
    @Denumire = N'Medic primar specialist',
    @Descriere = N'Medic primar cu specializare în cardiologie',
    @EsteActiv = 1,
    @ModificatDe = N'admin@valyanclinic.ro';
```

**Excepții:**
- `50001` - Denumirea există deja
- `50002` - Poziția nu există

### 8. sp_Pozitii_Delete
Soft delete - dezactivează o poziție (setează `Este_Activ = 0`).

**Parametri:**
- `@Id` (UNIQUEIDENTIFIER) - ID-ul poziției **(obligatoriu)**
- `@ModificatDe` (NVARCHAR(100)) - Utilizator care șterge **(obligatoriu)**

**Exemplu:**
```sql
EXEC sp_Pozitii_Delete 
    @Id = '12345678-1234-1234-1234-123456789012',
    @ModificatDe = N'admin@valyanclinic.ro';
```

**Excepții:**
- `50002` - Poziția nu există

### 9. sp_Pozitii_HardDelete
**⚠️ UTILIZARE CU PRECAUȚIE** - Șterge definitiv o poziție din baza de date.

**Parametri:**
- `@Id` (UNIQUEIDENTIFIER) - ID-ul poziției **(obligatoriu)**

**Exemplu:**
```sql
EXEC sp_Pozitii_HardDelete @Id = '12345678-1234-1234-1234-123456789012';
```

**⚠️ Notă**: Verificați întâi dacă poziția nu este referențiată în alte tabele (ex: `Personal`, `PersonalMedical`).

### 10. sp_Pozitii_CheckUnique
Verifică dacă o denumire este unică.

**Parametri:**
- `@Denumire` (NVARCHAR(200)) - Denumirea de verificat **(obligatoriu)**
- `@ExcludeId` (UNIQUEIDENTIFIER, opțional) - ID pentru excludere (folosit la UPDATE)

**Return:**
- `Denumire_Exists` (INT) - 1 dacă există, 0 dacă nu există

**Exemplu:**
```sql
EXEC sp_Pozitii_CheckUnique 
    @Denumire = N'Medic primar',
    @ExcludeId = '12345678-1234-1234-1234-123456789012';
```

### 11. sp_Pozitii_GetStatistics
Returnează statistici despre poziții pentru dashboard.

**Exemplu:**
```sql
EXEC sp_Pozitii_GetStatistics;
```

**Return:**
- `Categorie` - Descrierea statisticii
- `Numar` - Numărul total de poziții
- `Active` - Numărul de poziții active

## Instalare

### 1. Crearea tabelei și popularea datelor
```sql
-- Rulați scriptul din:
DevSupport/Database/TableStructure/Pozitii_Complete.sql
```

### 2. Crearea stored procedures
```sql
-- Rulați scriptul din:
DevSupport/Database/StoredProcedures/sp_Pozitii.sql
```

## Securitate

✅ **Protecție împotriva SQL Injection:**
- Toate stored procedures folosesc parametri
- Validare input pentru `SortDirection` și `SortColumn`
- Whitelist pentru coloanele de sortare

✅ **Soft Delete:**
- Poziția nu este ștearsă fizic în mod implicit
- Se folosește `sp_Pozitii_Delete` pentru soft delete
- `sp_Pozitii_HardDelete` disponibil pentru ștergere fizică (cu precauție)

## Relații cu Alte Tabele

Tabela `Pozitii` poate fi referențiată de:
- `Personal` - prin coloana `Functia` (relație indirectă prin denumire)
- `PersonalMedical` - prin coloana `Pozitie` (relație indirectă prin denumire)

**⚠️ Recomandare:** Adăugați o coloană `Id_Pozitie` (UNIQUEIDENTIFIER) în tabelele `Personal` și `PersonalMedical` pentru o relație foreign key directă.

## Exemple de Utilizare în Cod C#

```csharp
// Obținere listă poziții pentru dropdown
var pozitii = await connection.QueryAsync<DropdownOption>(
    "sp_Pozitii_GetDropdownOptions", 
    new { EsteActiv = true },
    commandType: CommandType.StoredProcedure);

// Creare poziție nouă
var result = await connection.QueryFirstOrDefaultAsync<Pozitie>(
    "sp_Pozitii_Create",
    new 
    {
        Denumire = "Asistent medical principal",
        Descriere = "Asistent medical cu experiență > 10 ani",
        EsteActiv = true,
        CreatDe = currentUser
    },
    commandType: CommandType.StoredProcedure);

// Verificare unicitate
var exists = await connection.QueryFirstOrDefaultAsync<int>(
    "sp_Pozitii_CheckUnique",
    new { Denumire = "Medic primar", ExcludeId = (Guid?)null },
    commandType: CommandType.StoredProcedure);
```

## Întreținere

### Backup
```sql
-- Backup date poziții
SELECT * INTO Pozitii_Backup_20250120 FROM Pozitii;
```

### Audit
Monitorizați:
- `Data_Crearii` și `Creat_De` pentru tracking creări
- `Data_Ultimei_Modificari` și `Modificat_De` pentru tracking modificări

## Versiuni

- **v1.0** (2025-01-20) - Versiune inițială
  - Creare tabel
  - 20 poziții predefinite
  - 11 stored procedures
  - Documentație completă

---

**Autor:** ValyanClinic Development Team  
**Database:** ValyanMed  
**Last Updated:** 2025-01-20
