using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace ValyanClinic.Services.Authentication;

/// <summary>
/// Custom Authentication State Provider pentru ValyanClinic
/// Sincronizat cu Cookie Authentication
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;

    public CustomAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CustomAuthenticationStateProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            _logger.LogInformation("========== GetAuthenticationStateAsync START ==========");
   
            var httpContext = _httpContextAccessor.HttpContext;
         
            if (httpContext == null)
            {
                _logger.LogWarning("❌ HttpContext is NULL");
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            }

            _logger.LogInformation("✅ HttpContext available");
            _logger.LogInformation("   User.Identity.Name: {Name}", httpContext.User?.Identity?.Name ?? "NULL");
            _logger.LogInformation("   User.Identity.IsAuthenticated: {IsAuth}", httpContext.User?.Identity?.IsAuthenticated);
            _logger.LogInformation("   User.Claims.Count: {Count}", httpContext.User?.Claims?.Count() ?? 0);
       
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("✅ User authenticated: {Username}", httpContext.User.Identity.Name);
                
                // Log all claims for debugging
                foreach (var claim in httpContext.User.Claims)
                {
                    _logger.LogDebug("   Claim: {Type} = {Value}", claim.Type, claim.Value);
                }
                
                return Task.FromResult(new AuthenticationState(httpContext.User));
            }

            _logger.LogWarning("❌ No authenticated user found");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error retrieving authentication state");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
    }

    /// <summary>
    /// Notifică Blazor că starea de autentificare s-a schimbat
    /// Apelat după login/logout
    /// </summary>
    public void NotifyAuthenticationChanged()
    {
        _logger.LogInformation("🔔 NotifyAuthenticationChanged called");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
