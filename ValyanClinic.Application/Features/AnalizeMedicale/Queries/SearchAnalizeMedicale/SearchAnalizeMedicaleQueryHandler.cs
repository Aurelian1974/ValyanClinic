using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.ViewModels;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Queries.SearchAnalizeMedicale;

/// <summary>
/// Handler pentru căutare analize medicale în nomenclator
/// </summary>
public class SearchAnalizeMedicaleQueryHandler 
    : IRequestHandler<SearchAnalizeMedicaleQuery, Result<PagedResult<AnalizaMedicalaListDto>>>
{
    private readonly IAnalizaMedicalaRepository _repository;
    private readonly ILogger<SearchAnalizeMedicaleQueryHandler> _logger;

    public SearchAnalizeMedicaleQueryHandler(
        IAnalizaMedicalaRepository repository,
        ILogger<SearchAnalizeMedicaleQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<AnalizaMedicalaListDto>>> Handle(
        SearchAnalizeMedicaleQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Searching analize: SearchTerm={SearchTerm}, CategorieId={CategorieId}, Page={Page}",
                request.SearchTerm, request.CategorieId, request.PageNumber);

            var (items, totalCount) = await _repository.SearchAsync(
                request.SearchTerm,
                request.CategorieId,
                request.LaboratorId,
                request.DoarActive,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var dtos = items.Select(a => new AnalizaMedicalaListDto
            {
                AnalizaID = a.AnalizaID,
                NumeAnaliza = a.NumeAnaliza,
                NumeScurt = a.NumeScurt,
                Acronime = a.Acronime,
                Pret = a.Pret,
                Moneda = a.Moneda,
                // Aceste câmpuri sunt populate din JOIN în stored procedure
                NumeCategorie = a.NumeCategorie ?? string.Empty,
                CategorieIcon = a.CategorieIcon,
                NumeLaborator = a.NumeLaborator ?? string.Empty,
                LaboratorAcronim = a.LaboratorAcronim
            }).ToList();

            _logger.LogInformation(
                "Search completed: Found {Count} of {Total}",
                dtos.Count, totalCount);

            return Result<PagedResult<AnalizaMedicalaListDto>>.Success(
                PagedResult<AnalizaMedicalaListDto>.Success(
                    dtos,
                    request.PageNumber,
                    request.PageSize,
                    totalCount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching analize medicale");
            return Result<PagedResult<AnalizaMedicalaListDto>>.Failure(
                $"Eroare la căutare: {ex.Message}");
        }
    }
}
