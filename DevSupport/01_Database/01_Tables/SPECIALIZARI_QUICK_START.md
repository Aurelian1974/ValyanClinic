## Instalare Rapidă - Tabela Specializari

## 📋 Descriere
Scripturile SQL pentru crearea și configurarea tabelei **Specializari** în baza de date **ValyanMed**.

Tabela conține **66 specializări medicale** organizate în **5 categorii**:
- **Medicală** (35 specializări)
- **Chirurgicală** (14 specializări)
- **Laborator și Diagnostic** (9 specializări)
- **Stomatologie** (8 specializări)
- **Farmaceutică** (4 specializări)

---

## 📦 Fișiere Incluse

### 1. **Specializari_Complete.sql** ⭐ RECOMANDAT
Script complet care include:
- Creare tabel cu structură completă
- Populare cu 66 specializări medicale predefinite
- Indexuri și constrângeri
- Trigger pentru audit
- Comentarii pentru documentație

**📍 Locație:** `DevSupport/Database/TableStructure/Specializari_Complete.sql`

### 2. **sp_Specializari.sql**
Toate stored procedures pentru operațiuni CRUD (13 total):
- sp_Specializari_GetAll (listă paginată cu filtre)
- sp_Specializari_GetCount (număr total)
- sp_Specializari_GetById (căutare după ID)
- sp_Specializari_GetByDenumire (căutare după denumire)
- sp_Specializari_GetByCategorie (filtrare după categorie)
- sp_Specializari_GetCategorii (listă categorii)
- sp_Specializari_GetDropdownOptions (opțiuni pentru dropdown-uri)
- sp_Specializari_Create (creare specializare nouă)
- sp_Specializari_Update (actualizare specializare)
- sp_Specializari_Delete (soft delete)
- sp_Specializari_HardDelete (ștergere fizică)
- sp_Specializari_CheckUnique (verificare unicitate)
- sp_Specializari_GetStatistics (statistici)

**📍 Locație:** `DevSupport/Database/StoredProcedures/sp_Specializari.sql`

### 3. **Specializari_Install.sql** 🚀 INSTALARE AUTOMATĂ
Script master pentru instalare completă automată (All-in-One):
- Creare tabel
- Populare cu 66 specializări
- Creare SP-uri principale (4)
- Verificare automată

**📍 Locație:** `DevSupport/Database/TableStructure/Specializari_Install.sql`

### 4. **Specializari_Verify.sql** ✅ VERIFICARE
Script pentru verificarea instalării și testarea funcționalității:
- Verificare structură tabel
- Verificare constrângeri și indexuri
- Verificare date populate (66 specializări)
- Verificare SP-uri
- Teste funcționale
- Verificare integritate date

**📍 Locație:** `DevSupport/Database/TableStructure/Specializari_Verify.sql`

---

## 🚀 Instalare Rapidă

### Opțiunea 1: Instalare Automată (RECOMANDAT)
```sql
-- Rulați acest script pentru instalare completă automată:
USE [ValyanMed]
GO

-- Rulați:
-- DevSupport/Database/TableStructure/Specializari_Install.sql
```

### Opțiunea 2: Instalare Manuală (Pas cu Pas)
```sql
-- 1. Creați tabelul și populați datele
USE [ValyanMed]
GO
-- Rulați: DevSupport/Database/TableStructure/Specializari_Complete.sql

-- 2. Creați stored procedures complete
-- Rulați: DevSupport/Database/StoredProcedures/sp_Specializari.sql

-- 3. Verificați instalarea
-- Rulați: DevSupport/Database/TableStructure/Specializari_Verify.sql
```

---

## ✅ Verificare Instalare

### Test Rapid
```sql
USE [ValyanMed]
GO

-- Verificare tabel
SELECT COUNT(*) AS TotalSpecializari FROM Specializari;
-- Ar trebui să returneze: 66

-- Verificare categorii
SELECT Categorie, COUNT(*) AS Numar
FROM Specializari
GROUP BY Categorie
ORDER BY Categorie;

-- Verificare SP-uri
SELECT name FROM sys.procedures WHERE name LIKE 'sp_Specializari_%';

-- Test funcțional
EXEC sp_Specializari_GetAll @PageNumber = 1, @PageSize = 10;
```

### Verificare Completă
```sql
-- Rulați scriptul de verificare:
-- DevSupport/Database/TableStructure/Specializari_Verify.sql
```

---

## 📊 Date Incluse

### Specializări Medicale (35)
- Alergologie și imunologie clinică
- Anestezie și terapie intensivă
- Boli infecțioase
- Cardiologie
- Cardiologie pediatrică
- Dermatovenerologie
- Diabet zaharat, nutriție și boli metabolice
- Endocrinologie
- Expertiza medicală a capacității de muncă
- Farmacologie clinică
- Gastroenterologie
- Gastroenterologie pediatrică
- Genetică medicală
- Geriatrie și gerontologie
- Hematologie
- Medicină de familie
- Medicină de urgență
- Medicină internă
- Medicină fizică și de reabilitare
- Medicina muncii
- Medicină sportivă
- Nefrologie
- Nefrologie pediatrică
- Neonatologie
- Neurologie
- Neurologie pediatrică
- Oncologie medicală
- Oncologie și hematologie pediatrică
- Pediatrie
- Pneumologie
- Pneumologie pediatrică
- Psihiatrie
- Psihiatrie pediatrică
- Radioterapie
- Reumatologie

### Specializări Chirurgicale (14)
- Chirurgie cardiovasculară
- Chirurgie generală
- Chirurgie orală și maxilo-facială
- Chirurgie pediatrică
- Chirurgie plastică, estetică și microchirurgie reconstructivă
- Chirurgie toracică
- Chirurgie vasculară
- Neurochirurgie
- Obstetrică-ginecologie
- Oftalmologie
- Ortopedie pediatrică
- Ortopedie și traumatologie
- Otorinolaringologie
- Urologie

### Specializări Laborator și Diagnostic (9)
- Anatomie patologică
- Epidemiologie
- Igienă
- Medicină de laborator
- Medicină legală
- Medicină nucleară
- Microbiologie medicală
- Radiologie-imagistică medicală
- Sănătate publică și management

### Specializări Stomatologie (8)
- Chirurgie dento-alveolară
- Ortodonție și ortopedie dento-facială
- Endodonție
- Parodontologie
- Pedodonție
- Protetică dentară
- Chirurgie stomatologică și maxilo-facială
- Stomatologie generală

### Specializări Farmaceutice (4)
- Farmacie clinică
- Analize medico-farmaceutice de laborator
- Farmacie generală
- Industrie farmaceutică și cosmetică

---

## 🔧 Utilizare în Cod C#

### Exemplu: Obținere listă pentru dropdown
```csharp
using Dapper;
using System.Data;

public async Task<IEnumerable<DropdownOption>> GetSpecializariDropdownAsync(string categorie = null)
{
    using var connection = _connectionFactory.CreateConnection();
    
    return await connection.QueryAsync<DropdownOption>(
        "sp_Specializari_GetDropdownOptions",
        new { Categorie = categorie, EsteActiv = true },
        commandType: CommandType.StoredProcedure
    );
}
```

### Exemplu: Obținere specializări pe categorie
```csharp
public async Task<IEnumerable<Specializare>> GetSpecializariByCategorie(string categorie)
{
    using var connection = _connectionFactory.CreateConnection();
    
    return await connection.QueryAsync<Specializare>(
        "sp_Specializari_GetByCategorie",
        new { Categorie = categorie, EsteActiv = true },
        commandType: CommandType.StoredProcedure
    );
}
```

### Exemplu: Obținere categorii pentru filtru
```csharp
public async Task<IEnumerable<CategorieInfo>> GetCategoriiAsync()
{
    using var connection = _connectionFactory.CreateConnection();
    
    return await connection.QueryAsync<CategorieInfo>(
        "sp_Specializari_GetCategorii",
        commandType: CommandType.StoredProcedure
    );
}
```

### Exemplu: Creare specializare nouă
```csharp
public async Task<Specializare> CreateSpecializareAsync(
    string denumire, 
    string categorie, 
    string descriere = null)
{
    using var connection = _connectionFactory.CreateConnection();
    
    return await connection.QueryFirstOrDefaultAsync<Specializare>(
        "sp_Specializari_Create",
        new 
        {
            Denumire = denumire,
            Categorie = categorie,
            Descriere = descriere,
            EsteActiv = true,
            CreatDe = _currentUser.Email
        },
        commandType: CommandType.StoredProcedure
    );
}
```

---

## 🛡️ Securitate

✅ **SQL Injection Prevention:**
- Toate SP-urile folosesc parametri
- Validare input pentru sortare
- Whitelist pentru coloane de sortare

✅ **Soft Delete:**
- Specializarea NU este ștearsă fizic în mod implicit
- Folosiți `sp_Specializari_Delete` pentru soft delete
- `sp_Specializari_HardDelete` disponibil pentru cazuri speciale

✅ **Audit Trail:**
- `Data_Crearii` + `Creat_De`
- `Data_Ultimei_Modificari` + `Modificat_De`
- Trigger automat pentru actualizare timestamp

✅ **Integritate Date:**
- Constrangere UNIQUE pe denumire
- Index pe categorie pentru filtrare rapidă
- Coloana Categorie pentru organizare logică

---

## 📈 Statistici

După instalare, veți avea:
- **66 specializări medicale** organizate
- **5 categorii** distincte
- **13 stored procedures** complete
- **4 indexuri** pentru performanță
- **1 trigger** pentru audit automat

---

## 🐛 Troubleshooting

### Eroare: "Object already exists"
```sql
-- Ștergeți tabelul existent:
DROP TABLE IF EXISTS dbo.Specializari;

-- Apoi rulați din nou scriptul de instalare
```

### Eroare: "Stored procedure already exists"
```sql
-- SP-urile vor fi șterse automat și recreate de script
-- Dacă persistă eroarea, ștergeți manual:
DROP PROCEDURE IF EXISTS sp_Specializari_GetAll;
-- ... pentru fiecare SP
```

### Date incomplete după instalare
```sql
-- Verificați câte specializări au fost inserate:
SELECT COUNT(*) FROM Specializari;

-- Ar trebui să fie 66
-- Dacă e mai puțin, verificați erorile în timpul INSERT-urilor
```

### Categorii lipsă
```sql
-- Verificați categoriile disponibile:
SELECT DISTINCT Categorie, COUNT(*) AS Numar
FROM Specializari
GROUP BY Categorie;

-- Ar trebui să aveți 5 categorii
```

---

## 🔗 Relații cu Alte Tabele

Tabela `Specializari` poate fi referențiată de:
- `PersonalMedical` - prin coloana `SpecializareID` sau `Specializare` (relație foreign key recomandată)
- `Consultatii` - pentru specializarea consultației
- `Programari` - pentru filtrare după specializare

**⚠️ Recomandare:** Adăugați o coloană `Id_Specializare` (UNIQUEIDENTIFIER) în tabelele relevante pentru o relație foreign key directă.

---

## 📞 Support

Pentru probleme sau întrebări:
1. Verificați **Specializari_README.md** pentru documentație completă (va fi creat separat)
2. Rulați **Specializari_Verify.sql** pentru diagnostic
3. Contactați echipa de dezvoltare ValyanClinic

---

## 📌 Versiuni

- **v1.0** (2025-01-20) - Release inițial
  - Tabel Specializari cu 66 specializări predefinite
  - 13 Stored Procedures
  - 5 categorii organizate
  - Documentație completă
  - Scripturi de instalare și verificare

---

## 📚 Sursă Date

Datele pentru specializări au fost preluate din:
- **Fișier sursă:** `DevSupport/Documentation/Development/specializari.txt`
- **Standard:** Specializări medicale conform normelor din România
- **Organizare:** Categorii logice pentru ușurința în utilizare

---

**Database:** ValyanMed  
**Autor:** ValyanClinic Development Team  
**Last Updated:** 2025-01-20  
**Total Specializări:** 66  
**Categorii:** 5
