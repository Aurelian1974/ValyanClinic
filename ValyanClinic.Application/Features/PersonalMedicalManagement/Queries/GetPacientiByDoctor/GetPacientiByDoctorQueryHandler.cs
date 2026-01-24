using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPacientiByDoctor;

/// <summary>
/// Handler pentru obținerea pacienților asociați cu un doctor
/// </summary>
public class GetPacientiByDoctorQueryHandler : IRequestHandler<GetPacientiByDoctorQuery, Result<List<PacientAsociatDto>>>
{
    private readonly IPacientPersonalMedicalRepository _relatieRepository;
    private readonly ILogger<GetPacientiByDoctorQueryHandler> _logger;

    public GetPacientiByDoctorQueryHandler(
        IPacientPersonalMedicalRepository relatieRepository,
        ILogger<GetPacientiByDoctorQueryHandler> _logger)
    {
        _relatieRepository = relatieRepository;
        this._logger = _logger;
    }

    public async Task<Result<List<PacientAsociatDto>>> Handle(
        GetPacientiByDoctorQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting pacienti for doctor: {DoctorID}", request.DoctorID);

            // Get pacienti folosind repository
            var pacienti = await _relatieRepository.GetPacientiByDoctorAsync(
                request.DoctorID,
                apenumereActivi: false, // toate relațiile (active + inactive)
                tipRelatie: null,
                cancellationToken);

            _logger.LogInformation(
                "Found {Count} pacienti for doctor {DoctorID} ({Active} active, {Inactive} inactive)",
                pacienti.Count,
                request.DoctorID,
                pacienti.Count(p => p.EsteActiv),
                pacienti.Count(p => !p.EsteActiv));

            return Result<List<PacientAsociatDto>>.Success(pacienti);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pacienti for doctor: {DoctorID}", request.DoctorID);
            return Result<List<PacientAsociatDto>>.Failure($"Error: {ex.Message}");
        }
    }
}
