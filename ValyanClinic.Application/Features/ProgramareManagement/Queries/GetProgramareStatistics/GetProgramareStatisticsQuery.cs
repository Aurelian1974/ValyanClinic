using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramareStatistics;

/// <summary>
/// Query pentru obținerea statisticilor despre programări pentru un interval de date.
/// Util pentru dashboard-uri și raportare.
/// </summary>
public class GetProgramareStatisticsQuery : IRequest<Result<ProgramareStatisticsDto>>
{
    /// <summary>
    /// Data de început pentru statistici (implicit: prima zi a lunii curente).
    /// </summary>
    public DateTime? DataStart { get; set; }

    /// <summary>
    /// Data de sfârșit pentru statistici (implicit: ultima zi a lunii curente).
    /// </summary>
    public DateTime? DataEnd { get; set; }

    /// <summary>
    /// ID-ul medicului pentru statistici specifice (opțional, null = global).
    /// </summary>
    public Guid? DoctorID { get; set; }

    public GetProgramareStatisticsQuery(DateTime? dataStart = null, DateTime? dataEnd = null, Guid? doctorID = null)
    {
        DataStart = dataStart;
        DataEnd = dataEnd;
        DoctorID = doctorID;
    }

    public GetProgramareStatisticsQuery()
    {
    }
}
