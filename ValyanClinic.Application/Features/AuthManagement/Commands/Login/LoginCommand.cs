using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.AuthManagement.Commands.Login;

/// <summary>
/// Command pentru autentificarea unui utilizator
/// </summary>
public record LoginCommand : IRequest<Result<LoginResultDto>>
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool RememberMe { get; init; }
    public bool ResetPasswordOnFirstLogin { get; init; }
}

/// <summary>
/// DTO returnat după autentificare
/// </summary>
public class LoginResultDto
{
    public Guid UtilizatorID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool RequiresPasswordReset { get; set; }
    public string? Token { get; set; } // Pentru JWT în viitor
}
