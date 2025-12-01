using ValyanClinic.Domain.Entities;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface pentru Pacient
/// </summary>
public interface IPacientRepository
{
    // Read operations
    Task<Pacient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Pacient?> GetByCodPacientAsync(string codPacient, CancellationToken cancellationToken = default);
    Task<Pacient?> GetByCNPAsync(string cnp, CancellationToken cancellationToken = default);
    Task<IEnumerable<Pacient>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<Pacient> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchText = null,
        string? judet = null,
        bool? asigurat = null,
        bool? activ = null,
        string sortColumn = "Nume",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(
        string? searchText = null,
        string? judet = null,
        bool? asigurat = null,
        bool? activ = null,
        CancellationToken cancellationToken = default);

    // Lookup operations
    Task<IEnumerable<(string Value, string Text, int NumarPacienti)>> GetJudeteAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<(Guid Id, string DisplayText, string? CNP, DateTime DataNasterii)>> GetDropdownOptionsAsync(
        bool activ = true,
        CancellationToken cancellationToken = default);

    // Write operations
    Task<Pacient> CreateAsync(Pacient pacient, CancellationToken cancellationToken = default);
    Task<Pacient> UpdateAsync(Pacient pacient, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, string modificatDe, CancellationToken cancellationToken = default);
    Task<bool> HardDeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // Business operations
    Task<string> GenerateNextCodPacientAsync(CancellationToken cancellationToken = default);
    Task<(bool CnpExists, bool CodPacientExists)> CheckUniqueAsync(
        string? cnp = null,
        string? codPacient = null,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
    Task<bool> UpdateUltimaVizitaAsync(
        Guid id,
        DateTime dataVizita,
        string modificatDe,
        CancellationToken cancellationToken = default);

    // Statistics
    Task<Dictionary<string, (int Total, int Activi)>> GetStatisticsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Pacient>> GetBirthdaysAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
}
