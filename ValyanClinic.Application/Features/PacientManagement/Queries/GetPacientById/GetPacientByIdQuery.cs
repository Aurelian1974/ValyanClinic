using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById;

/// <summary>
/// Query pentru obtinerea detaliilor unui pacient
/// </summary>
public class GetPacientByIdQuery : IRequest<Result<PacientDetailDto>>
{
    public Guid Id { get; set; }

    public GetPacientByIdQuery(Guid id)
    {
        Id = id;
    }
}
