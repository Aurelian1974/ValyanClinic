using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetDoctorSchedule;

/// <summary>
/// Query pentru obținerea programului complet al unui medic pentru un interval de date (calendar view).
/// </summary>
public class GetDoctorScheduleQuery : IRequest<Result<IEnumerable<ProgramareListDto>>>
{
    /// <summary>
  /// ID-ul medicului.
    /// </summary>
    public Guid DoctorID { get; set; }

    /// <summary>
    /// Data de început a intervalului.
    /// </summary>
    public DateTime DataStart { get; set; }

    /// <summary>
 /// Data de sfârșit a intervalului.
    /// </summary>
    public DateTime DataEnd { get; set; }

    public GetDoctorScheduleQuery(Guid doctorID, DateTime dataStart, DateTime dataEnd)
    {
        DoctorID = doctorID;
  DataStart = dataStart;
        DataEnd = dataEnd;
    }

    public GetDoctorScheduleQuery()
    {
    }
}
