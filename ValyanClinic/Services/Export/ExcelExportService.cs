using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;

namespace ValyanClinic.Services.Export;

/// <summary>
/// Service pentru exportul datelor în format Excel folosind ClosedXML
/// </summary>
public interface IExcelExportService
{
    Task<byte[]> ExportProgramariToExcelAsync(IEnumerable<ProgramareListDto> programari, string sheetName = "Programări");
    Task<byte[]> ExportPacientiToExcelAsync(IEnumerable<PacientListDto> pacienti, string sheetName = "Pacienti");
}

public class ExcelExportService : IExcelExportService
{
    private readonly ILogger<ExcelExportService> _logger;

    public ExcelExportService(ILogger<ExcelExportService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> ExportPacientiToExcelAsync(IEnumerable<PacientListDto> pacienti, string sheetName = "Pacienti")
    {
        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            // Header row
            var headers = new[] { "Cod", "Nume Complet", "CNP", "Data Nașterii", "Vârstă", "Sex", 
                                  "Telefon", "Email", "Localitate", "Județ", "Asigurat", 
                                  "Casa Asigurări", "Ultima Vizită", "Nr. Vizite", "Status" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            // Style header
            var headerRow = worksheet.Range(1, 1, 1, headers.Length);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#3B82F6");
            headerRow.Style.Font.FontColor = XLColor.White;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Data rows
            int row = 2;
            foreach (var pacient in pacienti)
            {
                worksheet.Cell(row, 1).Value = pacient.Cod_Pacient;
                worksheet.Cell(row, 2).Value = pacient.NumeComplet;
                worksheet.Cell(row, 3).Value = pacient.CNP;
                worksheet.Cell(row, 4).Value = pacient.Data_Nasterii.ToString("dd.MM.yyyy");
                worksheet.Cell(row, 5).Value = pacient.Varsta.ToString();
                worksheet.Cell(row, 6).Value = pacient.Sex == "M" ? "Masculin" : "Feminin";
                worksheet.Cell(row, 7).Value = pacient.Telefon ?? "-";
                worksheet.Cell(row, 8).Value = pacient.Email ?? "-";
                worksheet.Cell(row, 9).Value = pacient.Localitate ?? "-";
                worksheet.Cell(row, 10).Value = pacient.Judet ?? "-";
                worksheet.Cell(row, 11).Value = pacient.Asigurat ? "Da" : "Nu";
                worksheet.Cell(row, 12).Value = pacient.Casa_Asigurari ?? "-";
                worksheet.Cell(row, 13).Value = pacient.Ultima_Vizita?.ToString("dd.MM.yyyy") ?? "-";
                worksheet.Cell(row, 14).Value = pacient.Nr_Total_Vizite;
                worksheet.Cell(row, 15).Value = pacient.Activ ? "Activ" : "Inactiv";

                // Alternate row coloring
                if (row % 2 == 0)
                {
                    worksheet.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
                }

                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Freeze header row
            worksheet.SheetView.FreezeRows(1);

            // Convert to byte array
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var bytes = stream.ToArray();

            _logger.LogInformation("Export Excel generat: {RowCount} pacienti", pacienti.Count());
            return await Task.FromResult(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la generarea exportului Excel pentru pacienti");
            throw;
        }
    }

    public async Task<byte[]> ExportProgramariToExcelAsync(IEnumerable<ProgramareListDto> programari, string sheetName = "Programări")
    {
        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            // Header row
            worksheet.Cell(1, 1).Value = "Data Programare";
            worksheet.Cell(1, 2).Value = "Ora Început";
            worksheet.Cell(1, 3).Value = "Ora Sfârșit";
            worksheet.Cell(1, 4).Value = "Pacient";
            worksheet.Cell(1, 5).Value = "Doctor";
            worksheet.Cell(1, 6).Value = "Specializare";
            worksheet.Cell(1, 7).Value = "Tip Programare";
            worksheet.Cell(1, 8).Value = "Status";
            worksheet.Cell(1, 9).Value = "Durata (min)";
            worksheet.Cell(1, 10).Value = "Observații";

            // Style header
            var headerRow = worksheet.Range(1, 1, 1, 10);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Data rows
            int row = 2;
            foreach (var programare in programari)
            {
                worksheet.Cell(row, 1).Value = programare.DataProgramare.ToString("dd.MM.yyyy");
                worksheet.Cell(row, 2).Value = programare.OraInceput.ToString(@"hh\:mm");
                worksheet.Cell(row, 3).Value = programare.OraSfarsit.ToString(@"hh\:mm");
                worksheet.Cell(row, 4).Value = programare.PacientNumeComplet;
                worksheet.Cell(row, 5).Value = programare.DoctorNumeComplet;
                worksheet.Cell(row, 6).Value = programare.DoctorSpecializare ?? "-";
                worksheet.Cell(row, 7).Value = GetTipDisplay(programare.TipProgramare);
                worksheet.Cell(row, 8).Value = GetStatusDisplay(programare.Status);
                worksheet.Cell(row, 9).Value = programare.DurataMinute;
                worksheet.Cell(row, 10).Value = programare.Observatii ?? "-";

                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Convert to byte array
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var bytes = stream.ToArray();

            _logger.LogInformation("Export Excel generat: {RowCount} programări", programari.Count());
            return await Task.FromResult(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la generarea exportului Excel");
            throw;
        }
    }

    private string GetStatusDisplay(string status)
    {
        return status switch
        {
            "Programata" => "Programată",
            "Confirmata" => "Confirmată",
            "CheckedIn" => "Check-in",
            "InConsultatie" => "În consultație",
            "Finalizata" => "Finalizată",
            "Anulata" => "Anulată",
            "NoShow" => "Nu s-a prezentat",
            _ => status
        };
    }

    private string GetTipDisplay(string? tip)
    {
        return tip switch
        {
            "ConsultatieInitiala" => "Consultație Inițială",
            "ControlPeriodic" => "Control Periodic",
            "Consultatie" => "Consultație",
            "Investigatie" => "Investigație",
            "Procedura" => "Procedură",
            "Urgenta" => "Urgență",
            "Telemedicina" => "Telemedicină",
            "LaDomiciliu" => "La Domiciliu",
            _ => tip ?? "-"
        };
    }
}
