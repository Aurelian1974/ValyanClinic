using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ICD10Management.DTOs;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.ICD10Management.Queries.SearchICD10;

/// <summary>
/// Handler pentru căutare coduri ICD-10
/// Folosește IICD10Repository (returnează Domain entities) și mapează la DTOs
/// </summary>
public class SearchICD10QueryHandler : IRequestHandler<SearchICD10Query, Result<IEnumerable<ICD10SearchResultDto>>>
{
    private readonly IICD10Repository _icd10Repository;
    private readonly ILogger<SearchICD10QueryHandler> _logger;

    public SearchICD10QueryHandler(
     IICD10Repository icd10Repository,
        ILogger<SearchICD10QueryHandler> logger)
    {
   _icd10Repository = icd10Repository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ICD10SearchResultDto>>> Handle(
        SearchICD10Query request,
  CancellationToken cancellationToken)
    {
  try
{
          _logger.LogInformation(
 "[ICD10Search] Searching: Term='{SearchTerm}', Category={Category}, OnlyCommon={OnlyCommon}, MaxResults={MaxResults}",
  request.SearchTerm, request.Category, request.OnlyCommon, request.MaxResults);

   // Dacă se cer doar coduri frecvente (OnlyCommon=true) și nu e search term
  if (request.OnlyCommon && string.IsNullOrWhiteSpace(request.SearchTerm))
     {
        var commonCodes = await _icd10Repository.GetCommonCodesAsync(request.MaxResults);
      var commonCodesDtos = MapToSearchResultDtos(commonCodes);

       _logger.LogInformation("[ICD10Search] ✅ Returned {Count} common codes", commonCodesDtos.Count());
          
      return Result<IEnumerable<ICD10SearchResultDto>>.Success(commonCodesDtos);
  }

 // Căutare normală
   var domainResults = await _icd10Repository.SearchAsync(
       request.SearchTerm ?? "", 
       request.MaxResults);

   // Filtrare după categorie (dacă e specificată)
 if (!string.IsNullOrEmpty(request.Category))
     {
     domainResults = domainResults.Where(r => 
 r.Category?.Equals(request.Category, StringComparison.OrdinalIgnoreCase) == true);
 }

  // Filtrare după OnlyLeafNodes (dacă e specificat)
  if (request.OnlyLeafNodes)
       {
domainResults = domainResults.Where(r => r.IsLeafNode);
            }

 var finalResults = MapToSearchResultDtos(domainResults);

 _logger.LogInformation("[ICD10Search] ✅ Returned {Count} codes", finalResults.Count());

            if (finalResults.Any())
       {
     var sample = string.Join(", ", finalResults.Take(3).Select(c => $"{c.Code} - {c.ShortDescription}"));
  _logger.LogDebug("[ICD10Search] Sample: {Sample}", sample);
    }

    return Result<IEnumerable<ICD10SearchResultDto>>.Success(finalResults);
}
      catch (Exception ex)
        {
_logger.LogError(ex, "[ICD10Search] ❌ EXCEPTION during search: {Message}", ex.Message);
     return Result<IEnumerable<ICD10SearchResultDto>>.Failure($"Eroare căutare ICD-10: {ex.Message}");
}
    }

    /// <summary>
    /// Mapează Domain entities (ICD10Code) la DTOs (ICD10SearchResultDto)
    /// </summary>
   private List<ICD10SearchResultDto> MapToSearchResultDtos(IEnumerable<Domain.Entities.ICD10Code> codes)
    {
        return codes.Select(c => new ICD10SearchResultDto
        {
            ICD10_ID = c.ICD10_ID,
      Code = c.Code,
            FullCode = c.FullCode,
            ShortDescription = c.ShortDescription, // Entity property mapează automat RO->EN
       LongDescription = c.LongDescription, // Entity property mapează automat RO->EN
     Category = c.Category,
   Severity = c.Severity,
            IsCommon = c.IsCommon,
    IsLeafNode = c.IsLeafNode,
            IsBillable = c.IsBillable,
   IsTranslated = c.IsTranslated,
        RelevanceScore = 100 // Default - ar putea fi calculat de SP
        }).ToList();
    }
}
