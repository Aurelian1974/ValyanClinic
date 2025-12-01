using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ValyanClinic.Application.Features.PacientManagement.Commands.CreatePacient;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using Xunit;

namespace ValyanClinic.Tests.Application.PacientManagement.Commands;

/// <summary>
/// Unit tests for CreatePacientCommandHandler.
/// Tests business logic validation, repository interactions, and error handling.
/// </summary>
/// <remarks>
/// Pattern: AAA (Arrange-Act-Assert)
/// Tools: xUnit, Moq, FluentAssertions
/// Coverage: ~8 test scenarios for CreatePacientCommand handler
/// </remarks>
public class CreatePacientCommandHandlerTests
{
    private readonly Mock<IPacientRepository> _mockRepository;
    private readonly Mock<ILogger<CreatePacientCommandHandler>> _mockLogger;
    private readonly CreatePacientCommandHandler _handler;

    public CreatePacientCommandHandlerTests()
    {
        _mockRepository = new Mock<IPacientRepository>();
        _mockLogger = new Mock<ILogger<CreatePacientCommandHandler>>();
        _handler = new CreatePacientCommandHandler(_mockRepository.Object, _mockLogger.Object);
    }

    #region Valid Scenarios

    /// <summary>
    /// Tests that a valid command with all required fields returns success.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            CNP = "1234567890123",
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M",
            Telefon = "0712345678",
            Email = "ion.popescu@example.com",
            Judet = "Bucuresti",
            Localitate = "Sector 1",
            Adresa = "Str. Exemplu nr. 1",
            Activ = true,
            CreatDe = "TestUser"
        };

        var mockPacient = new Pacient
        {
            Id = Guid.NewGuid(),
            Cod_Pacient = "PAC00001",
            Nume = command.Nume,
            Prenume = command.Prenume,
            CNP = command.CNP,
            Data_Nasterii = command.Data_Nasterii,
            Sex = command.Sex
        };

        // Setup: GenerateNextCodPacientAsync
        _mockRepository
            .Setup(r => r.GenerateNextCodPacientAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("PAC00001");

        // Setup: CheckUniqueAsync (CNP and Cod are unique)
        _mockRepository
            .Setup(r => r.CheckUniqueAsync(It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, false));

        // Setup: CreateAsync
        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPacient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(mockPacient.Id);
        result.SuccessMessage.Should().Contain("succes");

        // Verify interactions
        _mockRepository.Verify(r => r.CheckUniqueAsync(
            command.CNP, null, null, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.GenerateNextCodPacientAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that a valid command without CNP (optional field) returns success.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommandWithoutCNP_ReturnsSuccess()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Pop",
            Prenume = "Maria",
            Data_Nasterii = new DateTime(1985, 5, 15),
            Sex = "F",
            Activ = true,
            CreatDe = "TestUser"
        };

        var mockPacient = new Pacient
        {
            Id = Guid.NewGuid(),
            Cod_Pacient = "PAC00002",
            Nume = command.Nume,
            Prenume = command.Prenume
        };

        _mockRepository
            .Setup(r => r.GenerateNextCodPacientAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("PAC00002");

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPacient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(mockPacient.Id);

        // Verify CNP uniqueness check was NOT called (CNP is empty)
        _mockRepository.Verify(r => r.CheckUniqueAsync(
            It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Missing Required Fields

    /// <summary>
    /// Tests that missing Nume (required field) returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_MissingNume_ReturnsFailure()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "", // Missing
            Prenume = "Ion",
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Numele este obligatoriu"));

        // Verify CreateAsync was NOT called
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that missing Prenume (required field) returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_MissingPrenume_ReturnsFailure()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "", // Missing
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Prenumele este obligatoriu"));

        // Verify CreateAsync was NOT called
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Duplicate CNP

    /// <summary>
    /// Tests that duplicate CNP returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_DuplicateCNP_ReturnsFailure()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            CNP = "1234567890123", // Duplicate
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M"
        };

        // Setup: CNP already exists
        _mockRepository
            .Setup(r => r.CheckUniqueAsync(command.CNP, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, false)); // CNP exists

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("CNP") && e.Contains("există deja"));

        // Verify CreateAsync was NOT called
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Invalid CNP Format

    /// <summary>
    /// Tests that CNP with invalid length returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_InvalidCNPLength_ReturnsFailure()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            CNP = "12345", // Invalid length (not 13 digits)
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("13 cifre"));

        // Verify CreateAsync was NOT called
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that CNP with non-digit characters returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_CNPWithNonDigits_ReturnsFailure()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            CNP = "1234567890ABC", // Contains letters
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("13 cifre"));

        // Verify CreateAsync was NOT called
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Invalid Date of Birth

    /// <summary>
    /// Tests that future birth date returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_FutureBirthDate_ReturnsFailure()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            Data_Nasterii = DateTime.Now.AddYears(1), // Future date
            Sex = "M"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("viitor"));

        // Verify CreateAsync was NOT called
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that birth date before 1900 returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_BirthDateBefore1900_ReturnsFailure()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            Data_Nasterii = new DateTime(1800, 1, 1), // Before 1900
            Sex = "M"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("invalidă"));

        // Verify CreateAsync was NOT called
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Invalid Sex

    /// <summary>
    /// Tests that invalid sex value returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_InvalidSex_ReturnsFailure()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "X" // Invalid (must be M or F)
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Masculin") || e.Contains("Feminin"));

        // Verify CreateAsync was NOT called
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Repository Exception

    /// <summary>
    /// Tests that repository exception returns failure with error message.
    /// </summary>
    [Fact]
    public async Task Handle_RepositoryException_ReturnsFailure()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            CNP = "1234567890123",
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M"
        };

        // Setup: CheckUniqueAsync succeeds
        _mockRepository
            .Setup(r => r.CheckUniqueAsync(It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, false));

        // Setup: GenerateNextCodPacientAsync succeeds
        _mockRepository
            .Setup(r => r.GenerateNextCodPacientAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("PAC00001");

        // Setup: CreateAsync throws exception
        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Eroare") && e.Contains("Database connection failed"));

        // Verify logger logged error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce);
    }

    #endregion

    #region Logging Verification

    /// <summary>
    /// Tests that successful creation logs information messages.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_LogsInformation()
    {
        // Arrange
        var command = new CreatePacientCommand
        {
            Nume = "Popescu",
            Prenume = "Ion",
            CNP = "1234567890123",
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M"
        };

        var mockPacient = new Pacient
        {
            Id = Guid.NewGuid(),
            Cod_Pacient = "PAC00001",
            Nume = command.Nume,
            Prenume = command.Prenume
        };

        _mockRepository
            .Setup(r => r.GenerateNextCodPacientAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("PAC00001");

        _mockRepository
            .Setup(r => r.CheckUniqueAsync(It.IsAny<string>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, false));

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPacient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify logger logged information (START, Creating, SUCCESS)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("START") ||
                                             v.ToString()!.Contains("Creating") ||
                                             v.ToString()!.Contains("SUCCESS")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce);
    }

    #endregion
}
