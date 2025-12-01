using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;
using DomainDoctorDto = ValyanClinic.Domain.DTOs.DoctorAsociatDto;
using AppDoctorDto = ValyanClinic.Application.Features.PacientPersonalMedicalManagement.DTOs.DoctorAsociatDto;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Queries.GetDoctoriByPacient;

/// <summary>
/// Handler pentru GetDoctoriByPacientQuery.
/// Obține lista de doctori asociați unui pacient folosind repository pattern.
/// </summary>
public class GetDoctoriByPacientQueryHandler : IRequestHandler<GetDoctoriByPacientQuery, Result<List<AppDoctorDto>>>
{
    private readonly IPacientPersonalMedicalRepository _repository;
    private readonly ILogger<GetDoctoriByPacientQueryHandler> _logger;

    /// <summary>
    /// Constructor pentru GetDoctoriByPacientQueryHandler.
    /// </summary>
    /// <param name="repository">
    /// Repository pentru accesarea relațiilor pacient-personal medical.
    /// </param>
    /// <param name="logger">
    /// Logger pentru înregistrarea operațiilor și erorilor.
    /// </param>
    public GetDoctoriByPacientQueryHandler(
        IPacientPersonalMedicalRepository repository,
        ILogger<GetDoctoriByPacientQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Procesează query-ul pentru obținerea doctorilor asociați unui pacient.
    /// </summary>
    /// <param name="request">Query-ul cu parametrii de căutare.</param>
    /// <param name="cancellationToken">Token pentru anularea operației.</param>
    /// <returns>
    /// Result care conține lista de doctori asociați sau mesaje de eroare.
    /// </returns>
    public async Task<Result<List<AppDoctorDto>>> Handle(
        GetDoctoriByPacientQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[GetDoctoriByPacientQueryHandler] Processing query for PacientID={PacientID}, ApenumereActivi={ApenumereActivi}",
                request.PacientID, request.ApenumereActivi);

            // Repository returns Domain DTOs
            var domainDoctori = await _repository.GetDoctoriByPacientAsync(
                request.PacientID,
                request.ApenumereActivi,
                cancellationToken);

            // Map Domain DTOs to Application DTOs
            var applicationDoctori = domainDoctori.Select(d => new AppDoctorDto
            {
                RelatieID = d.RelatieID,
                PersonalMedicalID = d.PersonalMedicalID,
                DoctorNumeComplet = d.DoctorNumeComplet,
                DoctorSpecializare = d.DoctorSpecializare,
                DoctorTelefon = d.DoctorTelefon,
                DoctorEmail = d.DoctorEmail,
                DoctorDepartament = d.DoctorDepartament,
                TipRelatie = d.TipRelatie,
                DataAsocierii = d.DataAsocierii,
                DataDezactivarii = d.DataDezactivarii,
                EsteActiv = d.EsteActiv,
                ZileDeAsociere = d.ZileDeAsociere,
                Observatii = d.Observatii,
                Motiv = d.Motiv
            }).ToList();

            _logger.LogInformation(
                "[GetDoctoriByPacientQueryHandler] Query successful - found {Count} doctori",
                applicationDoctori.Count);

            return Result<List<AppDoctorDto>>.Success(applicationDoctori);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex,
                "[GetDoctoriByPacientQueryHandler] Invalid arguments for PacientID={PacientID}",
                request.PacientID);

            return Result<List<AppDoctorDto>>.Failure($"Parametri invalizi: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[GetDoctoriByPacientQueryHandler] Error obtaining doctori for PacientID={PacientID}",
                request.PacientID);

            return Result<List<AppDoctorDto>>.Failure($"Eroare la obținerea doctorilor: {ex.Message}");
        }
    }
}
