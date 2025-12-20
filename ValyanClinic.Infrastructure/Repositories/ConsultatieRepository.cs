using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Infrastructure.Data;
using ValyanClinic.Infrastructure.Repositories.Interfaces;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository pentru gestionarea consultatiilor medicale - DOAR STORED PROCEDURES
/// </summary>
public class ConsultatieRepository : IConsultatieRepository
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

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatie.ConsultatieID, DbType.Guid, ParameterDirection.InputOutput);
            parameters.Add("@ProgramareID", consultatie.ProgramareID);
            parameters.Add("@PacientID", consultatie.PacientID);
            parameters.Add("@MedicID", consultatie.MedicID);
            parameters.Add("@DataConsultatie", consultatie.DataConsultatie);
            parameters.Add("@OraConsultatie", consultatie.OraConsultatie);
            parameters.Add("@TipConsultatie", consultatie.TipConsultatie);

            // Motive Prezentare
            parameters.Add("@MotivPrezentare", consultatie.MotivPrezentare);
            parameters.Add("@IstoricBoalaActuala", consultatie.IstoricBoalaActuala);

            // Antecedente Heredo-Colaterale
            parameters.Add("@AHC_Mama", consultatie.AHC_Mama);
            parameters.Add("@AHC_Tata", consultatie.AHC_Tata);
            parameters.Add("@AHC_Frati", consultatie.AHC_Frati);
            parameters.Add("@AHC_Bunici", consultatie.AHC_Bunici);
            parameters.Add("@AHC_Altele", consultatie.AHC_Altele);

            // Antecedente Fiziologice
            parameters.Add("@AF_Nastere", consultatie.AF_Nastere);
            parameters.Add("@AF_Dezvoltare", consultatie.AF_Dezvoltare);
            parameters.Add("@AF_Menstruatie", consultatie.AF_Menstruatie);
            parameters.Add("@AF_Sarcini", consultatie.AF_Sarcini);
            parameters.Add("@AF_Alaptare", consultatie.AF_Alaptare);

            // Antecedente Personale Patologice
            parameters.Add("@APP_BoliCopilarieAdolescenta", consultatie.APP_BoliCopilarieAdolescenta);
            parameters.Add("@APP_BoliAdult", consultatie.APP_BoliAdult);
            parameters.Add("@APP_Interventii", consultatie.APP_Interventii);
            parameters.Add("@APP_Traumatisme", consultatie.APP_Traumatisme);
            parameters.Add("@APP_Transfuzii", consultatie.APP_Transfuzii);
            parameters.Add("@APP_Alergii", consultatie.APP_Alergii);
            parameters.Add("@APP_Medicatie", consultatie.APP_Medicatie);

            // Conditii Socio-Economice
            parameters.Add("@Profesie", consultatie.Profesie);
            parameters.Add("@ConditiiLocuinta", consultatie.ConditiiLocuinta);
            parameters.Add("@ConditiiMunca", consultatie.ConditiiMunca);
            parameters.Add("@ObiceiuriAlimentare", consultatie.ObiceiuriAlimentare);
            parameters.Add("@Toxice", consultatie.Toxice);

            // Examen General
            parameters.Add("@StareGenerala", consultatie.StareGenerala);
            parameters.Add("@Constitutie", consultatie.Constitutie);
            parameters.Add("@Atitudine", consultatie.Atitudine);
            parameters.Add("@Facies", consultatie.Facies);
            parameters.Add("@Tegumente", consultatie.Tegumente);
            parameters.Add("@Mucoase", consultatie.Mucoase);
            parameters.Add("@GangliniLimfatici", consultatie.GangliniLimfatici);

            // Semne Vitale
            parameters.Add("@Greutate", consultatie.Greutate);
            parameters.Add("@Inaltime", consultatie.Inaltime);
            parameters.Add("@IMC", consultatie.IMC);
            parameters.Add("@Temperatura", consultatie.Temperatura);
            parameters.Add("@TensiuneArteriala", consultatie.TensiuneArteriala);
            parameters.Add("@Puls", consultatie.Puls);
            parameters.Add("@FreccventaRespiratorie", consultatie.FreccventaRespiratorie);
            parameters.Add("@SaturatieO2", consultatie.SaturatieO2);
            parameters.Add("@Glicemie", consultatie.Glicemie);

            // Examen pe Aparate
            parameters.Add("@ExamenCardiovascular", consultatie.ExamenCardiovascular);
            parameters.Add("@ExamenRespiratoriu", consultatie.ExamenRespiratoriu);
            parameters.Add("@ExamenDigestiv", consultatie.ExamenDigestiv);
            parameters.Add("@ExamenUrinar", consultatie.ExamenUrinar);
            parameters.Add("@ExamenNervos", consultatie.ExamenNervos);
            parameters.Add("@ExamenLocomotor", consultatie.ExamenLocomotor);
            parameters.Add("@ExamenEndocrin", consultatie.ExamenEndocrin);
            parameters.Add("@ExamenORL", consultatie.ExamenORL);
            parameters.Add("@ExamenOftalmologic", consultatie.ExamenOftalmologic);
            parameters.Add("@ExamenDermatologic", consultatie.ExamenDermatologic);

            // Investigatii
            parameters.Add("@InvestigatiiLaborator", consultatie.InvestigatiiLaborator);
            parameters.Add("@InvestigatiiImagistice", consultatie.InvestigatiiImagistice);
            parameters.Add("@InvestigatiiEKG", consultatie.InvestigatiiEKG);
            parameters.Add("@AlteInvestigatii", consultatie.AlteInvestigatii);

            // Diagnostic
            parameters.Add("@DiagnosticPozitiv", consultatie.DiagnosticPozitiv);
            parameters.Add("@DiagnosticDiferential", consultatie.DiagnosticDiferential);
            parameters.Add("@DiagnosticEtiologic", consultatie.DiagnosticEtiologic);
            parameters.Add("@CoduriICD10", consultatie.CoduriICD10);

            // Tratament
            parameters.Add("@TratamentMedicamentos", consultatie.TratamentMedicamentos);
            parameters.Add("@TratamentNemedicamentos", consultatie.TratamentNemedicamentos);
            parameters.Add("@RecomandariDietetice", consultatie.RecomandariDietetice);
            parameters.Add("@RecomandariRegimViata", consultatie.RecomandariRegimViata);

            // Recomandari
            parameters.Add("@InvestigatiiRecomandate", consultatie.InvestigatiiRecomandate);
            parameters.Add("@ConsulturiSpecialitate", consultatie.ConsulturiSpecialitate);
            parameters.Add("@DataUrmatoareiProgramari", consultatie.DataUrmatoareiProgramari);
            parameters.Add("@RecomandariSupraveghere", consultatie.RecomandariSupraveghere);

            // Prognostic & Concluzie
            parameters.Add("@Prognostic", consultatie.Prognostic);
            parameters.Add("@Concluzie", consultatie.Concluzie);

            // Observatii
            parameters.Add("@ObservatiiMedic", consultatie.ObservatiiMedic);
            parameters.Add("@NotePacient", consultatie.NotePacient);

            // Status
            parameters.Add("@Status", consultatie.Status);
            parameters.Add("@DataFinalizare", consultatie.DataFinalizare);

            // Audit
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

            var parameters = new { ProgramareID = programareId };

            var result = await connection.QueryFirstOrDefaultAsync<Consultatie>(
                "sp_Consultatie_GetByProgramare",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result;
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

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatie.ConsultatieID);
            parameters.Add("@ProgramareID", consultatie.ProgramareID);
            parameters.Add("@PacientID", consultatie.PacientID);
            parameters.Add("@MedicID", consultatie.MedicID);
            parameters.Add("@DataConsultatie", consultatie.DataConsultatie);
            parameters.Add("@OraConsultatie", consultatie.OraConsultatie);
            parameters.Add("@TipConsultatie", consultatie.TipConsultatie);

            // Motive Prezentare
            parameters.Add("@MotivPrezentare", consultatie.MotivPrezentare);
            parameters.Add("@IstoricBoalaActuala", consultatie.IstoricBoalaActuala);

            // Antecedente Heredo-Colaterale
            parameters.Add("@AHC_Mama", consultatie.AHC_Mama);
            parameters.Add("@AHC_Tata", consultatie.AHC_Tata);
            parameters.Add("@AHC_Frati", consultatie.AHC_Frati);
            parameters.Add("@AHC_Bunici", consultatie.AHC_Bunici);
            parameters.Add("@AHC_Altele", consultatie.AHC_Altele);

            // Antecedente Fiziologice
            parameters.Add("@AF_Nastere", consultatie.AF_Nastere);
            parameters.Add("@AF_Dezvoltare", consultatie.AF_Dezvoltare);
            parameters.Add("@AF_Menstruatie", consultatie.AF_Menstruatie);
            parameters.Add("@AF_Sarcini", consultatie.AF_Sarcini);
            parameters.Add("@AF_Alaptare", consultatie.AF_Alaptare);

            // Antecedente Personale Patologice
            parameters.Add("@APP_BoliCopilarieAdolescenta", consultatie.APP_BoliCopilarieAdolescenta);
            parameters.Add("@APP_BoliAdult", consultatie.APP_BoliAdult);
            parameters.Add("@APP_Interventii", consultatie.APP_Interventii);
            parameters.Add("@APP_Traumatisme", consultatie.APP_Traumatisme);
            parameters.Add("@APP_Transfuzii", consultatie.APP_Transfuzii);
            parameters.Add("@APP_Alergii", consultatie.APP_Alergii);
            parameters.Add("@APP_Medicatie", consultatie.APP_Medicatie);

            // Conditii Socio-Economice
            parameters.Add("@Profesie", consultatie.Profesie);
            parameters.Add("@ConditiiLocuinta", consultatie.ConditiiLocuinta);
            parameters.Add("@ConditiiMunca", consultatie.ConditiiMunca);
            parameters.Add("@ObiceiuriAlimentare", consultatie.ObiceiuriAlimentare);
            parameters.Add("@Toxice", consultatie.Toxice);

            // Examen General
            parameters.Add("@StareGenerala", consultatie.StareGenerala);
            parameters.Add("@Constitutie", consultatie.Constitutie);
            parameters.Add("@Atitudine", consultatie.Atitudine);
            parameters.Add("@Facies", consultatie.Facies);
            parameters.Add("@Tegumente", consultatie.Tegumente);
            parameters.Add("@Mucoase", consultatie.Mucoase);
            parameters.Add("@GangliniLimfatici", consultatie.GangliniLimfatici);

            // Semne Vitale
            parameters.Add("@Greutate", consultatie.Greutate);
            parameters.Add("@Inaltime", consultatie.Inaltime);
            parameters.Add("@IMC", consultatie.IMC);
            parameters.Add("@Temperatura", consultatie.Temperatura);
            parameters.Add("@TensiuneArteriala", consultatie.TensiuneArteriala);
            parameters.Add("@Puls", consultatie.Puls);
            parameters.Add("@FreccventaRespiratorie", consultatie.FreccventaRespiratorie);
            parameters.Add("@SaturatieO2", consultatie.SaturatieO2);
            parameters.Add("@Glicemie", consultatie.Glicemie);

            // Examen pe Aparate
            parameters.Add("@ExamenCardiovascular", consultatie.ExamenCardiovascular);
            parameters.Add("@ExamenRespiratoriu", consultatie.ExamenRespiratoriu);
            parameters.Add("@ExamenDigestiv", consultatie.ExamenDigestiv);
            parameters.Add("@ExamenUrinar", consultatie.ExamenUrinar);
            parameters.Add("@ExamenNervos", consultatie.ExamenNervos);
            parameters.Add("@ExamenLocomotor", consultatie.ExamenLocomotor);
            parameters.Add("@ExamenEndocrin", consultatie.ExamenEndocrin);
            parameters.Add("@ExamenORL", consultatie.ExamenORL);
            parameters.Add("@ExamenOftalmologic", consultatie.ExamenOftalmologic);
            parameters.Add("@ExamenDermatologic", consultatie.ExamenDermatologic);

            // Investigatii
            parameters.Add("@InvestigatiiLaborator", consultatie.InvestigatiiLaborator);
            parameters.Add("@InvestigatiiImagistice", consultatie.InvestigatiiImagistice);
            parameters.Add("@InvestigatiiEKG", consultatie.InvestigatiiEKG);
            parameters.Add("@AlteInvestigatii", consultatie.AlteInvestigatii);

            // Diagnostic
            parameters.Add("@DiagnosticPozitiv", consultatie.DiagnosticPozitiv);
            parameters.Add("@DiagnosticDiferential", consultatie.DiagnosticDiferential);
            parameters.Add("@DiagnosticEtiologic", consultatie.DiagnosticEtiologic);
            parameters.Add("@CoduriICD10", consultatie.CoduriICD10);

            // Tratament
            parameters.Add("@TratamentMedicamentos", consultatie.TratamentMedicamentos);
            parameters.Add("@TratamentNemedicamentos", consultatie.TratamentNemedicamentos);
            parameters.Add("@RecomandariDietetice", consultatie.RecomandariDietetice);
            parameters.Add("@RecomandariRegimViata", consultatie.RecomandariRegimViata);

            // Recomandari
            parameters.Add("@InvestigatiiRecomandate", consultatie.InvestigatiiRecomandate);
            parameters.Add("@ConsulturiSpecialitate", consultatie.ConsulturiSpecialitate);
            parameters.Add("@DataUrmatoareiProgramari", consultatie.DataUrmatoareiProgramari);
            parameters.Add("@RecomandariSupraveghere", consultatie.RecomandariSupraveghere);

            // Prognostic & Concluzie
            parameters.Add("@Prognostic", consultatie.Prognostic);
            parameters.Add("@Concluzie", consultatie.Concluzie);

            // Observatii
            parameters.Add("@ObservatiiMedic", consultatie.ObservatiiMedic);
            parameters.Add("@NotePacient", consultatie.NotePacient);

            // Status
            parameters.Add("@Status", consultatie.Status);
            parameters.Add("@DataFinalizare", consultatie.DataFinalizare);
            parameters.Add("@DurataMinute", consultatie.DurataMinute);

            // Audit
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

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultatieID", consultatie.ConsultatieID, DbType.Guid, ParameterDirection.InputOutput);
            parameters.Add("@ProgramareID", consultatie.ProgramareID);
            parameters.Add("@PacientID", consultatie.PacientID);
            parameters.Add("@MedicID", consultatie.MedicID);
            parameters.Add("@DataConsultatie", consultatie.DataConsultatie);
            parameters.Add("@OraConsultatie", consultatie.OraConsultatie);
            parameters.Add("@TipConsultatie", consultatie.TipConsultatie);

            // Essential fields for draft (cele mai frecvent completate)
            parameters.Add("@MotivPrezentare", consultatie.MotivPrezentare);
            parameters.Add("@IstoricBoalaActuala", consultatie.IstoricBoalaActuala);
            parameters.Add("@Greutate", consultatie.Greutate);
            parameters.Add("@Inaltime", consultatie.Inaltime);
            parameters.Add("@IMC", consultatie.IMC);
            parameters.Add("@Temperatura", consultatie.Temperatura);
            parameters.Add("@TensiuneArteriala", consultatie.TensiuneArteriala);
            parameters.Add("@Puls", consultatie.Puls);
            parameters.Add("@DiagnosticPozitiv", consultatie.DiagnosticPozitiv);
            parameters.Add("@CoduriICD10", consultatie.CoduriICD10);
            parameters.Add("@TratamentMedicamentos", consultatie.TratamentMedicamentos);
            parameters.Add("@ObservatiiMedic", consultatie.ObservatiiMedic);

            // Audit - use CreatDe for both create and update in draft mode
            var userId = consultatie.ModificatDe ?? consultatie.CreatDe;
            parameters.Add("@CreatDeSauModificatDe", userId);

            await connection.ExecuteAsync(
                "sp_Consultatie_SaveDraft",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 30);

            var consultatieId = parameters.Get<Guid>("@ConsultatieID");

            _logger.LogInformation("[ConsultatieRepository] Consultatie draft saved: {ConsultatieID}", consultatieId);

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

            // Căutăm consultații cu Status != 'Finalizata' (nefinalizate)
            // Stored procedure sp_Consultatie_SaveDraft setează Status = 'In desfasurare'
            var sql = @"
                SELECT TOP 1 *
                FROM Consultatii
                WHERE PacientID = @PacientID
                  AND [Status] NOT IN ('Finalizata', 'Anulata')
                  AND CAST(DataConsultatie AS DATE) = CAST(@DataConsultatie AS DATE)
                  AND (@MedicID IS NULL OR MedicID = @MedicID)
                  AND (@ProgramareID IS NULL OR ProgramareID = @ProgramareID)
                ORDER BY DataCreare DESC";

            var parameters = new
            {
                PacientID = pacientId,
                MedicID = medicId,
                DataConsultatie = dataConsultatie ?? DateTime.Today,
                ProgramareID = programareId
            };

            var result = await connection.QueryFirstOrDefaultAsync<Consultatie>(
                sql,
                parameters,
                commandTimeout: 15);

            if (result != null)
            {
                _logger.LogInformation(
                    "[ConsultatieRepository] Draft consultatie found: {ConsultatieID} (Status: {Status}) for PacientID: {PacientID}",
                    result.ConsultatieID, result.Status, pacientId);
            }
            else
            {
                _logger.LogInformation(
                    "[ConsultatieRepository] No draft consultatie found for PacientID: {PacientID}, DataConsultatie: {Data}",
                    pacientId, dataConsultatie ?? DateTime.Today);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "[ConsultatieRepository] Error getting draft consultatie for PacientID: {PacientID}", 
                pacientId);
            throw;
        }
    }
}
