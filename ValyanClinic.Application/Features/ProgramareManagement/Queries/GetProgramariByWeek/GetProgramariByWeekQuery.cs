using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByWeek;

/// <summary>
/// ✅ NEW Query - Get all programări for a WEEK (7 days) with a SINGLE DB call
/// PERFORMANCE: Replaces 7 separate GetProgramariByDateQuery calls
/// </summary>
public class GetProgramariByWeekQuery : IRequest<Result<IEnumerable<ProgramareListDto>>>
{
    /// <summary>
    /// Start date of the week (Monday)
    /// </summary>
    public DateTime WeekStartDate { get; set; }

    /// <summary>
    /// End date of the week (Sunday)
    /// </summary>
    public DateTime WeekEndDate { get; set; }

    /// <summary>
    /// Optional doctor filter (null = all doctors)
    /// </summary>
    public Guid? DoctorID { get; set; }

    public GetProgramariByWeekQuery(DateTime weekStartDate, DateTime weekEndDate, Guid? doctorID = null)
    {
        WeekStartDate = weekStartDate;
        WeekEndDate = weekEndDate;
        DoctorID = doctorID;
    }

    public GetProgramariByWeekQuery()
    {
    }
}
