using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.DepartamentManagement.Commands.DeleteDepartament;

public record DeleteDepartamentCommand(Guid IdDepartament) : IRequest<Result<bool>>;
