using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Services.Authentication;
using ValyanClinic.Domain.Interfaces.Repositories;
using System.Text.Json.Serialization;
using ValyanClinic.Application.Features.AuthManagement.DTOs;

namespace ValyanClinic.Components.Pages.Auth;

/// <summary>
/// Login page component for user authentication.
/// Handles username/password authentication, remember me functionality,
/// session management, and role-based redirection.
/// </summary>
/// <remarks>
/// This component interacts with:
/// - JavaScript ValyanAuth.login API for cookie-based authentication
/// - UserSessionRepository for session tracking
/// - CustomAuthenticationStateProvider for Blazor auth state
/// 
/// Authentication flow:
/// 1. User submits credentials
/// 2. JS API validates and sets HTTP-only cookie
/// 3. Session created in database for audit
/// 4. Blazor auth state updated
/// 5. User redirected based on role
/// </remarks>
public partial class Login : ComponentBase
{
    #region Constants

    /// <summary>
    /// JavaScript function name for login API call
    /// </summary>
    private const string JS_LOGIN_FUNCTION = "ValyanAuth.login";

    /// <summary>
    /// LocalStorage key for remembering username
    /// </summary>
    private const string LOCALSTORAGE_USERNAME_KEY = "rememberedUsername";

    /// <summary>
    /// Delay in milliseconds to allow authentication state to propagate
    /// </summary>
    private const int AUTH_STATE_PROPAGATION_DELAY_MS = 50;

    /// <summary>
    /// Delay in milliseconds before redirect for password reset notification
    /// </summary>
    private const int PASSWORD_RESET_NOTIFICATION_DELAY_MS = 2000;

    #endregion

    #region Dependency Injection

    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<Login> Logger { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private CustomAuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;
    [Inject] private IUserSessionRepository UserSessionRepository { get; set; } = default!;

    #endregion

    #region State

    /// <summary>
    /// Form model containing username, password, and authentication options
    /// </summary>
    private LoginFormModel LoginModel { get; set; } = new();

    /// <summary>
    /// Indicates if login operation is in progress
    /// </summary>
    private bool IsLoading { get; set; }

    /// <summary>
    /// Indicates if password should be visible (text input)
    /// </summary>
    private bool ShowPassword { get; set; }

    /// <summary>
    /// Error message to display to user (null if no error)
    /// </summary>
    private string? ErrorMessage { get; set; }

    #endregion

    #region Computed Properties for Markup

    /// <summary>
    /// Determines if there's an error message to display
    /// </summary>
    private bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Returns the input type for password field (text when visible, password when hidden)
    /// </summary>
    private string PasswordInputType => ShowPassword ? "text" : "password";

    /// <summary>
    /// Returns the FontAwesome icon class for password toggle button
    /// </summary>
    private string PasswordToggleIcon => ShowPassword ? "fa-eye-slash" : "fa-eye";

    /// <summary>
    /// Returns the ARIA label for password toggle button (for screen readers)
    /// </summary>
    private string PasswordToggleAriaLabel => ShowPassword ? "Ascunde parola" : "Afișează parola";

    /// <summary>
    /// Returns the title tooltip for password toggle button
    /// </summary>
    private string PasswordToggleTitle => ShowPassword ? "Ascunde parola" : "Afișează parola";

    /// <summary>
    /// Returns the ARIA label for login button with context-aware text
    /// </summary>
    private string LoginButtonAriaLabel => IsLoading
        ? "Se autentifică, vă rugăm așteptați"
        : "Autentificare în sistem";

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// Called after the component has finished rendering.
    /// On first render, attempts to load saved username from localStorage.
    /// </summary>
    /// <param name="firstRender">True if this is the first render after initialization</param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Attempt to restore previously saved username
            var savedUsername = await GetSavedUsernameAsync();
            if (!string.IsNullOrEmpty(savedUsername))
            {
                LoginModel.Username = savedUsername;
                LoginModel.RememberMe = true;
                StateHasChanged(); // Trigger re-render with populated username
            }
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles the login form submission.
    /// Validates credentials via JavaScript API, creates user session,
    /// manages remember me functionality, and redirects based on user role.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// Authentication flow:
    /// 1. Call JS API to validate credentials and set HTTP-only cookie
    /// 2. Create session record in database for audit trail
    /// 3. Save/clear username based on RememberMe checkbox
    /// 4. Notify Blazor about authentication state change
    /// 5. Redirect to role-specific dashboard
    /// 
    /// Error handling:
    /// - Invalid credentials: Display error message from API
    /// - Network errors: Display generic error message
    /// - Session creation errors: Log but don't block login
    /// </remarks>
    private async Task HandleLoginAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Logger.LogInformation("Login attempt for user: {Username}", LoginModel.Username);

            // Call JavaScript API to validate credentials and set authentication cookie
            // Note: Cookie is HTTP-only and managed by the API Controller, not by Blazor
            var result = await JSRuntime.InvokeAsync<LoginResult>(
                JS_LOGIN_FUNCTION,
                LoginModel.Username,
                LoginModel.Password,
                false, // RememberMe doesn't affect cookie (always session-only)
                LoginModel.ResetPasswordOnFirstLogin);

            Logger.LogInformation("Login API Response - Success: {Success}", result?.Success ?? false);

            if (result?.Data != null)
            {
                Logger.LogInformation("Login Data - Username: {Username}, Role: {Role}",
                    result.Data.Username, result.Data.Rol);
            }

            if (result?.Success == true && result.Data != null)
            {
                await HandleSuccessfulLoginAsync(result.Data);
            }
            else
            {
                await HandleFailedLoginAsync(result?.Message);
            }
        }
        catch (JSException jsEx)
        {
            // JavaScript interop error (e.g., API not available, network error)
            Logger.LogError(jsEx, "JavaScript error during login for user: {Username}", LoginModel.Username);
            ErrorMessage = "Eroare de comunicare cu serverul. Verificați conexiunea la internet.";
        }
        catch (Exception ex)
        {
            // Unexpected error
            Logger.LogError(ex, "Unexpected error during login for user: {Username}", LoginModel.Username);
            ErrorMessage = "A apărut o eroare neașteptată. Vă rugăm încercați din nou.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Handles successful login workflow.
    /// Creates user session, manages remember me, notifies auth state, and redirects.
    /// </summary>
    /// <param name="loginData">Authenticated user data from API</param>
    private async Task HandleSuccessfulLoginAsync(LoginResponseData loginData)
    {
        Logger.LogInformation("Login successful for user: {Username}, Role: {Role}",
            loginData.Username, loginData.Rol);

        // Create session in database for security audit and tracking
        await CreateUserSessionAsync(loginData.UtilizatorID);

        // Handle Remember Me functionality (save username to localStorage)
        await ManageRememberMeAsync();

        // Notify Blazor authentication state provider about the login
        AuthStateProvider.NotifyAuthenticationChanged();

        // Handle first-time login password reset requirement
        if (loginData.RequiresPasswordReset)
        {
            Logger.LogInformation("Password reset required for first login");
            ErrorMessage = "Prima logare - veți fi redirecționat la pagina de resetare parolă";
            await Task.Delay(PASSWORD_RESET_NOTIFICATION_DELAY_MS);
            // TODO: Implement redirect to password reset page
        }

        // Small delay to allow authentication state to propagate through Blazor
        await Task.Delay(AUTH_STATE_PROPAGATION_DELAY_MS);

        // Redirect to role-specific dashboard
        var redirectUrl = GetRoleBasedRedirectUrl(loginData.Rol);

        Logger.LogInformation("Redirecting user {Username} with role {Role} to {Url}",
            loginData.Username, loginData.Rol, redirectUrl);

        // Force reload to ensure cookie is read correctly by Blazor Server
        // Session cookie will persist until browser is completely closed
        NavigationManager.NavigateTo(redirectUrl, forceLoad: true);
    }

    /// <summary>
    /// Handles failed login attempt.
    /// Logs failure and displays appropriate error message to user.
    /// </summary>
    /// <param name="errorMessage">Error message from API or null</param>
    private async Task HandleFailedLoginAsync(string? errorMessage)
    {
        ErrorMessage = errorMessage ?? "Nume de utilizator sau parolă incorecte. Verificați datele introduse.";

        Logger.LogWarning("Login failed for user: {Username}, Error: {Error}",
            LoginModel.Username, ErrorMessage);

        // Add small delay for UX (prevents instant error flash)
        await Task.Delay(100);
    }

    /// <summary>
    /// Creates user session record in database for security audit and tracking.
    /// Session creation failure is logged but doesn't block login.
    /// </summary>
    /// <param name="utilizatorId">ID of the authenticated user</param>
    private async Task CreateUserSessionAsync(Guid utilizatorId)
    {
        try
        {
            var httpContext = HttpContextAccessor.HttpContext;

            // Extract client information for session tracking
            var adresaIP = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
            var dispozitiv = GetDeviceType(userAgent);

            Logger.LogInformation("Creating session in database - User: {Username}, IP: {IP}, Device: {Device}",
                LoginModel.Username, adresaIP, dispozitiv);

            // Create session record with unique token
            var (sessionId, sessionToken) = await UserSessionRepository.CreateAsync(
                utilizatorId,
                adresaIP,
                userAgent,
                dispozitiv);

            Logger.LogInformation("Session created successfully - SessionID: {SessionID}",
                sessionId);
        }
        catch (Exception ex)
        {
            // Log error but don't block login
            // Session tracking is important but not critical for authentication
            Logger.LogError(ex, "Error creating user session in database");
        }
    }

    /// <summary>
    /// Manages Remember Me functionality by saving or clearing username in localStorage.
    /// </summary>
    private async Task ManageRememberMeAsync()
    {
        if (LoginModel.RememberMe)
        {
            await SaveUsernameAsync(LoginModel.Username);
            Logger.LogInformation("Username saved to localStorage (RememberMe enabled)");
        }
        else
        {
            await ClearSavedUsernameAsync();
            Logger.LogInformation("Username cleared from localStorage (RememberMe disabled)");
        }
    }

    /// <summary>
    /// Determines device type from User-Agent string.
    /// Used for session tracking and analytics.
    /// </summary>
    /// <param name="userAgent">HTTP User-Agent header value</param>
    /// <returns>Device type: Mobile, Tablet, or Desktop</returns>
    private string GetDeviceType(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return "Desktop";

        var ua = userAgent.ToLower();

        // Check for mobile devices
        if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone"))
            return "Mobile";

        // Check for tablets
        if (ua.Contains("tablet") || ua.Contains("ipad"))
            return "Tablet";

        // Default to desktop
        return "Desktop";
    }

    /// <summary>
    /// Returns the appropriate redirect URL based on user role.
    /// </summary>
    /// <param name="role">User role from authentication</param>
    /// <returns>Dashboard URL for the given role</returns>
    private string GetRoleBasedRedirectUrl(string role) => role switch
    {
        "Doctor" or "Medic" => "/dashboard/medic",
        "Receptioner" => "/dashboard/receptioner",
        "Administrator" or "Admin" => "/dashboard",
        "Asistent" or "Asistent Medical" => "/dashboard",
        "Manager" => "/dashboard",
        _ => "/dashboard" // Default to main dashboard
    };

    /// <summary>
    /// Toggles password visibility between text and password input types.
    /// </summary>
    private void TogglePasswordVisibility()
    {
        ShowPassword = !ShowPassword;
    }

    /// <summary>
    /// Handles "Forgot Password" link click.
    /// Currently logs the event for future implementation.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// TODO: Implement forgot password workflow:
    /// - Navigate to password reset page
    /// - Send reset email with token
    /// - Validate token and allow password change
    /// </remarks>
    private async Task HandleForgotPasswordAsync()
    {
        Logger.LogInformation("Forgot password clicked for user context: {Username}",
            LoginModel.Username ?? "unknown");

        // Placeholder for future implementation
        await Task.CompletedTask;
    }

    #endregion

    #region LocalStorage Helpers

    /// <summary>
    /// Retrieves saved username from browser localStorage.
    /// Used to restore username when RememberMe was previously checked.
    /// </summary>
    /// <returns>Saved username or null if not found</returns>
    private async Task<string?> GetSavedUsernameAsync()
    {
        try
        {
            return await JSRuntime.InvokeAsync<string>("localStorage.getItem", LOCALSTORAGE_USERNAME_KEY);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving saved username from localStorage");
            return null;
        }
    }

    /// <summary>
    /// Saves username to browser localStorage for Remember Me functionality.
    /// </summary>
    /// <param name="username">Username to save</param>
    private async Task SaveUsernameAsync(string username)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", LOCALSTORAGE_USERNAME_KEY, username);
            Logger.LogDebug("Username saved to localStorage: {Username}", username);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving username to localStorage");
        }
    }

    /// <summary>
    /// Clears saved username from browser localStorage.
    /// Called when RememberMe is unchecked or on logout.
    /// </summary>
    private async Task ClearSavedUsernameAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("localStorage.removeItem", LOCALSTORAGE_USERNAME_KEY);
            Logger.LogDebug("Saved username cleared from localStorage");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error clearing saved username from localStorage");
        }
    }

    #endregion
}
