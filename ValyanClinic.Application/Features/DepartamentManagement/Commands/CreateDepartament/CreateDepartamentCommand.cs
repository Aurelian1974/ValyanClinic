using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.DepartamentManagement.Commands.CreateDepartament;

public class CreateDepartamentCommand : IRequest<Result<Guid>>
{
    public Guid? IdTipDepartament { get; set; }
    public string DenumireDepartament { get; set; } = string.Empty;
    public string? DescriereDepartament { get; set; }
}
