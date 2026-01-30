using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.ViewModels;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Queries;

/// <summary>
/// Handler pentru query-ul de obținere a analizelor medicale ale unui pacient
/// Caută în tabela ConsultatieAnalizeMedicale (analize efectuate/importate)
/// </summary>
public class GetAnalizeMedicaleByPacientQueryHandler
    : IRequestHandler<GetAnalizeMedicaleByPacientQuery, Result<List<AnalizeMedicaleGroupDto>>>
{
    private readonly IConsultatieAnalizaMedicalaRepository _analizaRepository;

    public GetAnalizeMedicaleByPacientQueryHandler(IConsultatieAnalizaMedicalaRepository analizaRepository)
    {
        _analizaRepository = analizaRepository;
    }

    public async Task<Result<List<AnalizeMedicaleGroupDto>>> Handle(
        GetAnalizeMedicaleByPacientQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get toate analizele pentru pacient (doar cele cu rezultate)
            var analize = await _analizaRepository.GetByPacientIdAsync(
                request.PacientId,
                doarCuRezultate: true,
                cancellationToken: cancellationToken);

            // Map entități -> DTOs
            var analizeDtos = analize.Select(a => new AnalizaMedicalaDto
            {
                Id = a.Id,
                PacientId = request.PacientId,
                ConsultatieId = a.ConsultatieID,
                DataDocument = a.DataEfectuare ?? a.DataRecomandare,
                NumeDocument = $"Import {(a.DataEfectuare ?? a.DataRecomandare):dd.MM.yyyy}",
                SursaDocument = a.LocEfectuare,
                BatchId = a.ConsultatieID,
                Categorie = a.TipAnaliza,
                NumeAnaliza = a.NumeAnaliza,
                Rezultat = a.ValoareRezultat,
                UnitateMasura = a.UnitatiMasura,
                IntervalReferinta = FormatIntervalReferinta(a.ValoareNormalaMin, a.ValoareNormalaMax),
                InAfaraLimitelor = a.EsteInAfaraLimitelor,
                DataCrearii = a.DataCreare
            }).ToList();

            // Grupăm analizele pe ConsultatieId (BatchId) și DataDocument
            var groups = analizeDtos
                .GroupBy(a => new { a.BatchId, a.DataDocument, a.NumeDocument, a.SursaDocument })
                .Select(g => new AnalizeMedicaleGroupDto
                {
                    DataDocument = g.Key.DataDocument,
                    NumeDocument = g.Key.NumeDocument,
                    SursaDocument = g.Key.SursaDocument,
                    BatchId = g.Key.BatchId,
                    Analize = g.ToList()
                })
                .OrderByDescending(g => g.DataDocument)
                .ToList();

            return Result<List<AnalizeMedicaleGroupDto>>.Success(groups);
        }
        catch (Exception ex)
        {
            return Result<List<AnalizeMedicaleGroupDto>>.Failure($"Eroare la încărcarea analizelor: {ex.Message}");
        }
    }

    private static string? FormatIntervalReferinta(decimal? min, decimal? max)
    {
        if (min.HasValue && max.HasValue)
            return $"{min} - {max}";
        if (min.HasValue)
            return $"> {min}";
        if (max.HasValue)
            return $"< {max}";
        return null;
    }
}
