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
            _logger.LogInformation("🔄 HANDLER: Creating Specializare: {Denumire}", request.Denumire);

            var specializare = new Specializare
            {
                Id = Guid.NewGuid(),
                Denumire = request.Denumire,
                Categorie = request.Categorie,
                Descriere = request.Descriere,
                EsteActiv = request.EsteActiv,
                DataCrearii = DateTime.Now,
                DataUltimeiModificari = DateTime.Now,
                CreatDe = request.CreatDe,
                ModificatDe = request.CreatDe
            };

            _logger.LogInformation("🔄 HANDLER: Calling repository.CreateAsync...");
            var createdId = await _repository.CreateAsync(specializare, cancellationToken);

            if (createdId == Guid.Empty)
            {
                _logger.LogWarning("❌ HANDLER: Repository returned empty GUID - create failed");
                return Result<Guid>.Failure(new List<string> { "Crearea specializarii a esuat" });
            }

            _logger.LogInformation("✅ HANDLER: Specializare created successfully: {Id}", createdId);
            return Result<Guid>.Success(createdId);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "❌ HANDLER: ArgumentException creating Specializare: {Denumire}", request.Denumire);
            _logger.LogError("❌ HANDLER: ArgumentException Message: {Message}", ex.Message);
            return Result<Guid>.Failure(new List<string> { ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "❌ HANDLER: InvalidOperationException creating Specializare: {Denumire}", request.Denumire);
            _logger.LogError("❌ HANDLER: InvalidOperationException Message: {Message}", ex.Message);
            return Result<Guid>.Failure(new List<string> { ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ HANDLER: Exception creating Specializare: {Denumire}", request.Denumire);
            _logger.LogError("❌ HANDLER: Exception Type: {Type}", ex.GetType().Name);
            _logger.LogError("❌ HANDLER: Exception Message: {Message}", ex.Message);

            return Result<Guid>.Failure(new List<string> { ex.Message });
        }
    }
}
