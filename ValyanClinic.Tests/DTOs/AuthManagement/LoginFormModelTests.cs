using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using ValyanClinic.Application.Features.AuthManagement.DTOs;

namespace ValyanClinic.Tests.DTOs.AuthManagement;

/// <summary>
/// Unit tests for LoginFormModel data annotations validation.
/// Tests validation attributes on Username and Password properties.
/// </summary>
public class LoginFormModelTests
{
    #region Test Helpers

    /// <summary>
    /// Validates a model and returns validation results.
    /// </summary>
    private static List<ValidationResult> ValidateModel(LoginFormModel model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
        return validationResults;
    }

    #endregion

    #region Username Validation Tests

    [Fact]
    public void Username_WhenEmpty_ShouldFailValidation()
    {
        // Arrange
        var model = new LoginFormModel
        {
            Username = string.Empty,
            Password = "ValidPassword123"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains("Username"));
        results.First(r => r.MemberNames.Contains("Username"))
            .ErrorMessage.Should().Be("Numele de utilizator este obligatoriu");
    }

    [Fact]
    public void Username_WhenNull_ShouldFailValidation()
    {
        // Arrange
        var model = new LoginFormModel
        {
            Username = null!,
            Password = "ValidPassword123"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains("Username"));
    }

    [Theory]
    [InlineData("a")] // Valid (no MinimumLength on Username)
    [InlineData("ab")] // Valid (no MinimumLength on Username)
    [InlineData("abc")] // Valid
    public void Username_WithAnyLength_ShouldPassValidation_WhenNotExceedingMax(string username)
    {
        // Arrange
        var model = new LoginFormModel
        {
            Username = username,
            Password = "ValidPassword123"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().NotContain(r => r.MemberNames.Contains("Username"));
    }

    [Fact]
    public void Username_WhenExceedsMaxLength_ShouldFailValidation()
    {
        // Arrange
        var model = new LoginFormModel
        {
            Username = new string('a', 101), // 101 characters (exceeds max 100)
            Password = "ValidPassword123"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains("Username"));
        results.First(r => r.MemberNames.Contains("Username"))
            .ErrorMessage.Should().Contain("100 de caractere");
    }

    [Theory]
    [InlineData("validuser")] // Valid lowercase
    [InlineData("ValidUser")] // Valid mixed case
    [InlineData("valid.user")] // Valid with dot
    [InlineData("valid_user")] // Valid with underscore
    [InlineData("valid@user")] // Valid with @
    [InlineData("valid-user")] // Valid with dash
    [InlineData("user123")] // Valid with numbers
    [InlineData("123user")] // Valid starting with number
    public void Username_WhenValid_ShouldPassValidation(string username)
    {
        // Arrange
        var model = new LoginFormModel
        {
            Username = username,
            Password = "ValidPassword123"
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().NotContain(r => r.MemberNames.Contains("Username"));
    }

    #endregion

    #region Password Validation Tests

    [Fact]
    public void Password_WhenEmpty_ShouldFailValidation()
    {
        // Arrange
        var model = new LoginFormModel
        {
            Username = "validuser",
            Password = string.Empty
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains("Password"));
        results.First(r => r.MemberNames.Contains("Password"))
            .ErrorMessage.Should().Be("Parola este obligatorie");
    }

    [Fact]
    public void Password_WhenNull_ShouldFailValidation()
    {
        // Arrange
        var model = new LoginFormModel
        {
            Username = "validuser",
            Password = null!
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains("Password"));
    }

    [Theory]
    [InlineData("12345")] // Too short (5 chars)
    [InlineData("a")] // Way too short (1 char)
    [InlineData("")] // Empty
    public void Password_WhenTooShort_ShouldFailValidation(string password)
    {
        // Arrange
        var model = new LoginFormModel
        {
            Username = "validuser",
            Password = password
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().Contain(r => r.MemberNames.Contains("Password"));
    }

    [Fact]
    public void Password_WhenTooLong_ShouldFailValidation()
    {
        // Arrange
        var model = new LoginFormModel
        {
            Username = "validuser",
            Password = new string('a', 101) // 101 characters
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().ContainSingle(r => r.MemberNames.Contains("Password"));
        results.First(r => r.MemberNames.Contains("Password"))
            .ErrorMessage.Should().Contain("100 de caractere");
    }

    [Theory]
    [InlineData("123456")] // Minimum valid (6 chars)
    [InlineData("ValidPassword123")] // Valid with mixed case and numbers
    [InlineData("MyP@ssw0rd!")] // Valid with special characters
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // Maximum valid (100 chars)
    public void Password_WhenValid_ShouldPassValidation(string password)
    {
        // Arrange
        var model = new LoginFormModel
        {
            Username = "validuser",
            Password = password
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().NotContain(r => r.MemberNames.Contains("Password"));
    }

    #endregion

    #region Optional Fields Tests

    [Fact]
    public void RememberMe_DefaultValue_ShouldBeFalse()
    {
        // Arrange & Act
        var model = new LoginFormModel();

        // Assert
        model.RememberMe.Should().BeFalse();
    }

    [Fact]
    public void ResetPasswordOnFirstLogin_DefaultValue_ShouldBeTrue()
    {
        // Arrange & Act
        var model = new LoginFormModel();

        // Assert
        model.ResetPasswordOnFirstLogin.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RememberMe_CanBeSetToAnyBoolValue(bool value)
    {
        // Arrange
        var model = new LoginFormModel
        {
            Username = "validuser",
            Password = "ValidPassword123",
            RememberMe = value
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        model.RememberMe.Should().Be(value);
        results.Should().BeEmpty(); // RememberMe doesn't affect validation
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ResetPasswordOnFirstLogin_CanBeSetToAnyBoolValue(bool value)
    {
        // Arrange
        var model = new LoginFormModel
        {
            Username = "validuser",
            Password = "ValidPassword123",
            ResetPasswordOnFirstLogin = value
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        model.ResetPasswordOnFirstLogin.Should().Be(value);
        results.Should().BeEmpty(); // ResetPasswordOnFirstLogin doesn't affect validation
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void ValidModel_WithAllFieldsValid_ShouldPassValidation()
    {
        // Arrange
        var model = new LoginFormModel
        {
            Username = "valid.user@domain",
            Password = "SecureP@ssw0rd",
            RememberMe = true,
            ResetPasswordOnFirstLogin = false
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void InvalidModel_WithAllFieldsInvalid_ShouldFailValidationForBoth()
    {
        // Arrange
        var model = new LoginFormModel
        {
            Username = string.Empty,
            Password = string.Empty
        };

        // Act
        var results = ValidateModel(model);

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(r => r.MemberNames.Contains("Username"));
        results.Should().Contain(r => r.MemberNames.Contains("Password"));
    }

    #endregion
}
