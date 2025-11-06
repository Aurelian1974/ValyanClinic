using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.AuthManagement.Commands.Login;
using ValyanClinic.Services.Authentication;

namespace ValyanClinic.Components.Pages.Auth;

public partial class Login : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<Login> Logger { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private CustomAuthenticationStateProvider AuthStateProvider { get; set; } = default!;

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

            var command = new LoginCommand
            {
                Username = LoginModel.Username,
                Password = LoginModel.Password,
                RememberMe = LoginModel.RememberMe,
                ResetPasswordOnFirstLogin = LoginModel.ResetPasswordOnFirstLogin
            };

            var result = await Mediator.Send(command);

            if (result.IsSuccess && result.Value != null)
            {
                Logger.LogInformation("Login successful for user: {Username}", LoginModel.Username);

                // ✅ IMPORTANT: Marchează utilizatorul ca autentificat
                await AuthStateProvider.MarkUserAsAuthenticated(
                    result.Value.Username,
                    result.Value.Email,
                    result.Value.Rol,
                    result.Value.UtilizatorID);

                // Handle Remember Me
                if (LoginModel.RememberMe)
                {
                    await SaveUsername(LoginModel.Username);
                }
                else
                {
                    await ClearSavedUsername();
                }

                // Handle Password Reset on First Login
                if (result.Value.RequiresPasswordReset)
                {
                    Logger.LogInformation("Password reset required for first login");
                    // TODO: Navigate to password reset page
                    // NavigationManager.NavigateTo("/reset-password");
                    // return;

                    // Temporary: Show message and continue
                    ErrorMessage = "Prima logare - ar trebui redirectionat la resetare parola";
                    await Task.Delay(2000);
                }

                // ✅ Redirect to dashboard
                NavigationManager.NavigateTo("/dashboard", forceLoad: true);
            }
            else
            {
                ErrorMessage = result.FirstError ?? "Eroare la autentificare";
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

    private void TogglePasswordVisibility()
    {
        ShowPassword = !ShowPassword;
    }

    private void HandleForgotPassword()
    {
        Logger.LogInformation("Forgot password clicked");
        // TODO: Implement forgot password functionality
        // NavigationManager.NavigateTo("/forgot-password");
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

        public bool RememberMe { get; set; } = true; // Default checked

        public bool ResetPasswordOnFirstLogin { get; set; } = true; // Default checked
    }
}
