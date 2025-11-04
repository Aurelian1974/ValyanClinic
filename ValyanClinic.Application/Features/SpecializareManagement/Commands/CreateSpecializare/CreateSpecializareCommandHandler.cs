using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.SpecializareManagement.Commands.CreateSpecializare;

public class CreateSpecializareCommandHandler : IRequestHandler<CreateSpecializareCommand, Result<Guid>>
{
    private readonly ISpecializareRepository _repository;
    private readonly ILogger<CreateSpecializareCommandHandler> _logger;

    public CreateSpecializareCommandHandler(
        ISpecializareRepository repository,
        ILogger<CreateSpecializareCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateSpecializareCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating Specializare: {Denumire}", request.Denumire);

            var specializare = new Specializare
            {
                Id = Guid.NewGuid(),
                Denumire = request.Denumire,
                Categorie = request.Categorie,
                Descriere = request.Descriere,
                EsteActiv = request.EsteActiv,
                DataCrearii = DateTime.UtcNow,
                DataUltimeiModificari = DateTime.UtcNow,
                CreatDe = request.CreatDe,
                ModificatDe = request.CreatDe
            };

            var createdSpecializare = await _repository.CreateAsync(specializare, cancellationToken);

            if (createdSpecializare == null)
            {
                _logger.LogWarning("Failed to create Specializare: {Denumire}", request.Denumire);
                return Result<Guid>.Failure(new List<string> { "Crearea specializarii a esuat" });
            }

            _logger.LogInformation("Specializare created successfully: {Id}", createdSpecializare.Id);
            return Result<Guid>.Success(createdSpecializare.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Specializare: {Denumire}", request.Denumire);
            return Result<Guid>.Failure(new List<string> { ex.Message });
        }
    }
}
