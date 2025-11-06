using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.UtilizatorManagement.Commands.UpdateUtilizator;

public record UpdateUtilizatorCommand : IRequest<Result<bool>>
{
    public Guid UtilizatorID { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
  public string Rol { get; init; } = string.Empty;
 public bool EsteActiv { get; init; }
    public string ModificatDe { get; init; } = "System";
}
