using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PozitieManagement.Commands.CreatePozitie;

public class CreatePozitieCommandHandler : IRequestHandler<CreatePozitieCommand, Result<Guid>>
{
    private readonly IPozitieRepository _repository;
    private readonly ILogger<CreatePozitieCommandHandler> _logger;

    public CreatePozitieCommandHandler(
        IPozitieRepository repository,
        ILogger<CreatePozitieCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreatePozitieCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating Pozitie: {Denumire}", request.Denumire);

            var pozitie = new Pozitie
            {
                Id = Guid.NewGuid(),
                Denumire = request.Denumire,
                Descriere = request.Descriere,
                EsteActiv = request.EsteActiv,
                DataCrearii = DateTime.UtcNow,
                DataUltimeiModificari = DateTime.UtcNow,
                CreatDe = request.CreatDe,
                ModificatDe = request.CreatDe
            };

            var createdId = await _repository.CreateAsync(pozitie, cancellationToken);

            if (createdId == Guid.Empty)
            {
                _logger.LogWarning("Failed to create Pozitie: {Denumire}", request.Denumire);
                return Result<Guid>.Failure(new List<string> { "Crearea pozitiei a esuat" });
            }

            _logger.LogInformation("Pozitie created successfully: {Id}", createdId);
            return Result<Guid>.Success(createdId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Pozitie: {Denumire}", request.Denumire);
            return Result<Guid>.Failure(new List<string> { ex.Message });
        }
    }
}
