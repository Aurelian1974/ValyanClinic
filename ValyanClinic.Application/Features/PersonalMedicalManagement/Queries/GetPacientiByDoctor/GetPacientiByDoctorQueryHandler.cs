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

    /// <summary>
    /// Initializes a new instance of <see cref="GetPacientiByDoctorQueryHandler"/> with the specified repository and logger.
    /// </summary>
    /// <param name="relatieRepository">Repository used to retrieve patient-to-doctor relationships.</param>
    /// <param name="_logger">Logger for diagnostic and error messages produced by this handler.</param>
    public GetPacientiByDoctorQueryHandler(
        IPacientPersonalMedicalRepository relatieRepository,
        ILogger<GetPacientiByDoctorQueryHandler> _logger)
    {
        _relatieRepository = relatieRepository;
        this._logger = _logger;
    }

    /// <summary>
    /// Retrieves the patients associated with the doctor specified in the request.
    /// </summary>
    /// <param name="request">Query containing the doctor identifier used to find associated patients.</param>
    /// <returns>
    /// A Result containing the list of associated PacientAsociatDto on success; a failure Result with an error message on failure.
    /// </returns>
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