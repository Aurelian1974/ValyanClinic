using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Domain.Interfaces.Security;

namespace ValyanClinic.Application.Features.AuthManagement.Commands.Login;

/// <summary>
/// Handler pentru autentificarea utilizatorului
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResultDto>>
{
  private readonly IUtilizatorRepository _utilizatorRepository;
  private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUtilizatorRepository utilizatorRepository,
 IPasswordHasher passwordHasher,
    ILogger<LoginCommandHandler> logger)
    {
        _utilizatorRepository = utilizatorRepository;
    _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<LoginResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("========== LoginCommandHandler START ==========");
     _logger.LogInformation("Login attempt for username: {Username}", request.Username);

     // 1. Găsește utilizatorul după username
      var utilizator = await _utilizatorRepository.GetByUsernameAsync(request.Username, cancellationToken);

            if (utilizator == null)
      {
                _logger.LogWarning("Login failed: User not found - {Username}", request.Username);
     return Result<LoginResultDto>.Failure("Nume de utilizator sau parola incorecte");
}

       // 2. Verifică dacă utilizatorul este activ
  if (!utilizator.EsteActiv)
      {
            _logger.LogWarning("Login failed: User is inactive - {Username}", request.Username);
return Result<LoginResultDto>.Failure("Contul este inactiv. Contactati administratorul.");
            }

     // 3. Verifică parola (BCrypt face verificarea cu salt-ul inclus în hash)
    var passwordValid = _passwordHasher.VerifyPassword(request.Password, utilizator.PasswordHash);

  if (!passwordValid)
   {
                _logger.LogWarning("Login failed: Invalid password - {Username}", request.Username);

        // Incrementează încercări eșuate
        await _utilizatorRepository.IncrementIncercariEsuateAsync(utilizator.UtilizatorID, cancellationToken);

   return Result<LoginResultDto>.Failure("Nume de utilizator sau parola incorecte");
       }

            // 4. Verifică dacă contul este blocat (> 5 încercări eșuate)
    if (utilizator.NumarIncercariEsuate >= 5)
            {
      _logger.LogWarning("Login failed: Account locked due to too many failed attempts - {Username}", request.Username);
                return Result<LoginResultDto>.Failure("Cont blocat din cauza prea multor incercari esuate. Contactati administratorul.");
    }

     // 5. Login SUCCESS - Actualizează ultima autentificare
          await _utilizatorRepository.UpdateUltimaAutentificareAsync(utilizator.UtilizatorID, cancellationToken);

   _logger.LogInformation("Login successful for user: {Username}, Rol: {Rol}", request.Username, utilizator.Rol);

          // 6. Creează DTO rezultat
        var result = new LoginResultDto
   {
    UtilizatorID = utilizator.UtilizatorID,
        Username = utilizator.Username,
       Email = utilizator.Email ?? string.Empty,
          Rol = utilizator.Rol ?? "User",
             RequiresPasswordReset = request.ResetPasswordOnFirstLogin && utilizator.DataUltimaAutentificare == null
     };

  _logger.LogInformation("========== LoginCommandHandler END (SUCCESS) ==========");

            return Result<LoginResultDto>.Success(result, "Autentificare reusita");
        }
        catch (Exception ex)
        {
     _logger.LogError(ex, "========== LoginCommandHandler EXCEPTION ==========");
      _logger.LogError("Exception Type: {Type}", ex.GetType().FullName);
        _logger.LogError("Exception Message: {Message}", ex.Message);

            return Result<LoginResultDto>.Failure("A aparut o eroare la autentificare. Va rugam incercati din nou.");
        }
    }
}
