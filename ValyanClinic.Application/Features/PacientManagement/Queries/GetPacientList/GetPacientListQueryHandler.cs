using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;

/// <summary>
/// Handler pentru GetPacientListQuery
/// </summary>
public class GetPacientListQueryHandler : IRequestHandler<GetPacientListQuery, Result<PagedResult<PacientListDto>>>
{
    private readonly IPacientRepository _pacientRepository;
    private readonly ILogger<GetPacientListQueryHandler> _logger;

    public GetPacientListQueryHandler(
        IPacientRepository pacientRepository,
        ILogger<GetPacientListQueryHandler> logger)
    {
        _pacientRepository = pacientRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<PacientListDto>>> Handle(
        GetPacientListQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("========== GetPacientListQueryHandler START ==========");
        _logger.LogInformation("Query Parameters: Page={Page}, Size={Size}, Search={Search}",
            request.PageNumber, request.PageSize, request.SearchText);

        try
        {
            _logger.LogInformation("Calling repository.GetPagedAsync...");

            var (items, totalCount) = await _pacientRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchText,
                request.Judet,
                request.Asigurat,
                request.Activ,
                request.SortColumn,
                request.SortDirection,
                cancellationToken);

            _logger.LogInformation("Repository returned {Count} items, Total={Total}",
                items?.Count() ?? 0, totalCount);

            var dtoList = items.Select(p => new PacientListDto
            {
                Id = p.Id,
                Cod_Pacient = p.Cod_Pacient,
                CNP = p.CNP,
                Nume = p.Nume,
                Prenume = p.Prenume,
                NumeComplet = p.NumeComplet,
                Data_Nasterii = p.Data_Nasterii,
                Varsta = p.Varsta,
                Sex = p.Sex,
                Telefon = p.Telefon,
                Email = p.Email,
                Judet = p.Judet,
                Localitate = p.Localitate,
                AdresaCompleta = p.AdresaCompleta,
                Asigurat = p.Asigurat,
                Casa_Asigurari = p.Casa_Asigurari,
                Ultima_Vizita = p.Ultima_Vizita,
                Nr_Total_Vizite = p.Nr_Total_Vizite,
                Activ = p.Activ
            }).ToList();

            _logger.LogInformation("Mapped {Count} DTOs", dtoList.Count);

            var pagedResult = PagedResult<PacientListDto>.Success(
                dtoList,
                request.PageNumber,
                request.PageSize,
                totalCount);

            _logger.LogInformation("========== GetPacientListQueryHandler END (SUCCESS) ==========");
            return Result<PagedResult<PacientListDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "========== GetPacientListQueryHandler EXCEPTION ==========");
            _logger.LogError("Exception Type: {Type}", ex.GetType().FullName);
            _logger.LogError("Exception Message: {Message}", ex.Message);
            _logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);

            if (ex.InnerException != null)
            {
                _logger.LogError("Inner Exception: {InnerMessage}", ex.InnerException.Message);
            }

            return Result<PagedResult<PacientListDto>>.Failure(
                $"Eroare la obtinerea listei de pacienti: {ex.Message}");
        }
    }
}
