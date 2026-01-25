using Microsoft.AspNetCore.Mvc;
using MediatR;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.ExportPersonalMedical;

namespace ValyanClinic.Controllers;

/// <summary>
/// API Controller pentru operații cu personalul medical.
/// Thin controller - delegă toate operațiile către MediatR.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PersonalMedicalController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PersonalMedicalController> _logger;

    public PersonalMedicalController(
        IMediator mediator,
        ILogger<PersonalMedicalController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Exportă datele personalului medical în format CSV sau Excel.
    /// </summary>
    /// <param name="format">Formatul de export: "csv" sau "excel"</param>
    /// <param name="search">Termen de căutare (nume, prenume, email)</param>
    /// <param name="departament">Filtru departament</param>
    /// <param name="pozitie">Filtru poziție</param>
    /// <param name="esteActiv">Filtru status activ (null = toate)</param>
    /// <param name="sortColumn">Coloană de sortare (default: Nume)</param>
    /// <param name="sortDirection">Direcție sortare: ASC sau DESC</param>
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] string format = "csv",
        [FromQuery] string? search = null,
        [FromQuery] string? departament = null,
        [FromQuery] string? pozitie = null,
        [FromQuery] string? esteActiv = null,
        [FromQuery] string sortColumn = "Nume",
        [FromQuery] string sortDirection = "ASC")
    {
        try
        {
            // Parse esteActiv string → bool?
            bool? activ = null;
            if (!string.IsNullOrEmpty(esteActiv))
            {
                if (bool.TryParse(esteActiv, out var parsed))
                    activ = parsed;
            }

            // Create query
            var query = new ExportPersonalMedicalQuery
            {
                Format = format,
                Search = search,
                Departament = departament,
                Pozitie = pozitie,
                EsteActiv = activ,
                SortColumn = sortColumn,
                SortDirection = sortDirection
            };

            // Delegate to MediatR
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Export failed: {Message}", result.Message);

                if (result.Message.Contains("No data"))
                    return NoContent();

                return BadRequest(new { message = result.Message });
            }

            // Return file
            return File(
                result.Value!.FileBytes,
                result.Value.ContentType,
                result.Value.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during export");
            return StatusCode(500, "Export failed");
        }
    }
}
