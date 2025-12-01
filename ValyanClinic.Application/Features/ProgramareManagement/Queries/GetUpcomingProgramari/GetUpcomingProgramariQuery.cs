using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetUpcomingProgramari;

/// <summary>
/// Query pentru obținerea programărilor viitoare (următoarele N zile).
/// Util pentru dashboard-uri și reminder-uri.
/// </summary>
public class GetUpcomingProgramariQuery : IRequest<Result<IEnumerable<ProgramareListDto>>>
{
    /// <summary>
    /// Numărul de zile în viitor (implicit: 7).
    /// </summary>
    public int Days { get; set; } = 7;

    /// <summary>
    /// ID-ul medicului (opțional, null = toți medicii).
    /// </summary>
    public Guid? DoctorID { get; set; }

    public GetUpcomingProgramariQuery(int days = 7, Guid? doctorID = null)
    {
        Days = days;
        DoctorID = doctorID;
    }

    public GetUpcomingProgramariQuery()
    {
    }
}
