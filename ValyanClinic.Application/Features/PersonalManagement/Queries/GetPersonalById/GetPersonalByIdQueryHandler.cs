using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PersonalManagement.Queries.GetPersonalById;

/// <summary>
/// Handler pentru GetPersonalByIdQuery - ALINIAT CU DB REALA
/// </summary>
public class GetPersonalByIdQueryHandler : IRequestHandler<GetPersonalByIdQuery, Result<PersonalDetailDto>>
{
    private readonly IPersonalRepository _personalRepository;
    private readonly ILogger<GetPersonalByIdQueryHandler> _logger;

    public GetPersonalByIdQueryHandler(
        IPersonalRepository personalRepository,
        ILogger<GetPersonalByIdQueryHandler> logger)
    {
        _personalRepository = personalRepository;
        _logger = logger;
    }

    public async Task<Result<PersonalDetailDto>> Handle(GetPersonalByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obtin detalii personal: {PersonalId}", request.Id);

            var personal = await _personalRepository.GetByIdAsync(request.Id, cancellationToken);
            if (personal == null)
            {
                _logger.LogWarning("Personalul cu ID-ul {PersonalId} nu a fost gasit", request.Id);
                return Result<PersonalDetailDto>.Failure($"Angajatul cu ID-ul {request.Id} nu a fost gasit");
            }

            var dto = new PersonalDetailDto
            {
                Id_Personal = personal.Id_Personal,
                Cod_Angajat = personal.Cod_Angajat,
                Nume = personal.Nume,
                Prenume = personal.Prenume,
                Nume_Anterior = personal.Nume_Anterior,
                CNP = personal.CNP,
                Data_Nasterii = personal.Data_Nasterii,
                Locul_Nasterii = personal.Locul_Nasterii,
                Nationalitate = personal.Nationalitate,
                Cetatenie = personal.Cetatenie,
                Telefon_Personal = personal.Telefon_Personal,
                Telefon_Serviciu = personal.Telefon_Serviciu,
                Email_Personal = personal.Email_Personal,
                Email_Serviciu = personal.Email_Serviciu,
                Adresa_Domiciliu = personal.Adresa_Domiciliu,
                Judet_Domiciliu = personal.Judet_Domiciliu,
                Oras_Domiciliu = personal.Oras_Domiciliu,
                Cod_Postal_Domiciliu = personal.Cod_Postal_Domiciliu,
                Adresa_Resedinta = personal.Adresa_Resedinta,
                Judet_Resedinta = personal.Judet_Resedinta,
                Oras_Resedinta = personal.Oras_Resedinta,
                Cod_Postal_Resedinta = personal.Cod_Postal_Resedinta,
                Stare_Civila = personal.Stare_Civila,
                Functia = personal.Functia,
                Departament = personal.Departament,
                Serie_CI = personal.Serie_CI,
                Numar_CI = personal.Numar_CI,
                Eliberat_CI_De = personal.Eliberat_CI_De,
                Data_Eliberare_CI = personal.Data_Eliberare_CI,
                Valabil_CI_Pana = personal.Valabil_CI_Pana,
                Status_Angajat = personal.Status_Angajat,
                Observatii = personal.Observatii,
                Data_Crearii = personal.Data_Crearii,
                Data_Ultimei_Modificari = personal.Data_Ultimei_Modificari,
                Creat_De = personal.Creat_De,
                Modificat_De = personal.Modificat_De
            };

            _logger.LogInformation("Detalii personal obtinute cu succes: {PersonalId}", request.Id);

            return Result<PersonalDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la obtinerea detaliilor personalului: {PersonalId}", request.Id);
            return Result<PersonalDetailDto>.Failure($"Eroare la obtinerea detaliilor angajatului: {ex.Message}");
        }
    }
}
