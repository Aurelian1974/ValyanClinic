using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.RemoveRelatie;
using ValyanClinic.Domain.Interfaces.Repositories;
using Xunit;

namespace ValyanClinic.Tests.Application.PacientPersonalMedicalManagement.Commands;

/// <summary>
/// Unit tests pentru RemoveRelatieCommandHandler.
/// Testează toate scenariile: success, validation errors, business logic errors, SQL errors.
/// </summary>
/// <remarks>
/// Folosește xUnit, FluentAssertions, Moq conform ghidului de proiect.
/// Pattern: AAA (Arrange, Act, Assert)
/// </remarks>
public class RemoveRelatieCommandHandlerTests
{
    private readonly Mock<IPacientPersonalMedicalRepository> _mockRepository;
    private readonly Mock<ILogger<RemoveRelatieCommandHandler>> _mockLogger;
    private readonly RemoveRelatieCommandHandler _handler;

    /// <summary>
    /// Constructor pentru setup comun al tuturor testelor.
    /// Creează mock-uri pentru dependencies și instanțiază handler-ul.
    /// </summary>
    public RemoveRelatieCommandHandlerTests()
    {
        _mockRepository = new Mock<IPacientPersonalMedicalRepository>();
        _mockLogger = new Mock<ILogger<RemoveRelatieCommandHandler>>();
        _handler = new RemoveRelatieCommandHandler(_mockRepository.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Testează că un command valid cu RelatieID valid returnează success.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var relatieId = Guid.NewGuid();
        var command = new RemoveRelatieCommand(RelatieID: relatieId);

        _mockRepository
            .Setup(r => r.RemoveRelatieAsync(relatieId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();

        _mockRepository.Verify(
            r => r.RemoveRelatieAsync(relatieId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Testează că un command cu RelatieID = null returnează failure cu mesaj de validare.
    /// </summary>
    [Fact]
    public async Task Handle_NullRelatieId_ReturnsFailure()
    {
        // Arrange
        var command = new RemoveRelatieCommand(RelatieID: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.FirstError.Should().Contain("RelatieID este obligatoriu");

        _mockRepository.Verify(
            r => r.RemoveRelatieAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Testează că un command cu RelatieID = Guid.Empty returnează failure cu mesaj de validare.
    /// </summary>
    [Fact]
    public async Task Handle_EmptyRelatieId_ReturnsFailure()
    {
        // Arrange
        var command = new RemoveRelatieCommand(RelatieID: Guid.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.FirstError.Should().Contain("RelatieID este obligatoriu");

        _mockRepository.Verify(
            r => r.RemoveRelatieAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Testează că repository aruncă InvalidOperationException când relația nu există sau este deja inactivă.
    /// Handler trebuie să returneze failure cu mesajul excepției.
    /// </summary>
    [Fact]
    public async Task Handle_RelatieNotFound_ReturnsFailure()
    {
        // Arrange
        var relatieId = Guid.NewGuid();
        var command = new RemoveRelatieCommand(RelatieID: relatieId);

        _mockRepository
            .Setup(r => r.RemoveRelatieAsync(relatieId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Relația nu a fost găsită sau este deja inactivă."));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.FirstError.Should().Contain("Relația nu a fost găsită");

        _mockRepository.Verify(
            r => r.RemoveRelatieAsync(relatieId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Testează că ArgumentException din repository returnează failure cu mesaj de parametri invalizi.
    /// </summary>
    [Fact]
    public async Task Handle_RepositoryThrowsArgumentException_ReturnsFailure()
    {
        // Arrange
        var relatieId = Guid.NewGuid();
        var command = new RemoveRelatieCommand(RelatieID: relatieId);

        _mockRepository
            .Setup(r => r.RemoveRelatieAsync(relatieId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("RelatieId nu poate fi Guid.Empty", nameof(relatieId)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.FirstError.Should().Contain("Parametri invalizi");
    }

    /// <summary>
    /// Testează că orice altă excepție din repository returnează failure cu mesaj generic.
    /// </summary>
    [Fact]
    public async Task Handle_RepositoryThrowsGenericException_ReturnsFailure()
    {
        // Arrange
        var relatieId = Guid.NewGuid();
        var command = new RemoveRelatieCommand(RelatieID: relatieId);

        _mockRepository
            .Setup(r => r.RemoveRelatieAsync(relatieId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.FirstError.Should().Contain("Eroare la dezactivarea relației");
    }

    /// <summary>
    /// Testează că logging-ul funcționează corect pentru success case.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_LogsInformationMessages()
    {
        // Arrange
        var relatieId = Guid.NewGuid();
        var command = new RemoveRelatieCommand(RelatieID: relatieId);

        _mockRepository
            .Setup(r => r.RemoveRelatieAsync(relatieId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing command")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("dezactivată cu succes")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Testează că logging-ul funcționează corect pentru failure case (InvalidOperationException).
    /// </summary>
    [Fact]
    public async Task Handle_RelatieNotFound_LogsWarning()
    {
        // Arrange
        var relatieId = Guid.NewGuid();
        var command = new RemoveRelatieCommand(RelatieID: relatieId);

        _mockRepository
            .Setup(r => r.RemoveRelatieAsync(relatieId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Relația nu există"));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("nu a fost găsită")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Testează că CancellationToken este propagat corect la repository.
    /// </summary>
    [Fact]
    public async Task Handle_CancellationTokenPropagated_RepositoryReceivesToken()
    {
        // Arrange
        var relatieId = Guid.NewGuid();
        var command = new RemoveRelatieCommand(RelatieID: relatieId);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockRepository
            .Setup(r => r.RemoveRelatieAsync(relatieId, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _mockRepository.Verify(
            r => r.RemoveRelatieAsync(relatieId, cancellationToken),
            Times.Once);
    }

    /// <summary>
    /// Testează edge case: multiple consecutive calls cu același RelatieID.
    /// Al doilea call trebuie să eșueze cu InvalidOperationException (deja inactivă).
    /// </summary>
    [Fact]
    public async Task Handle_MultipleCallsSameRelatieId_SecondCallFails()
    {
        // Arrange
        var relatieId = Guid.NewGuid();
        var command = new RemoveRelatieCommand(RelatieID: relatieId);

        // Primul call reușește
        _mockRepository
            .SetupSequence(r => r.RemoveRelatieAsync(relatieId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .ThrowsAsync(new InvalidOperationException("Relația este deja inactivă"));

        // Act
        var result1 = await _handler.Handle(command, CancellationToken.None);
        var result2 = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeFalse();
        // Handler poate returna orice mesaj care conține "Relația" (flexibil pentru diferite implementări)
        result2.FirstError.Should().ContainAny("Relația", "inactivă", "găsită");

        _mockRepository.Verify(
            r => r.RemoveRelatieAsync(relatieId, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
}
