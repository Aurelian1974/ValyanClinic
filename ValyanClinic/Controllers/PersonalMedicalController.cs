using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using ValyanClinic.Application.Services.Export;

namespace ValyanClinic.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonalMedicalController : ControllerBase
{
    private readonly IPersonalMedicalExportService _exportService;
    private readonly ILogger<PersonalMedicalController> _logger;

    public PersonalMedicalController(IPersonalMedicalExportService exportService, ILogger<PersonalMedicalController> logger)
    {
        _exportService = exportService;
        _logger = logger;
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string format = "csv", [FromQuery] string? search = null, [FromQuery] string? departament = null, [FromQuery] string? pozitie = null, [FromQuery] string? esteActiv = null, [FromQuery] string sortColumn = "Nume", [FromQuery] string sortDirection = "ASC")
    {
        try
        {
            bool? activ = null;
            if (!string.IsNullOrEmpty(esteActiv))
            {
                if (bool.TryParse(esteActiv, out var parsed)) activ = parsed;
            }

            byte[] bytes;
            string contentType;
            string fileName;

            try
            {
                if (format.Equals("excel", StringComparison.OrdinalIgnoreCase))
                {
                    bytes = await _exportService.ExportToExcelAsync(search, departament, pozitie, activ, sortColumn, sortDirection);
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fileName = $"PersonalMedical_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                }
                else
                {
                    bytes = await _exportService.ExportToCsvAsync(search, departament, pozitie, activ, sortColumn, sortDirection);
                    contentType = "text/csv";
                    fileName = $"PersonalMedical_{DateTime.Now:yyyyMMddHHmmss}.csv";
                }

                if (bytes == null || bytes.Length == 0)
                {
                    // No data to export with given filters
                    return NoContent();
                }

                return File(bytes, contentType, fileName);
            }
            catch (FluentValidation.ValidationException vex)
            {
                _logger.LogWarning(vex, "Export validation failed");
                return BadRequest(new { message = "Invalid export parameters", errors = vex.Errors.Select(e => e.ErrorMessage) });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export failed");
            return StatusCode(500, "Export failed");
        }
    }
}