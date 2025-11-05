# Pacienti_PersonalMedical - Junction Table

## 🎯 Ce face?

Implementează o relație **Many-to-Many** între **Pacienti** și **PersonalMedical** (doctori).

- ✅ Un **pacient** poate avea **mai mulți doctori** (cardiolog, pneumolog, medic de familie)
- ✅ Un **doctor** poate avea **mai mulți pacienți**

---

## 🚀 Quick Start (1 minut)

```powershell
cd DevSupport\Scripts\PowerShellScripts
.\QuickStart-PacientiPersonalMedical.ps1
```

**Asta e tot!** Scriptul face automat:
- ✅ Creează tabela `Pacienti_PersonalMedical`
- ✅ Creează 8 stored procedures
- ✅ Adaugă indecși pentru performanță
- ✅ Rulează teste automate
- ✅ Adaugă date de test

---

## 📁 Fișiere create

```
DevSupport/
├── Database/
│   ├── TableStructure/
│   │   └── Pacienti_PersonalMedical_Complete.sql ......... Tabela
│   └── StoredProcedures/
│     └── sp_Pacienti_PersonalMedical.sql ................ 8 SP-uri
├── Scripts/
│   └── PowerShellScripts/
│       ├── QuickStart-PacientiPersonalMedical.ps1 ......... Quick Start
│       ├── Deploy-PacientiPersonalMedical.ps1 ............. Deployment
│       └── Test-PacientiPersonalMedical.ps1 ............... Tests
└── Documentation/
    └── Database/
        └── Pacienti_PersonalMedical_Documentation.md ...... Docs
```

---

## 🗂️ Structura tabelei

| Coloană | Tip | Descriere |
|---------|-----|-----------|
| **Id** | UNIQUEIDENTIFIER | PK - ID relație |
| **PacientID** | UNIQUEIDENTIFIER | FK către Pacienti |
| **PersonalMedicalID** | UNIQUEIDENTIFIER | FK către PersonalMedical |
| **TipRelatie** | NVARCHAR(50) | MedicPrimar, Specialist, etc. |
| **DataAsocierii** | DATETIME2 | Când a început relația |
| **DataDezactivarii** | DATETIME2 | Când s-a terminat (NULL = activă) |
| **EsteActiv** | BIT | Relația este activă? |
| **Observatii** | NVARCHAR(MAX) | Note despre relație |
| **Motiv** | NVARCHAR(500) | De ce a fost alocat doctorul |
| + audit fields | ... | Data_Crearii, Modificat_De, etc. |

---

## 📝 Stored Procedures (8)

### 1. **GetDoctoriByPacient** - Doctori pentru un pacient
```sql
EXEC sp_PacientiPersonalMedical_GetDoctoriByPacient 
    @PacientID = 'GUID-PACIENT'
```

### 2. **GetPacientiByDoctor** - Pacienți pentru un doctor
```sql
EXEC sp_PacientiPersonalMedical_GetPacientiByDoctor 
    @PersonalMedicalID = 'GUID-DOCTOR'
```

### 3. **AddRelatie** - Adaugă relație nouă
```sql
EXEC sp_PacientiPersonalMedical_AddRelatie 
@PacientID = 'GUID-PACIENT',
@PersonalMedicalID = 'GUID-DOCTOR',
    @TipRelatie = 'Specialist',
    @Motiv = 'Pacient cu probleme cardiace'
```

### 4. **RemoveRelatie** - Dezactivează relație (soft delete)
```sql
EXEC sp_PacientiPersonalMedical_RemoveRelatie 
    @RelatieID = 'GUID-RELATIE'
```

### 5. **ReactiveazaRelatie** - Reactivează relație
```sql
EXEC sp_PacientiPersonalMedical_ReactiveazaRelatie 
    @RelatieID = 'GUID-RELATIE'
```

### 6. **UpdateRelatie** - Actualizează detalii
```sql
EXEC sp_PacientiPersonalMedical_UpdateRelatie 
    @RelatieID = 'GUID-RELATIE',
    @TipRelatie = 'MedicConsultant'
```

### 7. **GetStatistici** - Statistici generale
```sql
EXEC sp_PacientiPersonalMedical_GetStatistici
```

### 8. **GetRelatieById** - Detalii complete pentru o relație
```sql
EXEC sp_PacientiPersonalMedical_GetRelatieById 
    @RelatieID = 'GUID-RELATIE'
```

---

## 💡 Exemple de utilizare

### Exemplu 1: Asociază pacient cu 2 doctori (cardiolog + pneumolog)

```sql
-- Pacient
DECLARE @PacientID UNIQUEIDENTIFIER = 
 (SELECT TOP 1 Id FROM Pacienti WHERE Nume = 'Popescu' AND Prenume = 'Ion')

-- Doctor 1: Cardiolog
DECLARE @Cardiolog UNIQUEIDENTIFIER = 
    (SELECT TOP 1 PersonalID FROM PersonalMedical 
     WHERE Specializare = 'Cardiologie' AND EsteActiv = 1)

-- Doctor 2: Pneumolog
DECLARE @Pneumolog UNIQUEIDENTIFIER = 
    (SELECT TOP 1 PersonalID FROM PersonalMedical 
     WHERE Specializare = 'Pneumologie' AND EsteActiv = 1)

-- Asociere 1
EXEC sp_PacientiPersonalMedical_AddRelatie 
    @PacientID = @PacientID,
    @PersonalMedicalID = @Cardiolog,
    @TipRelatie = 'Specialist',
    @Motiv = 'Hipertensiune'

-- Asociere 2
EXEC sp_PacientiPersonalMedical_AddRelatie 
    @PacientID = @PacientID,
    @PersonalMedicalID = @Pneumolog,
    @TipRelatie = 'Specialist',
    @Motiv = 'Astm bronsic'

-- Vezi toți doctorii pacientului
EXEC sp_PacientiPersonalMedical_GetDoctoriByPacient 
    @PacientID = @PacientID
```

### Exemplu 2: Vezi toți pacienții unui doctor

```sql
DECLARE @DoctorID UNIQUEIDENTIFIER = 
    (SELECT TOP 1 PersonalID FROM PersonalMedical 
     WHERE Nume = 'Ionescu' AND Prenume = 'Maria')

-- Toți pacienții
EXEC sp_PacientiPersonalMedical_GetPacientiByDoctor 
    @PersonalMedicalID = @DoctorID,
    @ApenumereActivi = 1

-- Doar pacienții la care este medic primar
EXEC sp_PacientiPersonalMedical_GetPacientiByDoctor 
    @PersonalMedicalID = @DoctorID,
    @ApenumereActivi = 1,
    @TipRelatie = 'MedicPrimar'
```

---

## 🧪 Testare

```powershell
cd DevSupport\Scripts\PowerShellScripts
.\Test-PacientiPersonalMedical.ps1
```

**10 teste automate:**
- ✅ Verificare existență tabelă
- ✅ Verificare Foreign Keys
- ✅ Verificare Stored Procedures
- ✅ Test AddRelatie (success + duplicate prevention)
- ✅ Test GetDoctoriByPacient
- ✅ Test GetPacientiByDoctor
- ✅ Test GetStatistici
- ✅ Test UpdateRelatie
- ✅ Test RemoveRelatie (soft delete)

---

## 📊 Integrare C#

### Entity (Domain)
```csharp
public class PacientPersonalMedical
{
    public Guid Id { get; set; }
    public Guid PacientID { get; set; }
    public Guid PersonalMedicalID { get; set; }
    public string? TipRelatie { get; set; }
    public DateTime DataAsocierii { get; set; }
    public bool EsteActiv { get; set; }
    // ... alte proprietăți
}
```

### DTOs (Application)
```csharp
public class DoctorAsociatDto
{
    public Guid RelatieID { get; set; }
    public string DoctorNumeComplet { get; set; }
    public string? DoctorSpecializare { get; set; }
    public string? TipRelatie { get; set; }
 public DateTime DataAsocierii { get; set; }
    public bool EsteActiv { get; set; }
}
```

### Query (MediatR)
```csharp
public record GetDoctoriByPacientQuery(Guid PacientID) 
    : IRequest<Result<List<DoctorAsociatDto>>>;
```

---

## ✅ Checklist

- [x] Script SQL pentru creare tabelă
- [x] Script SQL pentru stored procedures
- [x] Script PowerShell deployment
- [x] Script PowerShell testare
- [x] Documentație completă
- [ ] **Entity C# în ValyanClinic.Domain**
- [ ] **DTOs în ValyanClinic.Application**
- [ ] **Queries/Commands (MediatR)**
- [ ] **Repository în Infrastructure**
- [ ] **UI în Blazor components**

---

## 📖 Documentație completă

Vezi documentația completă pentru detalii tehnice:
```
DevSupport\Documentation\Database\Pacienti_PersonalMedical_Documentation.md
```

---

## 🎯 Ce urmează?

1. ✅ **Deploy database** (DONE - rulează QuickStart)
2. ⏳ **Creează entități C#** (TODO)
3. ⏳ **Creează queries** (TODO)
4. ⏳ **Actualizează UI** (TODO)

---

## 📞 Support

Pentru probleme:
1. Verifică documentația completă
2. Rulează test suite pentru diagnostic
3. Verifică logs în SQL Server

---

**Status:** ✅ **READY FOR USE**  
**Created:** 2025-01-23  
**Version:** 1.0
