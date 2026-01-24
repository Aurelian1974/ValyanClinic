using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.ViewModels;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Queries;

/// <summary>
/// Handler pentru obținerea analizelor medicale din consultație
/// </summary>
public class GetAnalizeMedicaleByConsultatieQueryHandler
    : IRequestHandler<GetAnalizeMedicaleByConsultatieQuery, Result<List<ConsultatieAnalizaMedicalaDto>>>
{
    private readonly IConsultatieAnalizaMedicalaRepository _analizaRepository;

    public GetAnalizeMedicaleByConsultatieQueryHandler(IConsultatieAnalizaMedicalaRepository analizaRepository)
    {
        _analizaRepository = analizaRepository;
    }

    public async Task<Result<List<ConsultatieAnalizaMedicalaDto>>> Handle(
        GetAnalizeMedicaleByConsultatieQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var analize = await _analizaRepository.GetByConsultatieIdWithDetailsAsync(
                request.ConsultatieId,
                cancellationToken);

            // Map entități -> DTOs
            var analizeDtos = analize.Select(a => new ConsultatieAnalizaMedicalaDto
            {
                Id = a.Id,
                ConsultatieID = a.ConsultatieID,
                TipAnaliza = a.TipAnaliza,
                NumeAnaliza = a.NumeAnaliza,
                CodAnaliza = a.CodAnaliza,
                StatusAnaliza = a.StatusAnaliza,
                DataRecomandare = a.DataRecomandare,
                DataProgramata = a.DataProgramata,
                DataEfectuare = a.DataEfectuare,
                LocEfectuare = a.LocEfectuare,
                Prioritate = a.Prioritate,
                EsteCito = a.EsteCito,
                IndicatiiClinice = a.IndicatiiClinice,
                ObservatiiRecomandare = a.ObservatiiRecomandare,
                AreRezultate = a.AreRezultate,
                DataRezultate = a.DataRezultate,
                ValoareRezultat = a.ValoareRezultat,
                UnitatiMasura = a.UnitatiMasura,
                ValoareNormalaMin = a.ValoareNormalaMin,
                ValoareNormalaMax = a.ValoareNormalaMax,
                EsteInAfaraLimitelor = a.EsteInAfaraLimitelor,
                InterpretareMedic = a.InterpretareMedic,
                ConclusiiAnaliza = a.ConclusiiAnaliza,
                CaleFisierRezultat = a.CaleFisierRezultat,
                TipFisier = a.TipFisier,
                Pret = a.Pret,
                Decontat = a.Decontat,
                Detalii = a.Detalii?.Select(d => new ConsultatieAnalizaDetaliuDto
                {
                    Id = d.Id,
                    AnalizaMedicalaID = d.AnalizaMedicalaID,
                    NumeParametru = d.NumeParametru,
                    CodParametru = d.CodParametru,
                    Valoare = d.Valoare,
                    UnitatiMasura = d.UnitatiMasura,
                    TipValoare = d.TipValoare,
                    ValoareNormalaMin = d.ValoareNormalaMin,
                    ValoareNormalaMax = d.ValoareNormalaMax,
                    ValoareNormalaText = d.ValoareNormalaText,
                    EsteAnormal = d.EsteAnormal,
                    NivelGravitate = d.NivelGravitate,
                    Observatii = d.Observatii
                }).ToList()
            }).ToList();

            return Result<List<ConsultatieAnalizaMedicalaDto>>.Success(analizeDtos);
        }
        catch (Exception ex)
        {
            return Result<List<ConsultatieAnalizaMedicalaDto>>.Failure(
                $"Eroare la încărcarea analizelor: {ex.Message}");
        }
    }
}
