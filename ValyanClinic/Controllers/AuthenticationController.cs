using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MediatR;
using ValyanClinic.Application.Features.AuthManagement.Commands.Login;

namespace ValyanClinic.Controllers;

/// <summary>
/// API Controller pentru autentificare - setează cookies ÎNAINTE de Blazor rendering
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
 private readonly IMediator _mediator;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
   IMediator mediator,
        ILogger<AuthenticationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
    try
        {
    _logger.LogInformation("API Login attempt for user: {Username}", request.Username);

            var command = new LoginCommand
      {
     Username = request.Username,
     Password = request.Password,
                RememberMe = request.RememberMe,
     ResetPasswordOnFirstLogin = request.ResetPasswordOnFirstLogin
  };

            var result = await _mediator.Send(command);

     if (!result.IsSuccess || result.Value == null)
  {
   _logger.LogWarning("API Login failed for user: {Username}", request.Username);
     return Unauthorized(new { message = result.FirstError ?? "Autentificare esuata" });
      }

         // Create claims
            var claims = new[]
   {
    new Claim(ClaimTypes.NameIdentifier, result.Value.UtilizatorID.ToString()),
       new Claim(ClaimTypes.Name, result.Value.Username),
      new Claim(ClaimTypes.Email, result.Value.Email),
   new Claim(ClaimTypes.Role, result.Value.Rol),
        new Claim("PersonalMedicalID", result.Value.PersonalMedicalID.ToString()), // ✅ NOU: PersonalMedicalID
    new Claim("LoginTime", DateTime.UtcNow.ToString("O"))
    };

       var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // ✅ Sign in cu sliding expiration (30 minute timeout)
    // Cookie-ul expira după 30 minute de inactivitate
 await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
  principal,
                new AuthenticationProperties
   {
 // ✅ Session cookie (nu persistă între restart-uri browser)
     IsPersistent = false,
          
   // ✅ NU setăm ExpiresUtc explicit
    // ExpireTimeSpan din Program.cs (30 minute) se va aplica automat
         ExpiresUtc = null,
      
      // ✅ Allow sliding expiration
             AllowRefresh = true,
       
     // ✅ IssuedUtc pentru tracking
        IssuedUtc = DateTimeOffset.UtcNow
         });

 // CRITICAL: Update HttpContext.User manually for current request
            // SignInAsync only sets the cookie for NEXT request, not current one
     HttpContext.User = principal;

    _logger.LogInformation("✅ COOKIE CREATED:");
            _logger.LogInformation("  - Type: Session cookie (browser session)");
         _logger.LogInformation("  - Timeout: 30 minutes (sliding expiration)");
            _logger.LogInformation("  - IssuedUtc: {IssuedUtc}", DateTimeOffset.UtcNow);
            _logger.LogInformation("User authenticated: {IsAuthenticated}", HttpContext.User.Identity?.IsAuthenticated);
     _logger.LogInformation("API Login successful for user: {Username}", request.Username);

    return Ok(new LoginResponse
     {
                Success = true,
    Username = result.Value.Username,
       Email = result.Value.Email,
                Rol = result.Value.Rol,
            UtilizatorID = result.Value.UtilizatorID,
        PersonalMedicalID = result.Value.PersonalMedicalID, // ✅ NOU
  RequiresPasswordReset = result.Value.RequiresPasswordReset
   });
        }
catch (Exception ex)
        {
            _logger.LogError(ex, "API Login exception for user: {Username}", request.Username);
 return StatusCode(500, new { message = "A aparut o eroare la autentificare" });
     }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
         _logger.LogInformation("API Logout successful");
            return Ok(new { success = true });
        }
      catch (Exception ex)
        {
 _logger.LogError(ex, "API Logout exception");
          return StatusCode(500, new { message = "A aparut o eroare la deconectare" });
        }
    }

    [HttpGet("check")]
    public IActionResult CheckAuthentication()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
   return Ok(new
            {
    authenticated = true,
     username = User.Identity.Name,
      role = User.FindFirst(ClaimTypes.Role)?.Value
            });
    }

return Ok(new { authenticated = false });
  }

 [HttpPost("test-hash")]
    public IActionResult TestHash([FromBody] TestHashRequest request)
    {
      try
        {
        _logger.LogInformation("Testing hash for password: {Password}", request.Password);
            
          // Use the BCrypt hasher from DI
            var hasher = new ValyanClinic.Infrastructure.Security.BCryptPasswordHasher(_logger as ILogger<ValyanClinic.Infrastructure.Security.BCryptPasswordHasher>);
    
    // Test verification
            bool isValid = hasher.VerifyPassword(request.Password, request.Hash);
            
            _logger.LogInformation("Hash verification result: {Result}", isValid);
  
       // Generate new hash for comparison
     string newHash = hasher.HashPassword(request.Password);
     
   return Ok(new
  {
        inputPassword = request.Password,
       inputHash = request.Hash,
     verificationResult = isValid,
                newHashGenerated = newHash,
         message = isValid ? "Hash is VALID" : "Hash is INVALID - use newHashGenerated"
         });
  }
        catch (Exception ex)
 {
        _logger.LogError(ex, "Error testing hash");
    return StatusCode(500, new { error = ex.Message });
   }
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public bool ResetPasswordOnFirstLogin { get; set; }
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
  public Guid UtilizatorID { get; set; }
    public Guid PersonalMedicalID { get; set; } // ✅ NOU: Pentru PersonalMedical
    public bool RequiresPasswordReset { get; set; }
}

public class TestHashRequest
{
    public string Password { get; set; } = string.Empty;
 public string Hash { get; set; } = string.Empty;
}
