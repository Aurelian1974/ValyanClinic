using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Queries.GetPacientiByDoctor;

/// <summary>
/// Handler pentru obținerea pacienților asociați unui doctor.
/// Utilizează repository pattern pentru accesarea datelor.
/// </summary>
public class GetPacientiByDoctorQueryHandler : IRequestHandler<GetPacientiByDoctorQuery, Result<List<PacientAsociatDto>>>
{
    private readonly IPacientPersonalMedicalRepository _repository;
    private readonly ILogger<GetPacientiByDoctorQueryHandler> _logger;

    public GetPacientiByDoctorQueryHandler(
        IPacientPersonalMedicalRepository repository,
        ILogger<GetPacientiByDoctorQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<PacientAsociatDto>>> Handle(
        GetPacientiByDoctorQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[GetPacientiByDoctorHandler] Fetching pacienti for PersonalMedicalID: {PersonalMedicalID}, ApenumereActivi: {ApenumereActivi}, TipRelatie: {TipRelatie}",
                request.PersonalMedicalID, request.ApenumereActivi, request.TipRelatie);

            // Utilizează repository pentru a obține pacienții
            var pacienti = await _repository.GetPacientiByDoctorAsync(
                request.PersonalMedicalID,
                request.ApenumereActivi,
                request.TipRelatie,
                cancellationToken);

            _logger.LogInformation(
                "[GetPacientiByDoctorHandler] Found {Count} pacienti for PersonalMedicalID: {PersonalMedicalID} ({Active} active, {Inactive} inactive)",
                pacienti.Count,
                request.PersonalMedicalID,
                pacienti.Count(p => p.EsteActiv),
                pacienti.Count(p => !p.EsteActiv));

            return Result<List<PacientAsociatDto>>.Success(pacienti);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex,
                "[GetPacientiByDoctorHandler] Invalid arguments for PersonalMedicalID: {PersonalMedicalID}",
                request.PersonalMedicalID);

            return Result<List<PacientAsociatDto>>.Failure($"Parametri invalizi: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[GetPacientiByDoctorHandler] Error fetching pacienti for PersonalMedicalID: {PersonalMedicalID}",
                request.PersonalMedicalID);

            return Result<List<PacientAsociatDto>>.Failure($"Eroare la obținerea pacienților: {ex.Message}");
        }
    }
}
