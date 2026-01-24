using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands;

/// <summary>
/// Handler pentru crearea unei analize medicale recomandate
/// </summary>
public class CreateAnalizaMedicalaCommandHandler
    : IRequestHandler<CreateAnalizaMedicalaCommand, Result<Guid>>
{
    private readonly IConsultatieAnalizaMedicalaRepository _analizaRepository;

    public CreateAnalizaMedicalaCommandHandler(IConsultatieAnalizaMedicalaRepository analizaRepository)
    {
        _analizaRepository = analizaRepository;
    }

    public async Task<Result<Guid>> Handle(
        CreateAnalizaMedicalaCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var analiza = new ConsultatieAnalizaMedicala
            {
                ConsultatieID = request.ConsultatieID,
                TipAnaliza = request.TipAnaliza,
                NumeAnaliza = request.NumeAnaliza,
                CodAnaliza = request.CodAnaliza,
                StatusAnaliza = "Recomandata",
                DataRecomandare = DateTime.Now,
                Prioritate = request.Prioritate,
                EsteCito = request.EsteCito,
                IndicatiiClinice = request.IndicatiiClinice,
                ObservatiiRecomandare = request.ObservatiiRecomandare,
                AreRezultate = false,
                EsteInAfaraLimitelor = false,
                Decontat = false,
                CreatDe = request.CreatDe
            };

            var id = await _analizaRepository.CreateAsync(analiza, cancellationToken);

            return Result<Guid>.Success(id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Eroare la crearea analizei: {ex.Message}");
        }
    }
}
