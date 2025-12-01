using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.DTOs;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Queries.GetDoctoriByPacient;
using ValyanClinic.Domain.Interfaces.Repositories;
using Xunit;

namespace ValyanClinic.Tests.Application.PacientPersonalMedicalManagement.Queries;

/// <summary>
/// Unit tests pentru GetDoctoriByPacientQueryHandler.
/// Testează toate scenariile: success, validation errors, SQL errors, edge cases.
/// </summary>
/// <remarks>
/// Folosește xUnit, FluentAssertions, Moq conform ghidului de proiect.
/// Pattern: AAA (Arrange, Act, Assert)
/// </remarks>
public class GetDoctoriByPacientQueryHandlerTests
{
    private readonly Mock<IPacientPersonalMedicalRepository> _mockRepository;
    private readonly Mock<ILogger<GetDoctoriByPacientQueryHandler>> _mockLogger;
    private readonly GetDoctoriByPacientQueryHandler _handler;

    /// <summary>
    /// Constructor pentru setup comun al tuturor testelor.
    /// Creează mock-uri pentru dependencies și instanțiază handler-ul.
    /// </summary>
    public GetDoctoriByPacientQueryHandlerTests()
    {
        _mockRepository = new Mock<IPacientPersonalMedicalRepository>();
        _mockLogger = new Mock<ILogger<GetDoctoriByPacientQueryHandler>>();
        _handler = new GetDoctoriByPacientQueryHandler(_mockRepository.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Testează că un query valid cu PacientID valid returnează success cu listă de doctori.
    /// </summary>
    [Fact]
    public async Task Handle_ValidQuery_ReturnsSuccessWithDoctorList()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var query = new GetDoctoriByPacientQuery(pacientId, ApenumereActivi: true);

        var expectedDoctori = new List<ValyanClinic.Domain.DTOs.DoctorAsociatDto>
        {
            new()
            {
                RelatieID = Guid.NewGuid(),
                PersonalMedicalID = Guid.NewGuid(),
                DoctorNumeComplet = "Dr. Popescu Ion",
                DoctorSpecializare = "Cardiologie",
                TipRelatie = "MedicPrimar",
                DataAsocierii = DateTime.Now.AddMonths(-6),
                EsteActiv = true,
                ZileDeAsociere = 180
            },
            new()
            {
                RelatieID = Guid.NewGuid(),
                PersonalMedicalID = Guid.NewGuid(),
                DoctorNumeComplet = "Dr. Ionescu Maria",
                DoctorSpecializare = "Pneumologie",
                TipRelatie = "Specialist",
                DataAsocierii = DateTime.Now.AddMonths(-3),
                EsteActiv = true,
                ZileDeAsociere = 90
            }
        };

        _mockRepository
            .Setup(r => r.GetDoctoriByPacientAsync(pacientId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDoctori);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value[0].DoctorNumeComplet.Should().Be("Dr. Popescu Ion");
        result.Value[1].DoctorNumeComplet.Should().Be("Dr. Ionescu Maria");

        _mockRepository.Verify(
            r => r.GetDoctoriByPacientAsync(pacientId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Testează că un query cu PacientID = Guid.Empty returnează failure cu mesaj de eroare.
    /// </summary>
    [Fact]
    public async Task Handle_EmptyPacientId_ReturnsFailure()
    {
        // Arrange
        var query = new GetDoctoriByPacientQuery(Guid.Empty, ApenumereActivi: true);

        _mockRepository
            .Setup(r => r.GetDoctoriByPacientAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("PacientId nu poate fi Guid.Empty", nameof(query.PacientID)));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.FirstError.Should().Contain("Parametri invalizi");
    }

    /// <summary>
    /// Testează că un query valid dar fără doctori asociați returnează success cu listă goală.
    /// </summary>
    [Fact]
    public async Task Handle_NoDoctorsFound_ReturnsSuccessWithEmptyList()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var query = new GetDoctoriByPacientQuery(pacientId, ApenumereActivi: true);

        _mockRepository
            .Setup(r => r.GetDoctoriByPacientAsync(pacientId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ValyanClinic.Domain.DTOs.DoctorAsociatDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        _mockRepository.Verify(
            r => r.GetDoctoriByPacientAsync(pacientId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Testează că o excepție SQL din repository returnează failure cu mesaj de eroare.
    /// </summary>
    [Fact]
    public async Task Handle_RepositoryThrowsSqlException_ReturnsFailure()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var query = new GetDoctoriByPacientQuery(pacientId, ApenumereActivi: true);

        _mockRepository
            .Setup(r => r.GetDoctoriByPacientAsync(pacientId, true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.FirstError.Should().Contain("Eroare la obținerea doctorilor");
    }

    /// <summary>
    /// Testează că query cu ApenumereActivi = false returnează și doctori inactivi.
    /// </summary>
    [Fact]
    public async Task Handle_ApenumereActiviFalse_ReturnsAllDoctors()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var query = new GetDoctoriByPacientQuery(pacientId, ApenumereActivi: false);

        var expectedDoctori = new List<ValyanClinic.Domain.DTOs.DoctorAsociatDto>
        {
            new()
            {
                RelatieID = Guid.NewGuid(),
                PersonalMedicalID = Guid.NewGuid(),
                DoctorNumeComplet = "Dr. Activ",
                EsteActiv = true,
                ZileDeAsociere = 100
            },
            new()
            {
                RelatieID = Guid.NewGuid(),
                PersonalMedicalID = Guid.NewGuid(),
                DoctorNumeComplet = "Dr. Inactiv",
                EsteActiv = false,
                DataDezactivarii = DateTime.Now.AddDays(-10),
                ZileDeAsociere = 50
            }
        };

        _mockRepository
            .Setup(r => r.GetDoctoriByPacientAsync(pacientId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDoctori);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(d => d.EsteActiv);
        result.Value.Should().Contain(d => !d.EsteActiv);

        _mockRepository.Verify(
            r => r.GetDoctoriByPacientAsync(pacientId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Testează că logging-ul funcționează corect pentru success case.
    /// </summary>
    [Fact]
    public async Task Handle_ValidQuery_LogsInformationMessages()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var query = new GetDoctoriByPacientQuery(pacientId, ApenumereActivi: true);

        _mockRepository
            .Setup(r => r.GetDoctoriByPacientAsync(pacientId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ValyanClinic.Domain.DTOs.DoctorAsociatDto>
            {
                new() { RelatieID = Guid.NewGuid(), DoctorNumeComplet = "Test Doctor", EsteActiv = true }
            });

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing query")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Query successful")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Testează că mapping-ul de la Domain DTOs la Application DTOs funcționează corect.
    /// </summary>
    [Fact]
    public async Task Handle_ValidQuery_MapsAllDtoPropertiesCorrectly()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var relatieId = Guid.NewGuid();
        var personalMedicalId = Guid.NewGuid();
        var dataAsocierii = DateTime.Now.AddMonths(-2);
        
        var query = new GetDoctoriByPacientQuery(pacientId, ApenumereActivi: true);

        var domainDto = new ValyanClinic.Domain.DTOs.DoctorAsociatDto
        {
            RelatieID = relatieId,
            PersonalMedicalID = personalMedicalId,
            DoctorNumeComplet = "Dr. Test Complete",
            DoctorSpecializare = "Test Specializare",
            DoctorTelefon = "0712345678",
            DoctorEmail = "test@test.com",
            DoctorDepartament = "Test Departament",
            TipRelatie = "MedicPrimar",
            DataAsocierii = dataAsocierii,
            DataDezactivarii = null,
            EsteActiv = true,
            ZileDeAsociere = 60,
            Observatii = "Test observatii",
            Motiv = "Test motiv"
        };

        _mockRepository
            .Setup(r => r.GetDoctoriByPacientAsync(pacientId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ValyanClinic.Domain.DTOs.DoctorAsociatDto> { domainDto });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var mappedDto = result.Value[0];
        mappedDto.RelatieID.Should().Be(relatieId);
        mappedDto.PersonalMedicalID.Should().Be(personalMedicalId);
        mappedDto.DoctorNumeComplet.Should().Be("Dr. Test Complete");
        mappedDto.DoctorSpecializare.Should().Be("Test Specializare");
        mappedDto.DoctorTelefon.Should().Be("0712345678");
        mappedDto.DoctorEmail.Should().Be("test@test.com");
        mappedDto.DoctorDepartament.Should().Be("Test Departament");
        mappedDto.TipRelatie.Should().Be("MedicPrimar");
        mappedDto.DataAsocierii.Should().Be(dataAsocierii);
        mappedDto.DataDezactivarii.Should().BeNull();
        mappedDto.EsteActiv.Should().BeTrue();
        mappedDto.ZileDeAsociere.Should().Be(60);
        mappedDto.Observatii.Should().Be("Test observatii");
        mappedDto.Motiv.Should().Be("Test motiv");
    }
}
