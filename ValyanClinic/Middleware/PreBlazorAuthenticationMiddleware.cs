using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ValyanClinic.Middleware;

/// <summary>
/// Middleware pentru autentificare care setează cookie-ul ÎNAINTE de Blazor rendering
/// </summary>
public class PreBlazorAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PreBlazorAuthenticationMiddleware> _logger;

    public PreBlazorAuthenticationMiddleware(
 RequestDelegate next,
        ILogger<PreBlazorAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check for authentication request header (set by Blazor component)
        if (context.Request.Headers.TryGetValue("X-Auth-Credentials", out var credentialsHeader))
        {
            try
            {
                var credentials = JsonSerializer.Deserialize<AuthCredentials>(credentialsHeader.ToString());

                if (credentials != null)
                {
                    // Here you would validate credentials and create claims
                    // For now, we'll assume validation is done elsewhere

                    var claims = new[]
                             {
   new Claim(ClaimTypes.NameIdentifier, credentials.UtilizatorID.ToString()),
         new Claim(ClaimTypes.Name, credentials.Username),
    new Claim(ClaimTypes.Email, credentials.Email),
     new Claim(ClaimTypes.Role, credentials.Rol),
    new Claim("LoginTime", DateTime.Now.ToString("O"))
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await context.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                      principal,
                                 new AuthenticationProperties
                                 {
                                     IsPersistent = credentials.RememberMe,
                                     ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                                 });

                    context.User = principal;

                    _logger.LogInformation("User authenticated via middleware: {Username}", credentials.Username);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PreBlazorAuthenticationMiddleware");
            }
        }

        await _next(context);
    }

    private class AuthCredentials
    {
        public Guid UtilizatorID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
