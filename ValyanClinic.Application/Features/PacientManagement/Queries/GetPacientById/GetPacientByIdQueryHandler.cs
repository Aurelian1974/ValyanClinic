using MediatR;
using ValyanClinic.Application.Common.Exceptions;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById;

/// <summary>
/// Handler pentru GetPacientByIdQuery
/// </summary>
public class GetPacientByIdQueryHandler : IRequestHandler<GetPacientByIdQuery, Result<PacientDetailDto>>
{
    private readonly IPacientRepository _pacientRepository;

    public GetPacientByIdQueryHandler(IPacientRepository pacientRepository)
    {
        _pacientRepository = pacientRepository;
    }

    public async Task<Result<PacientDetailDto>> Handle(
        GetPacientByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var pacient = await _pacientRepository.GetByIdAsync(request.Id, cancellationToken);

            if (pacient == null)
            {
                throw new NotFoundException($"Pacientul cu ID-ul {request.Id} nu a fost gasit.");
            }

            var dto = new PacientDetailDto
            {
                Id = pacient.Id,
                Cod_Pacient = pacient.Cod_Pacient,
                CNP = pacient.CNP,
                Nume = pacient.Nume,
                Prenume = pacient.Prenume,
                NumeComplet = pacient.NumeComplet,
                Data_Nasterii = pacient.Data_Nasterii,
                Varsta = pacient.Varsta,
                Sex = pacient.Sex,
                Telefon = pacient.Telefon,
                Telefon_Secundar = pacient.Telefon_Secundar,
                Email = pacient.Email,
                Judet = pacient.Judet,
                Localitate = pacient.Localitate,
                Adresa = pacient.Adresa,
                AdresaCompleta = pacient.AdresaCompleta,
                Cod_Postal = pacient.Cod_Postal,
                Asigurat = pacient.Asigurat,
                CNP_Asigurat = pacient.CNP_Asigurat,
                Nr_Card_Sanatate = pacient.Nr_Card_Sanatate,
                Casa_Asigurari = pacient.Casa_Asigurari,
                Alergii = pacient.Alergii,
                Boli_Cronice = pacient.Boli_Cronice,
                Medic_Familie = pacient.Medic_Familie,
                Persoana_Contact = pacient.Persoana_Contact,
                Telefon_Urgenta = pacient.Telefon_Urgenta,
                Relatie_Contact = pacient.Relatie_Contact,
                Data_Inregistrare = pacient.Data_Inregistrare,
                Ultima_Vizita = pacient.Ultima_Vizita,
                Nr_Total_Vizite = pacient.Nr_Total_Vizite,
                Activ = pacient.Activ,
                Observatii = pacient.Observatii,
                Data_Crearii = pacient.Data_Crearii,
                Data_Ultimei_Modificari = pacient.Data_Ultimei_Modificari,
                Creat_De = pacient.Creat_De,
                Modificat_De = pacient.Modificat_De
            };

            return Result<PacientDetailDto>.Success(dto);
        }
        catch (NotFoundException ex)
        {
            return Result<PacientDetailDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<PacientDetailDto>.Failure(
                $"Eroare la obtinerea detaliilor pacientului: {ex.Message}");
        }
    }
}
