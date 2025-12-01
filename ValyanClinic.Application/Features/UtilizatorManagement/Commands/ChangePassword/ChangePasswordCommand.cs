using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.UtilizatorManagement.Commands.ChangePassword;

public record ChangePasswordCommand : IRequest<Result<bool>>
{
    public Guid UtilizatorID { get; init; }
    public string NewPassword { get; init; } = string.Empty;
    public string ModificatDe { get; init; } = "System";
}
