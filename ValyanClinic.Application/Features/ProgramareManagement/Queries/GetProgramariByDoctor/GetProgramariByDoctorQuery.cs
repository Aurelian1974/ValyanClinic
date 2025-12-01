using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByDoctor;

/// <summary>
/// Query pentru obținerea programărilor unui medic într-un interval de date.
/// </summary>
public class GetProgramariByDoctorQuery : IRequest<Result<IEnumerable<ProgramareListDto>>>
{
    /// <summary>
    /// ID-ul medicului.
    /// </summary>
    public Guid DoctorID { get; set; }

    /// <summary>
    /// Data de început (implicit: azi).
    /// </summary>
    public DateTime? DataStart { get; set; }

    /// <summary>
    /// Data de sfârșit (implicit: azi + 7 zile).
    /// </summary>
    public DateTime? DataEnd { get; set; }

    public GetProgramariByDoctorQuery(Guid doctorID, DateTime? dataStart = null, DateTime? dataEnd = null)
    {
        DoctorID = doctorID;
        DataStart = dataStart;
        DataEnd = dataEnd;
    }

    public GetProgramariByDoctorQuery()
    {
    }
}
