using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

namespace ValyanClinic.Application.Services.ScrisoareMedicala;

/// <summary>
/// Implementare serviciu pentru generarea Scrisorii Medicale Anexa 43
/// Conform Ordin MS nr. 1411/2016
/// </summary>
public class ScrisoareMedicalaService : IScrisoareMedicalaService
{
    private readonly ILogger<ScrisoareMedicalaService> _logger;

    public ScrisoareMedicalaService(ILogger<ScrisoareMedicalaService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<ScrisoareMedicalaDto>> GenerateFromConsultatieAsync(
        Guid consultatieId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Scrisoare Medicală for Consultatie {ConsultatieId}", consultatieId);

        // TODO: Implement real data fetching from repository
        // For now, return mock data
        await Task.CompletedTask;
        
        var mockData = GenerateMockData();
        mockData.ConsultatieId = consultatieId;
        
        return Result<ScrisoareMedicalaDto>.Success(mockData);
    }

    /// <inheritdoc />
    public async Task<Result<ScrisoareMedicalaDto>> GenerateFromDraftAsync(
        ConsulatieDetailDto consultatie,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Scrisoare Medicală from draft for Consultatie {ConsultatieId}", 
            consultatie.ConsultatieID);

        await Task.CompletedTask;

        var dto = new ScrisoareMedicalaDto
        {
            // Header - Clinică
            NumeClinica = "ValyanClinic",
            TipClinica = "Clinică Medicală de Specialitate",
            AdresaClinica = "Str. Exemplu nr. 123, Sector 1, București",
            TelefonClinica = "021 123 4567",
            EmailClinica = "contact@valyanclinic.ro",
            CUIClinica = "RO12345678",
            RegistruComertClinica = "J40/1234/2020",
            ContractCAS = "C-2024-1234",
            CASJudet = "CAS București",
            NumarRegistruConsultatii = $"RC-{DateTime.Now.Year}-{new Random().Next(1000, 9999)}",

            // Consultație
            ConsultatieId = consultatie.ConsultatieID,
            DataConsultatie = consultatie.DataConsultatie,
            TipConsultatie = consultatie.TipConsultatie,

            // Pacient
            PacientId = consultatie.PacientID,
            PacientNumeComplet = consultatie.PacientNumeComplet,
            PacientCNP = consultatie.PacientCNP,
            PacientDataNasterii = consultatie.PacientDataNasterii,
            PacientVarsta = consultatie.Varsta,
            PacientSex = consultatie.PacientSex,
            PacientTelefon = consultatie.PacientTelefon,

            // Motiv prezentare
            MotivPrezentare = consultatie.MotivPrezentare,
            IstoricBoalaActuala = consultatie.IstoricBoalaActuala,

            // Antecedente (SIMPLIFIED)
            AntecendenteHeredoColaterale = consultatie.IstoricFamilial ?? "Fără antecedente semnificative.",
            AntecendentePatologicePersonale = consultatie.IstoricMedicalPersonal ?? "Fără antecedente patologice semnificative.",
            Alergii = consultatie.PacientAlergii,
            MedicatieCronicaAnterioara = null, // No longer tracked separately
            FactoriDeRisc = null, // No longer tracked separately

            // Examen clinic
            StareGenerala = consultatie.StareGenerala,
            TensiuneArteriala = consultatie.TensiuneArteriala,
            Puls = consultatie.Puls,
            Temperatura = consultatie.Temperatura,
            FrecventaRespiratorie = consultatie.FreccventaRespiratorie,
            Greutate = consultatie.Greutate,
            Inaltime = consultatie.Inaltime,
            IMC = consultatie.IMC,
            IMCCategorie = consultatie.IMCCategorie,
            SaturatieO2 = consultatie.SaturatieO2,
            ExamenCardiovascular = consultatie.ExamenCardiovascular,
            ExamenRespiratoriu = consultatie.ExamenRespiratoriu,
            ExamenDigestiv = consultatie.ExamenDigestiv,

            // Investigații
            RezultatEKG = consultatie.InvestigatiiEKG,
            AlteInvestigatii = consultatie.AlteInvestigatii,

            // Diagnostic
            DiagnosticPrincipal = ParseDiagnosticPrincipal(consultatie.DiagnosticPozitiv, consultatie.CoduriICD10),

            // Tratament
            TratamentAnterior = consultatie.TratamentMedicamentos,

            // Recomandări
            Recomandari = ParseRecomandari(consultatie),

            // Medic
            MedicId = consultatie.MedicID,
            MedicNumeComplet = consultatie.MedicNumeComplet,
            MedicSpecializare = consultatie.MedicSpecializare,

            // Metadata
            DataEmitere = DateTime.Now
        };

        // Parse diagnostice secundare
        dto.DiagnosticeSecundare = ParseDiagnosticeSecundare(consultatie.CoduriICD10Secundare);

        return Result<ScrisoareMedicalaDto>.Success(dto);
    }

    /// <inheritdoc />
    public ScrisoareMedicalaDto GenerateMockData()
    {
        _logger.LogInformation("Generating mock data for Scrisoare Medicală");

        return new ScrisoareMedicalaDto
        {
            // Header - Clinică
            NumeClinica = "ValyanClinic",
            TipClinica = "Clinică Medicală de Specialitate",
            AdresaClinica = "Str. Exemplu nr. 123, Sector 1, București",
            TelefonClinica = "021 123 4567",
            EmailClinica = "contact@valyanclinic.ro",
            CUIClinica = "RO12345678",
            RegistruComertClinica = "J40/1234/2020",
            ContractCAS = "C-2024-1234",
            CASJudet = "CAS București",
            NumarRegistruConsultatii = "RC-2025-4567",

            // Consultație
            ConsultatieId = Guid.NewGuid(),
            DataConsultatie = DateTime.Now,
            TipConsultatie = "Control",

            // Pacient
            PacientId = Guid.NewGuid(),
            PacientNumeComplet = "Ionescu Maria",
            PacientCNP = "2850315123456",
            PacientDataNasterii = new DateTime(1985, 3, 15),
            PacientVarsta = 39,
            PacientSex = "Feminin",
            PacientAdresa = "Str. Florilor nr. 25, București",
            PacientTelefon = "0722 123 456",
            PacientEmail = "maria.ionescu@email.com",

            // Motiv prezentare
            MotivPrezentare = "Control cardiologic periodic în contextul hipertensiunii arteriale cunoscute și tratate. Acuză cefalee occipitală intermitentă în ultimele 2 săptămâni, predominant matinal, asociată cu senzație de amețeală la schimbarea bruscă a poziției. Valori TA crescute la automonitorizare (160-170/100 mmHg), în ciuda complianței la tratamentul actual.",

            // Afecțiune oncologică
            EsteAfectiuneOncologica = false,

            // Diagnostic
            DiagnosticPrincipal = new DiagnosticScrisoareDto
            {
                CodICD10 = "I10",
                Denumire = "Hipertensiune arterială esențială",
                Detalii = "HTA esențială stadiul II, risc cardiovascular înalt. Afectare de organ țintă: HVS concentrică. Control tensional suboptimal sub tratament actual.",
                EstePrincipal = true
            },
            DiagnosticeSecundare = new List<DiagnosticScrisoareDto>
            {
                new()
                {
                    CodICD10 = "E11.9",
                    Denumire = "Diabet zaharat tip 2 fără complicații",
                    Detalii = "Control glicemic acceptabil (HbA1c 7.2%). Fără complicații micro sau macrovasculare decelate.",
                    EstePrincipal = false
                },
                new()
                {
                    CodICD10 = "E78.0",
                    Denumire = "Hipercolesterolemie pură",
                    Detalii = "LDL-C peste țintă pentru risc CV înalt (<70 mg/dl).",
                    EstePrincipal = false
                }
            },

            // Anamneza
            AntecendenteHeredoColaterale = "Tată - HTA, DZ tip 2; Mamă - cardiopatie ischemică.",
            AntecendentePatologicePersonale = "HTA esențială din 2020, DZ tip 2 din 2021, hipercolesterolemie. Fără intervenții chirurgicale în antecedente.",
            Alergii = "Penicilină (reacție cutanată).",
            MedicatieCronicaAnterioara = "Ramipril 5mg/zi, Metformin 850mg x2/zi, Atorvastatină 20mg/zi.",
            FactoriDeRisc = "Supraponderalitate (IMC 26.4 kg/m²), sedentarism, stres ocupațional, antecedente familiale cardiovasculare (tată cu HTA și DZ, mamă cu cardiopatie ischemică).",

            // Examen clinic
            StareGenerala = "Bună",
            TensiuneArteriala = "155/95",
            Puls = 78,
            Temperatura = 36.4m,
            FrecventaRespiratorie = 16,
            Greutate = 72,
            Inaltime = 165,
            IMC = 26.4m,
            IMCCategorie = "Supraponderal",
            SaturatieO2 = 98,
            ExamenClinicGeneral = "Stare generală bună, conștientă, cooperantă, orientată temporo-spațial. Tegumente normal colorate, fără edeme. Mucoase roz, umede. FR=16/min.",
            ExamenClinicLocal = "Cord - zgomote cardiace ritmice, bine bătute, fără sufluri. Șocul apexian în spațiul V intercostal pe linia medioclaviculară stângă. Puls periferic prezent și simetric bilateral. Jugulare neturgesente. Fără edeme gambiere.",
            ExamenCardiovascular = "Cord - zgomote cardiace ritmice, bine bătute, fără sufluri.",

            // Rezultate laborator
            RezultateNormale = new List<RezultatLaboratorDto>
            {
                new() { Denumire = "Creatinină", Valoare = "0.9", Unitate = "mg/dl" },
                new() { Denumire = "Potasiu", Valoare = "4.2", Unitate = "mEq/L" },
                new() { Denumire = "Hemoglobină", Valoare = "13.2", Unitate = "g/dl" },
                new() { Denumire = "Leucocite", Valoare = "6800", Unitate = "/mm³" }
            },
            RezultatePatologice = new List<RezultatLaboratorDto>
            {
                new() { Denumire = "Glicemie", Valoare = "128", Unitate = "mg/dl", ValoareNormala = "VN<100", EstePatologic = true },
                new() { Denumire = "HbA1c", Valoare = "7.2", Unitate = "%", ValoareNormala = "VN<5.7%", EstePatologic = true },
                new() { Denumire = "Colesterol total", Valoare = "195", Unitate = "mg/dl", EstePatologic = true },
                new() { Denumire = "LDL-C", Valoare = "112", Unitate = "mg/dl", ValoareNormala = "țintă <70", EstePatologic = true },
                new() { Denumire = "Trigliceride", Valoare = "156", Unitate = "mg/dl", EstePatologic = true }
            },

            // Investigații paraclinice
            RezultatEKG = "Ritm sinusal, AV 78/min. Ax QRS intermediar. Semne de hipertrofie ventriculară stângă (indice Sokolow-Lyon 38mm). Fără modificări ST-T.",
            RezultatEcografie = "VS cu geometrie concentrică (SIV 12mm, PP 11mm). Funcție sistolică VS păstrată (FE 58%). Disfuncție diastolică grad I (relaxare întârziată). Valve morfologic normale. Fără revărsat pericardic.",
            RezultatRx = "Nu s-a efectuat.",

            // Tratament anterior
            TratamentAnterior = "Ramipril 5mg 1cp/zi dimineața, Metformin 850mg 1cp x2/zi, Atorvastatină 20mg 1cp/zi seara.",

            // Alte informații
            AlteInformatii = "Pacienta prezintă complianță bună la tratament. Automonitorizare TA inconstantă. Dietă parțial respectată. Sedentarism. Necesită intensificarea tratamentului antihipertensiv și ajustarea tratamentului hipolipemiant pentru atingerea țintelor terapeutice.",

            // Tratament recomandat
            TratamentRecomandat = new List<MedicamentScrisoareDto>
            {
                new()
                {
                    Denumire = "Ramipril",
                    Doza = "10 mg",
                    Frecventa = "1x/zi dimineața",
                    Durata = "3 luni",
                    Observatii = "creștere doză"
                },
                new()
                {
                    Denumire = "Indapamidă",
                    Doza = "1.5 mg",
                    Frecventa = "1x/zi dimineața",
                    Durata = "3 luni",
                    Observatii = "adăugare diuretic"
                },
                new()
                {
                    Denumire = "Atorvastatină",
                    Doza = "40 mg",
                    Frecventa = "1x/zi seara",
                    Durata = "3 luni",
                    Observatii = "creștere doză"
                },
                new()
                {
                    Denumire = "Metformin",
                    Doza = "850 mg",
                    Frecventa = "2x/zi (mic dejun, cină)",
                    Durata = "3 luni",
                    Observatii = null
                }
            },

            // Recomandări
            Recomandari = new List<string>
            {
                "Automonitorizare TA de 2x/zi (dimineața înainte de medicație și seara) - notare în jurnal",
                "Regim alimentar hiposodat (max 5g sare/zi), hipolipidic, hipoglucidic",
                "Reducere ponderală - țintă IMC <25 kg/m²",
                "Activitate fizică aerobică moderată - minimum 150 min/săptămână",
                "Evitare fumat și consum excesiv de alcool",
                "Tehnici de management al stresului",
                "Control cardiologic peste 3 luni cu: profil lipidic, HbA1c, ionogramă, EKG"
            },

            // Checkbox sections
            AreIndicatieInternare = false,
            SaEliberatPrescriptie = true,
            SeriePrescriptie = "Seria A nr. 1234567",
            NuSaEliberatPrescriptieNuAFostNecesar = false,
            SaEliberatConcediuMedical = false,
            NuSaEliberatConcediuNuAFostNecesar = true,
            SaEliberatRecomandareIngrijiriDomiciliu = false,
            NuSaEliberatIngrijiriNuAFostNecesar = true,
            SaEliberatPrescriptieDispozitive = false,
            NuSaEliberatDispozitiveNuAFostNecesar = true,

            // Medic
            MedicId = Guid.NewGuid(),
            MedicNumeComplet = "Dr. Adrian Popescu",
            MedicSpecializare = "Medic Primar Cardiologie",
            MedicCodParafa = "DR001234",

            // Transmitere
            TransmiterePrinAsigurat = true,
            TransmiterePrinEmail = false,

            // Metadata
            DataEmitere = DateTime.Now
        };
    }

    #region Private Helper Methods

    private static string BuildAntecendenteHeredoString(ConsulatieDetailDto consultatie)
    {
        // SIMPLIFIED - just return IstoricFamilial or default message
        return !string.IsNullOrWhiteSpace(consultatie.IstoricFamilial) 
            ? consultatie.IstoricFamilial 
            : "Fără antecedente semnificative.";
    }

    private static string BuildAntecendentePatologiceString(ConsulatieDetailDto consultatie)
    {
        // SIMPLIFIED - just return IstoricMedicalPersonal or default message
        return !string.IsNullOrWhiteSpace(consultatie.IstoricMedicalPersonal) 
            ? consultatie.IstoricMedicalPersonal 
            : "Fără antecedente patologice semnificative.";
    }

    private static DiagnosticScrisoareDto? ParseDiagnosticPrincipal(string? diagnostic, string? codIcd10)
    {
        if (string.IsNullOrWhiteSpace(diagnostic) && string.IsNullOrWhiteSpace(codIcd10))
            return null;

        return new DiagnosticScrisoareDto
        {
            CodICD10 = codIcd10 ?? "",
            Denumire = diagnostic ?? "",
            EstePrincipal = true
        };
    }

    private static List<DiagnosticScrisoareDto> ParseDiagnosticeSecundare(string? coduriSecundare)
    {
        if (string.IsNullOrWhiteSpace(coduriSecundare))
            return new List<DiagnosticScrisoareDto>();

        // Parse format: "E11.9 - Diabet; E78.0 - Hipercolesterolemie"
        return coduriSecundare
            .Split(new[] { ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(d =>
            {
                var parts = d.Trim().Split(new[] { " - ", " – " }, StringSplitOptions.None);
                return new DiagnosticScrisoareDto
                {
                    CodICD10 = parts.Length > 0 ? parts[0].Trim() : "",
                    Denumire = parts.Length > 1 ? parts[1].Trim() : parts[0].Trim(),
                    EstePrincipal = false
                };
            })
            .ToList();
    }

    private static List<string> ParseRecomandari(ConsulatieDetailDto consultatie)
    {
        var recomandari = new List<string>();

        if (!string.IsNullOrWhiteSpace(consultatie.TratamentNemedicamentos))
            recomandari.Add(consultatie.TratamentNemedicamentos);
        if (!string.IsNullOrWhiteSpace(consultatie.RecomandariDietetice))
            recomandari.Add(consultatie.RecomandariDietetice);
        if (!string.IsNullOrWhiteSpace(consultatie.RecomandariRegimViata))
            recomandari.Add(consultatie.RecomandariRegimViata);
        if (!string.IsNullOrWhiteSpace(consultatie.InvestigatiiRecomandate))
            recomandari.Add(consultatie.InvestigatiiRecomandate);
        if (!string.IsNullOrWhiteSpace(consultatie.ConsulturiSpecialitate))
            recomandari.Add(consultatie.ConsulturiSpecialitate);
        if (!string.IsNullOrWhiteSpace(consultatie.RecomandariSupraveghere))
            recomandari.Add(consultatie.RecomandariSupraveghere);
        if (!string.IsNullOrWhiteSpace(consultatie.DataUrmatoareiProgramari))
            recomandari.Add($"Control la data de {consultatie.DataUrmatoareiProgramari}");

        return recomandari;
    }

    #endregion
}
