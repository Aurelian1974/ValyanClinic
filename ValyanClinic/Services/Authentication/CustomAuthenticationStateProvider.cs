using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace ValyanClinic.Services.Authentication;

/// <summary>
/// Custom Authentication State Provider pentru ValyanClinic
/// Gestionează starea de autentificare folosind Protected Session Storage
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;
    private const string UserSessionKey = "UserSession";

    public CustomAuthenticationStateProvider(
      ProtectedSessionStorage sessionStorage,
        ILogger<CustomAuthenticationStateProvider> logger)
    {
 _sessionStorage = sessionStorage;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSessionResult = await _sessionStorage.GetAsync<UserSession>(UserSessionKey);

      if (userSessionResult.Success && userSessionResult.Value != null)
     {
         var userSession = userSessionResult.Value;
             
    // Verifică dacă sesiunea este validă
     if (userSession.IsValid())
   {
              var claimsPrincipal = userSession.ToClaimsPrincipal();
         
     _logger.LogInformation("User authenticated: {Username}, Role: {Role}", 
              userSession.Username, userSession.Role);
          
        return new AuthenticationState(claimsPrincipal);
         }
        else
        {
  _logger.LogWarning("User session expired for: {Username}", userSession.Username);
              await ClearUserSession();
           }
  }

            // Nu există sesiune validă → utilizator neautentificat
   _logger.LogDebug("No valid user session found");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        catch (Exception ex)
  {
    _logger.LogError(ex, "Error retrieving authentication state");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
    }

    /// <summary>
    /// Marchează utilizatorul ca autentificat și salvează sesiunea
    /// </summary>
    public async Task MarkUserAsAuthenticated(string username, string email, string role, Guid utilizatorId)
    {
        try
        {
            var userSession = new UserSession
        {
                UtilizatorId = utilizatorId,
  Username = username,
         Email = email,
        Role = role,
           LoginTime = DateTime.Now,
 ExpirationTime = DateTime.Now.AddHours(8) // Sesiune 8 ore
            };

      await _sessionStorage.SetAsync(UserSessionKey, userSession);
            
 var claimsPrincipal = userSession.ToClaimsPrincipal();
    
            _logger.LogInformation("User marked as authenticated: {Username}, Role: {Role}", username, role);
            
       NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }
        catch (Exception ex)
     {
 _logger.LogError(ex, "Error marking user as authenticated");
            throw;
     }
    }

    /// <summary>
    /// Marchează utilizatorul ca neautentificat (logout)
    /// </summary>
    public async Task MarkUserAsLoggedOut()
    {
        try
        {
    await ClearUserSession();
       
            _logger.LogInformation("User logged out");
    
         var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking user as logged out");
          throw;
   }
    }

    /// <summary>
    /// Șterge sesiunea utilizatorului
    /// </summary>
    private async Task ClearUserSession()
    {
    try
    {
            await _sessionStorage.DeleteAsync(UserSessionKey);
        }
    catch (Exception ex)
        {
        _logger.LogError(ex, "Error clearing user session");
        }
    }
}

/// <summary>
/// Model pentru sesiunea utilizatorului
/// </summary>
public class UserSession
{
    public Guid UtilizatorId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
    public DateTime ExpirationTime { get; set; }

  /// <summary>
    /// Verifică dacă sesiunea este validă (nu a expirat)
    /// </summary>
    public bool IsValid()
    {
     return DateTime.Now < ExpirationTime;
    }

    /// <summary>
    /// Convertește sesiunea într-un ClaimsPrincipal
    /// </summary>
    public ClaimsPrincipal ToClaimsPrincipal()
    {
        var claims = new[]
        {
          new Claim(ClaimTypes.NameIdentifier, UtilizatorId.ToString()),
            new Claim(ClaimTypes.Name, Username),
        new Claim(ClaimTypes.Email, Email),
     new Claim(ClaimTypes.Role, Role),
            new Claim("LoginTime", LoginTime.ToString("O")),
            new Claim("ExpirationTime", ExpirationTime.ToString("O"))
        };

     var identity = new ClaimsIdentity(claims, "CustomAuthentication");
        return new ClaimsPrincipal(identity);
    }
}
