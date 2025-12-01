using FluentAssertions;
using ValyanClinic.Application.Features.AuthManagement.Commands.Login;

namespace ValyanClinic.Tests.Commands.AuthManagement;

/// <summary>
/// Unit tests for LoginCommandValidator FluentValidation rules.
/// Tests username and password validation logic.
/// </summary>
public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    #region Username Validation Tests

    [Fact]
    public void Username_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = string.Empty,
            Password = "ValidPassword123"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Username" &&
            e.ErrorMessage.Contains("obligatoriu"));
    }

    [Theory]
    [InlineData("a")] // Too short (1 char, minimum is 3)
    [InlineData("ab")] // Too short (2 chars)
    public void Username_WhenTooShort_ShouldHaveValidationError(string username)
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = username,
            Password = "ValidPassword123"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Username" &&
            e.ErrorMessage.Contains("între 3 și 100 caractere"));
    }

    [Fact]
    public void Username_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = new string('a', 101), // 101 characters (max is 100)
            Password = "ValidPassword123"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Username" &&
            e.ErrorMessage.Contains("între 3 și 100 caractere"));
    }

    [Theory]
    [InlineData("user name")] // Space not allowed
    [InlineData("user#name")] // # not allowed
    [InlineData("user$name")] // $ not allowed
    [InlineData("user%name")] // % not allowed
    [InlineData("user&name")] // & not allowed
    [InlineData("user*name")] // * not allowed
    [InlineData("user(name")] // ( not allowed
    [InlineData("user)name")] // ) not allowed
    [InlineData("user+name")] // + not allowed
    [InlineData("user=name")] // = not allowed
    [InlineData("user[name")] // [ not allowed
    [InlineData("user]name")] // ] not allowed
    [InlineData("user{name")] // { not allowed
    [InlineData("user}name")] // } not allowed
    [InlineData("user;name")] // ; not allowed
    [InlineData("user:name")] // : not allowed
    [InlineData("user'name")] // ' not allowed
    [InlineData("user\"name")] // " not allowed
    [InlineData("user<name")] // < not allowed
    [InlineData("user>name")] // > not allowed
    [InlineData("user/name")] // / not allowed
    [InlineData("user\\name")] // \ not allowed
    [InlineData("user|name")] // | not allowed
    [InlineData("user?name")] // ? not allowed
    public void Username_WithInvalidCharacters_ShouldHaveValidationError(string username)
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = username,
            Password = "ValidPassword123"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Username" &&
            e.ErrorMessage.Contains("caracterele: . _ @ -"));
    }

    [Theory]
    [InlineData("validuser")] // Valid alphanumeric
    [InlineData("ValidUser")] // Valid mixed case
    [InlineData("valid.user")] // Valid with dot
    [InlineData("valid_user")] // Valid with underscore
    [InlineData("valid@user")] // Valid with @
    [InlineData("valid-user")] // Valid with dash
    [InlineData("user123")] // Valid with numbers
    [InlineData("123user")] // Valid starting with number
    [InlineData("user.name_test@domain-123")] // Valid with all allowed characters
    public void Username_WithValidCharacters_ShouldPassValidation(string username)
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = username,
            Password = "ValidPassword123"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.PropertyName == "Username");
    }

    #endregion

    #region Password Validation Tests

    [Fact]
    public void Password_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "validuser",
            Password = string.Empty
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Password" &&
            e.ErrorMessage.Contains("obligatorie"));
    }

    [Theory]
    [InlineData("12345")] // Too short (5 chars, minimum is 6)
    [InlineData("a")] // Way too short (1 char)
    [InlineData("abc")] // Too short (3 chars)
    public void Password_WhenTooShort_ShouldHaveValidationError(string password)
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "validuser",
            Password = password
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Password" &&
            e.ErrorMessage.Contains("între 6 și 100 caractere"));
    }

    [Fact]
    public void Password_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "validuser",
            Password = new string('a', 101) // 101 characters (max is 100)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Password" &&
            e.ErrorMessage.Contains("între 6 și 100 caractere"));
    }

    [Theory]
    [InlineData("123456")] // Minimum valid (6 chars)
    [InlineData("ValidPassword123")] // Valid with mixed case and numbers
    [InlineData("MyP@ssw0rd!")] // Valid with special characters
    [InlineData("password with spaces")] // Valid with spaces
    [InlineData("παράδειγμα")] // Valid with unicode (Greek)
    public void Password_WithValidLength_ShouldPassValidation(string password)
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "validuser",
            Password = password
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.PropertyName == "Password");
    }

    #endregion

    #region Optional Fields Tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RememberMe_AnyValue_ShouldNotAffectValidation(bool rememberMe)
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "validuser",
            Password = "ValidPassword123",
            RememberMe = rememberMe
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ResetPasswordOnFirstLogin_AnyValue_ShouldNotAffectValidation(bool resetPassword)
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "validuser",
            Password = "ValidPassword123",
            ResetPasswordOnFirstLogin = resetPassword
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void ValidCommand_WithAllFieldsValid_ShouldPassValidation()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "valid.user@domain-123",
            Password = "SecureP@ssw0rd",
            RememberMe = true,
            ResetPasswordOnFirstLogin = false
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void InvalidCommand_WithAllFieldsInvalid_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = string.Empty,
            Password = string.Empty
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterThanOrEqualTo(2);
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Command_WithMinimumValidValues_ShouldPassValidation()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "abc", // Minimum 3 characters
            Password = "123456" // Minimum 6 characters
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Command_WithMaximumValidValues_ShouldPassValidation()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = new string('a', 100), // Maximum 100 characters
            Password = new string('b', 100) // Maximum 100 characters
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
