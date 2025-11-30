using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ValyanClinic.Application.Features.AuthManagement.Commands.Login;
using ValyanClinic.Application.Features.AuthManagement.DTOs;
using ValyanClinic.Components.Pages.Auth;
using MediatR;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ValyanClinic.Tests.Components.Auth;

/// <summary>
/// bUnit component tests for Login.razor component.
/// Tests rendering, form submission, validation, error display, and user interactions.
/// </summary>
public class LoginComponentTests : TestContext
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<AuthenticationStateProvider> _mockAuthStateProvider;
    private readonly Mock<NavigationManager> _mockNavigationManager;
    private readonly Mock<IJSRuntime> _mockJSRuntime;
    private readonly Mock<ILogger<Login>> _mockLogger;

    public LoginComponentTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        _mockNavigationManager = new Mock<NavigationManager>();
        _mockJSRuntime = new Mock<IJSRuntime>();
        _mockLogger = new Mock<ILogger<Login>>();

        // Register services
        Services.AddSingleton(_mockMediator.Object);
        Services.AddSingleton(_mockAuthStateProvider.Object);
        Services.AddSingleton(_mockNavigationManager.Object);
        Services.AddSingleton(_mockJSRuntime.Object);
        Services.AddSingleton(_mockLogger.Object);

        // Setup NavigationManager
        _mockNavigationManager.Setup(n => n.NavigateTo(It.IsAny<string>(), It.IsAny<bool>()));
    }

    #region Rendering Tests

    [Fact]
    public void Component_WhenRendered_ShouldDisplayLoginForm()
    {
        // Act
        var cut = RenderComponent<Login>();

        // Assert
        cut.Find("form").Should().NotBeNull();
        cut.Find("input[name='username']").Should().NotBeNull();
        cut.Find("input[name='password']").Should().NotBeNull();
        cut.Find("button[type='submit']").Should().NotBeNull();
    }

    [Fact]
    public void Component_WhenRendered_ShouldDisplayUsernameField()
    {
        // Act
        var cut = RenderComponent<Login>();

        // Assert
        var usernameInput = cut.Find("input[name='username']");
        usernameInput.GetAttribute("type").Should().Be("text");
        usernameInput.GetAttribute("autocomplete").Should().Be("username");
        usernameInput.GetAttribute("required").Should().NotBeNull();
    }

    [Fact]
    public void Component_WhenRendered_ShouldDisplayPasswordField()
    {
        // Act
        var cut = RenderComponent<Login>();

        // Assert
        var passwordInput = cut.Find("input[name='password']");
        passwordInput.GetAttribute("type").Should().Be("password");
        passwordInput.GetAttribute("autocomplete").Should().Be("current-password");
        passwordInput.GetAttribute("required").Should().NotBeNull();
    }

    [Fact]
    public void Component_WhenRendered_ShouldDisplayRememberMeCheckbox()
    {
        // Act
        var cut = RenderComponent<Login>();

        // Assert
        var rememberMeCheckbox = cut.Find("input[name='rememberMe']");
        rememberMeCheckbox.GetAttribute("type").Should().Be("checkbox");
    }

    [Fact]
    public void Component_WhenRendered_ShouldDisplaySubmitButton()
    {
        // Act
        var cut = RenderComponent<Login>();

        // Assert
        var submitButton = cut.Find("button[type='submit']");
        submitButton.TextContent.Should().Contain("Autentificare");
    }

    #endregion

    #region Password Toggle Tests

    [Fact]
    public void PasswordToggle_WhenClicked_ShouldChangePasswordVisibility()
    {
        // Arrange
        var cut = RenderComponent<Login>();
        var passwordInput = cut.Find("input[name='password']");
        var toggleButton = cut.Find(".password-toggle");

        // Assert initial state
        passwordInput.GetAttribute("type").Should().Be("password");

        // Act - Click toggle button
        toggleButton.Click();

        // Assert - Password should be visible
        passwordInput = cut.Find("input[name='password']");
        passwordInput.GetAttribute("type").Should().Be("text");

        // Act - Click toggle button again
        toggleButton.Click();

        // Assert - Password should be hidden again
        passwordInput = cut.Find("input[name='password']");
        passwordInput.GetAttribute("type").Should().Be("password");
    }

    [Fact]
    public void PasswordToggle_Icon_ShouldChangeWhenClicked()
    {
        // Arrange
        var cut = RenderComponent<Login>();
        var toggleButton = cut.Find(".password-toggle");

        // Assert initial icon (eye-slash = hidden)
        toggleButton.InnerHtml.Should().Contain("bi-eye-slash");

        // Act - Click to show password
        toggleButton.Click();

        // Assert - Icon should change to eye (visible)
        toggleButton = cut.Find(".password-toggle");
        toggleButton.InnerHtml.Should().Contain("bi-eye");
    }

    #endregion

    #region Form Submission Tests

    [Fact]
    public async Task Form_WhenSubmittedWithValidCredentials_ShouldCallMediator()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResultDto
            {
                Success = true,
                Username = "testuser",
                Email = "test@example.com",
                Rol = "Doctor",
                UtilizatorID = Guid.NewGuid(),
                PersonalMedicalID = Guid.NewGuid()
            });

        var cut = RenderComponent<Login>();
        var form = cut.Find("form");
        var usernameInput = cut.Find("input[name='username']");
        var passwordInput = cut.Find("input[name='password']");

        // Act
        usernameInput.Change("testuser");
        passwordInput.Change("ValidPassword123");
        await form.SubmitAsync();

        // Assert
        _mockMediator.Verify(
            m => m.Send(It.Is<LoginCommand>(cmd =>
                cmd.Username == "testuser" &&
                cmd.Password == "ValidPassword123"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Form_WhenSubmittedSuccessfully_ShouldNavigateToAppropriateUrl()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResultDto
            {
                Success = true,
                Username = "testuser",
                Email = "test@example.com",
                Rol = "Doctor",
                UtilizatorID = Guid.NewGuid(),
                PersonalMedicalID = Guid.NewGuid()
            });

        _mockJSRuntime
            .Setup(js => js.InvokeAsync<IJSVoidResult>(
                "ValyanAuth.login",
                It.IsAny<object[]>()))
            .ReturnsAsync(Mock.Of<IJSVoidResult>());

        var cut = RenderComponent<Login>();
        var form = cut.Find("form");
        var usernameInput = cut.Find("input[name='username']");
        var passwordInput = cut.Find("input[name='password']");

        // Act
        usernameInput.Change("testuser");
        passwordInput.Change("ValidPassword123");
        await form.SubmitAsync();

        // Allow async operations to complete
        await Task.Delay(100);

        // Assert
        _mockNavigationManager.Verify(
            n => n.NavigateTo(It.IsAny<string>(), false),
            Times.Once);
    }

    [Fact]
    public async Task Form_WhenSubmittedWithInvalidCredentials_ShouldDisplayErrorMessage()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResultDto
            {
                Success = false,
                ErrorMessage = "Nume de utilizator sau parolă incorecte"
            });

        var cut = RenderComponent<Login>();
        var form = cut.Find("form");
        var usernameInput = cut.Find("input[name='username']");
        var passwordInput = cut.Find("input[name='password']");

        // Act
        usernameInput.Change("wronguser");
        passwordInput.Change("WrongPassword123");
        await form.SubmitAsync();

        // Allow state update
        cut.Render();

        // Assert
        var errorMessage = cut.Find(".alert-danger");
        errorMessage.Should().NotBeNull();
        errorMessage.TextContent.Should().Contain("Nume de utilizator sau parolă incorecte");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Form_WhenUsernameIsEmpty_ShouldShowValidationError()
    {
        // Arrange
        var cut = RenderComponent<Login>();
        var form = cut.Find("form");
        var usernameInput = cut.Find("input[name='username']");
        var passwordInput = cut.Find("input[name='password']");

        // Act - Submit with empty username
        passwordInput.Change("ValidPassword123");
        form.Submit();

        // Assert
        var validationSummary = cut.Find(".validation-summary");
        validationSummary.Should().NotBeNull();
    }

    [Fact]
    public void Form_WhenPasswordIsEmpty_ShouldShowValidationError()
    {
        // Arrange
        var cut = RenderComponent<Login>();
        var form = cut.Find("form");
        var usernameInput = cut.Find("input[name='username']");
        var passwordInput = cut.Find("input[name='password']");

        // Act - Submit with empty password
        usernameInput.Change("testuser");
        form.Submit();

        // Assert
        var validationSummary = cut.Find(".validation-summary");
        validationSummary.Should().NotBeNull();
    }

    #endregion

    #region RememberMe Tests

    [Fact]
    public async Task RememberMe_WhenChecked_ShouldIncludeInCommand()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResultDto { Success = true });

        var cut = RenderComponent<Login>();
        var form = cut.Find("form");
        var usernameInput = cut.Find("input[name='username']");
        var passwordInput = cut.Find("input[name='password']");
        var rememberMeCheckbox = cut.Find("input[name='rememberMe']");

        // Act
        usernameInput.Change("testuser");
        passwordInput.Change("ValidPassword123");
        rememberMeCheckbox.Change(true);
        await form.SubmitAsync();

        // Assert
        _mockMediator.Verify(
            m => m.Send(It.Is<LoginCommand>(cmd => cmd.RememberMe == true),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public void Component_ShouldHaveAccessibleLabels()
    {
        // Act
        var cut = RenderComponent<Login>();

        // Assert
        var usernameInput = cut.Find("input[name='username']");
        usernameInput.GetAttribute("aria-label").Should().NotBeNullOrEmpty();

        var passwordInput = cut.Find("input[name='password']");
        passwordInput.GetAttribute("aria-label").Should().NotBeNullOrEmpty();

        var submitButton = cut.Find("button[type='submit']");
        submitButton.GetAttribute("aria-label").Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Component_ErrorMessage_ShouldHaveRoleAlert()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResultDto
            {
                Success = false,
                ErrorMessage = "Test error"
            });

        var cut = RenderComponent<Login>();
        var form = cut.Find("form");
        var usernameInput = cut.Find("input[name='username']");
        var passwordInput = cut.Find("input[name='password']");

        // Act
        usernameInput.Change("testuser");
        passwordInput.Change("password123");
        form.Submit();
        cut.Render();

        // Assert
        var errorDiv = cut.Find(".alert-danger");
        errorDiv.GetAttribute("role").Should().Be("alert");
    }

    #endregion

    #region Loading State Tests

    [Fact]
    public async Task Form_WhenSubmitting_ShouldShowLoadingState()
    {
        // Arrange
        var tcs = new TaskCompletionSource<LoginResultDto>();
        _mockMediator
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        var cut = RenderComponent<Login>();
        var form = cut.Find("form");
        var usernameInput = cut.Find("input[name='username']");
        var passwordInput = cut.Find("input[name='password']");

        // Act
        usernameInput.Change("testuser");
        passwordInput.Change("ValidPassword123");
        var submitTask = form.SubmitAsync();

        // Assert - Button should be disabled during submission
        var submitButton = cut.Find("button[type='submit']");
        submitButton.IsDisabled().Should().BeTrue();

        // Complete the submission
        tcs.SetResult(new LoginResultDto { Success = true });
        await submitTask;
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Form_WhenMediatorThrowsException_ShouldDisplayGenericError()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        var cut = RenderComponent<Login>();
        var form = cut.Find("form");
        var usernameInput = cut.Find("input[name='username']");
        var passwordInput = cut.Find("input[name='password']");

        // Act
        usernameInput.Change("testuser");
        passwordInput.Change("ValidPassword123");
        await form.SubmitAsync();
        cut.Render();

        // Assert
        var errorMessage = cut.Find(".alert-danger");
        errorMessage.Should().NotBeNull();
        errorMessage.TextContent.Should().Contain("eroare");
    }

    #endregion
}
