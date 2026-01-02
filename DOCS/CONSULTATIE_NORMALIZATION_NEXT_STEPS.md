# Consultatie Normalization - Next Steps (2026-01-02)

## 🎯 Progres Actual: 55% Complet

### ✅ CE AM REALIZAT AZI

#### 1. Infrastructure Layer - 100% COMPLET
- **8 metode upsert implementate** în `ConsultatieRepository.cs`:
  - `UpsertMotivePrezentareAsync()` - Dapper + SP (3 params)
  - `UpsertAntecedenteAsync()` - Dapper + SP (23 params)
  - `UpsertExamenObiectivAsync()` - Dapper + SP (30 params)
  - `UpsertInvestigatiiAsync()` - Dapper + SP (4 params)
  - `CreateAnalizaMedicalaAsync()` - Dapper + SP cu OUTPUT param
  - `UpsertDiagnosticAsync()` - Dapper + SP (6 params)
  - `UpsertTratamentAsync()` - Dapper + SP (8 params)
  - `UpsertConcluziiAsync()` - Dapper + SP (5 params)

- **Build Status**: ✅ Infrastructure compilează cu 0 erori

#### 2. Stored Procedure Lipsă - CREAT
- `ConsultatieInvestigatii_Upsert.sql` - SP nou creat pentru secțiunea Investigatii
- **Locație**: `DevSupport/01_Database/02_StoredProcedures/Consultatie/`

#### 3. Application Layer - 20% COMPLET
- ✅ **CreateConsulatieCommandHandler** - REFACTORIZAT complet
  - Pattern folosit: Master record + section-based upserts
  - 0 erori de compilare
  - Helper methods pentru validare (HasAnyAntecedente, HasAnyExamenObiectiv, etc.)

---

## 📋 CE TREBUIE FĂCUT URMĂTORUL

### Priority 1: Finalizare Application Layer (~200 erori)

#### Handler 1: SaveConsultatieDraftCommandHandler (~40 erori)
**Locație**: `ValyanClinic.Application/Features/ConsultatieManagement/Commands/SaveConsultatieDraft/`

**Strategy**:
```csharp
// Similar cu CreateConsulatieCommandHandler
var consultatieId = await _repository.SaveDraftAsync(masterRecord);

// Upsert doar secțiunile modificate
if (HasChanges(request.MotivePrezentare))
    await _repository.UpsertMotivePrezentareAsync(...);

if (HasChanges(request.Antecedente))
    await _repository.UpsertAntecedenteAsync(...);

// ... etc pentru fiecare secțiune
```

**Note**:
- SaveDraftAsync salvează doar master record
- Fiecare secțiune se salvează independent
- Verifică null-safety pe navigation properties

---

#### Handler 2: GetConsulatieByIdQueryHandler (~90 erori)
**Locație**: `ValyanClinic.Application/Features/ConsultatieManagement/Queries/GetConsulatieById/`

**Strategy**:
```csharp
// Repository returnează entitate cu navigation properties populate
var consultatie = await _repository.GetByIdAsync(consultatieId);
if (consultatie == null)
    return Result<ConsultatieDetailDto>.Failure("...");

// Map to DTO flatten (pentru backward compatibility cu UI)
var dto = new ConsultatieDetailDto
{
    // Master fields
    ConsultatieID = consultatie.ConsultatieID,
    PacientID = consultatie.PacientID,
    MedicID = consultatie.MedicID,
    DataConsultatie = consultatie.DataConsultatie,
    
    // Navigation properties - NULL SAFE!
    MotivPrezentare = consultatie.MotivePrezentare?.MotivPrezentare,
    IstoricBoalaActuala = consultatie.MotivePrezentare?.IstoricBoalaActuala,
    
    AHC_Mama = consultatie.Antecedente?.AHC_Mama,
    AHC_Tata = consultatie.Antecedente?.AHC_Tata,
    // ... toate câmpurile Antecedente (23 fields)
    
    StareGenerala = consultatie.ExamenObiectiv?.StareGenerala,
    Greutate = consultatie.ExamenObiectiv?.Greutate,
    // ... toate câmpurile ExamenObiectiv (30 fields)
    
    InvestigatiiLaborator = consultatie.Investigatii?.InvestigatiiLaborator,
    // ... Investigatii (4 fields)
    
    DiagnosticPozitiv = consultatie.Diagnostic?.DiagnosticPozitiv,
    CoduriICD10 = consultatie.Diagnostic?.CoduriICD10,
    // ... Diagnostic (6 fields)
    
    TratamentMedicamentos = consultatie.Tratament?.TratamentMedicamentos,
    // ... Tratament (8 fields)
    
    Prognostic = consultatie.Concluzii?.Prognostic,
    Concluzie = consultatie.Concluzii?.Concluzie,
    // ... Concluzii (5 fields)
};

return Result<ConsultatieDetailDto>.Success(dto);
```

**Critical**: Folosește null-conditional operator `?.` pentru toate navigation properties!

---

#### Handler 3: GetDraftConsulatieByPacientQueryHandler (~60 erori)
**Locație**: `ValyanClinic.Application/Features/ConsultatieManagement/Queries/GetDraftConsulatieByPacient/`

**Strategy**: Similar cu GetConsulatieByIdQueryHandler
- Repository: `GetDraftByPacientAsync()` returnează master + navigation props
- Mapping: Flatten navigation properties → DTO
- Null-safety: Verifică toate navigation props cu `?.`

---

#### Handler 4: GetConsulatieByProgramareQueryHandler (~90 erori)
**Locație**: `ValyanClinic.Application/Features/ConsultatieManagement/Queries/GetConsulatieByProgramare/`

**Strategy**: Similar cu GetConsulatieByIdQueryHandler
- Repository: `GetByProgramareIdAsync()` - **ATENȚIE**: Acum returnează doar master record!
- **TREBUIE MODIFICAT** repository method pentru a popula navigation properties!

**Fix necesar în ConsultatieRepository**:
```csharp
public async Task<Consultatie?> GetByProgramareIdAsync(Guid programareId, ...)
{
    // Găsește master record
    var consultatie = await connection.QueryFirstOrDefaultAsync<Consultatie>(...);
    
    if (consultatie == null)
        return null;
    
    // APOI: Load navigation properties (call GetByIdAsync)
    return await GetByIdAsync(consultatie.ConsultatieID, cancellationToken);
}
```

---

#### Handler 5 & 6: Probabil OK (verifică cu compilare)
- `GetConsultatiiByPacientQueryHandler` - Folosește doar master records (fără nav props)
- `GetConsultatiiByMedicQueryHandler` - Folosește doar master records (fără nav props)

Dacă au erori, probabil sunt minore (null-safety sau property renames).

---

### Priority 2: Deploy Database (După Application Layer)

#### 1. Backup Database
```sql
BACKUP DATABASE ValyanClinicDB 
TO DISK = 'D:\Backups\ValyanClinicDB_Before_Normalization_2026-01-02.bak'
WITH FORMAT, INIT, NAME = 'Pre-Normalization Backup';
```

#### 2. Run Migration Scripts (în ordine!)
```powershell
# 1. DROP old structure
sqlcmd -S localhost -d ValyanClinicDB -E -i "DevSupport/01_Database/06_Migrations/001_Consultatie_Normalization_DropOldStructure.sql"

# 2. CREATE new structure
sqlcmd -S localhost -d ValyanClinicDB -E -i "DevSupport/01_Database/06_Migrations/002_Consultatie_Normalization_CreateNewStructure.sql"
```

#### 3. Deploy Stored Procedures (13 SP-uri)
```powershell
$spFiles = @(
    "Consultatie_Create.sql",
    "Consultatie_GetById.sql",
    "Consultatie_GetByPacient.sql",
    "Consultatie_GetByMedic.sql",
    "Consultatie_GetByProgramare.sql",
    "Consultatie_GetDraftByPacient.sql",
    "Consultatie_SaveDraft.sql",
    "Consultatie_Finalize.sql",
    "Consultatie_Delete.sql",
    "ConsultatieMotivePrezentare_Upsert.sql",
    "ConsultatieAntecedente_Upsert.sql",
    "ConsultatieExamenObiectiv_Upsert.sql",
    "ConsultatieInvestigatii_Upsert.sql",  # NOU CREAT
    "ConsultatieAnalizaMedicala_Create.sql",
    "ConsultatieDiagnostic_Upsert.sql",
    "ConsultatieTratament_Upsert.sql",
    "ConsultatieConcluzii_Upsert.sql"
)

foreach ($file in $spFiles) {
    Write-Host "Deploying $file..." -ForegroundColor Cyan
    sqlcmd -S localhost -d ValyanClinicDB -E -i "DevSupport/01_Database/02_StoredProcedures/Consultatie/$file"
}
```

#### 4. Verify Deployment
```sql
-- Check tables
SELECT name FROM sys.tables WHERE name LIKE 'Consultatie%' ORDER BY name;

-- Check stored procedures
SELECT name, create_date FROM sys.procedures 
WHERE name LIKE 'Consultatie%' 
ORDER BY name;
```

---

### Priority 3: Update UI Components

#### Components Affected
1. `ConsultatieModal.razor` + `.razor.cs`
2. `AdministrareConsultatii.razor` + `.razor.cs`
3. Tab components (dacă există separate)

**Good News**: UI already uses tabs matching normalized structure! 
- Tab "Motiv Prezentare"
- Tab "Antecedente"
- Tab "Examen Obiectiv"
- Tab "Investigații"
- Tab "Diagnostic"
- Tab "Tratament"
- Tab "Concluzii"

**Strategy**:
- Păstrează model binding flatten în UI (DTO-uri flatten)
- Handlers se ocupă de mapping între flatten DTO ↔ normalized entities
- UI rămâne relativ neschimbat (backward compatibility)

---

## 🚨 BLOCAJE POTENȚIALE

### 1. GetByProgramareIdAsync Incomplete
**Problem**: Repository method returnează doar master record, fără navigation properties.

**Solution**: 
```csharp
// În ConsultatieRepository.cs
public async Task<Consultatie?> GetByProgramareIdAsync(Guid programareId, CancellationToken cancellationToken = default)
{
    var consultatie = await connection.QueryFirstOrDefaultAsync<Consultatie>(...);
    if (consultatie == null)
        return null;
    
    // Delegate to GetByIdAsync to populate navigation props
    return await GetByIdAsync(consultatie.ConsultatieID, cancellationToken);
}
```

### 2. Null Reference Exceptions
**Problem**: Navigation properties pot fi null dacă secțiunea nu a fost salvată.

**Solution**: Folosește null-conditional operator `?.` PESTE TOT în mapping:
```csharp
MotivPrezentare = consultatie.MotivePrezentare?.MotivPrezentare,
AHC_Mama = consultatie.Antecedente?.AHC_Mama,
```

### 3. Missing Fields in Commands
**Problem**: Command-ul `CreateConsultatieCommand` nu are câmpuri `Edeme` și `DocumenteAtatate`.

**Solution**: 
- Dacă UI nu folosește aceste câmpuri → Ignore
- Dacă UI folosește → Adaugă în Command și în Handler

---

## 📊 METRICI PROGRES

| Layer | Status | Erori Rămase | % Complet |
|-------|--------|--------------|-----------|
| Domain | ✅ Done | 0 | 100% |
| Database Scripts | ✅ Done | 0 | 100% |
| DTOs | ✅ Done | 0 | 100% |
| Infrastructure | ✅ Done | 0 | 100% |
| Application | 🟡 In Progress | ~200 | 20% |
| UI | ❌ Not Started | ? | 0% |
| Database Deploy | ❌ Not Started | N/A | 0% |

**Overall Progress**: ~55% Complete

---

## 🎯 ESTIMARE TIMP

| Task | Estimated Time |
|------|----------------|
| Finalizare Application Handlers | 2-3 ore |
| Deploy Database + SP-uri | 30 min |
| Update UI Components | 1-2 ore |
| Testing & Debugging | 1-2 ore |
| **TOTAL** | **5-8 ore** |

---

## 📝 QUICK REFERENCE

### Pattern: Command Handler (Create/Save)
```csharp
// 1. Create/Update master
var id = await _repository.CreateAsync(masterRecord);

// 2. Upsert sections (only if data exists)
if (HasData(request.Section))
    await _repository.UpsertSectionAsync(entity);
```

### Pattern: Query Handler (GetById/GetDraft)
```csharp
// 1. Get with navigation props
var consultatie = await _repository.GetByIdAsync(id);

// 2. Map to flatten DTO (NULL-SAFE!)
var dto = new DTO {
    Field = consultatie.Section?.Field
};
```

### Pattern: Repository Method Enhancement
```csharp
// If method returns master only, enhance to load nav props
public async Task<Consultatie?> Method(...)
{
    var master = await GetMasterRecord(...);
    if (master == null) return null;
    
    return await GetByIdAsync(master.ConsultatieID); // Full load
}
```

---

**Next Session**: Start with `SaveConsultatieDraftCommandHandler` (easiest, ~40 erori)
