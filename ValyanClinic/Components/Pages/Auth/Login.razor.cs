using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Services.Authentication;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Components.Pages.Auth;

public partial class Login : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<Login> Logger { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private CustomAuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;
    [Inject] private IUserSessionRepository UserSessionRepository { get; set; } = default!;

    // State
    private LoginFormModel LoginModel { get; set; } = new();
    private bool IsLoading { get; set; }
    private bool ShowPassword { get; set; }
    private string? ErrorMessage { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Load saved username if "Remember Me" was checked previously
            var savedUsername = await GetSavedUsername();
            if (!string.IsNullOrEmpty(savedUsername))
            {
                LoginModel.Username = savedUsername;
                LoginModel.RememberMe = true;
                StateHasChanged();
            }
        }
    }

    private async Task HandleLogin()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Logger.LogInformation("Attempting login for user: {Username}", LoginModel.Username);

            // Call API through JavaScript to ensure cookies are handled properly
            // NOTE: RememberMe nu se trimite la API - e doar pentru salvare username în localStorage
            var result = await JSRuntime.InvokeAsync<LoginResult>("ValyanAuth.login",
                 LoginModel.Username,
               LoginModel.Password,
                false, // RememberMe nu mai afectează cookie-ul (e întotdeauna session-only)
          LoginModel.ResetPasswordOnFirstLogin);

            if (result?.Success == true && result.Data != null)
            {
                Logger.LogInformation("Login successful for user: {Username}", LoginModel.Username);

                // ✅ CREARE SESIUNE ÎN BAZA DE DATE
                try
                {
                    var httpContext = HttpContextAccessor.HttpContext;
                    var adresaIP = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                    var userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
                    var dispozitiv = GetDeviceType(userAgent);

                    Logger.LogInformation("Creating session in database for user: {Username}, IP: {IP}",
                        LoginModel.Username, adresaIP);

                    var (sessionId, sessionToken) = await UserSessionRepository.CreateAsync(
                         result.Data.UtilizatorID,
                     adresaIP,
                     userAgent,
                         dispozitiv);

                    Logger.LogInformation("Session created successfully: {SessionID}, Token: {Token}",
                  sessionId, sessionToken);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error creating user session in database");
                    // Nu blocăm login-ul dacă sesiunea nu se creează - doar logăm eroarea
                }

                // Handle Remember Me - DOAR pentru username în localStorage
                if (LoginModel.RememberMe)
                {
                    await SaveUsername(LoginModel.Username);
                    Logger.LogInformation("Username saved to localStorage (RememberMe checked)");
                }
                else
                {
                    await ClearSavedUsername();
                    Logger.LogInformation("Username cleared from localStorage (RememberMe unchecked)");
                }

                // Notify Blazor about authentication change
                AuthStateProvider.NotifyAuthenticationChanged();

                // Handle Password Reset on First Login
                if (result.Data.RequiresPasswordReset)
                {
                    Logger.LogInformation("Password reset required for first login");
                    ErrorMessage = "Prima logare - ar trebui redirectionat la resetare parola";
                    await Task.Delay(2000);
                }

                // Redirect to dashboard with full page reload
                NavigationManager.NavigateTo("/dashboard", forceLoad: true);
            }
            else
            {
                ErrorMessage = result?.Message ?? "Eroare la autentificare";
                Logger.LogWarning("Login failed for user: {Username}, Error: {Error}",
              LoginModel.Username, ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during login for user: {Username}", LoginModel.Username);
            ErrorMessage = "A aparut o eroare la autentificare. Va rugam incercati din nou.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ✅ Helper pentru detectare tip dispozitiv
    private string GetDeviceType(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Desktop";

        var ua = userAgent.ToLower();

        if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone"))
            return "Mobile";

        if (ua.Contains("tablet") || ua.Contains("ipad"))
            return "Tablet";

        return "Desktop";
    }

    private void TogglePasswordVisibility()
    {
        ShowPassword = !ShowPassword;
    }

    private void HandleForgotPassword()
    {
        Logger.LogInformation("Forgot password clicked");
        // TODO: Implement forgot password functionality
    }

    #region LocalStorage Helpers

    private async Task<string?> GetSavedUsername()
    {
        try
        {
            return await JSRuntime.InvokeAsync<string>("localStorage.getItem", "rememberedUsername");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving saved username");
            return null;
        }
    }

    private async Task SaveUsername(string username)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "rememberedUsername", username);
            Logger.LogInformation("Username saved to localStorage: {Username}", username);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving username");
        }
    }

    private async Task ClearSavedUsername()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "rememberedUsername");
            Logger.LogInformation("Saved username cleared from localStorage");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error clearing saved username");
        }
    }

    #endregion

    // Form Model
    public class LoginFormModel
    {
        [Required(ErrorMessage = "Numele de utilizator este obligatoriu")]
        [StringLength(100, ErrorMessage = "Numele de utilizator nu poate depasi 100 de caractere")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parola este obligatorie")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Parola trebuie sa aiba intre 6 si 100 de caractere")]
        public string Password { get; set; } = string.Empty;

        // ✅ RememberMe = Salvează DOAR username-ul în localStorage (pentru convenience)
        // ❌ NU salvează parola, NU menține sesiunea, NU creează cookie persistent
        // Cookie-ul de autentificare este ÎNTOTDEAUNA session-only (se șterge la închiderea browser-ului)
        public bool RememberMe { get; set; } = false;

        public bool ResetPasswordOnFirstLogin { get; set; } = true;
    }

    // DTOs for JavaScript interop
    private class LoginResult
    {
        public bool Success { get; set; }
        public LoginResponseData? Data { get; set; }
        public string? Message { get; set; }
    }

    private class LoginResponseData
    {
        public bool Success { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; } = string.Empty;
        public string Rol { get; } = string.Empty;
        public Guid UtilizatorID { get; set; }
        public bool RequiresPasswordReset { get; set; }
    }
}
