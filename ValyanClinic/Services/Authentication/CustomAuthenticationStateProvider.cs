using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace ValyanClinic.Services.Authentication;

/// <summary>
/// Custom authentication state provider for ValyanClinic Blazor Server application.
/// Synchronizes with ASP.NET Core Cookie Authentication to provide authentication state to Blazor components.
/// </summary>
/// <remarks>
/// This provider:
/// - Reads authentication state from HTTP-only cookies set by AuthenticationController
/// - Provides authentication state to Blazor components via AuthenticationStateProvider
/// - Supports real-time authentication state changes via NotifyAuthenticationChanged()
/// - Works in Blazor Server context (has access to HttpContext)
/// 
/// Flow:
/// 1. Component requests authentication state (e.g., AuthorizeView)
/// 2. Provider reads HttpContext.User from cookie authentication
/// 3. Returns ClaimsPrincipal to Blazor
/// 4. On login/logout, NotifyAuthenticationChanged() triggers re-evaluation
/// 
/// Thread Safety:
/// - GetAuthenticationStateAsync() is called on Blazor circuit thread
/// - HttpContext is scoped to current request/circuit
/// - No shared mutable state between circuits
/// </remarks>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    #region Dependencies

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomAuthenticationStateProvider"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for current HTTP context</param>
    /// <param name="logger">Logger instance for diagnostics</param>
    public CustomAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CustomAuthenticationStateProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    #endregion

    #region AuthenticationStateProvider Implementation

    /// <summary>
    /// Gets the current authentication state asynchronously.
    /// Reads authentication state from HTTP cookie set by AuthenticationController.
    /// </summary>
    /// <returns>
    /// Task containing AuthenticationState with:
    /// - Authenticated ClaimsPrincipal if user is logged in
    /// - Anonymous ClaimsPrincipal if user is not authenticated
    /// </returns>
    /// <remarks>
    /// This method is called by Blazor whenever:
    /// - A component uses AuthorizeView or [Authorize] attribute
    /// - AuthenticationStateChanged event is triggered
    /// - Circuit initializes
    /// 
    /// Performance:
    /// - Minimal overhead (just reads HttpContext.User)
    /// - No database calls or external services
    /// - Synchronous read, wrapped in Task.FromResult
    /// 
    /// Error Handling:
    /// - Returns anonymous state if HttpContext is null (shouldn't happen in Blazor Server)
    /// - Returns anonymous state on any exception (logged for diagnostics)
    /// </remarks>
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;

            // HttpContext should always be available in Blazor Server
            // but check defensively to prevent NullReferenceException
            if (httpContext == null)
            {
                _logger.LogWarning("HttpContext is null in GetAuthenticationStateAsync - returning anonymous user");
                return CreateAnonymousStateAsync();
            }

            // Check if user is authenticated via cookie
            if (IsUserAuthenticated(httpContext))
            {
                // Log authentication check (DEBUG level for production)
                _logger.LogDebug("User authenticated: {Username}",
                    httpContext.User.Identity?.Name ?? "Unknown");

                // Return authenticated state with user's claims
                return Task.FromResult(new AuthenticationState(httpContext.User));
            }

            // User not authenticated
            _logger.LogDebug("User not authenticated - returning anonymous state");
            return CreateAnonymousStateAsync();
        }
        catch (Exception ex)
        {
            // Log error but don't throw - return anonymous state to prevent breaking UI
            _logger.LogError(ex, "Error retrieving authentication state - returning anonymous user");
            return CreateAnonymousStateAsync();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Notifies Blazor that the authentication state has changed.
    /// Triggers re-evaluation of authentication state across all components.
    /// </summary>
    /// <remarks>
    /// Call this method after:
    /// - Successful login (AuthenticationController.Login)
    /// - Logout (AuthenticationController.Logout)
    /// - Role changes
    /// - Permission updates
    /// 
    /// Effect:
    /// - All AuthorizeView components re-render
    /// - All [Authorize] checks re-evaluate
    /// - GetAuthenticationStateAsync() is called again
    /// 
    /// Performance:
    /// - Triggers Blazor re-render cycle
    /// - Should be called sparingly (only when auth state actually changes)
    /// </remarks>
    public void NotifyAuthenticationChanged()
    {
        _logger.LogInformation("Authentication state change notification triggered");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if the user is authenticated based on HttpContext.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    /// <returns>True if user is authenticated, false otherwise</returns>
    private static bool IsUserAuthenticated(HttpContext httpContext)
    {
        return httpContext.User?.Identity?.IsAuthenticated == true;
    }

    /// <summary>
    /// Creates an anonymous authentication state.
    /// Used when user is not authenticated or on error conditions.
    /// </summary>
    /// <returns>Task containing anonymous AuthenticationState</returns>
    private static Task<AuthenticationState> CreateAnonymousStateAsync()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        return Task.FromResult(new AuthenticationState(anonymousUser));
    }

    #endregion

    #region Debug Methods (Development Only)

#if DEBUG
    /// <summary>
    /// Logs detailed authentication state information for debugging (DEBUG ONLY).
    /// </summary>
    /// <remarks>
    /// ⚠️ THIS METHOD IS ONLY AVAILABLE IN DEBUG BUILDS.
    /// Logs all claims for the current user for troubleshooting authentication issues.
    /// Should NOT be available in production (sensitive data in claims).
    /// </remarks>
    public void LogDetailedAuthenticationState()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                _logger.LogWarning("[DEBUG] HttpContext is NULL");
                return;
            }

            _logger.LogInformation("[DEBUG] Authentication State Details:");
            _logger.LogInformation("  - User.Identity.Name: {Name}",
                httpContext.User?.Identity?.Name ?? "NULL");
            _logger.LogInformation("  - User.Identity.IsAuthenticated: {IsAuth}",
                httpContext.User?.Identity?.IsAuthenticated);
            _logger.LogInformation("  - User.Claims.Count: {Count}",
                httpContext.User?.Claims?.Count() ?? 0);

            if (httpContext.User?.Claims != null)
            {
                _logger.LogInformation("  - Claims:");
                foreach (var claim in httpContext.User.Claims)
                {
                    _logger.LogInformation("      {Type} = {Value}", claim.Type, claim.Value);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DEBUG] Error logging authentication state");
        }
    }
#endif

    #endregion
}
