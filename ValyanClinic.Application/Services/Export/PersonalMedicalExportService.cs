using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList;

namespace ValyanClinic.Application.Services.Export;

public class PersonalMedicalExportService : IPersonalMedicalExportService
{
    private readonly IMediator _mediator;
    private readonly ILogger<PersonalMedicalExportService> _logger;

    public PersonalMedicalExportService(IMediator mediator, ILogger<PersonalMedicalExportService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<byte[]> ExportToCsvAsync(string? searchText, string? departament, string? pozitie, bool? esteActiv, string sortColumn, string sortDirection, int maxRecords = 10000, CancellationToken cancellationToken = default)
    {
        // Page through results using the validated PageSize limits (validator requires PageSize between 10 and 1000)
        var pageSize = Math.Min(Math.Max(10, maxRecords), 1000); // pageSize capped to validator max
        var pageNumber = 1;
        var fetched = 0;
        var collected = new List<PersonalMedicalListDto>();

        while (fetched < maxRecords)
        {
            var query = new GetPersonalMedicalListQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                GlobalSearchText = string.IsNullOrWhiteSpace(searchText) ? null : searchText,
                FilterDepartament = departament,
                FilterPozitie = pozitie,
                FilterEsteActiv = esteActiv,
                SortColumn = sortColumn,
                SortDirection = sortDirection
            };

            var result = await _mediator.Send(query, cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("ExportCSV: query failed on page {Page}: {Errors}", pageNumber, string.Join(',', result.Errors ?? new List<string>()));
                break;
            }

            var pageItems = result.Value?.ToList() ?? new List<PersonalMedicalListDto>();
            if (!pageItems.Any()) break;

            var toTake = Math.Min(maxRecords - fetched, pageItems.Count);
            collected.AddRange(pageItems.Take(toTake));
            fetched += toTake;

            if (fetched >= maxRecords) break;
            if (pageItems.Count < pageSize) break; // last page

            pageNumber++;
        }

        var list = collected.AsEnumerable();

        var csv = new StringBuilder();

        // Header
        csv.AppendLine("Nume,Prenume,Specializare,NumarLicenta,Telefon,Email,Departament,Pozitie,Status,DataCreare");

        foreach (var p in list)
        {
            var status = p.EsteActiv == true ? "Activ" : "Inactiv";
            var line = string.Join(",",
                EscapeCsv(p.Nume),
                EscapeCsv(p.Prenume),
                EscapeCsv(p.Specializare),
                EscapeCsv(p.NumarLicenta),
                EscapeCsv(p.Telefon),
                EscapeCsv(p.Email),
                EscapeCsv(p.Departament),
                EscapeCsv(p.Pozitie),
                EscapeCsv(status),
                EscapeCsv(p.DataCreare?.ToString("yyyy-MM-dd")));

            csv.AppendLine(line);
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportToExcelAsync(string? searchText, string? departament, string? pozitie, bool? esteActiv, string sortColumn, string sortDirection, int maxRecords = 10000, CancellationToken cancellationToken = default)
    {
        // Build the same paged collection as CSV export but produce a proper .xlsx using ClosedXML
        var pageSize = Math.Min(Math.Max(10, maxRecords), 1000);
        var pageNumber = 1;
        var fetched = 0;
        var collected = new List<PersonalMedicalListDto>();

        while (fetched < maxRecords)
        {
            var query = new GetPersonalMedicalListQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                GlobalSearchText = string.IsNullOrWhiteSpace(searchText) ? null : searchText,
                FilterDepartament = departament,
                FilterPozitie = pozitie,
                FilterEsteActiv = esteActiv,
                SortColumn = sortColumn,
                SortDirection = sortDirection
            };

            var result = await _mediator.Send(query, cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("ExportExcel: query failed on page {Page}: {Errors}", pageNumber, string.Join(',', result.Errors ?? new List<string>()));
                break;
            }

            var pageItems = result.Value?.ToList() ?? new List<PersonalMedicalListDto>();
            if (!pageItems.Any()) break;

            var toTake = Math.Min(maxRecords - fetched, pageItems.Count);
            collected.AddRange(pageItems.Take(toTake));
            fetched += toTake;

            if (fetched >= maxRecords) break;
            if (pageItems.Count < pageSize) break; // last page

            pageNumber++;
        }

        // If no data, return empty byte[] so controller can handle NoContent
        if (!collected.Any()) return Array.Empty<byte>();

        using var ms = new System.IO.MemoryStream();

        // Create Excel workbook with ClosedXML
        using (var workbook = new ClosedXML.Excel.XLWorkbook())
        {
            var ws = workbook.Worksheets.Add("PersonalMedical");

            // Header row
            ws.Cell(1, 1).Value = "Nume";
            ws.Cell(1, 2).Value = "Prenume";
            ws.Cell(1, 3).Value = "Specializare";
            ws.Cell(1, 4).Value = "NumarLicenta";
            ws.Cell(1, 5).Value = "Telefon";
            ws.Cell(1, 6).Value = "Email";
            ws.Cell(1, 7).Value = "Departament";
            ws.Cell(1, 8).Value = "Pozitie";
            ws.Cell(1, 9).Value = "Status";
            ws.Cell(1, 10).Value = "DataCreare";

            var row = 2;
            foreach (var p in collected)
            {
                ws.Cell(row, 1).Value = p.Nume;
                ws.Cell(row, 2).Value = p.Prenume;
                ws.Cell(row, 3).Value = p.Specializare;
                ws.Cell(row, 4).Value = p.NumarLicenta;
                ws.Cell(row, 5).Value = p.Telefon;
                ws.Cell(row, 6).Value = p.Email;
                ws.Cell(row, 7).Value = p.Departament;
                ws.Cell(row, 8).Value = p.Pozitie;
                ws.Cell(row, 9).Value = p.EsteActiv == true ? "Activ" : "Inactiv";
                ws.Cell(row, 10).Value = p.DataCreare?.ToString("yyyy-MM-dd");
                row++;
            }

            // Styling: bold header and auto-fit
            ws.Row(1).Style.Font.Bold = true;
            ws.Columns(1, 10).AdjustToContents();

            workbook.SaveAs(ms);
        }

        return ms.ToArray();
    }

    private static string EscapeCsv(string? input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        var needsQuotes = input.Contains(',') || input.Contains('"') || input.Contains('\n') || input.Contains('\r');
        var value = input.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{value}\"" : value;
    }
}