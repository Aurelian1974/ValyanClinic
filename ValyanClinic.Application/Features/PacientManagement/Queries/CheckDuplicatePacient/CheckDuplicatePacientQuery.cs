using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PacientManagement.Queries.CheckDuplicatePacient;

/// <summary>
/// Query pentru verificarea existenței unui pacient duplicat
/// Verifică după CNP și/sau Nume + Prenume + Data Nașterii
/// </summary>
public class CheckDuplicatePacientQuery : IRequest<Result<DuplicatePacientResult>>
{
    /// <summary>
    /// CNP-ul de verificat (optional)
    /// </summary>
    public string? CNP { get; set; }

    /// <summary>
    /// Numele pacientului
    /// </summary>
    public string? Nume { get; set; }

    /// <summary>
    /// Prenumele pacientului
    /// </summary>
    public string? Prenume { get; set; }

    /// <summary>
    /// Data nașterii pacientului
    /// </summary>
    public DateTime? DataNasterii { get; set; }

    /// <summary>
    /// ID-ul pacientului curent (pentru excludere la editare)
    /// </summary>
    public Guid? ExcludeId { get; set; }
}
