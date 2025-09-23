# Verificare Stored Procedures - Baza de Date vs Cod

## 📋 Prezentare Generala

Acest document verifica concordanta dintre stored procedure-urile definite in baza de date si cele utilizate in codul aplicatiei ValyanClinic. Obiectivul este sa ne asiguram ca nu exista discrepante de denumire care ar putea cauza erori la runtime.

## 🔍 Metodologie de Verificare

Am analizat urmatoarele surse:
1. **Codul C# din Repository-uri** - pentru a identifica SP-urile apelate
2. **Scripturile SQL** - pentru a vedea SP-urile create
3. **Fisierele de documentatie** - pentru a intelege arhitectura

## 📊 Rezultatele Verificarii

### 1. **Personal Module (PersonalRepository.cs)**

| Stored Procedure | Utilizat in Metoda | Status |
|-----------------|-------------------|---------|
| `sp_Personal_GetAll` | `GetAllAsync()` | ✅ **Definit in cod** |
| `sp_Personal_GetById` | `GetByIdAsync()` | ✅ **Definit in cod** |
| `sp_Personal_Create` | `CreateAsync()` | ✅ **Definit in cod** |
| `sp_Personal_Update` | `UpdateAsync()` | ✅ **Definit in cod** |
| `sp_Personal_Delete` | `DeleteAsync()` | ✅ **Definit in cod** |
| `sp_Personal_CheckUnique` | `CheckUniqueAsync()` | ✅ **Definit in cod** |
| `sp_Personal_GetStatistics` | `GetStatisticsAsync()` | ✅ **Definit in cod** |

### 2. **PersonalMedical Module**

| Stored Procedure | Scop | Status |
|-----------------|------|---------|
| `sp_PersonalMedical_GetAll` | Lista personal medical cu filtrare si paginare | ✅ **Definit in script** |
| `sp_PersonalMedical_GetStatistics` | Statistici personal medical | ✅ **Definit in script** |
| `sp_PersonalMedical_GetById` | Personal medical dupa ID | ✅ **Definit in script** |
| `sp_PersonalMedical_CheckUnique` | Verificare unicitate Email si NumarLicenta | ✅ **Definit in script** |
| `sp_PersonalMedical_Create` | Creare personal medical nou | ✅ **Definit in script** |
| `sp_PersonalMedical_Update` | Actualizare personal medical | ✅ **Definit in script** |
| `sp_PersonalMedical_Delete` | stergere personal medical (soft delete) | ✅ **Definit in script** |
| `sp_PersonalMedical_GetDropdownOptions` | Optiuni pentru dropdown-uri | ✅ **Definit in script** |

### 3. **Departamente Module**

| Stored Procedure | Scop | Status |
|-----------------|------|---------|
| `sp_Departamente_GetAll` | Toate departamentele | ⚠️ **Definit in script dar nu gasit in Repository** |
| `sp_Departamente_GetByTip` | Departamente dupa tip | ✅ **Definit in cod PersonalMedical** |

### 4. **Location Module (JudetRepository.cs & LocalitateRepository.cs)**

| Stored Procedure | Repository | Metoda | Status |
|-----------------|-----------|---------|---------|
| `sp_Judete_GetAll` | JudetRepository | `GetAllAsync()` | ⚠️ **Folosit in cod dar nu gasit script** |
| `sp_Judete_GetOrderedByName` | JudetRepository | `GetOrderedByNameAsync()` | ⚠️ **Folosit in cod dar nu gasit script** |
| `sp_Judete_GetById` | JudetRepository | `GetByIdAsync()` | ⚠️ **Folosit in cod dar nu gasit script** |
| `sp_Judete_GetByCod` | JudetRepository | `GetByCodAsync()` | ⚠️ **Folosit in cod dar nu gasit script** |
| `sp_Localitati_GetAll` | LocalitateRepository | `GetAllAsync()` | ⚠️ **Folosit in cod dar nu gasit script** |
| `sp_Localitati_GetById` | LocalitateRepository | `GetByIdAsync()` | ⚠️ **Folosit in cod dar nu gasit script** |
| `sp_Localitati_GetByJudetId` | LocalitateRepository | `GetByJudetIdAsync()` | ⚠️ **Folosit in cod dar nu gasit script** |
| `sp_Localitati_GetByJudetIdOrdered` | LocalitateRepository | `GetByJudetIdOrderedAsync()` | ⚠️ **Folosit in cod dar nu gasit script** |

## 🚨 Probleme Identificate

### 1. **SP-uri lipsa pentru Location Module**
Repository-urile `JudetRepository` si `LocalitateRepository` folosesc SP-uri care nu au fost gasite in scripturile fornizate:

```csharp
// Exemple din JudetRepository.cs
await _connection.QueryAsync<JudetDto>("sp_Judete_GetAll", commandType: CommandType.StoredProcedure);
await _connection.QueryAsync<JudetDto>("sp_Judete_GetOrderedByName", commandType: CommandType.StoredProcedure);
```

### 2. **SP-uri create dar nefolosite**
Scriptul `sp_Departamente_Test.sql` testeaza `sp_Departamente_GetAll`, dar nu am gasit un repository dedicat pentru Departamente.

### 3. **Inconsistenta in Personal vs PersonalMedical**
Exista doua module similare (`Personal` si `PersonalMedical`) cu SP-uri separate, ceea ce poate crea confuzie.

## ✅ Recomandari pentru Rezolvare

### 1. **Urgent: Creati SP-urile lipsa pentru Location Module**

Creati urmatoarele scripturi in `DevSupport/Scripts/`:

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

Similar pentru Localitati.

### 2. **Implementati Repository pentru Departamente**

```csharp
// ValyanClinic.Infrastructure/Repositories/DepartamenteRepository.cs
public class DepartamenteRepository : IDepartamenteRepository
{
    // Implementare folosind sp_Departamente_GetAll, sp_Departamente_GetByTip, etc.
}
```

### 3. **Verificati in baza de date**

Rulati scriptul creat `VerifyStoredProcedureNames.sql` pentru a vedea exact care SP-uri exista in baza de date:

```sql
EXEC VerifyStoredProcedureNames.sql
```

### 4. **Testati toate SP-urile**

Dupa ce creati SP-urile lipsa, rulati testele:
- `sp_Departamente_Test.sql`
- `Test-PersonalMedicalStoredProcedures.ps1`
- Creati scripturi de test pentru Location Module

## 🎯 Plan de Actiune Prioritizat

### **Prioritate 1 - CRITICa** 
- [ ] Creati SP-urile pentru Location Module (Judete si Localitati)
- [ ] Testati ca aplicatia porneste fara erori

### **Prioritate 2 - IMPORTANTa**
- [ ] Implementati DepartamenteRepository
- [ ] Verificati toate SP-urile in baza de date folosind scriptul de verificare

### **Prioritate 3 - OPTIMIZARE**
- [ ] Documentati diferentele dintre Personal si PersonalMedical
- [ ] Considerati consolidarea celor doua module daca sunt redundante

## 🔧 Script de Verificare

Pentru a automatiza verificarea, folositi:

```bash
# Rulati scriptul SQL de verificare
sqlcmd -S "TS1828\ERP" -d "ValyanMed" -E -i "DevSupport/Scripts/VerifyStoredProcedureNames.sql"
```

## 📝 Concluzie

**Status general**: ⚠️ **Actiune necesara**

Aplicatia are o arhitectura solida cu SP-uri bine definite pentru modulele Personal si PersonalMedical, dar **lipsesc SP-urile critice pentru Location Module** (Judete si Localitati). Aceasta poate cauza erori la runtime cand se incearca incarcarea dropdown-urilor pentru judete si localitati.

**Estimare timp rezolvare**: 2-3 ore pentru crearea si testarea SP-urilor lipsa.

---

**Verificat de**: GitHub Copilot  
**Data verificarii**: {DATE}  
**Fisiere analizate**: 15+ fisiere C# si SQL  
**Status**: Necesita actiune pentru SP-uri Location Module
