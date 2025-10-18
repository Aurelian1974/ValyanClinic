using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.DepartamentManagement.Commands.UpdateDepartament;

public class UpdateDepartamentCommand : IRequest<Result<bool>>
{
    public Guid IdDepartament { get; set; }
    public Guid? IdTipDepartament { get; set; }
    public string DenumireDepartament { get; set; } = string.Empty;
    public string? DescriereDepartament { get; set; }
}
