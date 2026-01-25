using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.ExportPersonalMedical;

/// <summary>
/// Query pentru exportul datelor despre personalul medical (CSV sau Excel).
/// </summary>
public record ExportPersonalMedicalQuery : IRequest<Result<ExportPersonalMedicalResult>>
{
    /// <summary>
    /// Formatul de export: "csv" sau "excel"
    /// </summary>
    public string Format { get; init; } = "csv";

    /// <summary>
    /// Termen de căutare (nume, prenume, email)
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Filtru departament
    /// </summary>
    public string? Departament { get; init; }

    /// <summary>
    /// Filtru poziție
    /// </summary>
    public string? Pozitie { get; init; }

    /// <summary>
    /// Filtru status activ (null = toate)
    /// </summary>
    public bool? EsteActiv { get; init; }

    /// <summary>
    /// Coloană de sortare
    /// </summary>
    public string SortColumn { get; init; } = "Nume";

    /// <summary>
    /// Direcție sortare: "ASC" sau "DESC"
    /// </summary>
    public string SortDirection { get; init; } = "ASC";
}

/// <summary>
/// Rezultatul exportului - bytes și metadata pentru fișier
/// </summary>
public record ExportPersonalMedicalResult
{
    public byte[] FileBytes { get; init; } = Array.Empty<byte>();
    public string ContentType { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
}
