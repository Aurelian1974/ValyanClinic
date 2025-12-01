using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramariByPacient;

/// <summary>
/// Query pentru obținerea istoricului complet de programări al unui pacient.
/// </summary>
public class GetProgramariByPacientQuery : IRequest<Result<IEnumerable<ProgramareListDto>>>
{
    /// <summary>
    /// ID-ul pacientului.
    /// </summary>
    public Guid PacientID { get; set; }

    public GetProgramariByPacientQuery(Guid pacientID)
    {
        PacientID = pacientID;
    }

    public GetProgramariByPacientQuery()
    {
    }
}
