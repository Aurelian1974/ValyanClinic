using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;

namespace ValyanClinic.Application.Services.Pacienti;

/// <summary>
/// Implementation of IPacientDataService for managing patient data operations.
/// </summary>
/// <remarks>
/// <b>Responsibilities:</b>
/// - Transform UI requests into MediatR queries
/// - Map between PagedResult and PagedPacientData
/// - Handle errors gracefully with Result pattern
/// - Log all operations for debugging
/// 
/// <b>Pattern:</b> Service layer with dependency injection
/// <b>Testing:</b> Fully unit testable without UI dependencies
/// </remarks>
public class PacientDataService : IPacientDataService
{
    private readonly IMediator _mediator;
    private readonly ILogger<PacientDataService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PacientDataService"/> class.
    /// </summary>
    /// <param name="mediator">MediatR instance for sending queries</param>
    /// <param name="logger">Logger instance for diagnostics</param>
    public PacientDataService(IMediator mediator, ILogger<PacientDataService> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Loads paged patient data with filters and sorting options.
    /// </summary>
    public async Task<Result<PagedPacientData>> LoadPagedDataAsync(
        PacientFilters filters,
        PaginationOptions pagination,
        SortOptions sorting,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "[PacientDataService] Loading paged data: Page={Page}, Size={Size}, Search='{Search}', Judet={Judet}, Asigurat={Asigurat}, Activ={Activ}, Sort={Sort} {Direction}",
                pagination.PageNumber, pagination.PageSize, filters.SearchText, 
                filters.Judet, filters.Asigurat, filters.Activ, 
                sorting.Column, sorting.Direction);

            // Create MediatR query
            var query = new GetPacientListQuery
            {
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                SearchText = string.IsNullOrWhiteSpace(filters.SearchText) ? null : filters.SearchText,
                Judet = filters.Judet,
                Asigurat = filters.Asigurat,
                Activ = filters.Activ,
                SortColumn = sorting.Column,
                SortDirection = sorting.Direction
            };

            // Send query via MediatR
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    "[PacientDataService] Query failed with errors: {Errors}",
                    string.Join(", ", result.Errors ?? new List<string>()));
                return Result<PagedPacientData>.Failure(result.Errors);
            }

            // Map PagedResult to PagedPacientData
            var pagedData = new PagedPacientData
            {
                Items = result.Value?.Value?.ToList() ?? new List<PacientListDto>(),
                TotalCount = result.Value?.TotalCount ?? 0,
                CurrentPage = result.Value?.CurrentPage ?? pagination.PageNumber,
                PageSize = result.Value?.PageSize ?? pagination.PageSize
            };

            _logger.LogInformation(
                "[PacientDataService] Data loaded successfully: {Count} items, Total={Total}, Page={Page}/{TotalPages}",
                pagedData.Items.Count, pagedData.TotalCount, pagedData.CurrentPage, pagedData.TotalPages);

            return Result<PagedPacientData>.Success(pagedData);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("[PacientDataService] Operation cancelled by user");
            return Result<PagedPacientData>.Failure("Operație anulată");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PacientDataService] Error loading paged data");
            return Result<PagedPacientData>.Failure($"Eroare la încărcarea datelor: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads filter options (unique judete, etc.) from server.
    /// </summary>
    public async Task<Result<PacientFilterOptions>> LoadFilterOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("[PacientDataService] Loading filter options from server");

            // Load ALL data for filter options extraction (no paging)
            var query = new GetPacientListQuery
            {
                PageNumber = 1,
                PageSize = int.MaxValue, // Get all records
                SearchText = null,
                Judet = null,
                Asigurat = null,
                Activ = null,
                SortColumn = "Nume",
                SortDirection = "ASC"
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    "[PacientDataService] Failed to load filter options: {Errors}",
                    string.Join(", ", result.Errors ?? new List<string>()));
                return Result<PacientFilterOptions>.Failure(result.Errors);
            }

            var allData = result.Value?.Value?.ToList() ?? new List<PacientListDto>();

            // Extract unique judete
            var judete = allData
                .Where(p => !string.IsNullOrEmpty(p.Judet))
                .Select(p => p.Judet!)
                .Distinct()
                .OrderBy(j => j)
                .ToList();

            var filterOptions = new PacientFilterOptions
            {
                Judete = judete
            };

            _logger.LogInformation(
                "[PacientDataService] Filter options loaded: {JudetCount} judete",
                filterOptions.Judete.Count);

            return Result<PacientFilterOptions>.Success(filterOptions);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("[PacientDataService] Filter options loading cancelled");
            return Result<PacientFilterOptions>.Failure("Operație anulată");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PacientDataService] Error loading filter options");
            return Result<PacientFilterOptions>.Failure($"Eroare la încărcarea opțiunilor de filtrare: {ex.Message}");
        }
    }
}
