using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.UtilizatorManagement.Commands.CreateUtilizator;

public record CreateUtilizatorCommand : IRequest<Result<Guid>>
{
    public Guid PersonalMedicalID { get; init; }
 public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty; // Plain password - will be hashed
    public string Rol { get; init; } = "Utilizator";
    public bool EsteActiv { get; init; } = true;
    public string CreatDe { get; init; } = "System";
}
