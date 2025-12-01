using FluentAssertions;
using Moq;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using Xunit;

namespace ValyanClinic.Tests.Application.PacientManagement.Queries;

/// <summary>
/// Unit tests for GetPacientByIdQueryHandler.
/// Tests retrieval of pacient details, not found scenarios, and error handling.
/// </summary>
/// <remarks>
/// Pattern: AAA (Arrange-Act-Assert)
/// Tools: xUnit, Moq, FluentAssertions
/// Coverage: ~5 test scenarios for GetPacientByIdQuery handler
/// </remarks>
public class GetPacientByIdQueryHandlerTests
{
    private readonly Mock<IPacientRepository> _mockRepository;
    private readonly GetPacientByIdQueryHandler _handler;

    public GetPacientByIdQueryHandlerTests()
    {
        _mockRepository = new Mock<IPacientRepository>();
        _handler = new GetPacientByIdQueryHandler(_mockRepository.Object);
    }

    #region Valid Scenarios

    /// <summary>
    /// Tests that a valid ID returns pacient details successfully.
    /// </summary>
    [Fact]
    public async Task Handle_ValidId_ReturnsPacientDetails()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var query = new GetPacientByIdQuery(pacientId);

        var mockPacient = new Pacient
        {
            Id = pacientId,
            Cod_Pacient = "PAC00001",
            CNP = "1234567890123",
            Nume = "Popescu",
            Prenume = "Ion",
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M",
            Telefon = "0712345678",
            Email = "ion.popescu@example.com",
            Judet = "Bucuresti",
            Localitate = "Sector 1",
            Adresa = "Str. Exemplu nr. 1",
            Cod_Postal = "010101",
            Asigurat = true,
            Activ = true,
            Data_Inregistrare = DateTime.Now.AddYears(-1),
            Nr_Total_Vizite = 5
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPacient);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(pacientId);
        result.Value.Cod_Pacient.Should().Be(mockPacient.Cod_Pacient);
        result.Value.Nume.Should().Be(mockPacient.Nume);
        result.Value.Prenume.Should().Be(mockPacient.Prenume);
        result.Value.CNP.Should().Be(mockPacient.CNP);
        result.Value.Email.Should().Be(mockPacient.Email);

        // Verify repository interaction
        _mockRepository.Verify(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that query maps all DTO fields correctly from Pacient entity.
    /// </summary>
    [Fact]
    public async Task Handle_ValidQuery_MapsAllDtoFields()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var query = new GetPacientByIdQuery(pacientId);

        var mockPacient = new Pacient
        {
            Id = pacientId,
            Cod_Pacient = "PAC00001",
            CNP = "1234567890123",
            Nume = "Popescu",
            Prenume = "Ion",
            Data_Nasterii = new DateTime(1990, 1, 1),
            Sex = "M",
            Telefon = "0712345678",
            Telefon_Secundar = "0723456789",
            Email = "ion.popescu@example.com",
            Judet = "Bucuresti",
            Localitate = "Sector 1",
            Adresa = "Str. Exemplu nr. 1",
            Cod_Postal = "010101",
            Asigurat = true,
            CNP_Asigurat = "1234567890123",
            Nr_Card_Sanatate = "1234567890123456",
            Casa_Asigurari = "CNAS",
            Alergii = "Polen, Aspirina",
            Boli_Cronice = "Hipertensiune",
            Medic_Familie = "Dr. Ionescu",
            Persoana_Contact = "Maria Popescu",
            Telefon_Urgenta = "0734567890",
            Relatie_Contact = "Sotie",
            Data_Inregistrare = DateTime.Now.AddYears(-1),
            Ultima_Vizita = DateTime.Now.AddDays(-30),
            Nr_Total_Vizite = 5,
            Activ = true,
            Observatii = "Test observatii",
            Data_Crearii = DateTime.Now.AddYears(-1),
            Data_Ultimei_Modificari = DateTime.Now.AddDays(-10),
            Creat_De = "TestUser",
            Modificat_De = "TestUser"
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPacient);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // Verify all key fields are mapped
        result.Value!.Id.Should().Be(mockPacient.Id);
        result.Value.Cod_Pacient.Should().Be(mockPacient.Cod_Pacient);
        result.Value.CNP.Should().Be(mockPacient.CNP);
        result.Value.Nume.Should().Be(mockPacient.Nume);
        result.Value.Prenume.Should().Be(mockPacient.Prenume);
        result.Value.Data_Nasterii.Should().Be(mockPacient.Data_Nasterii);
        result.Value.Sex.Should().Be(mockPacient.Sex);
        result.Value.Telefon.Should().Be(mockPacient.Telefon);
        result.Value.Telefon_Secundar.Should().Be(mockPacient.Telefon_Secundar);
        result.Value.Email.Should().Be(mockPacient.Email);
        result.Value.Judet.Should().Be(mockPacient.Judet);
        result.Value.Localitate.Should().Be(mockPacient.Localitate);
        result.Value.Adresa.Should().Be(mockPacient.Adresa);
        result.Value.Cod_Postal.Should().Be(mockPacient.Cod_Postal);
        result.Value.Asigurat.Should().Be(mockPacient.Asigurat);
        result.Value.CNP_Asigurat.Should().Be(mockPacient.CNP_Asigurat);
        result.Value.Nr_Card_Sanatate.Should().Be(mockPacient.Nr_Card_Sanatate);
        result.Value.Casa_Asigurari.Should().Be(mockPacient.Casa_Asigurari);
        result.Value.Alergii.Should().Be(mockPacient.Alergii);
        result.Value.Boli_Cronice.Should().Be(mockPacient.Boli_Cronice);
        result.Value.Medic_Familie.Should().Be(mockPacient.Medic_Familie);
        result.Value.Persoana_Contact.Should().Be(mockPacient.Persoana_Contact);
        result.Value.Telefon_Urgenta.Should().Be(mockPacient.Telefon_Urgenta);
        result.Value.Relatie_Contact.Should().Be(mockPacient.Relatie_Contact);
        result.Value.Data_Inregistrare.Should().Be(mockPacient.Data_Inregistrare);
        result.Value.Ultima_Vizita.Should().Be(mockPacient.Ultima_Vizita);
        result.Value.Nr_Total_Vizite.Should().Be(mockPacient.Nr_Total_Vizite);
        result.Value.Activ.Should().Be(mockPacient.Activ);
        result.Value.Observatii.Should().Be(mockPacient.Observatii);
        result.Value.Creat_De.Should().Be(mockPacient.Creat_De);
        result.Value.Modificat_De.Should().Be(mockPacient.Modificat_De);
    }

    #endregion

    #region Pacient Not Found

    /// <summary>
    /// Tests that non-existent ID returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_PacientNotFound_ReturnsFailure()
    {
        // Arrange
        var pacientId = Guid.NewGuid();
        var query = new GetPacientByIdQuery(pacientId);

        // Setup: GetByIdAsync returns null (not found)
        _mockRepository
            .Setup(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pacient?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("nu a fost gasit"));
        result.Value.Should().BeNull();

        // Verify repository was called
        _mockRepository.Verify(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that invalid/empty GUID returns failure.
    /// </summary>
    [Fact]
    public async Task Handle_InvalidId_ReturnsFailure()
    {
        // Arrange
        var invalidId = Guid.Empty;
        var query = new GetPacientByIdQuery(invalidId);

        // Setup: GetByIdAsync returns null for invalid ID
        _mockRepository
            .Setup(r => r.GetByIdAsync(invalidId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pacient?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
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
        var pacientId = Guid.NewGuid();
        var query = new GetPacientByIdQuery(pacientId);

        // Setup: GetByIdAsync throws exception
        _mockRepository
            .Setup(r => r.GetByIdAsync(pacientId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Eroare") && e.Contains("Database connection failed"));
        result.Value.Should().BeNull();
    }

    #endregion
}
