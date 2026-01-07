using System.Text.RegularExpressions;
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;
using ValyanClinic.Application.Features.AnalizeMedicale.Queries.GetAnalizeRecomandate;
using ValyanClinic.Application.Features.AnalizeMedicale.Queries.GetAnalizeEfectuate;
using ValyanClinic.Application.Features.Investigatii.Queries;
using ValyanClinic.Application.DTOs.Investigatii;

namespace ValyanClinic.Application.Services.ScrisoareMedicala;

/// <summary>
/// Implementare serviciu pentru generarea Scrisorii Medicale Anexa 43
/// Conform Ordin MS nr. 1411/2016
/// </summary>
public class ScrisoareMedicalaService : IScrisoareMedicalaService
{
    private readonly ILogger<ScrisoareMedicalaService> _logger;
    private readonly IMediator _mediator;

    public ScrisoareMedicalaService(
        ILogger<ScrisoareMedicalaService> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
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

            // Motiv prezentare - Sanitize HTML (păstrează formatarea bold/italic)
            MotivPrezentare = SanitizeHtml(consultatie.MotivPrezentare),
            IstoricBoalaActuala = SanitizeHtml(consultatie.IstoricBoalaActuala),

            // Antecedente (SIMPLIFIED) - StripHtml for inline display in Scrisoare Medicala
            AntecendenteHeredoColaterale = StripHtml(consultatie.IstoricFamilial) ?? "Fără antecedente semnificative.",
            AntecendentePatologicePersonale = StripHtml(consultatie.IstoricMedicalPersonal) ?? "Fără antecedente patologice semnificative.",
            Alergii = StripHtml(consultatie.PacientAlergii),
            MedicatieCronicaAnterioara = StripHtml(consultatie.TratamentAnterior), // ✅ MAPPED from TratamentAnterior (Antecedente)
            FactoriDeRisc = StripHtml(consultatie.FactoriDeRisc), // ✅ MAPPED from FactoriDeRisc (Antecedente)

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
            Glicemie = consultatie.Glicemie,
            Tegumente = consultatie.Tegumente,
            Mucoase = consultatie.Mucoase,
            GanglioniLimfatici = consultatie.GanglioniLimfatici,
            Edeme = consultatie.Edeme,
            ExamenObiectivDetaliat = consultatie.ExamenObiectivDetaliat,
            AlteObservatiiClinice = consultatie.AlteObservatiiClinice,

            // Investigații
            RezultatEKG = consultatie.InvestigatiiEKG,
            AlteInvestigatii = consultatie.AlteInvestigatii,

            // Diagnostic - use NEW normalized fields with fallback to legacy
            DiagnosticPrincipal = ParseDiagnosticPrincipal(
                consultatie.CodICD10Principal, 
                consultatie.NumeDiagnosticPrincipal, 
                consultatie.DescriereDetaliataPrincipal,
                // Legacy fallback
                consultatie.DiagnosticPozitiv, 
                consultatie.CoduriICD10),

            // Tratament
            TratamentAnterior = consultatie.TratamentMedicamentos,

            // Recomandări
            Recomandari = ParseRecomandari(consultatie),

            // ✅ Anexa 43 - Checkbox fields
            EsteAfectiuneOncologica = consultatie.EsteAfectiuneOncologica,
            DetaliiAfectiuneOncologica = consultatie.DetaliiAfectiuneOncologica,
            AreIndicatieInternare = consultatie.AreIndicatieInternare,
            TermenInternare = consultatie.TermenInternare,
            SaEliberatPrescriptie = consultatie.SaEliberatPrescriptie ?? false,
            SeriePrescriptie = consultatie.SeriePrescriptie,
            NuSaEliberatPrescriptieNuAFostNecesar = !(consultatie.SaEliberatPrescriptie ?? false),
            SaEliberatConcediuMedical = consultatie.SaEliberatConcediuMedical ?? false,
            SerieConcediuMedical = consultatie.SerieConcediuMedical,
            NuSaEliberatConcediuNuAFostNecesar = !(consultatie.SaEliberatConcediuMedical ?? false),
            SaEliberatRecomandareIngrijiriDomiciliu = consultatie.SaEliberatIngrijiriDomiciliu ?? false,
            NuSaEliberatIngrijiriNuAFostNecesar = !(consultatie.SaEliberatIngrijiriDomiciliu ?? false),
            SaEliberatPrescriptieDispozitive = consultatie.SaEliberatDispozitiveMedicale ?? false,
            NuSaEliberatDispozitiveNuAFostNecesar = !(consultatie.SaEliberatDispozitiveMedicale ?? false),
            TransmiterePrinEmail = consultatie.TransmiterePrinEmail,
            EmailTransmitere = consultatie.EmailTransmitere,
            TransmiterePrinAsigurat = !consultatie.TransmiterePrinEmail,

            // Medic
            MedicId = consultatie.MedicID,
            MedicNumeComplet = consultatie.MedicNumeComplet,
            MedicSpecializare = consultatie.MedicSpecializare,

            // Metadata
            DataEmitere = DateTime.Now
        };

        // Parse diagnostice secundare - use normalized structure if available
        _logger.LogInformation("[ScrisoareMedicalaService] consultatie.DiagnosticeSecundare count: {Count}",
            consultatie.DiagnosticeSecundare?.Count ?? 0);
        
        dto.DiagnosticeSecundare = MapDiagnosticeSecundare(consultatie.DiagnosticeSecundare);
        
        _logger.LogInformation("[ScrisoareMedicalaService] dto.DiagnosticeSecundare count: {Count}",
            dto.DiagnosticeSecundare?.Count ?? 0);

        // Map tratament recomandat from MedicationList
        dto.TratamentRecomandat = MapTratamentRecomandat(consultatie.MedicationList);
        
        _logger.LogInformation("[ScrisoareMedicalaService] dto.TratamentRecomandat count: {Count}",
            dto.TratamentRecomandat?.Count ?? 0);

        // Încarcă analizele recomandate pentru consultație
        dto.AnalizeRecomandate = await LoadAnalizeRecomandateAsync(consultatie.ConsultatieID, cancellationToken);
        
        _logger.LogInformation("[ScrisoareMedicalaService] dto.AnalizeRecomandate count: {Count}",
            dto.AnalizeRecomandate?.Count ?? 0);

        // Încarcă analizele efectuate (cu rezultate) pentru consultație
        dto.AnalizeEfectuate = await LoadAnalizeEfectuateAsync(consultatie.ConsultatieID, cancellationToken);
        
        _logger.LogInformation("[ScrisoareMedicalaService] dto.AnalizeEfectuate count: {Count}",
            dto.AnalizeEfectuate?.Count ?? 0);

        // Încarcă investigațiile recomandate pentru consultație
        dto.InvestigatiiImagistice = await LoadInvestigatiiImagisticeAsync(consultatie.ConsultatieID, cancellationToken);
        dto.Explorari = await LoadExplorariAsync(consultatie.ConsultatieID, cancellationToken);
        dto.Endoscopii = await LoadEndoscopiiAsync(consultatie.ConsultatieID, cancellationToken);
        
        _logger.LogInformation("[ScrisoareMedicalaService] Investigații recomandate: Imagistice={Imagistice}, Explorări={Explorari}, Endoscopii={Endoscopii}",
            dto.InvestigatiiImagistice?.Count ?? 0,
            dto.Explorari?.Count ?? 0,
            dto.Endoscopii?.Count ?? 0);

        // Încarcă investigațiile efectuate pentru consultație
        dto.InvestigatiiImagisticeEfectuate = await LoadInvestigatiiImagisticeEfectuateAsync(consultatie.ConsultatieID, cancellationToken);
        dto.ExplorariEfectuate = await LoadExplorariEfectuateAsync(consultatie.ConsultatieID, cancellationToken);
        dto.EndoscopiiEfectuate = await LoadEndoscopiiEfectuateAsync(consultatie.ConsultatieID, cancellationToken);
        
        _logger.LogInformation("[ScrisoareMedicalaService] Investigații efectuate: Imagistice={Imagistice}, Explorări={Explorari}, Endoscopii={Endoscopii}",
            dto.InvestigatiiImagisticeEfectuate?.Count ?? 0,
            dto.ExplorariEfectuate?.Count ?? 0,
            dto.EndoscopiiEfectuate?.Count ?? 0);

        return Result<ScrisoareMedicalaDto>.Success(dto);
    }

    /// <summary>
    /// Încarcă analizele recomandate pentru o consultație
    /// </summary>
    private async Task<List<AnalizaRecomandataScrisoareDto>> LoadAnalizeRecomandateAsync(
        Guid consultatieId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetAnalizeRecomandateQuery(consultatieId);
            var result = await _mediator.Send(query, cancellationToken);
            
            if (result.IsSuccess && result.Value != null)
            {
                return result.Value.Select(a => new AnalizaRecomandataScrisoareDto
                {
                    NumeAnaliza = a.NumeAnaliza,
                    Categorie = a.TipAnaliza,
                    Prioritate = a.Prioritate,
                    EsteCito = a.EsteCito,
                    IndicatiiClinice = a.IndicatiiClinice
                }).ToList();
            }
            
            _logger.LogWarning("Failed to load analize recomandate for consultatie {ConsultatieId}: {Error}", 
                consultatieId, result.FirstError);
            return new List<AnalizaRecomandataScrisoareDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading analize recomandate for consultatie {ConsultatieId}", consultatieId);
            return new List<AnalizaRecomandataScrisoareDto>();
        }
    }

    /// <summary>
    /// Încarcă analizele efectuate (cu rezultate) pentru o consultație
    /// </summary>
    private async Task<List<AnalizaEfectuataScrisoareDto>> LoadAnalizeEfectuateAsync(
        Guid consultatieId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetAnalizeEfectuateQuery(consultatieId);
            var result = await _mediator.Send(query, cancellationToken);
            
            if (result.IsSuccess && result.Value != null)
            {
                return result.Value
                    .Where(a => a.AreRezultate) // Doar cele cu rezultate
                    .Select(a => new AnalizaEfectuataScrisoareDto
                    {
                        NumeAnaliza = a.NumeAnaliza ?? string.Empty,
                        Categorie = a.TipAnaliza,
                        DataEfectuare = a.DataEfectuare,
                        Laborator = a.LocEfectuare,
                        Rezultat = a.ValoareRezultat,
                        UnitateMasura = a.UnitatiMasura,
                        ValoriReferinta = FormatValoriReferinta(a.ValoareNormalaMin, a.ValoareNormalaMax),
                        EsteAnormal = a.EsteInAfaraLimitelor
                    }).ToList();
            }
            
            _logger.LogWarning("Failed to load analize efectuate for consultatie {ConsultatieId}: {Error}", 
                consultatieId, result.FirstError);
            return new List<AnalizaEfectuataScrisoareDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading analize efectuate for consultatie {ConsultatieId}", consultatieId);
            return new List<AnalizaEfectuataScrisoareDto>();
        }
    }

    /// <summary>
    /// Formatează valorile de referință pentru afișare
    /// </summary>
    private static string? FormatValoriReferinta(decimal? min, decimal? max)
    {
        if (!min.HasValue && !max.HasValue) return null;
        if (!min.HasValue) return $"< {max:G}";
        if (!max.HasValue) return $"> {min:G}";
        return $"{min:G} - {max:G}";
    }

    /// <summary>
    /// Încarcă investigațiile imagistice recomandate pentru o consultație
    /// </summary>
    private async Task<List<InvestigatieRecomandataScrisoareDto>> LoadInvestigatiiImagisticeAsync(
        Guid consultatieId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetInvestigatiiImagisticeRecomandateByConsultatieQuery(consultatieId);
            var result = await _mediator.Send(query, cancellationToken);
            
            if (result.IsSuccess && result.Value != null)
            {
                return result.Value.Select(i => new InvestigatieRecomandataScrisoareDto
                {
                    Denumire = i.DenumireInvestigatie,
                    Cod = i.CodInvestigatie,
                    Categorie = i.RegiuneAnatomica,
                    Prioritate = i.Prioritate,
                    EsteCito = i.EsteCito,
                    IndicatiiClinice = i.IndicatiiClinice,
                    Observatii = i.ObservatiiMedic
                }).ToList();
            }
            
            _logger.LogWarning("Failed to load investigatii imagistice for consultatie {ConsultatieId}: {Error}", 
                consultatieId, result.FirstError);
            return new List<InvestigatieRecomandataScrisoareDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading investigatii imagistice for consultatie {ConsultatieId}", consultatieId);
            return new List<InvestigatieRecomandataScrisoareDto>();
        }
    }

    /// <summary>
    /// Încarcă explorările funcționale recomandate pentru o consultație
    /// </summary>
    private async Task<List<InvestigatieRecomandataScrisoareDto>> LoadExplorariAsync(
        Guid consultatieId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetExplorariRecomandateByConsultatieQuery(consultatieId);
            var result = await _mediator.Send(query, cancellationToken);
            
            if (result.IsSuccess && result.Value != null)
            {
                return result.Value.Select(i => new InvestigatieRecomandataScrisoareDto
                {
                    Denumire = i.DenumireExplorare,
                    Cod = i.CodExplorare,
                    Categorie = "Explorări Funcționale",
                    Prioritate = i.Prioritate,
                    EsteCito = i.EsteCito,
                    IndicatiiClinice = i.IndicatiiClinice,
                    Observatii = i.ObservatiiMedic
                }).ToList();
            }
            
            _logger.LogWarning("Failed to load explorari for consultatie {ConsultatieId}: {Error}", 
                consultatieId, result.FirstError);
            return new List<InvestigatieRecomandataScrisoareDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading explorari for consultatie {ConsultatieId}", consultatieId);
            return new List<InvestigatieRecomandataScrisoareDto>();
        }
    }

    /// <summary>
    /// Încarcă endoscopiile recomandate pentru o consultație
    /// </summary>
    private async Task<List<InvestigatieRecomandataScrisoareDto>> LoadEndoscopiiAsync(
        Guid consultatieId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetEndoscopiiRecomandateByConsultatieQuery(consultatieId);
            var result = await _mediator.Send(query, cancellationToken);
            
            if (result.IsSuccess && result.Value != null)
            {
                return result.Value.Select(i => new InvestigatieRecomandataScrisoareDto
                {
                    Denumire = i.DenumireEndoscopie,
                    Cod = i.CodEndoscopie,
                    Categorie = "Endoscopii",
                    Prioritate = i.Prioritate,
                    EsteCito = i.EsteCito,
                    IndicatiiClinice = i.IndicatiiClinice,
                    Observatii = i.ObservatiiMedic
                }).ToList();
            }
            
            _logger.LogWarning("Failed to load endoscopii for consultatie {ConsultatieId}: {Error}", 
                consultatieId, result.FirstError);
            return new List<InvestigatieRecomandataScrisoareDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading endoscopii for consultatie {ConsultatieId}", consultatieId);
            return new List<InvestigatieRecomandataScrisoareDto>();
        }
    }

    /// <summary>
    /// Încarcă investigațiile imagistice efectuate pentru o consultație
    /// </summary>
    private async Task<List<InvestigatieEfectuataScrisoareDto>> LoadInvestigatiiImagisticeEfectuateAsync(
        Guid consultatieId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetInvestigatiiImagisticeEfectuateByConsultatieQuery(consultatieId);
            var result = await _mediator.Send(query, cancellationToken);
            
            if (result.IsSuccess && result.Value != null)
            {
                return result.Value.Select(i => new InvestigatieEfectuataScrisoareDto
                {
                    Denumire = i.DenumireInvestigatie,
                    Cod = i.CodInvestigatie,
                    Categorie = "Imagistică",
                    DataEfectuare = i.DataEfectuare,
                    Laborator = i.CentrulMedical,
                    MedicExecutant = i.MedicExecutant,
                    Rezultat = i.Rezultat,
                    Concluzie = i.Concluzii,
                    EsteAnormal = false,
                    Observatii = null
                }).ToList();
            }
            
            _logger.LogWarning("Failed to load investigatii imagistice efectuate for consultatie {ConsultatieId}: {Error}", 
                consultatieId, result.FirstError);
            return new List<InvestigatieEfectuataScrisoareDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading investigatii imagistice efectuate for consultatie {ConsultatieId}", consultatieId);
            return new List<InvestigatieEfectuataScrisoareDto>();
        }
    }

    /// <summary>
    /// Încarcă explorările funcționale efectuate pentru o consultație
    /// </summary>
    private async Task<List<InvestigatieEfectuataScrisoareDto>> LoadExplorariEfectuateAsync(
        Guid consultatieId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetExplorariEfectuateByConsultatieQuery(consultatieId);
            var result = await _mediator.Send(query, cancellationToken);
            
            if (result.IsSuccess && result.Value != null)
            {
                return result.Value.Select(i => new InvestigatieEfectuataScrisoareDto
                {
                    Denumire = i.DenumireExplorare,
                    Cod = null,
                    Categorie = "Explorări Funcționale",
                    DataEfectuare = i.DataEfectuare,
                    Laborator = i.CentrulMedical,
                    MedicExecutant = i.MedicExecutant,
                    Rezultat = i.Rezultat,
                    Concluzie = i.Concluzii,
                    EsteAnormal = false,
                    Observatii = null
                }).ToList();
            }
            
            _logger.LogWarning("Failed to load explorari efectuate for consultatie {ConsultatieId}: {Error}", 
                consultatieId, result.FirstError);
            return new List<InvestigatieEfectuataScrisoareDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading explorari efectuate for consultatie {ConsultatieId}", consultatieId);
            return new List<InvestigatieEfectuataScrisoareDto>();
        }
    }

    /// <summary>
    /// Încarcă endoscopiile efectuate pentru o consultație
    /// </summary>
    private async Task<List<InvestigatieEfectuataScrisoareDto>> LoadEndoscopiiEfectuateAsync(
        Guid consultatieId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetEndoscopiiEfectuateByConsultatieQuery(consultatieId);
            var result = await _mediator.Send(query, cancellationToken);
            
            if (result.IsSuccess && result.Value != null)
            {
                return result.Value.Select(i => new InvestigatieEfectuataScrisoareDto
                {
                    Denumire = i.DenumireEndoscopie,
                    Cod = null,
                    Categorie = "Endoscopii",
                    DataEfectuare = i.DataEfectuare,
                    Laborator = i.CentrulMedical,
                    MedicExecutant = i.MedicExecutant,
                    Rezultat = i.Rezultat,
                    Concluzie = i.Concluzii,
                    EsteAnormal = false,
                    Observatii = null
                }).ToList();
            }
            
            _logger.LogWarning("Failed to load endoscopii efectuate for consultatie {ConsultatieId}: {Error}", 
                consultatieId, result.FirstError);
            return new List<InvestigatieEfectuataScrisoareDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading endoscopii efectuate for consultatie {ConsultatieId}", consultatieId);
            return new List<InvestigatieEfectuataScrisoareDto>();
        }
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

    /// <summary>
    /// Parse diagnostic principal using NEW normalized fields with fallback to legacy
    /// </summary>
    private static DiagnosticScrisoareDto? ParseDiagnosticPrincipal(
        string? codIcd10Principal, 
        string? numeDiagnosticPrincipal, 
        string? descriereDetaliataPrincipal,
        string? legacyDiagnostic, 
        string? legacyCodIcd10)
    {
        // Try NEW fields first
        if (!string.IsNullOrWhiteSpace(codIcd10Principal) || !string.IsNullOrWhiteSpace(numeDiagnosticPrincipal))
        {
            return new DiagnosticScrisoareDto
            {
                CodICD10 = codIcd10Principal ?? "",
                Denumire = numeDiagnosticPrincipal ?? "",
                Detalii = StripHtml(descriereDetaliataPrincipal),
                EstePrincipal = true
            };
        }
        
        // Fallback to LEGACY fields
        if (string.IsNullOrWhiteSpace(legacyDiagnostic) && string.IsNullOrWhiteSpace(legacyCodIcd10))
            return null;

        return new DiagnosticScrisoareDto
        {
            CodICD10 = legacyCodIcd10 ?? "",
            Denumire = legacyDiagnostic ?? "",
            EstePrincipal = true
        };
    }

    /// <summary>
    /// Maps normalized DiagnosticeSecundare from database to ScrisoareMedicala DTOs
    /// </summary>
    private static List<DiagnosticScrisoareDto> MapDiagnosticeSecundare(List<DiagnosticSecundarDetailDto>? diagnostice)
    {
        if (diagnostice == null || !diagnostice.Any())
            return new List<DiagnosticScrisoareDto>();

        return diagnostice
            .Select(d => new DiagnosticScrisoareDto
            {
                CodICD10 = d.CodICD10 ?? "",
                Denumire = d.NumeDiagnostic ?? "",
                Detalii = StripHtml(d.Descriere),
                EstePrincipal = false
            })
            .ToList();
    }

    /// <summary>
    /// Maps MedicationList from consultation to TratamentRecomandat for Scrisoare Medicala
    /// </summary>
    private static List<MedicamentScrisoareDto> MapTratamentRecomandat(
        List<ValyanClinic.Application.Features.ConsultatieManagement.DTOs.MedicationRowDto>? medications)
    {
        if (medications == null || !medications.Any())
            return new List<MedicamentScrisoareDto>();

        return medications
            .Where(m => !string.IsNullOrWhiteSpace(m.Name))
            .Select(m => new MedicamentScrisoareDto
            {
                Denumire = m.Name,
                Doza = m.Dose ?? "",
                Frecventa = m.Frequency ?? "",
                Durata = m.Duration ?? "",
                Observatii = !string.IsNullOrWhiteSpace(m.Notes) ? m.Notes : null
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

    /// <summary>
    /// Elimină complet toate tag-urile HTML, returnând doar textul
    /// </summary>
    private static string? StripHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;
        
        // Elimină toate tag-urile HTML
        var result = Regex.Replace(html, @"<[^>]*>", string.Empty, RegexOptions.IgnoreCase);
        
        // Decodează entitățile HTML comune
        result = result.Replace("&nbsp;", " ");
        result = result.Replace("&amp;", "&");
        result = result.Replace("&lt;", "<");
        result = result.Replace("&gt;", ">");
        result = result.Replace("&quot;", "\"");
        
        // Curăță spațiile multiple și liniile noi multiple
        result = Regex.Replace(result, @"\s+", " ");
        
        return result.Trim();
    }

    /// <summary>
    /// Sanitizează HTML-ul păstrând tag-urile de formatare (bold, italic, underline)
    /// și listele (bullet list, numbered list)
    /// Folosit pentru afișarea în Scrisoarea Medicală cu formatare
    /// </summary>
    private static string? SanitizeHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return html;

        // Tag-uri permise: formatare + liste (vor fi păstrate)
        // strong, b, em, i, u, s, sub, sup, mark, ul, ol, li
        
        // Înlocuiește <br>, <br/>, <br /> cu <br/>
        var result = Regex.Replace(html, @"<br\s*/?>", "<br/>", RegexOptions.IgnoreCase);
        
        // Înlocuiește </p>, </div> cu <br/> pentru a păstra separarea (dar NU </li>)
        result = Regex.Replace(result, @"</(?:p|div)>", "<br/>", RegexOptions.IgnoreCase);
        
        // Elimină tag-urile de deschidere pentru p, div, span (dar păstrează ul, ol, li)
        result = Regex.Replace(result, @"<(?:p|div|span)[^>]*>", string.Empty, RegexOptions.IgnoreCase);
        
        // Elimină tag-urile de închidere pentru span
        result = Regex.Replace(result, @"</span>", string.Empty, RegexOptions.IgnoreCase);
        
        // Decodează entitățile HTML comune
        result = result.Replace("&nbsp;", " ");
        
        // Curăță <br/> multiple consecutive
        result = Regex.Replace(result, @"(<br\s*/?>\s*){3,}", "<br/><br/>", RegexOptions.IgnoreCase);
        
        // Elimină <br/> de la început și sfârșit
        result = Regex.Replace(result, @"^(\s*<br\s*/?>\s*)+", string.Empty, RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"(\s*<br\s*/?>\s*)+$", string.Empty, RegexOptions.IgnoreCase);
        
        return result.Trim();
    }

    #endregion
}
