using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByDate;

/// <summary>
/// Query pentru obținerea tuturor programărilor dintr-o zi specifică.
/// Opțional: poate fi filtrat după un medic specific.
/// </summary>
public class GetProgramariByDateQuery : IRequest<Result<IEnumerable<ProgramareListDto>>>
{
    /// <summary>
    /// Data pentru care se caută programările.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// ID-ul medicului (opțional, null = toți medicii).
    /// </summary>
    public Guid? DoctorID { get; set; }

 public GetProgramariByDateQuery(DateTime date, Guid? doctorID = null)
    {
     Date = date;
        DoctorID = doctorID;
    }

  public GetProgramariByDateQuery()
    {
 }
}
