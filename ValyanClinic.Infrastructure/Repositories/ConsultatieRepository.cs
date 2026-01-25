using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Data;
using ValyanClinic.Infrastructure.Data;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository pentru gestionarea consultatiilor medicale - DOAR STORED PROCEDURES.
/// Implementează cele 4 interfețe segregate conform Interface Segregation Principle.
/// </summary>
public class ConsultatieRepository :
    IConsultatieRepository,           // Legacy - pentru backward compatibility
    IConsultatieBaseRepository,       // CRUD basic
    IConsultatieDraftRepository,      // Draft operations
    IConsultatieDetailsRepository,    // Sections (Motive, Antecedente, etc.)
    IConsultatieClinicalDataRepository // Diagnostic + Treatment
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ConsultatieRepository> _logger;

    public ConsultatieRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<ConsultatieRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(Consultatie consultatie, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            // NORMALIZED STRUCTURE: Only master table columns
            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatie.ConsultatieID, DbType.Guid, ParameterDirection.InputOutput);
            parameters.Add("@ProgramareID", consultatie.ProgramareID);
            parameters.Add("@PacientID", consultatie.PacientID);
            parameters.Add("@MedicID", consultatie.MedicID);
            parameters.Add("@DataConsultatie", consultatie.DataConsultatie);
            parameters.Add("@OraConsultatie", consultatie.OraConsultatie);
            parameters.Add("@TipConsultatie", consultatie.TipConsultatie);
            parameters.Add("@Status", consultatie.Status ?? "In desfasurare");
            parameters.Add("@CreatDe", consultatie.CreatDe);

            await connection.ExecuteAsync(
                "sp_Consultatie_Create",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            var consultatieId = parameters.Get<Guid>("@ConsultatieID");

            _logger.LogInformation("[ConsultatieRepository] Consultatie created: {ConsultatieID}", consultatieId);

            return consultatieId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error creating consultatie");
            throw;
        }
    }

    public async Task<Consultatie?> GetByIdAsync(Guid consultatieId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new { ConsultatieID = consultatieId };

            var result = await connection.QueryFirstOrDefaultAsync<Consultatie>(
                "sp_Consultatie_GetById",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error getting consultatie by ID: {ConsultatieID}", consultatieId);
            throw;
        }
    }

    public async Task<IEnumerable<Consultatie>> GetByPacientIdAsync(Guid pacientId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new { PacientID = pacientId };

            var results = await connection.QueryAsync<Consultatie>(
                "sp_Consultatie_GetByPacient",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error getting consultatii by pacient: {PacientID}", pacientId);
            throw;
        }
    }

    public async Task<IEnumerable<Consultatie>> GetByMedicIdAsync(Guid medicId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new { MedicID = medicId };

            var results = await connection.QueryAsync<Consultatie>(
                "sp_Consultatie_GetByMedic",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error getting consultatii by medic: {MedicID}", medicId);
            throw;
        }
    }

    public async Task<Consultatie?> GetByProgramareIdAsync(Guid programareId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            // ✅ Reuse sp_Consultatie_GetDraftByPacient with ProgramareID filter
            // We need to provide a dummy PacientID, but the SP will filter by ProgramareID first
            var parameters = new DynamicParameters();
            parameters.Add("@PacientID", Guid.Empty); // SP will ignore this when ProgramareID is provided
            parameters.Add("@MedicID", (Guid?)null);
            parameters.Add("@DataConsultatie", DateTime.Today);
            parameters.Add("@ProgramareID", programareId);

            // Use same logic as GetDraftByPacientAsync to load all navigation properties
            var result = await connection.QueryAsync<dynamic>(
                "sp_Consultatie_GetDraftByPacient",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var row = result.FirstOrDefault();
            if (row == null)
            {
                _logger.LogInformation(
                    "[ConsultatieRepository] No consultatie found for ProgramareID: {ProgramareID}",
                    programareId);
                return null;
            }

            // DEBUG: Log what we got from SP
            try
            {
                var dict = (IDictionary<string, object>)row;
                _logger.LogInformation(
                    "[ConsultatieRepository] GetByProgramareIdAsync - SP returned {ColumnCount} columns. Sample: MotivePrezentare_Id={MotivId}, MotivPrezentare_MotivPrezentare={Motiv}, Antecedente_Id={AntId}, Antecedente_APP_Medicatie={Medicatie}",
                    dict.Count,
                    dict.ContainsKey("MotivePrezentare_Id") ? dict["MotivePrezentare_Id"] : "MISSING",
                    dict.ContainsKey("MotivePrezentare_MotivPrezentare") ? dict["MotivePrezentare_MotivPrezentare"] : "MISSING",
                    dict.ContainsKey("Antecedente_Id") ? dict["Antecedente_Id"] : "MISSING",
                    dict.ContainsKey("Antecedente_APP_Medicatie") ? dict["Antecedente_APP_Medicatie"] : "MISSING"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ConsultatieRepository] Failed to log dynamic object properties");
            }

            // Map master entity
            var consultatie = new Consultatie
            {
                ConsultatieID = row.ConsultatieID,
                ProgramareID = row.ProgramareID,
                PacientID = row.PacientID,
                MedicID = row.MedicID,
                DataConsultatie = row.DataConsultatie,
                OraConsultatie = row.OraConsultatie,
                TipConsultatie = row.TipConsultatie,
                Status = row.Status,
                DataFinalizare = row.DataFinalizare,
                DurataMinute = row.DurataMinute,
                DataCreare = row.DataCreare,
                CreatDe = row.CreatDe,
                DataUltimeiModificari = row.DataUltimeiModificari,
                ModificatDe = row.ModificatDe
            };

            // Map ConsultatieMotivePrezentare
            if (row.MotivePrezentare_Id != null)
            {
                consultatie.MotivePrezentare = new ConsultatieMotivePrezentare
                {
                    Id = row.MotivePrezentare_Id,
                    ConsultatieID = consultatie.ConsultatieID,
                    MotivPrezentare = row.MotivePrezentare_MotivPrezentare,
                    IstoricBoalaActuala = row.MotivePrezentare_IstoricBoalaActuala,
                    DataCreare = row.MotivePrezentare_DataCreare,
                    CreatDe = row.MotivePrezentare_CreatDe,
                    DataUltimeiModificari = row.MotivePrezentare_DataUltimeiModificari,
                    ModificatDe = row.MotivePrezentare_ModificatDe
                };
            }

            // Map ConsultatieAntecedente (Simplified + Anexa 43)
            if (row.Antecedente_Id != null)
            {
                consultatie.Antecedente = new ConsultatieAntecedente
                {
                    Id = row.Antecedente_Id,
                    ConsultatieID = consultatie.ConsultatieID,
                    IstoricMedicalPersonal = row.Antecedente_IstoricMedicalPersonal,
                    IstoricFamilial = row.Antecedente_IstoricFamilial,
                    TratamentAnterior = row.Antecedente_TratamentAnterior,
                    FactoriDeRisc = row.Antecedente_FactoriDeRisc,
                    Alergii = row.Antecedente_Alergii,
                    DataCreare = row.Antecedente_DataCreare,
                    CreatDe = row.Antecedente_CreatDe,
                    DataUltimeiModificari = row.Antecedente_DataUltimeiModificari,
                    ModificatDe = row.Antecedente_ModificatDe
                };
            }

            // Map ConsultatieExamenObiectiv
            if (row.ExamenObiectiv_Id != null)
            {
                consultatie.ExamenObiectiv = new ConsultatieExamenObiectiv
                {
                    Id = row.ExamenObiectiv_Id,
                    ConsultatieID = consultatie.ConsultatieID,
                    StareGenerala = row.ExamenObiectiv_StareGenerala,
                    Tegumente = row.ExamenObiectiv_Tegumente,
                    Mucoase = row.ExamenObiectiv_Mucoase,
                    Edeme = row.ExamenObiectiv_Edeme,
                    Greutate = row.ExamenObiectiv_Greutate,
                    Inaltime = row.ExamenObiectiv_Inaltime,
                    IMC = row.ExamenObiectiv_IMC,
                    Temperatura = row.ExamenObiectiv_Temperatura,
                    TensiuneArteriala = row.ExamenObiectiv_TensiuneArteriala,
                    Puls = row.ExamenObiectiv_Puls,
                    FreccventaRespiratorie = row.ExamenObiectiv_FreccventaRespiratorie,
                    SaturatieO2 = row.ExamenObiectiv_SaturatieO2,
                    Glicemie = row.ExamenObiectiv_Glicemie,
                    GanglioniLimfatici = row.ExamenObiectiv_GanglioniLimfatici,
                    ExamenObiectivDetaliat = row.ExamenObiectiv_ExamenObiectivDetaliat,
                    AlteObservatiiClinice = row.ExamenObiectiv_AlteObservatiiClinice,
                    DataCreare = row.ExamenObiectiv_DataCreare,
                    CreatDe = row.ExamenObiectiv_CreatDe,
                    DataUltimeiModificari = row.ExamenObiectiv_DataUltimeiModificari,
                    ModificatDe = row.ExamenObiectiv_ModificatDe
                };
            }

            // Map ConsultatieInvestigatii
            if (row.Investigatii_Id != null)
            {
                consultatie.Investigatii = new ConsultatieInvestigatii
                {
                    Id = row.Investigatii_Id,
                    ConsultatieID = consultatie.ConsultatieID,
                    InvestigatiiLaborator = row.Investigatii_InvestigatiiLaborator,
                    InvestigatiiImagistice = row.Investigatii_InvestigatiiImagistice,
                    InvestigatiiEKG = row.Investigatii_InvestigatiiEKG,
                    AlteInvestigatii = row.Investigatii_AlteInvestigatii,
                    DataCreare = row.Investigatii_DataCreare,
                    CreatDe = row.Investigatii_CreatDe,
                    DataUltimeiModificari = row.Investigatii_DataUltimeiModificari,
                    ModificatDe = row.Investigatii_ModificatDe
                };
            }

            // Map ConsultatieDiagnostic
            if (row.Diagnostic_Id != null)
            {
                consultatie.Diagnostic = new ConsultatieDiagnostic
                {
                    Id = row.Diagnostic_Id,
                    ConsultatieID = consultatie.ConsultatieID,
                    // Normalized structure
                    CodICD10Principal = row.Diagnostic_CodICD10Principal,
                    NumeDiagnosticPrincipal = row.Diagnostic_NumeDiagnosticPrincipal,
                    DescriereDetaliataPrincipal = row.Diagnostic_DescriereDetaliataPrincipal,
                    // Legacy fields
                    DiagnosticPozitiv = row.Diagnostic_DiagnosticPozitiv,
                    CoduriICD10 = row.Diagnostic_CoduriICD10,
                    DataCreare = row.Diagnostic_DataCreare,
                    CreatDe = row.Diagnostic_CreatDe,
                    DataUltimeiModificari = row.Diagnostic_DataUltimeiModificari,
                    ModificatDe = row.Diagnostic_ModificatDe
                };
            }

            // Map ConsultatieTratament
            if (row.Tratament_Id != null)
            {
                consultatie.Tratament = new ConsultatieTratament
                {
                    Id = row.Tratament_Id,
                    ConsultatieID = consultatie.ConsultatieID,
                    TratamentMedicamentos = row.Tratament_TratamentMedicamentos,
                    TratamentNemedicamentos = row.Tratament_TratamentNemedicamentos,
                    RecomandariDietetice = row.Tratament_RecomandariDietetice,
                    RecomandariRegimViata = row.Tratament_RecomandariRegimViata,
                    InvestigatiiRecomandate = row.Tratament_InvestigatiiRecomandate,
                    ConsulturiSpecialitate = row.Tratament_ConsulturiSpecialitate,
                    DataUrmatoareiProgramari = row.Tratament_DataUrmatoareiProgramari,
                    RecomandariSupraveghere = row.Tratament_RecomandariSupraveghere,
                    DataCreare = row.Tratament_DataCreare,
                    CreatDe = row.Tratament_CreatDe,
                    DataUltimeiModificari = row.Tratament_DataUltimeiModificari,
                    ModificatDe = row.Tratament_ModificatDe
                };
            }

            // Map ConsultatieConcluzii
            if (row.Concluzii_Id != null)
            {
                consultatie.Concluzii = new ConsultatieConcluzii
                {
                    Id = row.Concluzii_Id,
                    ConsultatieID = consultatie.ConsultatieID,
                    Prognostic = row.Concluzii_Prognostic,
                    Concluzie = row.Concluzii_Concluzie,
                    ObservatiiMedic = row.Concluzii_ObservatiiMedic,
                    NotePacient = row.Concluzii_NotePacient,
                    DocumenteAtatate = row.Concluzii_DocumenteAtatate,
                    // Scrisoare Medicala - Anexa 43
                    EsteAfectiuneOncologica = row.Concluzii_EsteAfectiuneOncologica ?? false,
                    DetaliiAfectiuneOncologica = row.Concluzii_DetaliiAfectiuneOncologica,
                    AreIndicatieInternare = row.Concluzii_AreIndicatieInternare ?? false,
                    TermenInternare = row.Concluzii_TermenInternare,
                    SaEliberatPrescriptie = row.Concluzii_SaEliberatPrescriptie,
                    SeriePrescriptie = row.Concluzii_SeriePrescriptie,
                    SaEliberatConcediuMedical = row.Concluzii_SaEliberatConcediuMedical,
                    SerieConcediuMedical = row.Concluzii_SerieConcediuMedical,
                    SaEliberatIngrijiriDomiciliu = row.Concluzii_SaEliberatIngrijiriDomiciliu,
                    SaEliberatDispozitiveMedicale = row.Concluzii_SaEliberatDispozitiveMedicale,
                    TransmiterePrinEmail = row.Concluzii_TransmiterePrinEmail ?? false,
                    EmailTransmitere = row.Concluzii_EmailTransmitere,
                    DataCreare = row.Concluzii_DataCreare,
                    CreatDe = row.Concluzii_CreatDe,
                    DataUltimeiModificari = row.Concluzii_DataUltimeiModificari,
                    ModificatDe = row.Concluzii_ModificatDe
                };
            }

            // ✅ Load secondary diagnoses separately (1:N relationship)
            if (consultatie.Diagnostic != null)
            {
                var diagnosticeSecundare = await GetDiagnosticeSecundareAsync(consultatie.ConsultatieID, cancellationToken);
                consultatie.Diagnostic.DiagnosticeSecundare = diagnosticeSecundare.ToList();
                
                _logger.LogInformation(
                    "[ConsultatieRepository] GetByProgramareIdAsync - Loaded {Count} secondary diagnoses for ConsultatieID: {ConsultatieID}",
                    consultatie.Diagnostic.DiagnosticeSecundare.Count, consultatie.ConsultatieID);
            }

            // ✅ Load medications separately (1:N relationship)
            var medicamente = await GetMedicamenteAsync(consultatie.ConsultatieID, cancellationToken);
            consultatie.Medicamente = medicamente.ToList();
            
            _logger.LogInformation(
                "[ConsultatieRepository] GetByProgramareIdAsync - Loaded {Count} medications for ConsultatieID: {ConsultatieID}",
                consultatie.Medicamente.Count, consultatie.ConsultatieID);

            var populatedSections = new object?[] { 
                consultatie.MotivePrezentare, consultatie.Antecedente, consultatie.ExamenObiectiv, 
                consultatie.Investigatii, consultatie.Diagnostic, consultatie.Tratament, consultatie.Concluzii 
            }.Count(s => s != null);

            _logger.LogInformation(
                "[ConsultatieRepository] GetByProgramareIdAsync RESULT - ConsultatieID: {ConsultatieID}, {Sections} populated sections. MotivePrezentare={HasMotiv} (Motiv='{Motiv}'), Antecedente={HasAnt}",
                consultatie.ConsultatieID, 
                populatedSections,
                consultatie.MotivePrezentare != null,
                consultatie.MotivePrezentare?.MotivPrezentare ?? "NULL",
                consultatie.Antecedente != null);

            return consultatie;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error getting consultatie by programare: {ProgramareID}", programareId);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Consultatie consultatie, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            // NORMALIZED STRUCTURE: Only master table columns
            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatie.ConsultatieID);
            parameters.Add("@Status", consultatie.Status);
            parameters.Add("@DataFinalizare", consultatie.DataFinalizare);
            parameters.Add("@DurataMinute", consultatie.DurataMinute);
            parameters.Add("@ModificatDe", consultatie.ModificatDe);

            var result = await connection.ExecuteAsync(
                "sp_Consultatie_Update",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            _logger.LogInformation("[ConsultatieRepository] Consultatie updated: {ConsultatieID}", consultatie.ConsultatieID);

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error updating consultatie: {ConsultatieID}", consultatie.ConsultatieID);
            throw;
        }
    }

    public async Task<Guid> SaveDraftAsync(Consultatie consultatie, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            // ✅ NORMALIZED STRUCTURE: Only 14 master columns (no denormalized data)
            // Denormalized data is saved via Upsert methods in Handler
            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatie.ConsultatieID, DbType.Guid, ParameterDirection.InputOutput);
            parameters.Add("@ProgramareID", consultatie.ProgramareID);
            parameters.Add("@PacientID", consultatie.PacientID);
            parameters.Add("@MedicID", consultatie.MedicID);
            parameters.Add("@DataConsultatie", consultatie.DataConsultatie);
            parameters.Add("@OraConsultatie", consultatie.OraConsultatie);
            parameters.Add("@TipConsultatie", consultatie.TipConsultatie);
            parameters.Add("@DurataMinute", consultatie.DurataMinute);

            // Audit - use CreatDe for both create and update in draft mode
            var userId = consultatie.ModificatDe ?? consultatie.CreatDe;
            parameters.Add("@CreatDeSauModificatDe", userId);

            await connection.ExecuteAsync(
                "sp_Consultatie_SaveDraft",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            var consultatieId = parameters.Get<Guid>("@ConsultatieID");

            _logger.LogInformation("[ConsultatieRepository] Consultatie draft saved (master only): {ConsultatieID}", consultatieId);

            return consultatieId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error saving consultatie draft: {ConsultatieID}", consultatie.ConsultatieID);
            throw;
        }
    }

    public async Task<bool> FinalizeAsync(Guid consultatieId, int durataMinute, Guid modificatDe, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatieId);
            parameters.Add("@DurataMinute", durataMinute);
            parameters.Add("@ModificatDe", modificatDe);

            var result = await connection.ExecuteAsync(
                "sp_Consultatie_Finalize",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            _logger.LogInformation("[ConsultatieRepository] Consultatie finalized: {ConsultatieID}, Duration: {DurataMinute} minutes", 
                consultatieId, durataMinute);

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error finalizing consultatie: {ConsultatieID}", consultatieId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid consultatieId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new { ConsultatieID = consultatieId };

            var result = await connection.ExecuteAsync(
                "sp_Consultatie_Delete",
                parameters,
                commandType: CommandType.StoredProcedure);

            _logger.LogInformation("[ConsultatieRepository] Consultatie deleted: {ConsultatieID}", consultatieId);

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error deleting consultatie: {ConsultatieID}", consultatieId);
            throw;
        }
    }

    public async Task<Consultatie?> GetDraftByPacientAsync(
        Guid pacientId,
        Guid? medicId = null,
        DateTime? dataConsultatie = null,
        Guid? programareId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PacientID", pacientId);
            parameters.Add("@MedicID", medicId);
            parameters.Add("@DataConsultatie", dataConsultatie ?? DateTime.Today);
            parameters.Add("@ProgramareID", programareId);

            // ✅ Use SP with Multi-Mapping to load all navigation properties
            var result = await connection.QueryAsync<dynamic>(
                "sp_Consultatie_GetDraftByPacient",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var row = result.FirstOrDefault();
            if (row == null)
            {
                _logger.LogInformation(
                    "[ConsultatieRepository] No draft consultatie found for PacientID: {PacientID}, DataConsultatie: {Data}",
                    pacientId, dataConsultatie ?? DateTime.Today);
                return null;
            }

            // Map master entity
            var consultatie = new Consultatie
            {
                ConsultatieID = row.ConsultatieID,
                ProgramareID = row.ProgramareID,
                PacientID = row.PacientID,
                MedicID = row.MedicID,
                DataConsultatie = row.DataConsultatie,
                OraConsultatie = row.OraConsultatie,
                TipConsultatie = row.TipConsultatie,
                Status = row.Status,
                DataFinalizare = row.DataFinalizare,
                DurataMinute = row.DurataMinute,
                DataCreare = row.DataCreare,
                CreatDe = row.CreatDe,
                DataUltimeiModificari = row.DataUltimeiModificari,
                ModificatDe = row.ModificatDe
            };

            // Map ConsultatieMotivePrezentare
            if (row.MotivePrezentare_Id != null)
            {
                consultatie.MotivePrezentare = new ConsultatieMotivePrezentare
                {
                    Id = row.MotivePrezentare_Id,
                    ConsultatieID = consultatie.ConsultatieID,
                    MotivPrezentare = row.MotivePrezentare_MotivPrezentare,
                    IstoricBoalaActuala = row.MotivePrezentare_IstoricBoalaActuala,
                    DataCreare = row.MotivePrezentare_DataCreare,
                    CreatDe = row.MotivePrezentare_CreatDe,
                    DataUltimeiModificari = row.MotivePrezentare_DataUltimeiModificari,
                    ModificatDe = row.MotivePrezentare_ModificatDe
                };
            }

            // Map ConsultatieAntecedente (Simplified + Anexa 43)
            if (row.Antecedente_Id != null)
            {
                consultatie.Antecedente = new ConsultatieAntecedente
                {
                    Id = row.Antecedente_Id,
                    ConsultatieID = consultatie.ConsultatieID,
                    IstoricMedicalPersonal = row.Antecedente_IstoricMedicalPersonal,
                    IstoricFamilial = row.Antecedente_IstoricFamilial,
                    TratamentAnterior = row.Antecedente_TratamentAnterior,
                    FactoriDeRisc = row.Antecedente_FactoriDeRisc,
                    Alergii = row.Antecedente_Alergii,
                    DataCreare = row.Antecedente_DataCreare,
                    CreatDe = row.Antecedente_CreatDe,
                    DataUltimeiModificari = row.Antecedente_DataUltimeiModificari,
                    ModificatDe = row.Antecedente_ModificatDe
                };
            }

            // Map ConsultatieExamenObiectiv
            if (row.ExamenObiectiv_Id != null)
            {
                consultatie.ExamenObiectiv = new ConsultatieExamenObiectiv
                {
                    Id = row.ExamenObiectiv_Id,
                    ConsultatieID = consultatie.ConsultatieID,
                    StareGenerala = row.ExamenObiectiv_StareGenerala,
                    Tegumente = row.ExamenObiectiv_Tegumente,
                    Mucoase = row.ExamenObiectiv_Mucoase,
                    Edeme = row.ExamenObiectiv_Edeme,
                    Greutate = row.ExamenObiectiv_Greutate,
                    Inaltime = row.ExamenObiectiv_Inaltime,
                    IMC = row.ExamenObiectiv_IMC,
                    Temperatura = row.ExamenObiectiv_Temperatura,
                    TensiuneArteriala = row.ExamenObiectiv_TensiuneArteriala,
                    Puls = row.ExamenObiectiv_Puls,
                    FreccventaRespiratorie = row.ExamenObiectiv_FreccventaRespiratorie,
                    SaturatieO2 = row.ExamenObiectiv_SaturatieO2,
                    Glicemie = row.ExamenObiectiv_Glicemie,
                    GanglioniLimfatici = row.ExamenObiectiv_GanglioniLimfatici,
                    ExamenObiectivDetaliat = row.ExamenObiectiv_ExamenObiectivDetaliat,
                    AlteObservatiiClinice = row.ExamenObiectiv_AlteObservatiiClinice,
                    DataCreare = row.ExamenObiectiv_DataCreare,
                    CreatDe = row.ExamenObiectiv_CreatDe,
                    DataUltimeiModificari = row.ExamenObiectiv_DataUltimeiModificari,
                    ModificatDe = row.ExamenObiectiv_ModificatDe
                };
            }

            // Map ConsultatieInvestigatii
            if (row.Investigatii_Id != null)
            {
                consultatie.Investigatii = new ConsultatieInvestigatii
                {
                    Id = row.Investigatii_Id,
                    ConsultatieID = consultatie.ConsultatieID,
                    InvestigatiiLaborator = row.Investigatii_InvestigatiiLaborator,
                    InvestigatiiImagistice = row.Investigatii_InvestigatiiImagistice,
                    InvestigatiiEKG = row.Investigatii_InvestigatiiEKG,
                    AlteInvestigatii = row.Investigatii_AlteInvestigatii,
                    DataCreare = row.Investigatii_DataCreare,
                    CreatDe = row.Investigatii_CreatDe,
                    DataUltimeiModificari = row.Investigatii_DataUltimeiModificari,
                    ModificatDe = row.Investigatii_ModificatDe
                };
            }

            // Map ConsultatieDiagnostic
            if (row.Diagnostic_Id != null)
            {
                consultatie.Diagnostic = new ConsultatieDiagnostic
                {
                    Id = row.Diagnostic_Id,
                    ConsultatieID = consultatie.ConsultatieID,
                    // Normalized structure
                    CodICD10Principal = row.Diagnostic_CodICD10Principal,
                    NumeDiagnosticPrincipal = row.Diagnostic_NumeDiagnosticPrincipal,
                    DescriereDetaliataPrincipal = row.Diagnostic_DescriereDetaliataPrincipal,
                    // Legacy fields
                    DiagnosticPozitiv = row.Diagnostic_DiagnosticPozitiv,
                    CoduriICD10 = row.Diagnostic_CoduriICD10,
                    DataCreare = row.Diagnostic_DataCreare,
                    CreatDe = row.Diagnostic_CreatDe,
                    DataUltimeiModificari = row.Diagnostic_DataUltimeiModificari,
                    ModificatDe = row.Diagnostic_ModificatDe
                };
            }

            // Map ConsultatieTratament
            if (row.Tratament_Id != null)
            {
                consultatie.Tratament = new ConsultatieTratament
                {
                    Id = row.Tratament_Id,
                    ConsultatieID = consultatie.ConsultatieID,
                    TratamentMedicamentos = row.Tratament_TratamentMedicamentos,
                    TratamentNemedicamentos = row.Tratament_TratamentNemedicamentos,
                    RecomandariDietetice = row.Tratament_RecomandariDietetice,
                    RecomandariRegimViata = row.Tratament_RecomandariRegimViata,
                    InvestigatiiRecomandate = row.Tratament_InvestigatiiRecomandate,
                    ConsulturiSpecialitate = row.Tratament_ConsulturiSpecialitate,
                    DataUrmatoareiProgramari = row.Tratament_DataUrmatoareiProgramari,
                    RecomandariSupraveghere = row.Tratament_RecomandariSupraveghere,
                    DataCreare = row.Tratament_DataCreare,
                    CreatDe = row.Tratament_CreatDe,
                    DataUltimeiModificari = row.Tratament_DataUltimeiModificari,
                    ModificatDe = row.Tratament_ModificatDe
                };
            }

            // Map ConsultatieConcluzii
            if (row.Concluzii_Id != null)
            {
                consultatie.Concluzii = new ConsultatieConcluzii
                {
                    Id = row.Concluzii_Id,
                    ConsultatieID = consultatie.ConsultatieID,
                    Prognostic = row.Concluzii_Prognostic,
                    Concluzie = row.Concluzii_Concluzie,
                    ObservatiiMedic = row.Concluzii_ObservatiiMedic,
                    NotePacient = row.Concluzii_NotePacient,
                    DocumenteAtatate = row.Concluzii_DocumenteAtatate,
                    // Scrisoare Medicala - Anexa 43
                    EsteAfectiuneOncologica = row.Concluzii_EsteAfectiuneOncologica ?? false,
                    DetaliiAfectiuneOncologica = row.Concluzii_DetaliiAfectiuneOncologica,
                    AreIndicatieInternare = row.Concluzii_AreIndicatieInternare ?? false,
                    TermenInternare = row.Concluzii_TermenInternare,
                    SaEliberatPrescriptie = row.Concluzii_SaEliberatPrescriptie,
                    SeriePrescriptie = row.Concluzii_SeriePrescriptie,
                    SaEliberatConcediuMedical = row.Concluzii_SaEliberatConcediuMedical,
                    SerieConcediuMedical = row.Concluzii_SerieConcediuMedical,
                    SaEliberatIngrijiriDomiciliu = row.Concluzii_SaEliberatIngrijiriDomiciliu,
                    SaEliberatDispozitiveMedicale = row.Concluzii_SaEliberatDispozitiveMedicale,
                    TransmiterePrinEmail = row.Concluzii_TransmiterePrinEmail ?? false,
                    EmailTransmitere = row.Concluzii_EmailTransmitere,
                    DataCreare = row.Concluzii_DataCreare,
                    CreatDe = row.Concluzii_CreatDe,
                    DataUltimeiModificari = row.Concluzii_DataUltimeiModificari,
                    ModificatDe = row.Concluzii_ModificatDe
                };
            }

            // ✅ Load secondary diagnoses separately (1:N relationship)
            if (consultatie.Diagnostic != null)
            {
                var diagnosticeSecundare = await GetDiagnosticeSecundareAsync(consultatie.ConsultatieID, cancellationToken);
                consultatie.Diagnostic.DiagnosticeSecundare = diagnosticeSecundare.ToList();
                
                _logger.LogInformation(
                    "[ConsultatieRepository] Loaded {Count} secondary diagnoses for ConsultatieID: {ConsultatieID}",
                    consultatie.Diagnostic.DiagnosticeSecundare.Count, consultatie.ConsultatieID);
            }

            // ✅ Load medications separately (1:N relationship)
            var medicamente = await GetMedicamenteAsync(consultatie.ConsultatieID, cancellationToken);
            consultatie.Medicamente = medicamente.ToList();
            
            _logger.LogInformation(
                "[ConsultatieRepository] Loaded {Count} medications for ConsultatieID: {ConsultatieID}",
                consultatie.Medicamente.Count, consultatie.ConsultatieID);

            var populatedSections = new object?[] { 
                consultatie.MotivePrezentare, consultatie.Antecedente, consultatie.ExamenObiectiv, 
                consultatie.Investigatii, consultatie.Diagnostic, consultatie.Tratament, consultatie.Concluzii 
            }.Count(s => s != null);

            _logger.LogInformation(
                "[ConsultatieRepository] Draft consultatie found: {ConsultatieID} (Status: {Status}) with {Sections} populated sections",
                consultatie.ConsultatieID, consultatie.Status, populatedSections);

            return consultatie;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "[ConsultatieRepository] Error getting draft consultatie for PacientID: {PacientID}", 
                pacientId);
            throw;
        }
    }

    // ==================== NORMALIZED STRUCTURE UPSERT METHODS ====================

    public async Task UpsertMotivePrezentareAsync(Guid consultatieId, ConsultatieMotivePrezentare entity)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatieId);
            parameters.Add("@MotivPrezentare", entity.MotivPrezentare);
            parameters.Add("@IstoricBoalaActuala", entity.IstoricBoalaActuala);
            parameters.Add("@ModificatDe", entity.ModificatDe ?? entity.CreatDe);

            await connection.ExecuteAsync(
                "ConsultatieMotivePrezentare_Upsert",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            _logger.LogInformation("[ConsultatieRepository] MotivePrezentare upserted for: {ConsultatieID}", consultatieId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error upserting MotivePrezentare for: {ConsultatieID}", consultatieId);
            throw;
        }
    }

    public async Task UpsertAntecedenteAsync(Guid consultatieId, ConsultatieAntecedente entity)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatieId);
            parameters.Add("@IstoricMedicalPersonal", entity.IstoricMedicalPersonal);
            parameters.Add("@IstoricFamilial", entity.IstoricFamilial);
            parameters.Add("@TratamentAnterior", entity.TratamentAnterior);
            parameters.Add("@FactoriDeRisc", entity.FactoriDeRisc);
            parameters.Add("@Alergii", entity.Alergii);
            parameters.Add("@ModificatDe", entity.ModificatDe ?? entity.CreatDe);

            await connection.ExecuteAsync(
                "ConsultatieAntecedente_Upsert",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            _logger.LogInformation("[ConsultatieRepository] Antecedente upserted for: {ConsultatieID}", consultatieId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error upserting Antecedente for: {ConsultatieID}", consultatieId);
            throw;
        }
    }

    public async Task UpsertExamenObiectivAsync(Guid consultatieId, ConsultatieExamenObiectiv entity)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatieId);
            parameters.Add("@StareGenerala", entity.StareGenerala);
            parameters.Add("@Tegumente", entity.Tegumente);
            parameters.Add("@Mucoase", entity.Mucoase);
            parameters.Add("@GanglioniLimfatici", entity.GanglioniLimfatici);
            parameters.Add("@Edeme", entity.Edeme);
            parameters.Add("@Greutate", entity.Greutate);
            parameters.Add("@Inaltime", entity.Inaltime);
            parameters.Add("@IMC", entity.IMC);
            parameters.Add("@Temperatura", entity.Temperatura);
            parameters.Add("@TensiuneArteriala", entity.TensiuneArteriala);
            parameters.Add("@Puls", entity.Puls);
            parameters.Add("@FreccventaRespiratorie", entity.FreccventaRespiratorie);
            parameters.Add("@SaturatieO2", entity.SaturatieO2);
            parameters.Add("@Glicemie", entity.Glicemie);
            parameters.Add("@ExamenObiectivDetaliat", entity.ExamenObiectivDetaliat);
            parameters.Add("@AlteObservatiiClinice", entity.AlteObservatiiClinice);
            parameters.Add("@ModificatDe", entity.ModificatDe ?? entity.CreatDe);

            await connection.ExecuteAsync(
                "ConsultatieExamenObiectiv_Upsert",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            _logger.LogInformation("[ConsultatieRepository] ExamenObiectiv upserted for: {ConsultatieID}", consultatieId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error upserting ExamenObiectiv for: {ConsultatieID}", consultatieId);
            throw;
        }
    }

    public async Task UpsertInvestigatiiAsync(Guid consultatieId, ConsultatieInvestigatii entity)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatieId);
            parameters.Add("@InvestigatiiLaborator", entity.InvestigatiiLaborator);
            parameters.Add("@InvestigatiiImagistice", entity.InvestigatiiImagistice);
            parameters.Add("@InvestigatiiEKG", entity.InvestigatiiEKG);
            parameters.Add("@AlteInvestigatii", entity.AlteInvestigatii);
            parameters.Add("@ModificatDe", entity.ModificatDe ?? entity.CreatDe);

            await connection.ExecuteAsync(
                "ConsultatieInvestigatii_Upsert",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            _logger.LogInformation("[ConsultatieRepository] Investigatii upserted for: {ConsultatieID}", consultatieId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error upserting Investigatii for: {ConsultatieID}", consultatieId);
            throw;
        }
    }

    public async Task UpsertDiagnosticAsync(Guid consultatieId, ConsultatieDiagnostic entity)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatieId);
            // Diagnostic Principal
            parameters.Add("@CodICD10Principal", entity.CodICD10Principal);
            parameters.Add("@NumeDiagnosticPrincipal", entity.NumeDiagnosticPrincipal);
            parameters.Add("@DescriereDetaliataPrincipal", entity.DescriereDetaliataPrincipal);
            // LEGACY: backwards compatibility
            parameters.Add("@DiagnosticPozitiv", entity.DiagnosticPozitiv);
            parameters.Add("@CoduriICD10", entity.CoduriICD10);
            parameters.Add("@ModificatDe", entity.ModificatDe ?? entity.CreatDe);

            await connection.ExecuteAsync(
                "ConsultatieDiagnostic_Upsert",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            _logger.LogInformation("[ConsultatieRepository] Diagnostic upserted for: {ConsultatieID}", consultatieId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error upserting Diagnostic for: {ConsultatieID}", consultatieId);
            throw;
        }
    }

    public async Task SyncDiagnosticeSecundareAsync(Guid consultatieId, IEnumerable<ConsultatieDiagnosticSecundar> diagnostice, Guid modificatDe)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            // Build JSON array for stored procedure - includes Id for MERGE logic
            var diagnosticeList = diagnostice.Select((d, index) => new
            {
                // Id: null = new diagnostic (INSERT), valid guid = existing (UPDATE)
                id = d.Id == Guid.Empty ? (Guid?)null : d.Id,
                ordine = d.OrdineAfisare > 0 ? d.OrdineAfisare : index + 1,
                codICD10 = d.CodICD10,
                numeDiagnostic = d.NumeDiagnostic,
                descriere = d.Descriere
            }).ToList();

            var json = System.Text.Json.JsonSerializer.Serialize(diagnosticeList);

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatieId);
            parameters.Add("@DiagnosticeJSON", json);
            parameters.Add("@ModificatDe", modificatDe);

            await connection.ExecuteAsync(
                "ConsultatieDiagnosticSecundar_Sync",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            _logger.LogInformation("[ConsultatieRepository] Synced {Count} diagnostice secundare for: {ConsultatieID}", 
                diagnosticeList.Count, consultatieId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error syncing diagnostice secundare for: {ConsultatieID}", consultatieId);
            throw;
        }
    }

    public async Task<IEnumerable<ConsultatieDiagnosticSecundar>> GetDiagnosticeSecundareAsync(Guid consultatieId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatieId);

            var result = await connection.QueryAsync<ConsultatieDiagnosticSecundar>(
                "ConsultatieDiagnosticSecundar_GetByConsultatie",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error getting diagnostice secundare for: {ConsultatieID}", consultatieId);
            throw;
        }
    }

    public async Task UpsertTratamentAsync(Guid consultatieId, ConsultatieTratament entity)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatieId);
            parameters.Add("@TratamentMedicamentos", entity.TratamentMedicamentos);
            parameters.Add("@TratamentNemedicamentos", entity.TratamentNemedicamentos);
            parameters.Add("@RecomandariDietetice", entity.RecomandariDietetice);
            parameters.Add("@RecomandariRegimViata", entity.RecomandariRegimViata);
            parameters.Add("@InvestigatiiRecomandate", entity.InvestigatiiRecomandate);
            parameters.Add("@ConsulturiSpecialitate", entity.ConsulturiSpecialitate);
            parameters.Add("@DataUrmatoareiProgramari", entity.DataUrmatoareiProgramari);
            parameters.Add("@RecomandariSupraveghere", entity.RecomandariSupraveghere);
            parameters.Add("@ModificatDe", entity.ModificatDe ?? entity.CreatDe);

            await connection.ExecuteAsync(
                "ConsultatieTratament_Upsert",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            _logger.LogInformation("[ConsultatieRepository] Tratament upserted for: {ConsultatieID}", consultatieId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error upserting Tratament for: {ConsultatieID}", consultatieId);
            throw;
        }
    }

    public async Task ReplaceMedicamenteAsync(Guid consultatieId, IEnumerable<ConsultatieMedicament> medicamente, Guid modificatDe)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            // Create DataTable for TVP - includes Id for MERGE logic
            var medicamenteTable = new DataTable();
            medicamenteTable.Columns.Add("Id", typeof(Guid));  // NULL = new, NOT NULL = existing
            medicamenteTable.Columns.Add("OrdineAfisare", typeof(int));
            medicamenteTable.Columns.Add("NumeMedicament", typeof(string));
            medicamenteTable.Columns.Add("Doza", typeof(string));
            medicamenteTable.Columns.Add("Frecventa", typeof(string));
            medicamenteTable.Columns.Add("Durata", typeof(string));
            medicamenteTable.Columns.Add("Cantitate", typeof(string));
            medicamenteTable.Columns.Add("Observatii", typeof(string));

            var ordine = 0;
            foreach (var med in medicamente)
            {
                if (!string.IsNullOrWhiteSpace(med.NumeMedicament))
                {
                    // If Id is empty Guid, pass DBNull (treated as new medication)
                    object idValue = med.Id == Guid.Empty ? DBNull.Value : (object)med.Id;
                    
                    medicamenteTable.Rows.Add(
                        idValue,
                        ordine++,
                        med.NumeMedicament,
                        med.Doza ?? (object)DBNull.Value,
                        med.Frecventa ?? (object)DBNull.Value,
                        med.Durata ?? (object)DBNull.Value,
                        med.Cantitate ?? (object)DBNull.Value,
                        med.Observatii ?? (object)DBNull.Value
                    );
                }
            }

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatieId);
            parameters.Add("@ModificatDe", modificatDe);

            // Use SqlCommand for TVP support - Microsoft.Data.SqlClient
            if (connection is SqlConnection sqlConn)
            {
                if (sqlConn.State != ConnectionState.Open)
                    await sqlConn.OpenAsync();
                    
                using var cmd = sqlConn.CreateCommand();
                cmd.CommandText = "ConsultatieMedicament_BulkReplace";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 30;

                cmd.Parameters.AddWithValue("@ConsultatieID", consultatieId);
                cmd.Parameters.AddWithValue("@ModificatDe", modificatDe);
                
                var tvpParam = cmd.Parameters.AddWithValue("@Medicamente", medicamenteTable);
                tvpParam.SqlDbType = SqlDbType.Structured;
                tvpParam.TypeName = "dbo.MedicamentListType";

                await cmd.ExecuteNonQueryAsync();
            }
            else
            {
                throw new InvalidOperationException("Connection must be Microsoft.Data.SqlClient.SqlConnection for TVP support");
            }

            _logger.LogInformation("[ConsultatieRepository] Replaced {Count} medicamente for: {ConsultatieID}", 
                medicamenteTable.Rows.Count, consultatieId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error replacing Medicamente for: {ConsultatieID}", consultatieId);
            throw;
        }
    }

    public async Task<IEnumerable<ConsultatieMedicament>> GetMedicamenteAsync(Guid consultatieId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var result = await connection.QueryAsync<ConsultatieMedicament>(
                "ConsultatieMedicament_GetByConsultatieId",
                new { ConsultatieID = consultatieId },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            _logger.LogInformation("[ConsultatieRepository] Retrieved {Count} medicamente for: {ConsultatieID}", 
                result.Count(), consultatieId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error getting Medicamente for: {ConsultatieID}", consultatieId);
            throw;
        }
    }

    public async Task UpsertConcluziiAsync(Guid consultatieId, ConsultatieConcluzii entity)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatieId);
            parameters.Add("@Prognostic", entity.Prognostic);
            parameters.Add("@Concluzie", entity.Concluzie);
            parameters.Add("@ObservatiiMedic", entity.ObservatiiMedic);
            parameters.Add("@NotePacient", entity.NotePacient);
            parameters.Add("@DocumenteAtatate", entity.DocumenteAtatate);
            
            // Scrisoare Medicala - Anexa 43
            parameters.Add("@EsteAfectiuneOncologica", entity.EsteAfectiuneOncologica);
            parameters.Add("@DetaliiAfectiuneOncologica", entity.DetaliiAfectiuneOncologica);
            parameters.Add("@AreIndicatieInternare", entity.AreIndicatieInternare);
            parameters.Add("@TermenInternare", entity.TermenInternare);
            parameters.Add("@SaEliberatPrescriptie", entity.SaEliberatPrescriptie);
            parameters.Add("@SeriePrescriptie", entity.SeriePrescriptie);
            parameters.Add("@SaEliberatConcediuMedical", entity.SaEliberatConcediuMedical);
            parameters.Add("@SerieConcediuMedical", entity.SerieConcediuMedical);
            parameters.Add("@SaEliberatIngrijiriDomiciliu", entity.SaEliberatIngrijiriDomiciliu);
            parameters.Add("@SaEliberatDispozitiveMedicale", entity.SaEliberatDispozitiveMedicale);
            parameters.Add("@TransmiterePrinEmail", entity.TransmiterePrinEmail);
            parameters.Add("@EmailTransmitere", entity.EmailTransmitere);
            
            parameters.Add("@ModificatDe", entity.ModificatDe ?? entity.CreatDe);

            await connection.ExecuteAsync(
                "ConsultatieConcluzii_Upsert",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            _logger.LogInformation("[ConsultatieRepository] Concluzii upserted for: {ConsultatieID}", consultatieId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConsultatieRepository] Error upserting Concluzii for: {ConsultatieID}", consultatieId);
            throw;
        }
    }
}
