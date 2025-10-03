using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PersonalManagement.Queries.GetPersonalList;

/// <summary>
/// Handler pentru GetPersonalListQuery - ALINIAT CU DB REALA
/// </summary>
public class GetPersonalListQueryHandler : IRequestHandler<GetPersonalListQuery, Result<IEnumerable<PersonalListDto>>>
{
    private readonly IPersonalRepository _personalRepository;
    private readonly ILogger<GetPersonalListQueryHandler> _logger;

    public GetPersonalListQueryHandler(
        IPersonalRepository personalRepository,
        ILogger<GetPersonalListQueryHandler> logger)
    {
        _personalRepository = personalRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<PersonalListDto>>> Handle(GetPersonalListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obtin lista completa de personal");

            var personalList = await _personalRepository.GetAllAsync(
                pageNumber: 1,
                pageSize: 1000, // Temporary - trebuie adaugat paginare in query
                cancellationToken: cancellationToken);
            
            var dtoList = personalList.Select(p => new PersonalListDto
            {
                Id_Personal = p.Id_Personal,
                Cod_Angajat = p.Cod_Angajat,
                Nume = p.Nume,
                Prenume = p.Prenume,
                CNP = p.CNP,
                Telefon_Personal = p.Telefon_Personal,
                Email_Personal = p.Email_Personal,
                Data_Nasterii = p.Data_Nasterii,
                Status_Angajat = p.Status_Angajat,
                Judet_Domiciliu = p.Judet_Domiciliu,
                Oras_Domiciliu = p.Oras_Domiciliu,
                Functia = p.Functia,
                Departament = p.Departament
            }).ToList();

            _logger.LogInformation("Lista de personal obtinuta cu succes: {Count} angajati", dtoList.Count);

            return Result<IEnumerable<PersonalListDto>>.Success(dtoList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la obtinerea listei de personal");
            return Result<IEnumerable<PersonalListDto>>.Failure($"Eroare la obtinerea listei de angajati: {ex.Message}");
        }
    }
}
