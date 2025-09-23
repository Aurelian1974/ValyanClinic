using Microsoft.AspNetCore.Components;
using ValyanClinic.Domain.Models;
using ValyanClinic.Application.Services;

namespace ValyanClinic.Components.Pages.LoginPage;

/// <summary>
/// Business Logic pentru Login.razor - REFACTORIZAT ?I ORGANIZAT ÎN FOLDER
/// Separat de markup, folose?te Rich Services ?i FluentValidation
/// </summary>
public partial class Login : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IAuthenticationService AuthService { get; set; } = default!;
    [Inject] private IUserSessionService SessionService { get; set; } = default!;

    // State Management separat în acela?i folder
    private LoginState _state = new();

    // Lifecycle Events
    protected override async Task OnInitializedAsync()
    {
        await CheckExistingSession();
    }

    #region Authentication Logic - Rich Service Integration

    private async Task HandleLogin()
    {
        try
        {
            if (!_state.CanAttemptLogin())
            {
                await ShowError("Nu se poate procesa cererea de autentificare în acest moment.");
                return;
            }

            _state.SetLoading(true);
            StateHasChanged();

            // Rich Authentication Service cu business logic
            var loginResult = await AuthService.AuthenticateAsync(_state.LoginRequest);
            
            // Result Pattern handling
            await HandleLoginResult(loginResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Login failed with exception: {ex.Message}");
            await ShowError("A ap?rut o eroare la autentificare. V? rug?m s? încerca?i din nou.");
        }
        finally
        {
            _state.SetLoading(false);
            StateHasChanged();
        }
    }

    private async Task HandleLoginResult(LoginResult result)
    {
        if (result.IsSuccess && result.UserSession != null)
        {
            _state.SetUser(result.UserSession);
            
            Console.WriteLine($"DEBUG: User authenticated successfully: {result.UserSession.Username}");
            
            // Salvez sesiunea
            await SaveUserSession(result.UserSession);
            
            // Navigate to dashboard after successful authentication
            await Task.Delay(500); // Show success briefly
            Navigation.NavigateTo("/dashboard", forceLoad: true);
        }
        else
        {
            // Specific error handling based on failure reason
            await HandleLoginFailure(result);
        }
    }

    private async Task HandleLoginFailure(LoginResult result)
    {
        switch (result.FailureReason)
        {
            case LoginFailureReason.ValidationError:
                _state.SetValidationErrors(result.Errors);
                break;
                
            case LoginFailureReason.AccountLocked:
                _state.IncrementLoginAttempts();
                var lockoutMessage = await AuthService.GetLockoutTimeRemainingAsync(_state.LoginRequest.Username);
                await ShowError(result.ErrorMessage ?? "Cont temporar blocat.");
                break;
                
            case LoginFailureReason.AccountDisabled:
                await ShowError("Contul este inactiv. Contacta?i administratorul.");
                break;
                
            case LoginFailureReason.TooManyAttempts:
                _state.IncrementLoginAttempts();
                await ShowError($"Prea multe încerc?ri. Încerc?ri r?mase: {5 - _state.LoginAttempts}");
                break;
                
            case LoginFailureReason.InvalidCredentials:
                _state.IncrementLoginAttempts();
                var remaining = 5 - _state.LoginAttempts;
                await ShowError($"Date de autentificare incorecte. Încerc?ri r?mase: {remaining}");
                break;
                
            case LoginFailureReason.SystemError:
            default:
                await ShowError(result.ErrorMessage ?? "A ap?rut o eroare de sistem.");
                break;
        }
    }

    #endregion

    #region UI Event Handlers

    private void TogglePasswordVisibility()
    {
        _state.TogglePasswordVisibility();
        StateHasChanged();
    }

    private async Task UseDemoCredentials(DemoCredential credential)
    {
        _state.LoginRequest.Username = credential.Username;
        _state.LoginRequest.Password = credential.Password;
        _state.ClearErrors();
        StateHasChanged();
        
        // Auto-focus pe butonul de login pentru UX mai bun
        await Task.Delay(100);
        StateHasChanged();
    }

    private void ToggleDemoCredentials()
    {
        _state.ShowDemoCredentials = !_state.ShowDemoCredentials;
        StateHasChanged();
    }

    #endregion

    #region Forgot Password & Support

    private async Task HandleForgotPassword()
    {
        await ShowInfo("Func?ionalitatea de resetare a parolei va fi implementat? în curând. Contacta?i administratorul pentru asisten??.");
    }

    private async Task ShowContactSupport()
    {
        await ShowInfo("Pentru suport tehnic, contacta?i echipa ValyanMed la support@valyanmed.ro sau +40 123 456 789.");
    }

    #endregion

    #region Session Management

    private async Task CheckExistingSession()
    {
        // În viitor: Check localStorage/cookie pentru sesiune existent?
        Console.WriteLine("DEBUG: Checking for existing user session...");
        await Task.CompletedTask;
    }

    private async Task SaveUserSession(UserSession userSession)
    {
        // Salvez sesiunea în serviciul de sesiuni
        var sessionToken = await SessionService.CreateSessionAsync(userSession);
        
        // În viitor: Salvez token-ul în localStorage pentru persisten??
        // await JSRuntime.InvokeVoidAsync("localStorage.setItem", "sessionToken", sessionToken);
        
        Console.WriteLine($"DEBUG: Session saved with token for user: {userSession.Username}");
        await Task.CompletedTask;
    }

    #endregion

    #region Error Handling & Messaging

    private async Task ShowError(string message)
    {
        _state.SetError(message);
        StateHasChanged();
        
        // Auto-clear error after 10 seconds
        await Task.Delay(10000);
        if (_state.ErrorMessage == message) // Only clear if it's still the same error
        {
            _state.ClearErrors();
            StateHasChanged();
        }
    }

    private async Task ShowInfo(string message)
    {
        // Pentru moment, folosesc console.log, în viitor toast notifications
        Console.WriteLine($"INFO: {message}");
        await Task.CompletedTask;
    }

    #endregion

    #region Keyboard Events

    private async Task HandleKeyPress(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && _state.CanAttemptLogin())
        {
            await HandleLogin();
        }
    }

    #endregion
}