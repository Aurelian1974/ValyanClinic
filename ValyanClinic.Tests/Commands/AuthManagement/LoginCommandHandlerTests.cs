using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ValyanClinic.Application.Features.AuthManagement.Commands.Login;
using ValyanClinic.Application.Features.AuthManagement.DTOs;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Domain.Interfaces.Security;

namespace ValyanClinic.Tests.Commands.AuthManagement;

/// <summary>
/// Unit tests for LoginCommandHandler business logic.
/// Tests authentication flow, error scenarios, and security features.
/// </summary>
public class LoginCommandHandlerTests
{
    private readonly Mock<IUtilizatorRepository> _mockRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ILogger<LoginCommandHandler>> _mockLogger;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _mockRepository = new Mock<IUtilizatorRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockLogger = new Mock<ILogger<LoginCommandHandler>>();
        
        _handler = new LoginCommandHandler(
            _mockRepository.Object,
            _mockPasswordHasher.Object,
            _mockLogger.Object);
    }

    #region Successful Login Tests

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = "ValidPassword123",
            RememberMe = false,
            ResetPasswordOnFirstLogin = false
        };

        var mockUser = CreateMockUser();
        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockUser);

        _mockPasswordHasher
            .Setup(h => h.VerifyPassword(command.Password, mockUser.PasswordHash))
            .Returns(true);

        _mockRepository
            .Setup(r => r.UpdateUltimaAutentificareAsync(mockUser.UtilizatorID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Username.Should().Be(mockUser.Username);
        result.Value.Email.Should().Be(mockUser.Email);
        result.Value.Rol.Should().Be(mockUser.Rol);
        result.Value.UtilizatorID.Should().Be(mockUser.UtilizatorID);
        result.Value.PersonalMedicalID.Should().Be(mockUser.PersonalMedicalID);
    }

    [Fact]
    public async Task Handle_WithValidCredentialsAndResetPassword_ShouldSetRequiresPasswordResetTrue()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "newuser",
            Password = "TempPassword123",
            ResetPasswordOnFirstLogin = true
        };

        var mockUser = CreateMockUser();
        mockUser.DataUltimaAutentificare = null; // First login

        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockUser);

        _mockPasswordHasher
            .Setup(h => h.VerifyPassword(command.Password, mockUser.PasswordHash))
            .Returns(true);

        _mockRepository
            .Setup(r => r.UpdateUltimaAutentificareAsync(mockUser.UtilizatorID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.RequiresPasswordReset.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ShouldUpdateLastAuthenticationDate()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = "ValidPassword123"
        };

        var mockUser = CreateMockUser();
        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockUser);

        _mockPasswordHasher
            .Setup(h => h.VerifyPassword(command.Password, mockUser.PasswordHash))
            .Returns(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r => r.UpdateUltimaAutentificareAsync(mockUser.UtilizatorID, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Failed Login - User Not Found Tests

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "nonexistent",
            Password = "AnyPassword123"
        };

        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Utilizator?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Contain("Nume de utilizator sau parolă incorecte");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldNotCallPasswordHasher()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "nonexistent",
            Password = "AnyPassword123"
        };

        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Utilizator?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockPasswordHasher.Verify(
            h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    #endregion

    #region Failed Login - Inactive Account Tests

    [Fact]
    public async Task Handle_WhenAccountInactive_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "inactiveuser",
            Password = "ValidPassword123"
        };

        var mockUser = CreateMockUser();
        mockUser.EsteActiv = false;

        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Contain("Contul este inactiv");
    }

    [Fact]
    public async Task Handle_WhenAccountInactive_ShouldNotCheckPassword()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "inactiveuser",
            Password = "ValidPassword123"
        };

        var mockUser = CreateMockUser();
        mockUser.EsteActiv = false;

        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockUser);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockPasswordHasher.Verify(
            h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    #endregion

    #region Failed Login - Account Locked Tests

    [Fact]
    public async Task Handle_WhenAccountLockedByFailedAttempts_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "lockeduser",
            Password = "ValidPassword123"
        };

        var mockUser = CreateMockUser();
        mockUser.NumarIncercariEsuate = 5; // Locked due to failed attempts

        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Contain("Cont blocat");
    }

    [Fact]
    public async Task Handle_WhenAccountLockedByDate_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "lockeduser",
            Password = "ValidPassword123"
        };

        var mockUser = CreateMockUser();
        mockUser.DataBlocare = DateTime.Now; // Locked by admin

        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Contain("Cont blocat");
    }

    [Fact]
    public async Task Handle_WhenAccountLocked_ShouldNotCheckPassword()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "lockeduser",
            Password = "ValidPassword123"
        };

        var mockUser = CreateMockUser();
        mockUser.NumarIncercariEsuate = 5;

        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockUser);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockPasswordHasher.Verify(
            h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    #endregion

    #region Failed Login - Invalid Password Tests

    [Fact]
    public async Task Handle_WhenPasswordInvalid_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = "WrongPassword123"
        };

        var mockUser = CreateMockUser();
        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockUser);

        _mockPasswordHasher
            .Setup(h => h.VerifyPassword(command.Password, mockUser.PasswordHash))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Contain("Nume de utilizator sau parolă incorecte");
    }

    [Fact]
    public async Task Handle_WhenPasswordInvalid_ShouldIncrementFailedAttempts()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = "WrongPassword123"
        };

        var mockUser = CreateMockUser();
        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockUser);

        _mockPasswordHasher
            .Setup(h => h.VerifyPassword(command.Password, mockUser.PasswordHash))
            .Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r => r.IncrementIncercariEsuateAsync(mockUser.UtilizatorID, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPasswordInvalid_ShouldNotUpdateLastAuthenticationDate()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = "WrongPassword123"
        };

        var mockUser = CreateMockUser();
        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockUser);

        _mockPasswordHasher
            .Setup(h => h.VerifyPassword(command.Password, mockUser.PasswordHash))
            .Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r => r.UpdateUltimaAutentificareAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = "ValidPassword123"
        };

        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Contain("A apărut o eroare");
    }

    [Fact]
    public async Task Handle_WhenPasswordHasherThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = "ValidPassword123"
        };

        var mockUser = CreateMockUser();
        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockUser);

        _mockPasswordHasher
            .Setup(h => h.VerifyPassword(command.Password, mockUser.PasswordHash))
            .Throws(new Exception("Hashing algorithm failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Contain("A apărut o eroare");
    }

    #endregion

    #region Security Tests

    [Fact]
    public async Task Handle_ShouldAlwaysReturnGenericMessageForInvalidCredentials()
    {
        // Arrange - User not found
        var command1 = new LoginCommand { Username = "nonexistent", Password = "Any123" };
        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command1.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Utilizator?)null);

        // Arrange - Wrong password
        var command2 = new LoginCommand { Username = "testuser", Password = "Wrong123" };
        var mockUser = CreateMockUser();
        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command2.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockUser);
        _mockPasswordHasher
            .Setup(h => h.VerifyPassword(command2.Password, mockUser.PasswordHash))
            .Returns(false);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert - Both should return the same generic message
        result1.FirstError.Should().Be(result2.FirstError);
        result1.FirstError.Should().Contain("Nume de utilizator sau parolă incorecte");
    }

    [Theory]
    [InlineData(0)] // No failed attempts
    [InlineData(1)] // One failed attempt
    [InlineData(2)] // Two failed attempts
    [InlineData(3)] // Three failed attempts
    [InlineData(4)] // Four failed attempts (not yet locked)
    public async Task Handle_WithFailedAttemptsLessThan5_ShouldAllowLogin(int failedAttempts)
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = "ValidPassword123"
        };

        var mockUser = CreateMockUser();
        mockUser.NumarIncercariEsuate = failedAttempts;

        _mockRepository
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockUser);

        _mockPasswordHasher
            .Setup(h => h.VerifyPassword(command.Password, mockUser.PasswordHash))
            .Returns(true);

        _mockRepository
            .Setup(r => r.UpdateUltimaAutentificareAsync(mockUser.UtilizatorID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a mock user entity for testing.
    /// </summary>
    private static Utilizator CreateMockUser()
    {
        return new Utilizator
        {
            UtilizatorID = Guid.NewGuid(),
            PersonalMedicalID = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            Rol = "Doctor",
            PasswordHash = "$2a$11$hashedpassword",
            Salt = "randomsalt",
            EsteActiv = true,
            NumarIncercariEsuate = 0,
            DataBlocare = null,
            DataUltimaAutentificare = DateTime.Now.AddDays(-1),
            DataCreare = DateTime.Now.AddMonths(-1),
            CreatDe = "Admin",
            DataUltimeiModificari = DateTime.Now.AddMonths(-1),
            ModificatDe = "Admin"
        };
    }

    #endregion
}
