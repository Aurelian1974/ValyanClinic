using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.CheckProgramareConflict;

/// <summary>
/// Query pentru verificarea conflictelor de programare (overbooking detection).
/// Returnează true dacă există conflict, false dacă intervalul e liber.
/// </summary>
public class CheckProgramareConflictQuery : IRequest<Result<bool>>
{
    /// <summary>
    /// ID-ul medicului pentru care se verifică conflictul.
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
    /// ID-ul programării de exclus (pentru edit mode - null pentru create).
    /// </summary>
    public Guid? ExcludeProgramareID { get; set; }

    public CheckProgramareConflictQuery(
 Guid doctorID,
      DateTime dataProgramare,
        TimeSpan oraInceput,
        TimeSpan oraSfarsit,
        Guid? excludeProgramareID = null)
    {
        DoctorID = doctorID;
        DataProgramare = dataProgramare;
        OraInceput = oraInceput;
        OraSfarsit = oraSfarsit;
        ExcludeProgramareID = excludeProgramareID;
    }

    // Constructor fără parametri pentru model binding
    public CheckProgramareConflictQuery()
    {
    }
}
