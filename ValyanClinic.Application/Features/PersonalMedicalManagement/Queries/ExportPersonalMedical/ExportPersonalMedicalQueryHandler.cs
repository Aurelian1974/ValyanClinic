using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Services.Export;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.ExportPersonalMedical;

/// <summary>
/// Handler pentru exportul datelor despre personalul medical.
/// Delegă logica de export către IPersonalMedicalExportService.
/// </summary>
public class ExportPersonalMedicalQueryHandler
    : IRequestHandler<ExportPersonalMedicalQuery, Result<ExportPersonalMedicalResult>>
{
    private readonly IPersonalMedicalExportService _exportService;
    private readonly ILogger<ExportPersonalMedicalQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ExportPersonalMedicalQueryHandler"/> with the required dependencies.
    /// </summary>
    /// <param name="exportService">Service responsible for generating Excel or CSV export data for personal medical records.</param>
    /// <param name="logger">Logger used to record informational, warning, and error events within the handler.</param>
    public ExportPersonalMedicalQueryHandler(
        IPersonalMedicalExportService exportService,
        ILogger<ExportPersonalMedicalQueryHandler> logger)
    {
        _exportService = exportService;
        _logger = logger;
    }

    /// <summary>
    /// Handles an export request for personal medical staff data and produces a downloadable file (Excel or CSV) according to the request format.
    /// </summary>
    /// <param name="request">The export query containing filter, sorting and format preferences.</param>
    /// <returns>A Result containing an ExportPersonalMedicalResult with the exported file bytes, content type, and file name on success; a failure Result with a descriptive message when no data is produced, validation fails, or an unexpected error occurs.</returns>
    public async Task<Result<ExportPersonalMedicalResult>> Handle(
        ExportPersonalMedicalQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Exporting PersonalMedical data: Format={Format}, Search={Search}, Departament={Departament}",
                request.Format, request.Search, request.Departament);

            byte[] bytes;
            string contentType;
            string fileName;

            if (request.Format.Equals("excel", StringComparison.OrdinalIgnoreCase))
            {
                bytes = await _exportService.ExportToExcelAsync(
                    request.Search,
                    request.Departament,
                    request.Pozitie,
                    request.EsteActiv,
                    request.SortColumn,
                    request.SortDirection);

                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = $"PersonalMedical_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            }
            else
            {
                bytes = await _exportService.ExportToCsvAsync(
                    request.Search,
                    request.Departament,
                    request.Pozitie,
                    request.EsteActiv,
                    request.SortColumn,
                    request.SortDirection);

                contentType = "text/csv";
                fileName = $"PersonalMedical_{DateTime.Now:yyyyMMddHHmmss}.csv";
            }

            if (bytes == null || bytes.Length == 0)
            {
                _logger.LogWarning("Export produced no data with given filters");
                return Result<ExportPersonalMedicalResult>.Failure("No data to export with given filters");
            }

            var result = new ExportPersonalMedicalResult
            {
                FileBytes = bytes,
                ContentType = contentType,
                FileName = fileName
            };

            _logger.LogInformation(
                "Export successful: {FileName}, Size={Size} bytes",
                fileName, bytes.Length);

            return Result<ExportPersonalMedicalResult>.Success(result);
        }
        catch (FluentValidation.ValidationException vex)
        {
            _logger.LogWarning(vex, "Export validation failed");
            var errors = string.Join(", ", vex.Errors.Select(e => e.ErrorMessage));
            return Result<ExportPersonalMedicalResult>.Failure($"Validation failed: {errors}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export failed");
            return Result<ExportPersonalMedicalResult>.Failure($"Export failed: {ex.Message}");
        }
    }
}