using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.AddRelatie;

public class AddRelatieCommandHandler : IRequestHandler<AddRelatieCommand, Result<Guid>>
{
    private readonly IPacientPersonalMedicalRepository _relatieRepository;
    private readonly ILogger<AddRelatieCommandHandler> _logger;

    public AddRelatieCommandHandler(
        IPacientPersonalMedicalRepository relatieRepository,
        ILogger<AddRelatieCommandHandler> logger)
    {
        _relatieRepository = relatieRepository;
        _logger = logger;
    }

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
                request.CreatDe, // Updated type for CreatDe
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
