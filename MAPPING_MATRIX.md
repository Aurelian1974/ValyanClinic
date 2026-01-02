# MAPPING MATRIX - Consultatie Normalization

**Data**: 2 Ianuarie 2026  
**Purpose**: Mapare exactă pentru refactorizare handlers Application Layer

---

## MAPARE A: SaveConsultatieDraftCommand → Entități (Write Operations)

### Command Properties (29 total)

| # | Command Property | Type | Target Entity | Target Property | Notes |
|---|------------------|------|---------------|-----------------|-------|
| 1 | ConsultatieID | Guid? | Consultatie | ConsultatieID | NULL = CREATE, valoare = UPDATE |
| 2 | ProgramareID | Guid? | Consultatie | ProgramareID | Nullable |
| 3 | PacientID | Guid | Consultatie | PacientID | Required |
| 4 | MedicID | Guid | Consultatie | MedicID | Required |
| 5 | DataConsultatie | DateTime | Consultatie | DataConsultatie | Required |
| 6 | OraConsultatie | TimeSpan | Consultatie | OraConsultatie | Required |
| 7 | TipConsultatie | string | Consultatie | TipConsultatie | Default: "Prima consultatie" |
| | | | | | |
| **TAB 1: MOTIV & ANTECEDENTE** | | | | | |
| 8 | MotivPrezentare | string? | ConsultatieMotivePrezentare | MotivPrezentare | ✅ Mapare directă |
| 9 | IstoricBoalaActuala | string? | ConsultatieMotivePrezentare | IstoricBoalaActuala | ✅ Mapare directă |
| 10 | APP_Medicatie | string? | ConsultatieAntecedente | APP_Medicatie | ✅ Mapare directă |
| | | | | | |
| **TAB 2: EXAMEN CLINIC** | | | | | |
| 11 | Greutate | decimal? | ConsultatieExamenObiectiv | Greutate | ✅ Semne vitale |
| 12 | Inaltime | decimal? | ConsultatieExamenObiectiv | Inaltime | ✅ Semne vitale |
| 13 | IMC | decimal? | ConsultatieExamenObiectiv | IMC | ✅ Semne vitale |
| 14 | Temperatura | decimal? | ConsultatieExamenObiectiv | Temperatura | ✅ Semne vitale |
| 15 | TensiuneArteriala | string? | ConsultatieExamenObiectiv | TensiuneArteriala | ✅ Semne vitale |
| 16 | Puls | int? | ConsultatieExamenObiectiv | Puls | ✅ Semne vitale |
| 17 | FreccventaRespiratorie | int? | ConsultatieExamenObiectiv | FreccventaRespiratorie | ✅ Semne vitale |
| 18 | SaturatieO2 | int? | ConsultatieExamenObiectiv | SaturatieO2 | ✅ Semne vitale |
| 19 | StareGenerala | string? | ConsultatieExamenObiectiv | StareGenerala | ✅ Examen general |
| 20 | Tegumente | string? | ConsultatieExamenObiectiv | Tegumente | ✅ Examen general |
| 21 | Mucoase | string? | ConsultatieExamenObiectiv | Mucoase | ✅ Examen general |
| 22 | Edeme | string? | ConsultatieExamenObiectiv | Edeme | ✅ Examen general |
| 23 | ExamenCardiovascular | string? | ConsultatieExamenObiectiv | ExamenCardiovascular | ✅ Examen aparate |
| 24 | InvestigatiiLaborator | string? | ConsultatieInvestigatii | InvestigatiiLaborator | ✅ Mapare directă |
| | | | | | |
| **TAB 3: DIAGNOSTIC & TRATAMENT** | | | | | |
| 25 | DiagnosticPozitiv | string? | ConsultatieDiagnostic | DiagnosticPozitiv | ✅ Mapare directă |
| 26 | DiagnosticDiferential | string? | ConsultatieDiagnostic | DiagnosticDiferential | ✅ Mapare directă |
| 27 | CoduriICD10 | string? | ConsultatieDiagnostic | CoduriICD10 | ✅ Mapare directă |
| 28 | CoduriICD10Secundare | string? | ConsultatieDiagnostic | CoduriICD10Secundare | ✅ Mapare directă |
| 29 | TratamentMedicamentos | string? | ConsultatieTratament | TratamentMedicamentos | ✅ Mapare directă |
| 30 | RecomandariRegimViata | string? | ConsultatieTratament | RecomandariRegimViata | ✅ Mapare directă |
| | | | | | |
| **TAB 4: CONCLUZII** | | | | | |
| 31 | Concluzie | string? | ConsultatieConcluzii | Concluzie | ✅ Mapare directă |
| 32 | ObservatiiMedic | string? | ConsultatieConcluzii | ObservatiiMedic | ✅ Mapare directă |
| 33 | DataUrmatoareiProgramari | string? | ConsultatieTratament | DataUrmatoareiProgramari | ✅ Mapare directă |
| | | | | | |
| **AUDIT** | | | | | |
| 34 | CreatDeSauModificatDe | Guid | Toate entitățile | CreatDe / ModificatDe | Conditional logic |

### Logica de Creare Entități (CRITICAL)

```csharp
// REGULA: Dacă command.Property == null → NU crea entitatea
// Fiecare entitate se creează DOAR dacă are măcar 1 proprietate NON-NULL

// 1. ConsultatieMotivePrezentare
if (!string.IsNullOrWhiteSpace(command.MotivPrezentare) || 
    !string.IsNullOrWhiteSpace(command.IstoricBoalaActuala))
{
    await repository.UpsertMotivePrezentareAsync(consultatieId, new ConsultatieMotivePrezentare
    {
        ConsultatieID = consultatieId,
        MotivPrezentare = command.MotivPrezentare,
        IstoricBoalaActuala = command.IstoricBoalaActuala,
        CreatDe = command.CreatDeSauModificatDe,
        DataCreare = DateTime.Now
    });
}

// 2. ConsultatieAntecedente
if (!string.IsNullOrWhiteSpace(command.APP_Medicatie))
{
    await repository.UpsertAntecedenteAsync(consultatieId, new ConsultatieAntecedente
    {
        ConsultatieID = consultatieId,
        APP_Medicatie = command.APP_Medicatie,
        CreatDe = command.CreatDeSauModificatDe,
        DataCreare = DateTime.Now
    });
}

// 3. ConsultatieExamenObiectiv
if (command.Greutate.HasValue || command.Inaltime.HasValue || 
    command.IMC.HasValue || command.Temperatura.HasValue ||
    !string.IsNullOrWhiteSpace(command.TensiuneArteriala) ||
    command.Puls.HasValue || command.FreccventaRespiratorie.HasValue ||
    command.SaturatieO2.HasValue || 
    !string.IsNullOrWhiteSpace(command.StareGenerala) ||
    !string.IsNullOrWhiteSpace(command.Tegumente) ||
    !string.IsNullOrWhiteSpace(command.Mucoase) ||
    !string.IsNullOrWhiteSpace(command.Edeme) ||
    !string.IsNullOrWhiteSpace(command.ExamenCardiovascular))
{
    await repository.UpsertExamenObiectivAsync(consultatieId, new ConsultatieExamenObiectiv
    {
        ConsultatieID = consultatieId,
        Greutate = command.Greutate,
        Inaltime = command.Inaltime,
        IMC = command.IMC,
        Temperatura = command.Temperatura,
        TensiuneArteriala = command.TensiuneArteriala,
        Puls = command.Puls,
        FreccventaRespiratorie = command.FreccventaRespiratorie,
        SaturatieO2 = command.SaturatieO2,
        StareGenerala = command.StareGenerala,
        Tegumente = command.Tegumente,
        Mucoase = command.Mucoase,
        Edeme = command.Edeme,
        ExamenCardiovascular = command.ExamenCardiovascular,
        CreatDe = command.CreatDeSauModificatDe,
        DataCreare = DateTime.Now
    });
}

// 4. ConsultatieInvestigatii
if (!string.IsNullOrWhiteSpace(command.InvestigatiiLaborator))
{
    await repository.UpsertInvestigatiiAsync(consultatieId, new ConsultatieInvestigatii
    {
        ConsultatieID = consultatieId,
        InvestigatiiLaborator = command.InvestigatiiLaborator,
        CreatDe = command.CreatDeSauModificatDe,
        DataCreare = DateTime.Now
    });
}

// 5. ConsultatieDiagnostic
if (!string.IsNullOrWhiteSpace(command.DiagnosticPozitiv) ||
    !string.IsNullOrWhiteSpace(command.DiagnosticDiferential) ||
    !string.IsNullOrWhiteSpace(command.CoduriICD10) ||
    !string.IsNullOrWhiteSpace(command.CoduriICD10Secundare))
{
    await repository.UpsertDiagnosticAsync(consultatieId, new ConsultatieDiagnostic
    {
        ConsultatieID = consultatieId,
        DiagnosticPozitiv = command.DiagnosticPozitiv,
        DiagnosticDiferential = command.DiagnosticDiferential,
        CoduriICD10 = command.CoduriICD10,
        CoduriICD10Secundare = command.CoduriICD10Secundare,
        CreatDe = command.CreatDeSauModificatDe,
        DataCreare = DateTime.Now
    });
}

// 6. ConsultatieTratament
if (!string.IsNullOrWhiteSpace(command.TratamentMedicamentos) ||
    !string.IsNullOrWhiteSpace(command.RecomandariRegimViata) ||
    !string.IsNullOrWhiteSpace(command.DataUrmatoareiProgramari))
{
    await repository.UpsertTratamentAsync(consultatieId, new ConsultatieTratament
    {
        ConsultatieID = consultatieId,
        TratamentMedicamentos = command.TratamentMedicamentos,
        RecomandariRegimViata = command.RecomandariRegimViata,
        DataUrmatoareiProgramari = command.DataUrmatoareiProgramari,
        CreatDe = command.CreatDeSauModificatDe,
        DataCreare = DateTime.Now
    });
}

// 7. ConsultatieConcluzii
if (!string.IsNullOrWhiteSpace(command.Concluzie) ||
    !string.IsNullOrWhiteSpace(command.ObservatiiMedic))
{
    await repository.UpsertConcluziiAsync(consultatieId, new ConsultatieConcluzii
    {
        ConsultatieID = consultatieId,
        Concluzie = command.Concluzie,
        ObservatiiMedic = command.ObservatiiMedic,
        CreatDe = command.CreatDeSauModificatDe,
        DataCreare = DateTime.Now
    });
}
```

---

## MAPARE B: Entități → ConsulatieDetailDto (Read Operations)

### Navigation Properties → Flatten DTO

| Entity | Properties | DTO Target Properties | Null-Safe Pattern |
|--------|-----------|----------------------|-------------------|
| **Consultatie (MASTER)** | | | |
| | ConsultatieID | ConsultatieID | Direct |
| | ProgramareID | ProgramareID | Direct |
| | PacientID | PacientID | Direct |
| | MedicID | MedicID | Direct |
| | DataConsultatie | DataConsultatie | Direct |
| | OraConsultatie | OraConsultatie | Direct |
| | TipConsultatie | TipConsultatie | Direct |
| | Status | Status | Direct |
| | DataFinalizare | DataFinalizare | Direct |
| | DurataMinute | DurataMinute | Direct |
| | DataCreare | DataCreare | Direct |
| | CreatDe | CreatDe | Direct |
| | DataUltimeiModificari | DataUltimeiModificari | Direct |
| | ModificatDe | ModificatDe | Direct |
| | | | |
| **ConsultatieMotivePrezentare** | | | |
| | MotivPrezentare | MotivPrezentare | `consultatie.MotivePrezentare?.MotivPrezentare` |
| | IstoricBoalaActuala | IstoricBoalaActuala | `consultatie.MotivePrezentare?.IstoricBoalaActuala` |
| | | | |
| **ConsultatieAntecedente** | | | |
| | AHC_Mama | AHC_Mama | `consultatie.Antecedente?.AHC_Mama` |
| | AHC_Tata | AHC_Tata | `consultatie.Antecedente?.AHC_Tata` |
| | AHC_Frati | AHC_Frati | `consultatie.Antecedente?.AHC_Frati` |
| | AHC_Bunici | AHC_Bunici | `consultatie.Antecedente?.AHC_Bunici` |
| | AHC_Altele | AHC_Altele | `consultatie.Antecedente?.AHC_Altele` |
| | AF_Nastere | AF_Nastere | `consultatie.Antecedente?.AF_Nastere` |
| | AF_Dezvoltare | AF_Dezvoltare | `consultatie.Antecedente?.AF_Dezvoltare` |
| | AF_Menstruatie | AF_Menstruatie | `consultatie.Antecedente?.AF_Menstruatie` |
| | AF_Sarcini | AF_Sarcini | `consultatie.Antecedente?.AF_Sarcini` |
| | AF_Alaptare | AF_Alaptare | `consultatie.Antecedente?.AF_Alaptare` |
| | APP_BoliCopilarieAdolescenta | APP_BoliCopilarieAdolescenta | `consultatie.Antecedente?.APP_BoliCopilarieAdolescenta` |
| | APP_BoliAdult | APP_BoliAdult | `consultatie.Antecedente?.APP_BoliAdult` |
| | APP_Interventii | APP_Interventii | `consultatie.Antecedente?.APP_Interventii` |
| | APP_Traumatisme | APP_Traumatisme | `consultatie.Antecedente?.APP_Traumatisme` |
| | APP_Transfuzii | APP_Transfuzii | `consultatie.Antecedente?.APP_Transfuzii` |
| | APP_Alergii | APP_Alergii | `consultatie.Antecedente?.APP_Alergii` |
| | APP_Medicatie | APP_Medicatie | `consultatie.Antecedente?.APP_Medicatie` |
| | Profesie | Profesie | `consultatie.Antecedente?.Profesie` |
| | ConditiiLocuinta | ConditiiLocuinta | `consultatie.Antecedente?.ConditiiLocuinta` |
| | ConditiiMunca | ConditiiMunca | `consultatie.Antecedente?.ConditiiMunca` |
| | ObiceiuriAlimentare | ObiceiuriAlimentare | `consultatie.Antecedente?.ObiceiuriAlimentare` |
| | Toxice | Toxice | `consultatie.Antecedente?.Toxice` |
| | | | |
| **ConsultatieExamenObiectiv** | | | |
| | StareGenerala | StareGenerala | `consultatie.ExamenObiectiv?.StareGenerala` |
| | Constitutie | Constitutie | `consultatie.ExamenObiectiv?.Constitutie` |
| | Atitudine | Atitudine | `consultatie.ExamenObiectiv?.Atitudine` |
| | Facies | Facies | `consultatie.ExamenObiectiv?.Facies` |
| | Tegumente | Tegumente | `consultatie.ExamenObiectiv?.Tegumente` |
| | Mucoase | Mucoase | `consultatie.ExamenObiectiv?.Mucoase` |
| | GangliniLimfatici | GangliniLimfatici | `consultatie.ExamenObiectiv?.GangliniLimfatici` |
| | Edeme | Edeme | `consultatie.ExamenObiectiv?.Edeme` |
| | Greutate | Greutate | `consultatie.ExamenObiectiv?.Greutate` |
| | Inaltime | Inaltime | `consultatie.ExamenObiectiv?.Inaltime` |
| | IMC | IMC | `consultatie.ExamenObiectiv?.IMC` |
| | Temperatura | Temperatura | `consultatie.ExamenObiectiv?.Temperatura` |
| | TensiuneArteriala | TensiuneArteriala | `consultatie.ExamenObiectiv?.TensiuneArteriala` |
| | Puls | Puls | `consultatie.ExamenObiectiv?.Puls` |
| | FreccventaRespiratorie | FreccventaRespiratorie | `consultatie.ExamenObiectiv?.FreccventaRespiratorie` |
| | SaturatieO2 | SaturatieO2 | `consultatie.ExamenObiectiv?.SaturatieO2` |
| | Glicemie | Glicemie | `consultatie.ExamenObiectiv?.Glicemie` |
| | ExamenCardiovascular | ExamenCardiovascular | `consultatie.ExamenObiectiv?.ExamenCardiovascular` |
| | ExamenRespiratoriu | ExamenRespiratoriu | `consultatie.ExamenObiectiv?.ExamenRespiratoriu` |
| | ExamenDigestiv | ExamenDigestiv | `consultatie.ExamenObiectiv?.ExamenDigestiv` |
| | ExamenUrinar | ExamenUrinar | `consultatie.ExamenObiectiv?.ExamenUrinar` |
| | ExamenNervos | ExamenNervos | `consultatie.ExamenObiectiv?.ExamenNervos` |
| | ExamenLocomotor | ExamenLocomotor | `consultatie.ExamenObiectiv?.ExamenLocomotor` |
| | ExamenEndocrin | ExamenEndocrin | `consultatie.ExamenObiectiv?.ExamenEndocrin` |
| | ExamenORL | ExamenORL | `consultatie.ExamenObiectiv?.ExamenORL` |
| | ExamenOftalmologic | ExamenOftalmologic | `consultatie.ExamenObiectiv?.ExamenOftalmologic` |
| | ExamenDermatologic | ExamenDermatologic | `consultatie.ExamenObiectiv?.ExamenDermatologic` |
| | | | |
| **ConsultatieInvestigatii** | | | |
| | InvestigatiiLaborator | InvestigatiiLaborator | `consultatie.Investigatii?.InvestigatiiLaborator` |
| | InvestigatiiImagistice | InvestigatiiImagistice | `consultatie.Investigatii?.InvestigatiiImagistice` |
| | InvestigatiiEKG | InvestigatiiEKG | `consultatie.Investigatii?.InvestigatiiEKG` |
| | AlteInvestigatii | AlteInvestigatii | `consultatie.Investigatii?.AlteInvestigatii` |
| | | | |
| **ConsultatieDiagnostic** | | | |
| | DiagnosticPozitiv | DiagnosticPozitiv | `consultatie.Diagnostic?.DiagnosticPozitiv` |
| | DiagnosticDiferential | DiagnosticDiferential | `consultatie.Diagnostic?.DiagnosticDiferential` |
| | DiagnosticEtiologic | DiagnosticEtiologic | `consultatie.Diagnostic?.DiagnosticEtiologic` |
| | CoduriICD10 | CoduriICD10 | `consultatie.Diagnostic?.CoduriICD10` |
| | CoduriICD10Secundare | CoduriICD10Secundare | `consultatie.Diagnostic?.CoduriICD10Secundare` |
| | | | |
| **ConsultatieTratament** | | | |
| | TratamentMedicamentos | TratamentMedicamentos | `consultatie.Tratament?.TratamentMedicamentos` |
| | TratamentNemedicamentos | TratamentNemedicamentos | `consultatie.Tratament?.TratamentNemedicamentos` |
| | RecomandariDietetice | RecomandariDietetice | `consultatie.Tratament?.RecomandariDietetice` |
| | RecomandariRegimViata | RecomandariRegimViata | `consultatie.Tratament?.RecomandariRegimViata` |
| | InvestigatiiRecomandate | InvestigatiiRecomandate | `consultatie.Tratament?.InvestigatiiRecomandate` |
| | ConsulturiSpecialitate | ConsulturiSpecialitate | `consultatie.Tratament?.ConsulturiSpecialitate` |
| | DataUrmatoareiProgramari | DataUrmatoareiProgramari | `consultatie.Tratament?.DataUrmatoareiProgramari` |
| | RecomandariSupraveghere | RecomandariSupraveghere | `consultatie.Tratament?.RecomandariSupraveghere` |
| | | | |
| **ConsultatieConcluzii** | | | |
| | Prognostic | Prognostic | `consultatie.Concluzii?.Prognostic` |
| | Concluzie | Concluzie | `consultatie.Concluzii?.Concluzie` |
| | ObservatiiMedic | ObservatiiMedic | `consultatie.Concluzii?.ObservatiiMedic` |
| | NotePacient | NotePacient | `consultatie.Concluzii?.NotePacient` |
| | DocumenteAtatate | DocumenteAtatate | `consultatie.Concluzii?.DocumenteAtatate` |

### Code Pattern pentru Query Handlers

```csharp
// În GetConsulatieByIdQueryHandler.cs
var dto = new ConsulatieDetailDto
{
    // MASTER fields
    ConsultatieID = consultatie.ConsultatieID,
    PacientID = consultatie.PacientID,
    MedicID = consultatie.MedicID,
    DataConsultatie = consultatie.DataConsultatie,
    OraConsultatie = consultatie.OraConsultatie,
    TipConsultatie = consultatie.TipConsultatie,
    Status = consultatie.Status,
    
    // Navigation properties - NULL SAFE
    MotivPrezentare = consultatie.MotivePrezentare?.MotivPrezentare,
    IstoricBoalaActuala = consultatie.MotivePrezentare?.IstoricBoalaActuala,
    
    AHC_Mama = consultatie.Antecedente?.AHC_Mama,
    AHC_Tata = consultatie.Antecedente?.AHC_Tata,
    APP_Medicatie = consultatie.Antecedente?.APP_Medicatie,
    
    StareGenerala = consultatie.ExamenObiectiv?.StareGenerala,
    Greutate = consultatie.ExamenObiectiv?.Greutate,
    TensiuneArteriala = consultatie.ExamenObiectiv?.TensiuneArteriala,
    
    InvestigatiiLaborator = consultatie.Investigatii?.InvestigatiiLaborator,
    
    DiagnosticPozitiv = consultatie.Diagnostic?.DiagnosticPozitiv,
    CoduriICD10 = consultatie.Diagnostic?.CoduriICD10,
    
    TratamentMedicamentos = consultatie.Tratament?.TratamentMedicamentos,
    RecomandariRegimViata = consultatie.Tratament?.RecomandariRegimViata,
    
    Concluzie = consultatie.Concluzii?.Concluzie,
    ObservatiiMedic = consultatie.Concluzii?.ObservatiiMedic,
    
    // ... toate celelalte properties din DTO
};
```

---

## IMPORTANT NOTES

### ⚠️ Common Errors to Avoid

1. **Property Name Mismatch** ❌
   - Wrong: `consultatie.Antecedente?.APP_Boli`
   - Correct: `consultatie.Antecedente?.APP_BoliCopilarieAdolescenta` or `APP_BoliAdult`

2. **Wrong Navigation Property Name** ❌
   - Wrong: `consultatie.Examen?.StareGenerala`
   - Correct: `consultatie.ExamenObiectiv?.StareGenerala`

3. **Typo in Property Names** ❌
   - Wrong: `GanguriLimfatice`
   - Correct: `GangliniLimfatici`

4. **Creating Empty Entities** ❌
   - Wrong: Always calling `UpsertMotivePrezentareAsync`
   - Correct: Only call if at least 1 property has value

5. **Not Using Null-Safe Navigation** ❌
   - Wrong: `consultatie.Antecedente.AHC_Mama`
   - Correct: `consultatie.Antecedente?.AHC_Mama`

### ✅ Verification Checklist

Before refactoring each handler:
- [ ] Read COMPLETE entity file to verify property names
- [ ] Read COMPLETE DTO file to verify target property names
- [ ] Copy property names EXACTLY (do NOT type from memory)
- [ ] Use null-safe navigation (`?.`) for all navigation properties
- [ ] Test conditional logic for entity creation (at least 1 non-null property)
- [ ] Compile after EACH file change
- [ ] Commit only if build SUCCESS

---

## REFERENCE: Entity Navigation Properties in Consultatie.cs

```csharp
public class Consultatie
{
    // ... master properties ...
    
    // Navigation Properties (1:1)
    public virtual ConsultatieMotivePrezentare? MotivePrezentare { get; set; }
    public virtual ConsultatieAntecedente? Antecedente { get; set; }
    public virtual ConsultatieExamenObiectiv? ExamenObiectiv { get; set; }
    public virtual ConsultatieInvestigatii? Investigatii { get; set; }
    public virtual ConsultatieDiagnostic? Diagnostic { get; set; }
    public virtual ConsultatieTratament? Tratament { get; set; }
    public virtual ConsultatieConcluzii? Concluzii { get; set; }
    
    // Navigation Property (1:N)
    public virtual ICollection<ConsultatieAnalizaMedicala>? AnalizeMedicale { get; set; }
}
```

---

**Last Updated**: 2 Ianuarie 2026, 00:30 UTC  
**Status**: ✅ VERIFIED - Ready for refactoring
