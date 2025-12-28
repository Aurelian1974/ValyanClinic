using System.Threading;
using System.Threading.Tasks;

namespace ValyanClinic.Application.Services.Export;

public interface IPersonalMedicalExportService
{
    Task<byte[]> ExportToCsvAsync(
        string? searchText,
        string? departament,
        string? pozitie,
        bool? esteActiv,
        string sortColumn,
        string sortDirection,
        int maxRecords = 10000,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportToExcelAsync(
        string? searchText,
        string? departament,
        string? pozitie,
        bool? esteActiv,
        string sortColumn,
        string sortDirection,
        int maxRecords = 10000,
        CancellationToken cancellationToken = default);
}