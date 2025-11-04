# 🏥 Setup Modul Pacienti - Instrucțiuni Complete

## ⚠️ PROBLEMĂ IDENTIFICATĂ

Aplicația rămâne în starea "Se încarcă datele..." deoarece:
- ❌ Tabelul `Pacienti` NU există în baza de date
- ❌ Stored procedures `sp_Pacienti_*` NU există în baza de date

Când aplicația pornește și încercă să încarce datele, apelează `sp_Pacienti_GetAll` care nu există, generând o excepție SQL.

---

## 🔧 SOLUȚIE RAPIDĂ (5 minute)

### Metoda 1: Script SQL Manual (RECOMANDAT)

1. **Deschide SQL Server Management Studio (SSMS)**
   - Server: `DESKTOP-9H54BCS\SQLSERVER`
   - Authentication: Windows Authentication

2. **Conectează-te la baza de date ValyanMed**
   ```sql
   USE [ValyanMed]
   GO
   ```

3. **Rulează scriptul complet**
   - Fișier: `DevSupport\Database\SETUP_PACIENTI_MANUAL.sql`
   - **SAU** copiază tot conținutul și apasă F5

4. **Verifică rezultatul**
   - Ar trebui să vezi mesajul: "SETUP MODUL PACIENTI - COMPLET!"
   - Verifică că există 3 pacienți de test

---

### Metoda 2: Scripturi Separate

Dacă preferi să rulezi scripturile separate:

#### Pasul 1: Creează tabelul
```bash
Fișier: DevSupport\Database\TableStructure\Pacienti_Complete.sql
```
Deschide în SSMS și rulează (F5)

#### Pasul 2: Creează stored procedures
```bash
Fișier: DevSupport\Database\StoredProcedures\sp_Pacienti.sql
```
Deschide în SSMS și rulează (F5)

---

## ✅ VERIFICARE SETUP CORECT

După ce ai rulat scripturile, execută următoarea interogare în SSMS:

```sql
USE [ValyanMed]
GO

-- 1. Verificare tabela
SELECT 
    CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'Pacienti')
    THEN '✓ Tabela Pacienti EXISTA'
    ELSE '✗ Tabela Pacienti NU EXISTA'
    END AS 'Status Tabela'

-- 2. Verificare stored procedures
SELECT COUNT(*) AS 'Nr Stored Procedures'
FROM sys.procedures 
WHERE name LIKE 'sp_Pacienti_%'

-- 3. Verificare date
SELECT COUNT(*) AS 'Nr Pacienti'
FROM Pacienti

-- 4. Vizualizare pacienti de test
SELECT Cod_Pacient, Nume, Prenume, Telefon 
FROM Pacienti
```

### Rezultat Așteptat:
- ✅ **Status Tabela**: `✓ Tabela Pacienti EXISTA`
- ✅ **Nr Stored Procedures**: `16` (sau cel puțin 3)
- ✅ **Nr Pacienti**: `3` (pacienți de test)

---

## 🚀 PORNIRE APLICAȚIE

După setup-ul corect al bazei de date:

1. **Pornește aplicația**
   ```bash
   F5 în Visual Studio
   ```
   SAU
   ```bash
   dotnet run --project ValyanClinic
   ```

2. **Navighează la modulul Pacienti**
   ```
   URL: https://localhost:XXXX/pacienti/vizualizare
   ```

3. **Ar trebui să vezi**:
   - Grid Syncfusion cu 3 pacienți de test
   - Butoane: Refresh, View, Add, Edit, Delete
   - Filtre și căutare funcționale

---

## 🐛 DEBUGGING

### Dacă aplicația tot nu pornește:

1. **Verifică logurile Serilog**
   ```bash
   ValyanClinic\Logs\errors-{DATA}.log
   ```

2. **Verifică connection string**
   ```json
   // ValyanClinic\appsettings.json
   "ConnectionStrings": {
     "DefaultConnection": "Server=DESKTOP-9H54BCS\\SQLSERVER;Database=ValyanMed;..."
   }
   ```

3. **Testează conexiunea SQL manual**
   ```sql
   -- Rulează în SSMS
   SELECT @@SERVERNAME AS 'Server Name'
   SELECT DB_NAME() AS 'Current Database'
   
   -- Testează stored procedure
   EXEC sp_Pacienti_GetAll
   ```

4. **Verifică Output Window în Visual Studio**
   - View → Output
   - Show output from: Debug

---

## 📊 STRUCTURA MODULULUI PACIENTI

### Fișiere Create:

#### Domain Layer
- ✅ `ValyanClinic.Domain\Entities\Pacient.cs`
- ✅ `ValyanClinic.Domain\Interfaces\Repositories\IPacientRepository.cs`

#### Infrastructure Layer
- ✅ `ValyanClinic.Infrastructure\Repositories\PacientRepository.cs`

#### Application Layer
- ✅ `ValyanClinic.Application\Features\PacientManagement\Queries\GetPacientList\*`
- ✅ `ValyanClinic.Application\Features\PacientManagement\Queries\GetPacientById\*`

#### Presentation Layer (Blazor)
- ✅ `ValyanClinic\Components\Pages\Pacienti\VizualizarePacienti.razor[.cs/.css]`
- ✅ `ValyanClinic\Components\Pages\Pacienti\AdministrarePacienti.razor`
- ✅ `ValyanClinic\Components\Pages\Pacienti\Modals\PacientViewModal.razor[.cs/.css]`

#### Database
- ✅ `DevSupport\Database\TableStructure\Pacienti_Complete.sql`
- ✅ `DevSupport\Database\StoredProcedures\sp_Pacienti.sql`
- ✅ `DevSupport\Database\SETUP_PACIENTI_MANUAL.sql` (script complet)

#### Program.cs
- ✅ `IPacientRepository` înregistrat în DI container

---

## 📝 URMĂTORII PAȘI (după pornirea cu succes)

1. **Testează funcționalitatea de vizualizare**
   - Grid cu paginare
   - Sortare pe coloane
   - Filtrare
   - View detalii pacient

2. **Implementează CRUD complet** (viitor)
   - Create pacient
   - Edit pacient
   - Delete pacient (soft delete)

3. **Adaugă validări** (viitor)
   - Validare CNP
   - Validare telefon/email
   - Validare date obligatorii

---

## 🆘 AJUTOR

Dacă întâmpini probleme:

1. **Verifică că SQL Server rulează**
   ```bash
   Services → SQL Server (SQLSERVER) → Status: Running
   ```

2. **Verifică permisiuni**
   - User-ul `DESKTOP-9H54BCS\User` trebuie să aibă acces la `ValyanMed`

3. **Rulează din nou scripturile**
   - Scripturile sunt idempotente (pot fi rulate de mai multe ori)

---

## ✨ CARACTERISTICI IMPLEMENTATE

- ✅ Clean Architecture (Domain, Application, Infrastructure, Presentation)
- ✅ CQRS cu MediatR
- ✅ Repository Pattern cu Dapper
- ✅ Stored Procedures pentru performance
- ✅ Syncfusion Blazor Grid cu paginare server-side
- ✅ Modal pentru vizualizare detalii (6 tabs)
- ✅ Design medical theme (verde)
- ✅ Responsive layout
- ✅ Logging cu Serilog

---

**Data ultimei actualizări:** 2025-01-23
**Status:** ✅ Gata de testare după setup-ul bazei de date
