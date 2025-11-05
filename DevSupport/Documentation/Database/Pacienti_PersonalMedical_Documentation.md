# Pacienti_PersonalMedical - Junction Table Documentation

**Data:** 2025-01-23  
**Database:** ValyanMed  
**Tip:** Many-to-Many Relationship Table  
**Status:** ✅ **READY FOR DEPLOYMENT**

---

## 📋 OVERVIEW

### Ce este?
`Pacienti_PersonalMedical` este o **tabelă de legătură (junction table)** care implementează o relație **Many-to-Many** între:
- **Pacienti** (pacienți)
- **PersonalMedical** (personal medical - doctori, asistenți, etc.)

### De ce avem nevoie?
Un pacient poate avea **mai mulți doctori** (cardiolog, pneumolog, medic de familie, etc.) și un doctor poate avea **mai mulți pacienți**. Tabela clasică nu permite acest lucru fără o tabelă de legătură.

---

## 🏗️ STRUCTURA TABELEI

### Coloane: 13

| Coloană | Tip | Descriere | Constraints |
|---------|-----|-----------|-------------|
| **Id** | UNIQUEIDENTIFIER | PK - identificator unic relație | PRIMARY KEY, NOT NULL, DEFAULT NEWSEQUENTIALID() |
| **PacientID** | UNIQUEIDENTIFIER | FK către Pacienti.Id | NOT NULL, FK |
| **PersonalMedicalID** | UNIQUEIDENTIFIER | FK către PersonalMedical.PersonalID | NOT NULL, FK |
| **TipRelatie** | NVARCHAR(50) | Tip relație (vezi mai jos) | CHECK constraint |
| **DataAsocierii** | DATETIME2(7) | Data când relația a început | NOT NULL, DEFAULT GETDATE() |
| **DataDezactivarii** | DATETIME2(7) | Data când relația s-a terminat | NULL |
| **EsteActiv** | BIT | Relația este activă? | NOT NULL, DEFAULT 1 |
| **Observatii** | NVARCHAR(MAX) | Observații despre relație | NULL |
| **Motiv** | NVARCHAR(500) | Motivul asocierii | NULL |
| **Data_Crearii** | DATETIME2(7) | Audit - când a fost creată | NOT NULL, DEFAULT GETDATE() |
| **Data_Ultimei_Modificari** | DATETIME2(7) | Audit - ultima modificare | NOT NULL, DEFAULT GETDATE() |
| **Creat_De** | NVARCHAR(100) | Audit - cine a creat | DEFAULT SYSTEM_USER |
| **Modificat_De** | NVARCHAR(100) | Audit - cine a modificat | DEFAULT SYSTEM_USER |

### TipRelatie - Valori permise:
- `MedicPrimar` - medicul principal al pacientului
- `MedicConsultant` - medic consultat ocazional
- `Specialist` - specialist într-un domeniu
- `MedicDeGarda` - medic de gardă alocat
- `MedicFamilie` - medic de familie
- `AsistentMedical` - asistent medical alocat
- `NULL` - tip nespecificat

---

## 🔗 RELATII (FOREIGN KEYS)

### FK_PacientiPersonalMedical_Pacient
- **Coloană:** PacientID
- **Referință:** Pacienti(Id)
- **On Delete:** CASCADE (ștergerea pacientului șterge și relațiile)

### FK_PacientiPersonalMedical_PersonalMedical
- **Coloană:** PersonalMedicalID
- **Referință:** PersonalMedical(PersonalID)
- **On Delete:** CASCADE (ștergerea doctorului șterge și relațiile)

---

## 🔒 CONSTRAINTS

### Primary Key
- `PK_Pacienti_PersonalMedical` pe coloana `Id`

### Unique Constraint
- `UQ_Pacient_PersonalMedical` pe `(PacientID, PersonalMedicalID)`
- **Efect:** Previne duplicate - același pacient NU poate fi asociat de 2 ori cu același doctor

### Check Constraint
- `CK_TipRelatie` - validează că TipRelatie are una din valorile permise

---

## 📊 INDEXES (pentru performanță)

### 1. IX_Pacienti_PersonalMedical_PacientID
- **Tip:** NONCLUSTERED
- **Coloane:** PacientID ASC, EsteActiv ASC
- **INCLUDE:** PersonalMedicalID, TipRelatie, DataAsocierii
- **Scop:** Căutare rapidă doctori după pacient

### 2. IX_Pacienti_PersonalMedical_PersonalMedicalID
- **Tip:** NONCLUSTERED
- **Coloane:** PersonalMedicalID ASC, EsteActiv ASC
- **INCLUDE:** PacientID, TipRelatie, DataAsocierii
- **Scop:** Căutare rapidă pacienți după doctor

### 3. IX_Pacienti_PersonalMedical_EsteActiv
- **Tip:** NONCLUSTERED
- **Coloană:** EsteActiv ASC
- **Filtered:** WHERE EsteActiv = 1
- **Scop:** Query-uri rapide doar pe relațiile active

### 4. IX_Pacienti_PersonalMedical_TipRelatie
- **Tip:** NONCLUSTERED
- **Coloane:** TipRelatie ASC, EsteActiv ASC
- **Scop:** Filtrare după tipul relației

### 5. IX_Pacienti_PersonalMedical_DataAsocierii
- **Tip:** NONCLUSTERED
- **Coloană:** DataAsocierii DESC
- **Scop:** Istoric cronologic (cele mai noi relații primele)

---

## ⚡ TRIGGER

### TR_Pacienti_PersonalMedical_UpdateTimestamp
- **Tip:** AFTER UPDATE
- **Scop:** Actualizează automat `Data_Ultimei_Modificari` și `Modificat_De` la fiecare UPDATE

---

## 📝 STORED PROCEDURES (8 proceduri)

### 1. sp_PacientiPersonalMedical_GetDoctoriByPacient
**Parametri:**
- `@PacientID` UNIQUEIDENTIFIER (required)
- `@ApenumereActivi` BIT (optional, default = 1)

**Returnează:** Toți doctorii unui pacient + detalii complete

**Exemplu:**
```sql
EXEC sp_PacientiPersonalMedical_GetDoctoriByPacient 
    @PacientID = 'GUID-PACIENT',
 @ApenumereActivi = 1
```

---

### 2. sp_PacientiPersonalMedical_GetPacientiByDoctor
**Parametri:**
- `@PersonalMedicalID` UNIQUEIDENTIFIER (required)
- `@ApenumereActivi` BIT (optional, default = 1)
- `@TipRelatie` NVARCHAR(50) (optional - filtru)

**Returnează:** Toți pacienții unui doctor + detalii complete

**Exemplu:**
```sql
EXEC sp_PacientiPersonalMedical_GetPacientiByDoctor 
    @PersonalMedicalID = 'GUID-DOCTOR',
 @ApenumereActivi = 1,
    @TipRelatie = 'MedicPrimar'
```

---

### 3. sp_PacientiPersonalMedical_AddRelatie
**Parametri:**
- `@PacientID` UNIQUEIDENTIFIER (required)
- `@PersonalMedicalID` UNIQUEIDENTIFIER (required)
- `@TipRelatie` NVARCHAR(50) (optional)
- `@Observatii` NVARCHAR(MAX) (optional)
- `@Motiv` NVARCHAR(500) (optional)
- `@CreatDe` NVARCHAR(100) (optional)

**Returnează:** Relația nou creată cu detalii complete

**Validări:**
- Verifică că pacientul există
- Verifică că doctorul există
- Previne duplicate (dacă există relație activă)

**Exemplu:**
```sql
EXEC sp_PacientiPersonalMedical_AddRelatie 
    @PacientID = 'GUID-PACIENT',
    @PersonalMedicalID = 'GUID-DOCTOR',
    @TipRelatie = 'Specialist',
    @Observatii = 'Pacient cu probleme cardiace',
    @Motiv = 'Recomandare medic de familie',
    @CreatDe = 'Dr. Popescu'
```

---

### 4. sp_PacientiPersonalMedical_RemoveRelatie
**Parametri:**
- `@RelatieID` UNIQUEIDENTIFIER (optional)
- `@PacientID` UNIQUEIDENTIFIER (optional)
- `@PersonalMedicalID` UNIQUEIDENTIFIER (optional)
- `@ModificatDe` NVARCHAR(100) (optional)

**Notă:** Trebuie specificat **fie** RelatieID, **fie** ambele PacientID + PersonalMedicalID

**Acțiune:** **Soft delete** - setează `EsteActiv = 0` și `DataDezactivarii = GETDATE()`

**Exemplu:**
```sql
-- Varianta 1: Cu RelatieID
EXEC sp_PacientiPersonalMedical_RemoveRelatie 
    @RelatieID = 'GUID-RELATIE',
    @ModificatDe = 'Dr. Ionescu'

-- Varianta 2: Cu PacientID + PersonalMedicalID
EXEC sp_PacientiPersonalMedical_RemoveRelatie 
    @PacientID = 'GUID-PACIENT',
    @PersonalMedicalID = 'GUID-DOCTOR',
    @ModificatDe = 'Admin'
```

---

### 5. sp_PacientiPersonalMedical_ReactiveazaRelatie
**Parametri:**
- `@RelatieID` UNIQUEIDENTIFIER (required)
- `@ModificatDe` NVARCHAR(100) (optional)

**Acțiune:** Reactivează o relație inactivă (setează `EsteActiv = 1`, șterge `DataDezactivarii`)

**Exemplu:**
```sql
EXEC sp_PacientiPersonalMedical_ReactiveazaRelatie 
    @RelatieID = 'GUID-RELATIE',
    @ModificatDe = 'Dr. Ionescu'
```

---

### 6. sp_PacientiPersonalMedical_UpdateRelatie
**Parametri:**
- `@RelatieID` UNIQUEIDENTIFIER (required)
- `@TipRelatie` NVARCHAR(50) (optional)
- `@Observatii` NVARCHAR(MAX) (optional)
- `@Motiv` NVARCHAR(500) (optional)
- `@ModificatDe` NVARCHAR(100) (optional)

**Notă:** Doar câmpurile trimise (non-NULL) vor fi actualizate

**Returnează:** Relația actualizată cu detalii complete

**Exemplu:**
```sql
EXEC sp_PacientiPersonalMedical_UpdateRelatie 
    @RelatieID = 'GUID-RELATIE',
    @TipRelatie = 'MedicConsultant',
 @Observatii = 'Observatii actualizate',
    @ModificatDe = 'Admin'
```

---

### 7. sp_PacientiPersonalMedical_GetStatistici
**Parametri:** Niciunul

**Returnează:** 3 result sets:
1. **Statistici generale:**
   - TotalRelatii, RelatiiActive, RelatiiInactive
   - TotalDoctori, DoctoriActivi
   - TotalPacienti, PacientiActivi
   - MediuZileAsociere

2. **Top 5 doctori cu cei mai mulți pacienți**

3. **Distribuție pe tip relație**

**Exemplu:**
```sql
EXEC sp_PacientiPersonalMedical_GetStatistici
```

---

### 8. sp_PacientiPersonalMedical_GetRelatieById
**Parametri:**
- `@RelatieID` UNIQUEIDENTIFIER (required)

**Returnează:** Detalii complete relație + date pacient + date doctor

**Exemplu:**
```sql
EXEC sp_PacientiPersonalMedical_GetRelatieById 
    @RelatieID = 'GUID-RELATIE'
```

---

## 🚀 DEPLOYMENT

### Metoda 1: PowerShell Script (Recomandat)
```powershell
cd DevSupport\Scripts\PowerShellScripts
.\Deploy-PacientiPersonalMedical.ps1
```

**Opțiuni:**
```powershell
# Custom server
.\Deploy-PacientiPersonalMedical.ps1 -ServerName "LOCALHOST\SQLEXPRESS"

# Skip date de test
.\Deploy-PacientiPersonalMedical.ps1 -SkipTestData

# Force recreate (șterge tabela existentă)
.\Deploy-PacientiPersonalMedical.ps1 -ForceRecreate
```

### Metoda 2: Manual SQL Scripts
1. Rulează `DevSupport\Database\TableStructure\Pacienti_PersonalMedical_Complete.sql`
2. Rulează `DevSupport\Database\StoredProcedures\sp_Pacienti_PersonalMedical.sql`

---

## 🧪 TESTING

### Run Test Suite
```powershell
cd DevSupport\Scripts\PowerShellScripts
.\Test-PacientiPersonalMedical.ps1
```

**Teste incluse:**
1. Verificare existență tabelă
2. Verificare Foreign Keys
3. Verificare Stored Procedures
4. Test AddRelatie - Success Case
5. Test AddRelatie - Duplicate Prevention
6. Test GetDoctoriByPacient
7. Test GetPacientiByDoctor
8. Test GetStatistici
9. Test UpdateRelatie
10. Test RemoveRelatie (Soft Delete)

---

## 💻 INTEGRARE C#

### 1. Entity (ValyanClinic.Domain)
```csharp
public class PacientPersonalMedical
{
public Guid Id { get; set; }
    public Guid PacientID { get; set; }
    public Guid PersonalMedicalID { get; set; }
    public string? TipRelatie { get; set; }
    public DateTime DataAsocierii { get; set; }
    public DateTime? DataDezactivarii { get; set; }
    public bool EsteActiv { get; set; }
  public string? Observatii { get; set; }
    public string? Motiv { get; set; }
    public DateTime Data_Crearii { get; set; }
    public DateTime Data_Ultimei_Modificari { get; set; }
    public string? Creat_De { get; set; }
    public string? Modificat_De { get; set; }
    
    // Navigation properties
    public Pacient? Pacient { get; set; }
    public PersonalMedical? PersonalMedical { get; set; }
}
```

### 2. DTOs (ValyanClinic.Application)
```csharp
// DTO pentru listă doctori
public class DoctorAsociatDto
{
    public Guid RelatieID { get; set; }
    public Guid PersonalMedicalID { get; set; }
    public string DoctorNumeComplet { get; set; }
    public string? DoctorSpecializare { get; set; }
    public string? TipRelatie { get; set; }
    public DateTime DataAsocierii { get; set; }
    public int ZileDeAsociere { get; set; }
    public bool EsteActiv { get; set; }
}

// DTO pentru listă pacienți
public class PacientAsociatDto
{
    public Guid RelatieID { get; set; }
    public Guid PacientID { get; set; }
    public string PacientNumeComplet { get; set; }
    public string? PacientCNP { get; set; }
    public int PacientVarsta { get; set; }
    public string? TipRelatie { get; set; }
    public DateTime DataAsocierii { get; set; }
    public int ZileDeAsociere { get; set; }
    public bool EsteActiv { get; set; }
}
```

### 3. Queries (MediatR)
```csharp
// Query pentru doctori pacient
public record GetDoctoriByPacientQuery(Guid PacientID, bool ApenumereActivi = true) 
    : IRequest<Result<List<DoctorAsociatDto>>>;

// Query pentru pacienți doctor
public record GetPacientiByDoctorQuery(Guid PersonalMedicalID, bool ApenumereActivi = true, string? TipRelatie = null) 
 : IRequest<Result<List<PacientAsociatDto>>>;

// Command pentru adăugare relație
public record AddRelatieCommand(Guid PacientID, Guid PersonalMedicalID, string? TipRelatie, string? Observatii, string? Motiv) 
    : IRequest<Result<Guid>>;

// Command pentru ștergere relație
public record RemoveRelatieCommand(Guid RelatieID) 
    : IRequest<Result>;
```

---

## 📊 EXEMPLE DE UTILIZARE

### Exemplu 1: Asociere pacient cu medic cardiolog
```sql
-- Găsește un pacient
DECLARE @PacientID UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Pacienti WHERE Nume = 'Popescu' AND Prenume = 'Ion')

-- Găsește un cardiolog
DECLARE @DoctorID UNIQUEIDENTIFIER = (SELECT TOP 1 PersonalID FROM PersonalMedical WHERE Specializare = 'Cardiologie' AND EsteActiv = 1)

-- Asociază pacientul cu cardiologul
EXEC sp_PacientiPersonalMedical_AddRelatie 
    @PacientID = @PacientID,
    @PersonalMedicalID = @DoctorID,
    @TipRelatie = 'Specialist',
  @Motiv = 'Pacient cu hipertensiune',
  @CreatDe = 'Dr. Ionescu'
```

### Exemplu 2: Vezi toți doctorii unui pacient
```sql
DECLARE @PacientID UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Pacienti WHERE Cod_Pacient = 'PACIENT00000001')

EXEC sp_PacientiPersonalMedical_GetDoctoriByPacient 
    @PacientID = @PacientID,
    @ApenumereActivi = 1
```

### Exemplu 3: Vezi toți pacienții unui doctor
```sql
DECLARE @DoctorID UNIQUEIDENTIFIER = (SELECT TOP 1 PersonalID FROM PersonalMedical WHERE Nume = 'Ionescu' AND Prenume = 'Maria')

EXEC sp_PacientiPersonalMedical_GetPacientiByDoctor 
    @PersonalMedicalID = @DoctorID,
    @ApenumereActivi = 1,
    @TipRelatie = 'MedicPrimar'
```

### Exemplu 4: Statistici
```sql
EXEC sp_PacientiPersonalMedical_GetStatistici
```

---

## ✅ CHECKLIST DEPLOYMENT

- [ ] Backup database înainte de deployment
- [ ] Rulează script creare tabelă
- [ ] Rulează script stored procedures
- [ ] Rulează test suite (verificare)
- [ ] Adaugă date de test (optional)
- [ ] Creează entitate C# în Domain
- [ ] Creează DTOs în Application
- [ ] Creează Queries/Commands în Application
- [ ] Creează Repository în Infrastructure
- [ ] Actualizează UI în Blazor
- [ ] Testează funcționalitate end-to-end

---

## 🎯 BENEFICII

✅ **Flexibilitate:** Un pacient poate avea oricâți doctori  
✅ **Scalabilitate:** Se poate extinde ușor cu coloane noi  
✅ **Audit complet:** Istoric complet al relațiilor  
✅ **Performanță:** Indexuri optimizate pentru query-uri rapide  
✅ **Integritate:** Foreign Keys previne date orfane  
✅ **Soft delete:** Păstrează istoricul relațiilor dezactivate  

---

## 📞 SUPPORT

Pentru întrebări sau probleme:
1. Verifică acest document
2. Rulează test suite pentru diagnostic
3. Verifică logs în SQL Server

---

**Status:** ✅ **READY FOR PRODUCTION**  
**Created by:** GitHub Copilot  
**Date:** 2025-01-23  
**Version:** 1.0
