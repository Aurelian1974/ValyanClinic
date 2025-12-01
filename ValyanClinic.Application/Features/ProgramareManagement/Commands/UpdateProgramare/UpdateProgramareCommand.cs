using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.ProgramareManagement.Commands.UpdateProgramare;

/// <summary>
/// Command pentru actualizarea unei programări existente.
/// Implementează pattern-ul CQRS cu MediatR.
/// </summary>
public class UpdateProgramareCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// ID-ul programării de actualizat.
    /// </summary>
    public Guid ProgramareID { get; set; }

    /// <summary>
    /// ID-ul pacientului.
    /// </summary>
    public Guid PacientID { get; set; }

    /// <summary>
    /// ID-ul medicului.
    /// </summary>
    public Guid DoctorID { get; set; }

    /// <summary>
    /// Data programării.
    /// </summary>
    public DateTime DataProgramare { get; set; }

    /// <summary>
    /// Ora de început.
    /// </summary>
    public TimeSpan OraInceput { get; set; }

    /// <summary>
    /// Ora de sfârșit.
    /// </summary>
    public TimeSpan OraSfarsit { get; set; }

    /// <summary>
    /// Tipul programării.
    /// </summary>
    public string? TipProgramare { get; set; }

    /// <summary>
    /// Statusul programării.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Observații.
    /// </summary>
    public string? Observatii { get; set; }

    /// <summary>
    /// ID-ul utilizatorului care modifică programarea.
    /// </summary>
    public Guid ModificatDe { get; set; }
}
