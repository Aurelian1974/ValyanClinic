using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ValyanClinic.Application.Features.PacientManagement.Commands.DeletePacient;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using Xunit;

namespace ValyanClinic.Tests.Application.PacientManagement.Commands;

/// <summary>
/// Unit tests for DeletePacientCommandHandler.
/// Tests soft delete, hard delete, not found scenarios, and error handling.
/// </summary>
/// <remarks>
/// Pattern: AAA (Arrange-Act-Assert)
/// Tools: xUnit, Moq, FluentAssertions
/// Coverage: ~6 test scenarios for DeletePacientCommand handler
/// </remarks>
public class DeletePacientCommandHandlerTests
{
    private readonly Mock<IPacientRepository> _mockRepository;
    private readonly Mock<ILogger<DeletePacientCommandHandler>> _mockLogger;
    private readonly DeletePacientCommandHandler _handler;

    public DeletePacientCommandHandlerTests()
    {
        _mockRepository = new Mock<IPacientRepository>();
        _mockLogger = new Mock<ILogger<DeletePacientCommandHandler>>();
        _handler = new DeletePacientCommandHandler(_mockRepository.Object, _mockLogger.Object);
    }

    #region Soft Delete

    /// <summary>
    /// Tests that soft delete (deactivate) returns success.
    /// </summary>
    [Fact]
    public async Task Handle_ValidSoftDelete_ReturnsSuccess()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var command = new DeletePacientCommand(pacientId, "TestUser", hardDelete: false);

        var existingPacient = new Pacient
        {
            Id = pacientId,
            Cod_Pacient = "PAC00001",
            Nume = "Popescu",
            Prenume = "Ion",
            Activ = true
        };

        // Setup: GetByIdAsync returns existing pacient
        _mockRepository
            .Setup(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPacient);

        // Setup: DeleteAsync (soft delete) succeeds
        _mockRepository
            .Setup(r => r.DeleteAsync(pacientId, command.ModificatDe, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.SuccessMessage.Should().Contain("dezactivat cu succes");

        // Verify interactions
        _mockRepository.Verify(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(pacientId, command.ModificatDe, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.HardDeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that soft delete marks pacient as inactive (not physically deleted).
    /// </summary>
    [Fact]
    public async Task Handle_SoftDelete_MarksPacientInactive()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var command = new DeletePacientCommand(pacientId, "TestUser", hardDelete: false);

        var existingPacient = new Pacient
        {
            Id = pacientId,
            Cod_Pacient = "PAC00001",
            Nume = "Popescu",
            Prenume = "Ion",
            Activ = true
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPacient);

        _mockRepository
            .Setup(r => r.DeleteAsync(pacientId, command.ModificatDe, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify soft delete was called (not hard delete)
        _mockRepository.Verify(r => r.DeleteAsync(pacientId, command.ModificatDe, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.HardDeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Hard Delete

    /// <summary>
    /// Tests that hard delete (physical deletion) returns success.
    /// </summary>
    [Fact]
    public async Task Handle_ValidHardDelete_ReturnsSuccess()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var command = new DeletePacientCommand(pacientId, "TestAdmin", hardDelete: true);

        var existingPacient = new Pacient
        {
            Id = pacientId,
            Cod_Pacient = "PAC00001",
            Nume = "Popescu",
            Prenume = "Ion"
        };

        // Setup: GetByIdAsync returns existing pacient
        _mockRepository
            .Setup(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPacient);

        // Setup: HardDeleteAsync succeeds
        _mockRepository
            .Setup(r => r.HardDeleteAsync(pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.SuccessMessage.Should().Contain("șters definitiv");

        // Verify interactions
        _mockRepository.Verify(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.HardDeleteAsync(pacientId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        // Verify warning was logged (hard delete is dangerous)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HARD DELETE")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce);
    }

    #endregion

    #region Pacient Not Found

    /// <summary>
    /// Tests that deleting non-existent pacient returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_PacientNotFound_ReturnsFailure()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var command = new DeletePacientCommand(pacientId, "TestUser", hardDelete: false);

        // Setup: GetByIdAsync returns null (not found)
        _mockRepository
            .Setup(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pacient?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("nu a fost găsit"));

        // Verify DeleteAsync was NOT called
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockRepository.Verify(r => r.HardDeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Repository Exception

    /// <summary>
    /// Tests that repository exception during delete returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_RepositoryException_ReturnsFailure()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var command = new DeletePacientCommand(pacientId, "TestUser", hardDelete: false);

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

        // Setup: DeleteAsync throws exception
        _mockRepository
            .Setup(r => r.DeleteAsync(pacientId, command.ModificatDe, It.IsAny<CancellationToken>()))
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
    /// Tests that successful delete logs information messages.
    /// </summary>
    [Fact]
    public async Task Handle_ValidDelete_LogsInformation()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var command = new DeletePacientCommand(pacientId, "TestUser", hardDelete: false);

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

        _mockRepository
            .Setup(r => r.DeleteAsync(pacientId, command.ModificatDe, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify logger logged information (START, Deleting, SUCCESS)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("START") ||
                                             v.ToString()!.Contains("Deleting") ||
                                             v.ToString()!.Contains("SUCCESS")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce);
    }

    #endregion
}
