using Dapper;
using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.ViewModels;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Queries;

/// <summary>
/// Handler pentru obținerea analizelor medicale din consultație
/// </summary>
public class GetAnalizeMedicaleByConsultatieQueryHandler 
    : IRequestHandler<GetAnalizeMedicaleByConsultatieQuery, Result<List<ConsultatieAnalizaMedicalaDto>>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetAnalizeMedicaleByConsultatieQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<List<ConsultatieAnalizaMedicalaDto>>> Handle(
        GetAnalizeMedicaleByConsultatieQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = @"
                -- Analize
                SELECT 
                    Id,
                    ConsultatieID,
                    TipAnaliza,
                    NumeAnaliza,
                    CodAnaliza,
                    StatusAnaliza,
                    DataRecomandare,
                    DataProgramata,
                    DataEfectuare,
                    LocEfectuare,
                    Prioritate,
                    EsteCito,
                    IndicatiiClinice,
                    ObservatiiRecomandare,
                    AreRezultate,
                    DataRezultate,
                    ValoareRezultat,
                    UnitatiMasura,
                    ValoareNormalaMin,
                    ValoareNormalaMax,
                    EsteInAfaraLimitelor,
                    InterpretareMedic,
                    ConclusiiAnaliza,
                    CaleFisierRezultat,
                    TipFisier,
                    Pret,
                    Decontat
                FROM ConsultatieAnalizeMedicale
                WHERE ConsultatieID = @ConsultatieId
                ORDER BY DataRecomandare DESC, NumeAnaliza;

                -- Detalii pentru analize cu rezultate
                SELECT 
                    d.Id,
                    d.AnalizaMedicalaID,
                    d.NumeParametru,
                    d.CodParametru,
                    d.Valoare,
                    d.UnitatiMasura,
                    d.TipValoare,
                    d.ValoareNormalaMin,
                    d.ValoareNormalaMax,
                    d.ValoareNormalaText,
                    d.EsteAnormal,
                    d.NivelGravitate,
                    d.Observatii
                FROM ConsultatieAnalizaDetalii d
                INNER JOIN ConsultatieAnalizeMedicale a ON d.AnalizaMedicalaID = a.Id
                WHERE a.ConsultatieID = @ConsultatieId
                ORDER BY d.NumeParametru";

            using var connection = _connectionFactory.CreateConnection();
            using var multi = await connection.QueryMultipleAsync(sql, new { request.ConsultatieId });
            
            var analize = (await multi.ReadAsync<ConsultatieAnalizaMedicalaDto>()).ToList();
            var detalii = (await multi.ReadAsync<ConsultatieAnalizaDetaliuDto>()).ToList();

            // Grupăm detaliile pe AnalizaMedicalaID
            var detaliiGrouped = detalii.GroupBy(d => d.AnalizaMedicalaID)
                                        .ToDictionary(g => g.Key, g => g.ToList());

            // Atasăm detaliile la fiecare analiză
            foreach (var analiza in analize)
            {
                if (detaliiGrouped.TryGetValue(analiza.Id, out var analizaDetalii))
                {
                    analiza.Detalii = analizaDetalii;
                }
            }

            return Result<List<ConsultatieAnalizaMedicalaDto>>.Success(analize);
        }
        catch (Exception ex)
        {
            return Result<List<ConsultatieAnalizaMedicalaDto>>.Failure(
                $"Eroare la încărcarea analizelor: {ex.Message}");
        }
    }
}
