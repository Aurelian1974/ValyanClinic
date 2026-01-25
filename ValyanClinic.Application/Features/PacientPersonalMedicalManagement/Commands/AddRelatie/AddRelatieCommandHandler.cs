using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.AddRelatie;

public class AddRelatieCommandHandler : IRequestHandler<AddRelatieCommand, Result<Guid>>
{
    private readonly IPacientPersonalMedicalRepository _relatieRepository;
    private readonly ILogger<AddRelatieCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="AddRelatieCommandHandler"/> with required dependencies.
    /// </summary>
    /// <param name="relatieRepository">Repository used to create and manage patient–personal-medical relationships.</param>
    /// <param name="logger">Logger for recording informational, warning, and error messages from the handler.</param>
    public AddRelatieCommandHandler(
        IPacientPersonalMedicalRepository relatieRepository,
        ILogger<AddRelatieCommandHandler> logger)
    {
        _relatieRepository = relatieRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles an AddRelatieCommand by creating a new patient–medical-staff relationship.
    /// </summary>
    /// <param name="request">The command containing PacientID, PersonalMedicalID, TipRelatie, Observatii, Motiv and CreatDe.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A Result containing the created relatie's Guid on success; on failure contains an error message.</returns>
    public async Task<Result<Guid>> Handle(AddRelatieCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "[AddRelatieHandler] Adding relatie: PacientID={PacientID}, DoctorID={DoctorID}, TipRelatie={TipRelatie}",
                request.PacientID, request.PersonalMedicalID, request.TipRelatie);

            var newRelatieId = await _relatieRepository.AddRelatieAsync(
                request.PacientID,
                request.PersonalMedicalID,
                request.TipRelatie,
                request.Observatii,
                request.Motiv,
                request.CreatDe,
                cancellationToken);

            _logger.LogInformation(
                "[AddRelatieHandler] Successfully created relatie: {RelatieID}",
                newRelatieId);

            return Result<Guid>.Success(newRelatieId, "Doctor adăugat cu succes!");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("[AddRelatieHandler] Validation error: {Message}", ex.Message);
            return Result<Guid>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AddRelatieHandler] Exception adding relatie");
            return Result<Guid>.Failure($"Eroare la crearea relației: {ex.Message}");
        }
    }
}