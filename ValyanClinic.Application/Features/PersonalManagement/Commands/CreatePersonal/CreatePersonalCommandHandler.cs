using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PersonalManagement.Commands.CreatePersonal;

/// <summary>
/// Handler pentru CreatePersonalCommand - ALINIAT CU DB REALA
/// </summary>
public class CreatePersonalCommandHandler : IRequestHandler<CreatePersonalCommand, Result<Guid>>
{
    private readonly IPersonalRepository _personalRepository;
    private readonly ILogger<CreatePersonalCommandHandler> _logger;

    public CreatePersonalCommandHandler(
        IPersonalRepository personalRepository,
        ILogger<CreatePersonalCommandHandler> logger)
    {
        _personalRepository = personalRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreatePersonalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creare personal: {Nume} {Prenume}", request.Nume, request.Prenume);

            // Verificam daca CNP-ul si Cod_Angajat exista deja
            var (cnpExists, codAngajatExists) = await _personalRepository.CheckUniqueAsync(
                request.CNP,
                request.Cod_Angajat,
                cancellationToken: cancellationToken);

            if (cnpExists)
            {
                _logger.LogWarning("CNP-ul {CNP} exista deja in baza de date", request.CNP);
                return Result<Guid>.Failure("Exista deja un angajat cu acest CNP");
            }

            if (codAngajatExists)
            {
                _logger.LogWarning("Codul de angajat {CodAngajat} exista deja", request.Cod_Angajat);
                return Result<Guid>.Failure("Exista deja un angajat cu acest cod");
            }

            // Cream entitatea Personal
            var personal = new Personal
            {
                Id_Personal = Guid.NewGuid(),
                Cod_Angajat = request.Cod_Angajat,
                Nume = request.Nume,
                Prenume = request.Prenume,
                Nume_Anterior = request.Nume_Anterior,
                CNP = request.CNP,
                Data_Nasterii = request.Data_Nasterii,
                Locul_Nasterii = request.Locul_Nasterii,
                Nationalitate = request.Nationalitate,
                Cetatenie = request.Cetatenie,
                Telefon_Personal = request.Telefon_Personal,
                Telefon_Serviciu = request.Telefon_Serviciu,
                Email_Personal = request.Email_Personal,
                Email_Serviciu = request.Email_Serviciu,
                Adresa_Domiciliu = request.Adresa_Domiciliu,
                Judet_Domiciliu = request.Judet_Domiciliu,
                Oras_Domiciliu = request.Oras_Domiciliu,
                Cod_Postal_Domiciliu = request.Cod_Postal_Domiciliu,
                Adresa_Resedinta = request.Adresa_Resedinta,
                Judet_Resedinta = request.Judet_Resedinta,
                Oras_Resedinta = request.Oras_Resedinta,
                Cod_Postal_Resedinta = request.Cod_Postal_Resedinta,
                Stare_Civila = request.Stare_Civila,
                Functia = request.Functia,
                Departament = request.Departament,
                Serie_CI = request.Serie_CI,
                Numar_CI = request.Numar_CI,
                Eliberat_CI_De = request.Eliberat_CI_De,
                Data_Eliberare_CI = request.Data_Eliberare_CI,
                Valabil_CI_Pana = request.Valabil_CI_Pana,
                Status_Angajat = request.Status_Angajat,
                Observatii = request.Observatii,
                Data_Crearii = DateTime.Now,
                Creat_De = request.CreatDe
            };

            // Salvam Personal
            var personalId = await _personalRepository.CreateAsync(personal, cancellationToken);

            _logger.LogInformation("Personal creat cu succes: {PersonalId}", personalId);

            return Result<Guid>.Success(personalId, "Angajatul a fost creat cu succes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la crearea personalului: {Nume} {Prenume}", request.Nume, request.Prenume);
            return Result<Guid>.Failure($"Eroare la crearea angajatului: {ex.Message}");
        }
    }
}
