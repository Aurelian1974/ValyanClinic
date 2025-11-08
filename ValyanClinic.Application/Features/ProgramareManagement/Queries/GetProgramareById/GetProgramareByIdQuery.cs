using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;

namespace ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramareById;

/// <summary>
/// Query pentru obținerea detaliilor complete ale unei programări după ID.
/// </summary>
public class GetProgramareByIdQuery : IRequest<Result<ProgramareDetailDto>>
{
    /// <summary>
    /// ID-ul programării.
    /// </summary>
    public Guid ProgramareID { get; set; }

    public GetProgramareByIdQuery(Guid programareID)
    {
        ProgramareID = programareID;
    }

    // Constructor fără parametri pentru model binding
    public GetProgramareByIdQuery()
    {
    }
}
