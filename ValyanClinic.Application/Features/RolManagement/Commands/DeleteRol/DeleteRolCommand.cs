using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.RolManagement.Commands.DeleteRol;

/// <summary>
/// Command pentru ștergerea unui rol.
/// </summary>
public record DeleteRolCommand(Guid Id) : IRequest<Result>;
