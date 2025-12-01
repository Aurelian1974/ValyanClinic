using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ValyanClinic.Application.Common.Exceptions;
using ValyanClinic.Application.Features.PacientManagement.Commands.UpdatePacient;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using Xunit;

namespace ValyanClinic.Tests.Application.PacientManagement.Commands;

/// <summary>
/// Unit tests for UpdatePacientCommandHandler.
/// Tests business logic validation, repository interactions, not found scenarios, and error handling.
/// </summary>
/// <remarks>
/// Pattern: AAA (Arrange-Act-Assert)
/// Tools: xUnit, Moq, FluentAssertions
/// Coverage: ~8 test scenarios for UpdatePacientCommand handler
/// </remarks>
public class UpdatePacientCommandHandlerTests
{
    private readonly Mock<IPacientRepository> _mockRepository;
    private readonly Mock<ILogger<UpdatePacientCommandHandler>> _mockLogger;
    private readonly UpdatePacientCommandHandler _handler;

    public UpdatePacientCommandHandlerTests()
    {
        _mockRepository = new Mock<IPacientRepository>();
        _mockLogger = new Mock<ILogger<UpdatePacientCommandHandler>>();
        _handler = new UpdatePacientCommandHandler(_mockRepository.Object, _mockLogger.Object);
    }

    #region Valid Update Scenarios

    /// <summary>
    /// Tests that a valid update command returns success.
    /// </summary>
    [Fact]
    public async Task Handle_ValidUpdate_ReturnsSuccess()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var command = new UpdatePacientCommand
        {
            Id = pacientId,
            Nume = "Popescu Updated",
            Prenume = "Ion",
            CNP = "1234567890123",
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M",
            Telefon = "0712345678",
            Email = "ion.updated@example.com",
            Activ = true,
            ModificatDe = "TestUser"
        };

        var existingPacient = new Pacient
        {
            Id = pacientId,
            Cod_Pacient = "PAC00001",
            Nume = "Popescu",
            Prenume = "Ion",
            CNP = "1234567890123",
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M"
        };

        var updatedPacient = new Pacient
        {
            Id = pacientId,
            Cod_Pacient = "PAC00001",
            Nume = command.Nume,
            Prenume = command.Prenume,
            CNP = command.CNP,
            Data_Nasterii = command.Data_Nasterii,
            Sex = command.Sex
        };

        // Setup: GetByIdAsync returns existing pacient
        _mockRepository
            .Setup(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPacient);

        // Setup: CheckUniqueAsync (CNP is unique - same as existing)
        _mockRepository
            .Setup(r => r.CheckUniqueAsync(command.CNP, null, pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, false));

        // Setup: UpdateAsync succeeds
        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedPacient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(pacientId);
        result.SuccessMessage.Should().Contain("actualizat cu succes");

        // Verify interactions
        _mockRepository.Verify(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.CheckUniqueAsync(command.CNP, null, pacientId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that update with all fields updates correctly.
    /// </summary>
    [Fact]
    public async Task Handle_ValidUpdate_UpdatesAllFields()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var command = new UpdatePacientCommand
        {
            Id = pacientId,
            Nume = "Updated Nume",
            Prenume = "Updated Prenume",
            CNP = "9876543210987",
            Data_Nasterii = new DateTime(1985, 5, 15),
            Sex = "F",
            Telefon = "0799999999",
            Email = "updated@test.com",
            Judet = "Cluj",
            Localitate = "Cluj-Napoca",
            Adresa = "Str. Noua nr. 10",
            Activ = false,
            Observatii = "Test observatii",
            ModificatDe = "TestUser"
        };

        var existingPacient = new Pacient
        {
            Id = pacientId,
            Cod_Pacient = "PAC00001",
            Nume = "Old Nume",
            Prenume = "Old Prenume",
            CNP = "1234567890123"
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPacient);

        _mockRepository
            .Setup(r => r.CheckUniqueAsync(command.CNP, null, pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, false));

        Pacient? capturedPacient = null;
        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()))
            .Callback<Pacient, CancellationToken>((p, ct) => capturedPacient = p)
            .ReturnsAsync((Pacient p, CancellationToken ct) => p);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedPacient.Should().NotBeNull();
        capturedPacient!.Nume.Should().Be(command.Nume);
        capturedPacient.Prenume.Should().Be(command.Prenume);
        capturedPacient.CNP.Should().Be(command.CNP);
        capturedPacient.Data_Nasterii.Should().Be(command.Data_Nasterii);
        capturedPacient.Sex.Should().Be(command.Sex);
        capturedPacient.Telefon.Should().Be(command.Telefon);
        capturedPacient.Email.Should().Be(command.Email);
        capturedPacient.Judet.Should().Be(command.Judet);
        capturedPacient.Activ.Should().Be(command.Activ);
        capturedPacient.Modificat_De.Should().Be(command.ModificatDe);
    }

    #endregion

    #region Pacient Not Found

    /// <summary>
    /// Tests that updating non-existent pacient returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_PacientNotFound_ReturnsFailure()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var command = new UpdatePacientCommand
        {
            Id = pacientId,
            Nume = "Popescu",
            Prenume = "Ion",
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M"
        };

        // Setup: GetByIdAsync returns null (not found)
        _mockRepository
            .Setup(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pacient?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("nu a fost găsit"));

        // Verify UpdateAsync was NOT called
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Duplicate CNP (Excluding Self)

    /// <summary>
    /// Tests that updating with CNP that belongs to another pacient returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_DuplicateCNP_ExcludingSelf_ReturnsFailure()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var command = new UpdatePacientCommand
        {
            Id = pacientId,
            Nume = "Popescu",
            Prenume = "Ion",
            CNP = "9999999999999", // CNP that belongs to another pacient
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M"
        };

        var existingPacient = new Pacient
        {
            Id = pacientId,
            Cod_Pacient = "PAC00001",
            Nume = "Popescu",
            Prenume = "Ion",
            CNP = "1234567890123" // Original CNP
        };

        // Setup: GetByIdAsync succeeds
        _mockRepository
            .Setup(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPacient);

        // Setup: CheckUniqueAsync - CNP exists for another pacient
        _mockRepository
            .Setup(r => r.CheckUniqueAsync(command.CNP, null, pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, false)); // CNP exists for different ID

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("CNP") && e.Contains("există deja"));

        // Verify UpdateAsync was NOT called
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Missing Required Fields

    /// <summary>
    /// Tests that missing Nume returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_MissingNume_ReturnsFailure()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var command = new UpdatePacientCommand
        {
            Id = pacientId,
            Nume = "", // Missing
            Prenume = "Ion",
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M"
        };

        var existingPacient = new Pacient
        {
            Id = pacientId,
            Cod_Pacient = "PAC00001",
            Nume = "Popescu",
            Prenume = "Ion"
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPacient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Numele este obligatoriu"));

        // Verify UpdateAsync was NOT called
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that missing Prenume returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_MissingPrenume_ReturnsFailure()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var command = new UpdatePacientCommand
        {
            Id = pacientId,
            Nume = "Popescu",
            Prenume = "", // Missing
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M"
        };

        var existingPacient = new Pacient
        {
            Id = pacientId,
            Cod_Pacient = "PAC00001",
            Nume = "Popescu",
            Prenume = "Ion"
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPacient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Prenumele este obligatoriu"));

        // Verify UpdateAsync was NOT called
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Repository Exception

    /// <summary>
    /// Tests that repository exception during update returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_RepositoryException_ReturnsFailure()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var command = new UpdatePacientCommand
        {
            Id = pacientId,
            Nume = "Popescu",
            Prenume = "Ion",
            CNP = "1234567890123",
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M"
        };

        var existingPacient = new Pacient
        {
            Id = pacientId,
            Cod_Pacient = "PAC00001",
            Nume = "Popescu",
            Prenume = "Ion"
        };

        // Setup: GetByIdAsync succeeds
        _mockRepository
            .Setup(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPacient);

        // Setup: CheckUniqueAsync succeeds
        _mockRepository
            .Setup(r => r.CheckUniqueAsync(It.IsAny<string>(), null, pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, false));

        // Setup: UpdateAsync throws exception
        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection lost"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Eroare") && e.Contains("Database connection lost"));

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
    /// Tests that successful update logs information messages.
    /// </summary>
    [Fact]
    public async Task Handle_ValidUpdate_LogsInformation()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var command = new UpdatePacientCommand
        {
            Id = pacientId,
            Nume = "Popescu",
            Prenume = "Ion",
            CNP = "1234567890123",
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M"
        };

        var existingPacient = new Pacient
        {
            Id = pacientId,
            Cod_Pacient = "PAC00001",
            Nume = "Popescu",
            Prenume = "Ion"
        };

        var updatedPacient = new Pacient
        {
            Id = pacientId,
            Cod_Pacient = "PAC00001",
            Nume = command.Nume,
            Prenume = command.Prenume
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPacient);

        _mockRepository
            .Setup(r => r.CheckUniqueAsync(It.IsAny<string>(), null, pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, false));

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Pacient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedPacient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify logger logged information (START, Updating, SUCCESS)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("START") ||
                                             v.ToString()!.Contains("Updating") ||
                                             v.ToString()!.Contains("SUCCESS")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce);
    }

    #endregion
}
