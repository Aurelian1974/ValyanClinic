# Instalare Rapidă - Tabela Pozitii

## 📋 Descriere
Scripturile SQL pentru crearea și configurarea tabelei **Pozitii** în baza de date **ValyanMed**.

## 📦 Fișiere Incluse

### 1. **Pozitii_Complete.sql** ⭐ RECOMANDAT
Script complet care include:
- Creare tabel cu structură completă
- Populare cu 20 poziții predefinite
- Indexuri și constrângeri
- Trigger pentru audit
- Comentarii pentru documentație

**📍 Locație:** `DevSupport/Database/TableStructure/Pozitii_Complete.sql`

### 2. **sp_Pozitii.sql**
Toate stored procedures pentru operațiuni CRUD:
- sp_Pozitii_GetAll (listă paginată cu filtre)
- sp_Pozitii_GetCount (număr total)
- sp_Pozitii_GetById (căutare după ID)
- sp_Pozitii_GetByDenumire (căutare după denumire)
- sp_Pozitii_GetDropdownOptions (opțiuni pentru dropdown-uri)
- sp_Pozitii_Create (creare poziție nouă)
- sp_Pozitii_Update (actualizare poziție)
- sp_Pozitii_Delete (soft delete)
- sp_Pozitii_HardDelete (ștergere fizică)
- sp_Pozitii_CheckUnique (verificare unicitate)
- sp_Pozitii_GetStatistics (statistici)

**📍 Locație:** `DevSupport/Database/StoredProcedures/sp_Pozitii.sql`

### 3. **Pozitii_Install.sql** 🚀 INSTALARE AUTOMATĂ
Script master pentru instalare completă automată (All-in-One):
- Creare tabel
- Populare date
- Creare SP-uri principale
- Verificare automată

**📍 Locație:** `DevSupport/Database/TableStructure/Pozitii_Install.sql`

### 4. **Pozitii_Verify.sql** ✅ VERIFICARE
Script pentru verificarea instalării și testarea funcționalității:
- Verificare structură tabel
- Verificare constrângeri și indexuri
- Verificare date populate
- Verificare SP-uri
- Teste funcționale

**📍 Locație:** `DevSupport/Database/TableStructure/Pozitii_Verify.sql`

### 5. **Pozitii_README.md** 📖 DOCUMENTAȚIE
Documentație completă cu:
- Structură tabel detaliată
- Descriere toate stored procedures
- Exemple de utilizare
- Ghid de securitate
- Exemple de cod C#

**📍 Locație:** `DevSupport/Database/TableStructure/Pozitii_README.md`

---

## 🚀 Instalare Rapidă

### Opțiunea 1: Instalare Automată (RECOMANDAT)
```sql
-- Rulați acest script pentru instalare completă automată:
USE [ValyanMed]
GO

-- Rulați:
-- DevSupport/Database/TableStructure/Pozitii_Install.sql
```

### Opțiunea 2: Instalare Manuală (Pas cu Pas)
```sql
-- 1. Creați tabelul și populați datele
USE [ValyanMed]
GO
-- Rulați: DevSupport/Database/TableStructure/Pozitii_Complete.sql

-- 2. Creați stored procedures
-- Rulați: DevSupport/Database/StoredProcedures/sp_Pozitii.sql

-- 3. Verificați instalarea
-- Rulați: DevSupport/Database/TableStructure/Pozitii_Verify.sql
```

---

## ✅ Verificare Instalare

### Test Rapid
```sql
USE [ValyanMed]
GO

-- Verificare tabel
SELECT * FROM Pozitii;

-- Verificare SP-uri
SELECT name FROM sys.procedures WHERE name LIKE 'sp_Pozitii_%';

-- Test funcțional
EXEC sp_Pozitii_GetAll @PageNumber = 1, @PageSize = 10;
```

### Verificare Completă
```sql
-- Rulați scriptul de verificare:
-- DevSupport/Database/TableStructure/Pozitii_Verify.sql
```

---

## 📊 Date Incluse

Lista pozițiilor predefinite (20 total):

**Personal Medical Superior:**
- Medic primar
- Medic specialist
- Medic rezident
- Medic stomatolog

**Personal Medical Specializat:**
- Farmacist
- Biolog
- Biochimist
- Chimist

**Poziții de Conducere:**
- Șef de secție
- Șef de laborator
- Șef de compartiment
- Farmacist-șef

**Personal Medical Asistent:**
- Asistent medical generalist
- Asistent medical cu studii superioare specialitatea medicina generală
- Asistent medical cu studii postliceale medicina generală
- Moașă

**Personal de Suport:**
- Infirmieră (debutantă și cu vechime)
- Îngrijitoare
- Brancardier
- Kinetoterapeut

---

## 🔧 Utilizare în Cod C#

### Exemplu: Obținere listă pentru dropdown
```csharp
using Dapper;
using System.Data;

public async Task<IEnumerable<DropdownOption>> GetPozitiiDropdownAsync()
{
    using var connection = _connectionFactory.CreateConnection();
    
    return await connection.QueryAsync<DropdownOption>(
        "sp_Pozitii_GetDropdownOptions",
        new { EsteActiv = true },
        commandType: CommandType.StoredProcedure
    );
}
```

### Exemplu: Creare poziție nouă
```csharp
public async Task<Pozitie> CreatePozitieAsync(string denumire, string descriere)
{
    using var connection = _connectionFactory.CreateConnection();
    
    return await connection.QueryFirstOrDefaultAsync<Pozitie>(
        "sp_Pozitii_Create",
        new 
        {
            Denumire = denumire,
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
- Poziția NU este ștearsă fizic în mod implicit
- Folosiți `sp_Pozitii_Delete` pentru soft delete
- `sp_Pozitii_HardDelete` disponibil pentru cazuri speciale

✅ **Audit Trail:**
- `Data_Crearii` + `Creat_De`
- `Data_Ultimei_Modificari` + `Modificat_De`
- Trigger automat pentru actualizare timestamp

---

## 🐛 Troubleshooting

### Eroare: "Object already exists"
```sql
-- Ștergeți tabelul existent:
DROP TABLE IF EXISTS dbo.Pozitii;

-- Apoi rulați din nou scriptul de instalare
```

### Eroare: "Stored procedure already exists"
```sql
-- SP-urile vor fi șterse automat și recreate de script
-- Dacă persistă eroarea, ștergeți manual:
DROP PROCEDURE IF EXISTS sp_Pozitii_GetAll;
-- ... pentru fiecare SP
```

### Date nu apar după instalare
```sql
-- Verificați dacă datele au fost inserate:
SELECT COUNT(*) FROM Pozitii;

-- Dacă e 0, rulați manual INSERT-urile din Pozitii_Complete.sql
```

---

## 📞 Support

Pentru probleme sau întrebări:
1. Verificați **Pozitii_README.md** pentru documentație completă
2. Rulați **Pozitii_Verify.sql** pentru diagnostic
3. Contactați echipa de dezvoltare ValyanClinic

---

## 📌 Versiuni

- **v1.0** (2025-01-20) - Release inițial
  - Tabel Pozitii cu 20 poziții predefinite
  - 11 Stored Procedures
  - Documentație completă
  - Scripturi de instalare și verificare

---

**Database:** ValyanMed  
**Autor:** ValyanClinic Development Team  
**Last Updated:** 2025-01-20
