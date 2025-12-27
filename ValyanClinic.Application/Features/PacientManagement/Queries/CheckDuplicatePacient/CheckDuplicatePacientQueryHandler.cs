using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.PacientManagement.Queries.CheckDuplicatePacient;

/// <summary>
/// Handler pentru CheckDuplicatePacientQuery
/// </summary>
public class CheckDuplicatePacientQueryHandler : IRequestHandler<CheckDuplicatePacientQuery, Result<DuplicatePacientResult>>
{
    private readonly IPacientRepository _pacientRepository;

    public CheckDuplicatePacientQueryHandler(IPacientRepository pacientRepository)
    {
        _pacientRepository = pacientRepository;
    }

    public async Task<Result<DuplicatePacientResult>> Handle(
        CheckDuplicatePacientQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = new DuplicatePacientResult
            {
                HasDuplicate = false,
                Type = DuplicateType.None
            };

            // 1. Verifică CNP duplicat (cea mai strictă verificare)
            if (!string.IsNullOrWhiteSpace(request.CNP) && request.CNP.Length == 13)
            {
                var pacientByCnp = await _pacientRepository.GetByCNPAsync(request.CNP, cancellationToken);
                
                if (pacientByCnp != null && pacientByCnp.Id != request.ExcludeId)
                {
                    return Result<DuplicatePacientResult>.Success(new DuplicatePacientResult
                    {
                        HasDuplicate = true,
                        Type = DuplicateType.ExactCNP,
                        DuplicatePacientId = pacientByCnp.Id,
                        DuplicatePacientName = $"{pacientByCnp.Nume} {pacientByCnp.Prenume}",
                        DuplicatePacientCod = pacientByCnp.Cod_Pacient,
                        WarningMessage = $"Atenție! Există deja un pacient cu acest CNP: {pacientByCnp.Nume} {pacientByCnp.Prenume} (Cod: {pacientByCnp.Cod_Pacient})"
                    });
                }
            }

            // 2. Verifică duplicat după Nume + Prenume + Data Nașterii
            if (!string.IsNullOrWhiteSpace(request.Nume) && 
                !string.IsNullOrWhiteSpace(request.Prenume) && 
                request.DataNasterii.HasValue)
            {
                // Caută pacienti cu date similare
                var (pacienti, _) = await _pacientRepository.GetPagedAsync(
                    pageNumber: 1,
                    pageSize: 10,
                    searchText: $"{request.Nume} {request.Prenume}",
                    cancellationToken: cancellationToken);

                foreach (var pacient in pacienti)
                {
                    // Exclude pacientul curent (la editare)
                    if (pacient.Id == request.ExcludeId)
                        continue;

                    // Verifică potrivire exactă pe nume și data nașterii
                    var numeMatch = string.Equals(pacient.Nume?.Trim(), request.Nume?.Trim(), StringComparison.OrdinalIgnoreCase);
                    var prenumeMatch = string.Equals(pacient.Prenume?.Trim(), request.Prenume?.Trim(), StringComparison.OrdinalIgnoreCase);
                    var dataNastereMatch = pacient.Data_Nasterii.Date == request.DataNasterii.Value.Date;

                    if (numeMatch && prenumeMatch && dataNastereMatch)
                    {
                        return Result<DuplicatePacientResult>.Success(new DuplicatePacientResult
                        {
                            HasDuplicate = true,
                            Type = DuplicateType.ExactNameAndBirthDate,
                            DuplicatePacientId = pacient.Id,
                            DuplicatePacientName = $"{pacient.Nume} {pacient.Prenume}",
                            DuplicatePacientCod = pacient.Cod_Pacient,
                            WarningMessage = $"Atenție! Există deja un pacient cu același nume și dată de naștere: {pacient.Nume} {pacient.Prenume} (Cod: {pacient.Cod_Pacient})"
                        });
                    }
                }
            }

            return Result<DuplicatePacientResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<DuplicatePacientResult>.Failure($"Eroare la verificarea duplicatelor: {ex.Message}");
        }
    }
}
