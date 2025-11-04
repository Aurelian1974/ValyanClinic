using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.SpecializareManagement.Queries.GetSpecializareById;

public class GetSpecializareByIdQueryHandler : IRequestHandler<GetSpecializareByIdQuery, Result<SpecializareDetailDto>>
{
    private readonly ISpecializareRepository _repository;
    private readonly ILogger<GetSpecializareByIdQueryHandler> _logger;

    public GetSpecializareByIdQueryHandler(
        ISpecializareRepository repository,
        ILogger<GetSpecializareByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<SpecializareDetailDto>> Handle(
        GetSpecializareByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GetSpecializareById: {Id}", request.Id);

            var specializare = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (specializare == null)
            {
                _logger.LogWarning("Specializare not found: {Id}", request.Id);
                return Result<SpecializareDetailDto>.Failure(new List<string> { "Specializarea nu a fost gasita" });
            }

            var dto = new SpecializareDetailDto
            {
                Id = specializare.Id,
                Denumire = specializare.Denumire,
                Categorie = specializare.Categorie,
                Descriere = specializare.Descriere,
                EsteActiv = specializare.EsteActiv,
                DataCrearii = specializare.DataCrearii,
                DataUltimeiModificari = specializare.DataUltimeiModificari,
                CreatDe = specializare.CreatDe,
                ModificatDe = specializare.ModificatDe
            };

            _logger.LogInformation("GetSpecializareById SUCCESS: {Id}", request.Id);
            return Result<SpecializareDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetSpecializareByIdQueryHandler");
            return Result<SpecializareDetailDto>.Failure(new List<string> { ex.Message });
        }
    }
}
