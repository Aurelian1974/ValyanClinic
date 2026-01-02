using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Exceptions;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PacientManagement.Commands.UpdatePacient;

/// <summary>
/// Handler pentru UpdatePacientCommand
/// </summary>
public class UpdatePacientCommandHandler : IRequestHandler<UpdatePacientCommand, Result<Guid>>
{
    private readonly IPacientRepository _pacientRepository;
    private readonly ILogger<UpdatePacientCommandHandler> _logger;
    private readonly IPacientNotifier? _notifier;

    public UpdatePacientCommandHandler(
        IPacientRepository pacientRepository,
        ILogger<UpdatePacientCommandHandler> logger,
        IPacientNotifier? notifier = null)
    {
        _pacientRepository = pacientRepository;
        _logger = logger;
        _notifier = notifier;
    }

    public async Task<Result<Guid>> Handle(UpdatePacientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("========== UpdatePacientCommandHandler START ==========");
        _logger.LogInformation("Updating pacient ID: {Id}", request.Id);

        try
        {
            // 1. Verificare existență pacient
            var existingPacient = await _pacientRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingPacient == null)
            {
                _logger.LogWarning("Pacient not found: {Id}", request.Id);
                throw new NotFoundException($"Pacientul cu ID-ul {request.Id} nu a fost găsit.");
            }

            _logger.LogInformation("Found existing pacient: {Nume} {Prenume}",
                existingPacient.Nume, existingPacient.Prenume);

            // 2. Validare business rules
            var validationResult = await ValidateBusinessRules(request, cancellationToken);
            if (!validationResult.IsSuccess)
            {
                _logger.LogWarning("Validation failed: {Errors}", validationResult.ErrorsAsString);
                return Result<Guid>.Failure(validationResult.Errors);
            }

            // 3. Actualizare entitate Pacient
            existingPacient.Nume = request.Nume;
            existingPacient.Prenume = request.Prenume;
            existingPacient.Data_Nasterii = request.Data_Nasterii;
            existingPacient.Sex = request.Sex;
            existingPacient.CNP = request.CNP;
            existingPacient.Telefon = request.Telefon;
            existingPacient.Telefon_Secundar = request.Telefon_Secundar;
            existingPacient.Email = request.Email;
            existingPacient.Judet = request.Judet;
            existingPacient.Localitate = request.Localitate;
            existingPacient.Adresa = request.Adresa;
            existingPacient.Cod_Postal = request.Cod_Postal;
            existingPacient.Asigurat = request.Asigurat;
            existingPacient.CNP_Asigurat = request.CNP_Asigurat;
            existingPacient.Nr_Card_Sanatate = request.Nr_Card_Sanatate;
            existingPacient.Casa_Asigurari = request.Casa_Asigurari;
            existingPacient.Alergii = request.Alergii;
            existingPacient.Boli_Cronice = request.Boli_Cronice;
            existingPacient.Medic_Familie = request.Medic_Familie;
            existingPacient.Persoana_Contact = request.Persoana_Contact;
            existingPacient.Telefon_Urgenta = request.Telefon_Urgenta;
            existingPacient.Relatie_Contact = request.Relatie_Contact;
            existingPacient.Activ = request.Activ;
            existingPacient.Observatii = request.Observatii;
            existingPacient.Modificat_De = request.ModificatDe;

            // 4. Salvare în baza de date
            _logger.LogInformation("Updating pacient in database...");
            var updatedPacient = await _pacientRepository.UpdateAsync(existingPacient, cancellationToken);

            _logger.LogInformation("Pacient updated successfully: {Id}", updatedPacient.Id);

            // 5. Notificare SignalR
            try
            {
                if (_notifier != null)
                {
                    await _notifier.NotifyPacientChangedAsync("Updated", updatedPacient.Id, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Notifier failed for Updated event");
            }

            _logger.LogInformation("========== UpdatePacientCommandHandler END (SUCCESS) ==========");

            return Result<Guid>.Success(
                updatedPacient.Id,
                $"Pacientul {updatedPacient.NumeComplet} a fost actualizat cu succes.");
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Pacient not found");
            return Result<Guid>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "========== UpdatePacientCommandHandler EXCEPTION ==========");
            _logger.LogError("Exception Type: {Type}", ex.GetType().FullName);
            _logger.LogError("Exception Message: {Message}", ex.Message);

            return Result<Guid>.Failure($"Eroare la actualizarea pacientului: {ex.Message}");
        }
    }

    private async Task<Result> ValidateBusinessRules(UpdatePacientCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        // Validare nume și prenume
        if (string.IsNullOrWhiteSpace(request.Nume))
            errors.Add("Numele este obligatoriu.");

        if (string.IsNullOrWhiteSpace(request.Prenume))
            errors.Add("Prenumele este obligatoriu.");

        // Validare sex
        if (!string.IsNullOrEmpty(request.Sex) && !new[] { "M", "F" }.Contains(request.Sex))
            errors.Add("Sexul trebuie să fie 'M' (Masculin) sau 'F' (Feminin).");

        // Validare dată naștere
        if (request.Data_Nasterii > DateTime.Now)
            errors.Add("Data nașterii nu poate fi în viitor.");

        if (request.Data_Nasterii < new DateTime(1900, 1, 1))
            errors.Add("Data nașterii este invalidă.");

        // Validare CNP (dacă este furnizat)
        if (!string.IsNullOrEmpty(request.CNP))
        {
            if (request.CNP.Length != 13 || !request.CNP.All(char.IsDigit))
                errors.Add("CNP-ul trebuie să conțină exact 13 cifre.");

            // Verificare unicitate CNP (exclude ID-ul curent)
            var (cnpExists, _) = await _pacientRepository.CheckUniqueAsync(
                cnp: request.CNP,
                excludeId: request.Id,
                cancellationToken: cancellationToken);

            if (cnpExists)
                errors.Add($"Un alt pacient cu CNP-ul {request.CNP} există deja în sistem.");
        }

        // Validare email (dacă este furnizat)
        if (!string.IsNullOrEmpty(request.Email))
        {
            if (!request.Email.Contains("@") || !request.Email.Contains("."))
                errors.Add("Formatul email-ului este invalid.");
        }

        // Validare telefon (dacă este furnizat)
        if (!string.IsNullOrEmpty(request.Telefon))
        {
            if (request.Telefon.Length < 10)
                errors.Add("Numărul de telefon trebuie să conțină cel puțin 10 caractere.");
        }

        // Validare asigurare
        if (request.Asigurat)
        {
            if (string.IsNullOrEmpty(request.CNP_Asigurat) && string.IsNullOrEmpty(request.Nr_Card_Sanatate))
                errors.Add("Pentru pacienții asigurați este necesar CNP-ul asiguratului sau numărul cardului de sănătate.");
        }

        return errors.Any()
            ? Result.Failure(errors)
            : Result.Success();
    }
}
