# Verificare Stored Procedures - Baza de Date vs Cod

## 📋 Prezentare Generală

Acest document verifică concordanța dintre stored procedure-urile definite în baza de date și cele utilizate în codul aplicației ValyanClinic. Obiectivul este să ne asigurăm că nu există discrepanțe de denumire care ar putea cauza erori la runtime.

## 🔍 Metodologie de Verificare

Am analizat următoarele surse:
1. **Codul C# din Repository-uri** - pentru a identifica SP-urile apelate
2. **Scripturile SQL** - pentru a vedea SP-urile create
3. **Fișierele de documentație** - pentru a înțelege arhitectura

## 📊 Rezultatele Verificării

### 1. **Personal Module (PersonalRepository.cs)**

| Stored Procedure | Utilizat în Metoda | Status |
|-----------------|-------------------|---------|
| `sp_Personal_GetAll` | `GetAllAsync()` | ✅ **Definit în cod** |
| `sp_Personal_GetById` | `GetByIdAsync()` | ✅ **Definit în cod** |
| `sp_Personal_Create` | `CreateAsync()` | ✅ **Definit în cod** |
| `sp_Personal_Update` | `UpdateAsync()` | ✅ **Definit în cod** |
| `sp_Personal_Delete` | `DeleteAsync()` | ✅ **Definit în cod** |
| `sp_Personal_CheckUnique` | `CheckUniqueAsync()` | ✅ **Definit în cod** |
| `sp_Personal_GetStatistics` | `GetStatisticsAsync()` | ✅ **Definit în cod** |

### 2. **PersonalMedical Module**

| Stored Procedure | Scop | Status |
|-----------------|------|---------|
| `sp_PersonalMedical_GetAll` | Listă personal medical cu filtrare și paginare | ✅ **Definit în script** |
| `sp_PersonalMedical_GetStatistics` | Statistici personal medical | ✅ **Definit în script** |
| `sp_PersonalMedical_GetById` | Personal medical după ID | ✅ **Definit în script** |
| `sp_PersonalMedical_CheckUnique` | Verificare unicitate Email și NumarLicenta | ✅ **Definit în script** |
| `sp_PersonalMedical_Create` | Creare personal medical nou | ✅ **Definit în script** |
| `sp_PersonalMedical_Update` | Actualizare personal medical | ✅ **Definit în script** |
| `sp_PersonalMedical_Delete` | Ștergere personal medical (soft delete) | ✅ **Definit în script** |
| `sp_PersonalMedical_GetDropdownOptions` | Opțiuni pentru dropdown-uri | ✅ **Definit în script** |

### 3. **Departamente Module**

| Stored Procedure | Scop | Status |
|-----------------|------|---------|
| `sp_Departamente_GetAll` | Toate departamentele | ⚠️ **Definit în script dar nu găsit în Repository** |
| `sp_Departamente_GetByTip` | Departamente după tip | ✅ **Definit în cod PersonalMedical** |

### 4. **Location Module (JudetRepository.cs & LocalitateRepository.cs)**

| Stored Procedure | Repository | Metoda | Status |
|-----------------|-----------|---------|---------|
| `sp_Judete_GetAll` | JudetRepository | `GetAllAsync()` | ⚠️ **Folosit în cod dar nu găsit script** |
| `sp_Judete_GetOrderedByName` | JudetRepository | `GetOrderedByNameAsync()` | ⚠️ **Folosit în cod dar nu găsit script** |
| `sp_Judete_GetById` | JudetRepository | `GetByIdAsync()` | ⚠️ **Folosit în cod dar nu găsit script** |
| `sp_Judete_GetByCod` | JudetRepository | `GetByCodAsync()` | ⚠️ **Folosit în cod dar nu găsit script** |
| `sp_Localitati_GetAll` | LocalitateRepository | `GetAllAsync()` | ⚠️ **Folosit în cod dar nu găsit script** |
| `sp_Localitati_GetById` | LocalitateRepository | `GetByIdAsync()` | ⚠️ **Folosit în cod dar nu găsit script** |
| `sp_Localitati_GetByJudetId` | LocalitateRepository | `GetByJudetIdAsync()` | ⚠️ **Folosit în cod dar nu găsit script** |
| `sp_Localitati_GetByJudetIdOrdered` | LocalitateRepository | `GetByJudetIdOrderedAsync()` | ⚠️ **Folosit în cod dar nu găsit script** |

## 🚨 Probleme Identificate

### 1. **SP-uri lipsă pentru Location Module**
Repository-urile `JudetRepository` și `LocalitateRepository` folosesc SP-uri care nu au fost găsite în scripturile fornizate:

```csharp
// Exemple din JudetRepository.cs
await _connection.QueryAsync<JudetDto>("sp_Judete_GetAll", commandType: CommandType.StoredProcedure);
await _connection.QueryAsync<JudetDto>("sp_Judete_GetOrderedByName", commandType: CommandType.StoredProcedure);
```

### 2. **SP-uri create dar nefolosite**
Scriptul `sp_Departamente_Test.sql` testează `sp_Departamente_GetAll`, dar nu am găsit un repository dedicat pentru Departamente.

### 3. **Inconsistență în Personal vs PersonalMedical**
Există două module similare (`Personal` și `PersonalMedical`) cu SP-uri separate, ceea ce poate crea confuzie.

## ✅ Recomandări pentru Rezolvare

### 1. **Urgent: Creați SP-urile lipsă pentru Location Module**

Creați următoarele scripturi în `DevSupport/Scripts/`:

```sql
-- sp_Judete_GetAll.sql
CREATE PROCEDURE [dbo].[sp_Judete_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdJudet, JudetGuid, CodJudet, Nume, Siruta, CodAuto, Ordine
    FROM Judete
    ORDER BY Nume;
END;

-- sp_Judete_GetOrderedByName.sql  
CREATE PROCEDURE [dbo].[sp_Judete_GetOrderedByName]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdJudet, JudetGuid, CodJudet, Nume, Siruta, CodAuto, Ordine
    FROM Judete
    ORDER BY Nume;
END;

-- sp_Judete_GetById.sql
CREATE PROCEDURE [dbo].[sp_Judete_GetById]
    @IdJudet INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdJudet, JudetGuid, CodJudet, Nume, Siruta, CodAuto, Ordine
    FROM Judete
    WHERE IdJudet = @IdJudet;
END;

-- sp_Judete_GetByCod.sql
CREATE PROCEDURE [dbo].[sp_Judete_GetByCod]
    @CodJudet VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdJudet, JudetGuid, CodJudet, Nume, Siruta, CodAuto, Ordine
    FROM Judete
    WHERE CodJudet = @CodJudet;
END;
```

Similar pentru Localități.

### 2. **Implementați Repository pentru Departamente**

```csharp
// ValyanClinic.Infrastructure/Repositories/DepartamenteRepository.cs
public class DepartamenteRepository : IDepartamenteRepository
{
    // Implementare folosind sp_Departamente_GetAll, sp_Departamente_GetByTip, etc.
}
```

### 3. **Verificați în baza de date**

Rulați scriptul creat `VerifyStoredProcedureNames.sql` pentru a vedea exact care SP-uri există în baza de date:

```sql
EXEC VerifyStoredProcedureNames.sql
```

### 4. **Testați toate SP-urile**

După ce creați SP-urile lipsă, rulați testele:
- `sp_Departamente_Test.sql`
- `Test-PersonalMedicalStoredProcedures.ps1`
- Creați scripturi de test pentru Location Module

## 🎯 Plan de Acțiune Prioritizat

### **Prioritate 1 - CRITICĂ** 
- [ ] Creați SP-urile pentru Location Module (Județe și Localități)
- [ ] Testați că aplicația pornește fără erori

### **Prioritate 2 - IMPORTANTĂ**
- [ ] Implementați DepartamenteRepository
- [ ] Verificați toate SP-urile în baza de date folosind scriptul de verificare

### **Prioritate 3 - OPTIMIZARE**
- [ ] Documentați diferențele dintre Personal și PersonalMedical
- [ ] Considerați consolidarea celor două module dacă sunt redundante

## 🔧 Script de Verificare

Pentru a automatiza verificarea, folosiți:

```bash
# Rulați scriptul SQL de verificare
sqlcmd -S "TS1828\ERP" -d "ValyanMed" -E -i "DevSupport/Scripts/VerifyStoredProcedureNames.sql"
```

## 📝 Concluzie

**Status general**: ⚠️ **Acțiune necesară**

Aplicația are o arhitectură solidă cu SP-uri bine definite pentru modulele Personal și PersonalMedical, dar **lipsesc SP-urile critice pentru Location Module** (Județe și Localități). Aceasta poate cauza erori la runtime când se încearcă încărcarea dropdown-urilor pentru județe și localități.

**Estimare timp rezolvare**: 2-3 ore pentru crearea și testarea SP-urilor lipsă.

---

**Verificat de**: GitHub Copilot  
**Data verificării**: {DATE}  
**Fișiere analizate**: 15+ fișiere C# și SQL  
**Status**: Necesită acțiune pentru SP-uri Location Module
