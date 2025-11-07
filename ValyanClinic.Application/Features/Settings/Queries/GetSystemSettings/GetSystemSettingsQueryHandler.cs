using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.Settings.Queries.GetSystemSettings;

public class GetSystemSettingsQueryHandler : IRequestHandler<GetSystemSettingsQuery, Result<List<SystemSettingDto>>>
{
    private readonly ISystemSettingsRepository _repository;
    private readonly ILogger<GetSystemSettingsQueryHandler> _logger;

    public GetSystemSettingsQueryHandler(
      ISystemSettingsRepository repository,
        ILogger<GetSystemSettingsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<SystemSettingDto>>> Handle(
        GetSystemSettingsQuery request,
        CancellationToken cancellationToken)
    {
   try
        {
        _logger.LogInformation("Fetching system settings for category: {Categorie}", request.Categorie);

       var settings = await _repository.GetByCategoryAsync(request.Categorie, cancellationToken);

       var dtos = settings.Select(s => new SystemSettingDto
       {
    SetareID = s.SetareID,
       Categorie = s.Categorie,
    Cheie = s.Cheie,
       Valoare = s.Valoare,
      Descriere = s.Descriere,
         ValoareDefault = s.ValoareDefault,
                TipDate = s.TipDate,
       EsteEditabil = s.EsteEditabil,
            DataCrearii = s.DataCrearii,
  DataModificarii = s.DataModificarii,
                ModificatDe = s.ModificatDe
            }).ToList();

        _logger.LogInformation("Found {Count} settings for category: {Categorie}", dtos.Count, request.Categorie);

            return Result<List<SystemSettingDto>>.Success(dtos);
        }
catch (Exception ex)
  {
   _logger.LogError(ex, "Error fetching system settings for category: {Categorie}", request.Categorie);
            return Result<List<SystemSettingDto>>.Failure($"Error: {ex.Message}");
        }
    }
}
