using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PozitieManagement.Queries.GetPozitieById;

public class GetPozitieByIdQueryHandler : IRequestHandler<GetPozitieByIdQuery, Result<PozitieDetailDto>>
{
    private readonly IPozitieRepository _repository;
    private readonly ILogger<GetPozitieByIdQueryHandler> _logger;

    public GetPozitieByIdQueryHandler(
        IPozitieRepository repository,
        ILogger<GetPozitieByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<PozitieDetailDto>> Handle(
        GetPozitieByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GetPozitieById: {Id}", request.Id);

            var pozitie = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (pozitie == null)
            {
                _logger.LogWarning("Pozitie not found: {Id}", request.Id);
                return Result<PozitieDetailDto>.Failure(new List<string> { "Pozitia nu a fost gasita" });
            }

            var dto = new PozitieDetailDto
            {
                Id = pozitie.Id,
                Denumire = pozitie.Denumire,
                Descriere = pozitie.Descriere,
                EsteActiv = pozitie.EsteActiv,
                DataCrearii = pozitie.DataCrearii,
                DataUltimeiModificari = pozitie.DataUltimeiModificari,
                CreatDe = pozitie.CreatDe,
                ModificatDe = pozitie.ModificatDe
            };

            _logger.LogInformation("GetPozitieById SUCCESS: {Id}", request.Id);
            return Result<PozitieDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPozitieByIdQueryHandler");
            return Result<PozitieDetailDto>.Failure(new List<string> { ex.Message });
        }
    }
}
