using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PersonalManagement.Commands.UpdatePersonal;

/// <summary>
/// Handler pentru UpdatePersonalCommand - ALINIAT CU DB REALA
/// </summary>
public class UpdatePersonalCommandHandler : IRequestHandler<UpdatePersonalCommand, Result<bool>>
{
    private readonly IPersonalRepository _personalRepository;
    private readonly ILogger<UpdatePersonalCommandHandler> _logger;

    public UpdatePersonalCommandHandler(
        IPersonalRepository personalRepository,
        ILogger<UpdatePersonalCommandHandler> logger)
    {
        _personalRepository = personalRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdatePersonalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Actualizare personal: {PersonalId}", request.Id_Personal);

            // Verificam daca personalul exista
            var existingPersonal = await _personalRepository.GetByIdAsync(request.Id_Personal, cancellationToken);
            if (existingPersonal == null)
            {
                _logger.LogWarning("Personalul cu ID-ul {PersonalId} nu a fost gasit", request.Id_Personal);
                return Result<bool>.Failure($"Angajatul cu ID-ul {request.Id_Personal} nu a fost gasit");
            }

            // Verificam unicitatea CNP si Cod_Angajat (exclus ID-ul curent)
            var (cnpExists, codAngajatExists) = await _personalRepository.CheckUniqueAsync(
                request.CNP,
                request.Cod_Angajat,
                request.Id_Personal,
                cancellationToken);

            if (cnpExists)
            {
                _logger.LogWarning("CNP-ul {CNP} exista deja la alt angajat", request.CNP);
                return Result<bool>.Failure("Exista deja un alt angajat cu acest CNP");
            }

            if (codAngajatExists)
            {
                _logger.LogWarning("Codul de angajat {CodAngajat} exista deja la alt angajat", request.Cod_Angajat);
                return Result<bool>.Failure("Exista deja un alt angajat cu acest cod");
            }

            // Actualizam entitatea Personal
            var personal = new Personal
            {
                Id_Personal = request.Id_Personal,
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
                Data_Ultimei_Modificari = DateTime.Now,
                Modificat_De = request.ModificatDe,
                Data_Crearii = existingPersonal.Data_Crearii,
                Creat_De = existingPersonal.Creat_De
            };

            // Salvam Personal
            var updated = await _personalRepository.UpdateAsync(personal, cancellationToken);

            if (!updated)
            {
                _logger.LogError("Eroare la actualizarea personalului {PersonalId}", request.Id_Personal);
                return Result<bool>.Failure("Eroare la actualizarea angajatului");
            }

            _logger.LogInformation("Personal actualizat cu succes: {PersonalId}", request.Id_Personal);

            return Result<bool>.Success(true, "Angajatul a fost actualizat cu succes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la actualizarea personalului: {PersonalId}", request.Id_Personal);
            return Result<bool>.Failure($"Eroare la actualizarea angajatului: {ex.Message}");
        }
    }
}
