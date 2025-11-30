using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Services.Authentication;
using ValyanClinic.Domain.Interfaces.Repositories;
using System.Text.Json.Serialization;

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
            var result = await JSRuntime.InvokeAsync<LoginResult>("ValyanAuth.login",
                 LoginModel.Username,
               LoginModel.Password,
                false, // RememberMe nu mai afectează cookie-ul (e întotdeauna session-only)
          LoginModel.ResetPasswordOnFirstLogin);

            // ✅ LOGGING pentru debugging
            Logger.LogInformation("Login API Response - Success: {Success}", result?.Success ?? false);
            if (result?.Data != null)
{
       Logger.LogInformation("Login Data - Username: {Username}, Rol: {Rol}", 
           result.Data.Username, result.Data.Rol);
            }

            if (result?.Success == true && result.Data != null)
            {
                Logger.LogInformation("Login successful for user: {Username}, Rol: {Rol}", 
        LoginModel.Username, result.Data.Rol);

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
         }

     // Handle Remember Me
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

    // ✅ WAIT pentru ca authentication state să se propage
   await Task.Delay(50); // Redus de la 100ms la 50ms

  // ✅ REDIRECT BAZAT PE ROL cu forceLoad: TRUE
            // forceLoad: TRUE este NECESAR pentru ca cookie-ul să fie citit corect!
            string redirectUrl = result.Data.Rol switch
            {
                "Doctor" or "Medic" => "/dashboard/medic",
                "Receptioner" => "/dashboard/receptioner",
                "Administrator" or "Admin" => "/dashboard",
                "Asistent" or "Asistent Medical" => "/dashboard",
                "Manager" => "/dashboard",
                _ => "/dashboard"
            };

            Logger.LogInformation("🔄 Redirecting user {Username} with role {Rol} to {Url}", 
                LoginModel.Username, result.Data.Rol, redirectUrl);

            // ✅ forceLoad: TRUE - reîncarcă pagina complet pentru a citi cookie-ul
            // Session cookie (IsPersistent = false) se va șterge când închizi TOATE ferestrele browser-ului
            NavigationManager.NavigateTo(redirectUrl, forceLoad: true);
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

    public class LoginFormModel
    {
        [Required(ErrorMessage = "Numele de utilizator este obligatoriu")]
        [StringLength(100, ErrorMessage = "Numele de utilizator nu poate depasi 100 de caractere")]
    public string Username { get; set; } = string.Empty;

      [Required(ErrorMessage = "Parola este obligatorie")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Parola trebuie sa aibe intre 6 si 100 de caractere")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;

        public bool ResetPasswordOnFirstLogin { get; set; } = true;
    }

    // ✅ DTOs CORECTATE pentru JavaScript interop (lowercase properties pentru JSON)
    private class LoginResult
    {
 [JsonPropertyName("success")]
  public bool Success { get; set; }
        
   [JsonPropertyName("data")]
        public LoginResponseData? Data { get; set; }
 
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    private class LoginResponseData
    {
        [JsonPropertyName("success")]
     public bool Success { get; set; }
    
     [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
        
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        
        [JsonPropertyName("rol")]
 public string Rol { get; set; } = string.Empty;
        
 [JsonPropertyName("utilizatorID")]
        public Guid UtilizatorID { get; set; }
        
        [JsonPropertyName("requiresPasswordReset")]
        public bool RequiresPasswordReset { get; set; }
    }
}
