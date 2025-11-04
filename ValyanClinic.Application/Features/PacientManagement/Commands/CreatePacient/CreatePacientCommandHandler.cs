using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PacientManagement.Commands.CreatePacient;

/// <summary>
/// Handler pentru CreatePacientCommand
/// </summary>
public class CreatePacientCommandHandler : IRequestHandler<CreatePacientCommand, Result<Guid>>
{
    private readonly IPacientRepository _pacientRepository;
    private readonly ILogger<CreatePacientCommandHandler> _logger;

    public CreatePacientCommandHandler(
        IPacientRepository pacientRepository,
        ILogger<CreatePacientCommandHandler> logger)
    {
        _pacientRepository = pacientRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreatePacientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("========== CreatePacientCommandHandler START ==========");
        _logger.LogInformation("Creating pacient: {Nume} {Prenume}", request.Nume, request.Prenume);

        try
        {
            // 1. Validare business rules
            var validationResult = await ValidateBusinessRules(request, cancellationToken);
            if (!validationResult.IsSuccess)
            {
                _logger.LogWarning("Validation failed: {Errors}", validationResult.ErrorsAsString);
                return Result<Guid>.Failure(validationResult.Errors);
            }

            // 2. Generare Cod_Pacient dacă nu este furnizat
            var codPacient = request.Cod_Pacient;
            if (string.IsNullOrEmpty(codPacient))
            {
                codPacient = await _pacientRepository.GenerateNextCodPacientAsync(cancellationToken);
                _logger.LogInformation("Generated Cod_Pacient: {CodPacient}", codPacient);
            }

            // 3. Creare entitate Pacient
            var pacient = new Pacient
            {
                Cod_Pacient = codPacient,
                CNP = request.CNP,
                Nume = request.Nume,
                Prenume = request.Prenume,
                Data_Nasterii = request.Data_Nasterii,
                Sex = request.Sex,
                Telefon = request.Telefon,
                Telefon_Secundar = request.Telefon_Secundar,
                Email = request.Email,
                Judet = request.Judet,
                Localitate = request.Localitate,
                Adresa = request.Adresa,
                Cod_Postal = request.Cod_Postal,
                Asigurat = request.Asigurat,
                CNP_Asigurat = request.CNP_Asigurat,
                Nr_Card_Sanatate = request.Nr_Card_Sanatate,
                Casa_Asigurari = request.Casa_Asigurari,
                Alergii = request.Alergii,
                Boli_Cronice = request.Boli_Cronice,
                Medic_Familie = request.Medic_Familie,
                Persoana_Contact = request.Persoana_Contact,
                Telefon_Urgenta = request.Telefon_Urgenta,
                Relatie_Contact = request.Relatie_Contact,
                Data_Inregistrare = DateTime.Now,
                Activ = request.Activ,
                Observatii = request.Observatii,
                Creat_De = request.CreatDe
            };

            // 4. Salvare în baza de date
            _logger.LogInformation("Saving pacient to database...");
            var createdPacient = await _pacientRepository.CreateAsync(pacient, cancellationToken);

            _logger.LogInformation("Pacient created successfully with ID: {Id}", createdPacient.Id);
            _logger.LogInformation("========== CreatePacientCommandHandler END (SUCCESS) ==========");

            return Result<Guid>.Success(
                createdPacient.Id,
                $"Pacientul {createdPacient.NumeComplet} a fost creat cu succes.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "========== CreatePacientCommandHandler EXCEPTION ==========");
            _logger.LogError("Exception Type: {Type}", ex.GetType().FullName);
            _logger.LogError("Exception Message: {Message}", ex.Message);

            return Result<Guid>.Failure($"Eroare la crearea pacientului: {ex.Message}");
        }
    }

    private async Task<Result> ValidateBusinessRules(CreatePacientCommand request, CancellationToken cancellationToken)
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

            // Verificare unicitate CNP
            var (cnpExists, _) = await _pacientRepository.CheckUniqueAsync(
                cnp: request.CNP,
                cancellationToken: cancellationToken);

            if (cnpExists)
                errors.Add($"Un pacient cu CNP-ul {request.CNP} există deja în sistem.");
        }

        // Validare Cod_Pacient (dacă este furnizat)
        if (!string.IsNullOrEmpty(request.Cod_Pacient))
        {
            var (_, codExists) = await _pacientRepository.CheckUniqueAsync(
                codPacient: request.Cod_Pacient,
                cancellationToken: cancellationToken);

            if (codExists)
                errors.Add($"Un pacient cu codul {request.Cod_Pacient} există deja în sistem.");
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
