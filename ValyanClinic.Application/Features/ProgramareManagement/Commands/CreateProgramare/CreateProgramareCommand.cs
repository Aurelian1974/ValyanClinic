using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.ProgramareManagement.Commands.CreateProgramare;

/// <summary>
/// Command pentru crearea unei programări noi.
/// Implementează pattern-ul CQRS cu MediatR.
/// </summary>
public class CreateProgramareCommand : IRequest<Result<Guid>>
{
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
    /// Statusul programării (default: "Programata").
    /// </summary>
    public string Status { get; set; } = "Programata";

    /// <summary>
    /// Observații.
    /// </summary>
    public string? Observatii { get; set; }

    /// <summary>
    /// ID-ul utilizatorului care creează programarea.
    /// </summary>
    public Guid CreatDe { get; set; }
}
