using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.RolManagement.Queries.GetPermisiuniDefinitii;

/// <summary>
/// Handler pentru GetPermisiuniDefinitiiQuery.
/// </summary>
public class GetPermisiuniDefinitiiQueryHandler : IRequestHandler<GetPermisiuniDefinitiiQuery, Result<List<CategoriePermisiuniDto>>>
{
    private readonly IRolRepository _repository;
    private readonly ILogger<GetPermisiuniDefinitiiQueryHandler> _logger;

    public GetPermisiuniDefinitiiQueryHandler(
        IRolRepository repository,
        ILogger<GetPermisiuniDefinitiiQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<CategoriePermisiuniDto>>> Handle(
        GetPermisiuniDefinitiiQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GetPermisiuniDefinitii");

            var permisiuni = await _repository.GetAllPermisiuniDefinitiiAsync(cancellationToken);

            var grupate = permisiuni
                .GroupBy(p => p.Categorie)
                .OrderBy(g => g.Key)
                .Select(g => new CategoriePermisiuniDto
                {
                    Categorie = g.Key,
                    Permisiuni = g.OrderBy(p => p.OrdineAfisare)
                        .Select(p => new PermisiuneDefinitieDto
                        {
                            Id = p.PermisiuneDefinitieID,
                            Cod = p.Cod,
                            Categorie = p.Categorie,
                            Denumire = p.Denumire,
                            Descriere = p.Descriere,
                            OrdineAfisare = p.OrdineAfisare
                        })
                        .ToList()
                })
                .ToList();

            _logger.LogInformation("GetPermisiuniDefinitii SUCCESS: {Count} categorii", grupate.Count);

            return Result<List<CategoriePermisiuniDto>>.Success(grupate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPermisiuniDefinitiiQueryHandler");
            return Result<List<CategoriePermisiuniDto>>.Failure(ex.Message);
        }
    }
}
