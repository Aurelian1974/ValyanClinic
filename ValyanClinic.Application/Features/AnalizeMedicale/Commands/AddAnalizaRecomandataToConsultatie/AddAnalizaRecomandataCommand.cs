using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands.AddAnalizaRecomandataToConsultatie;

/// <summary>
/// Command pentru adăugare analiză recomandată în consultație
/// </summary>
public record AddAnalizaRecomandataCommand : IRequest<Result<Guid>>
{
    public Guid ConsultatieID { get; init; }
    public Guid? AnalizaNomenclatorID { get; init; }
    public string NumeAnaliza { get; init; } = string.Empty;
    public string? CodAnaliza { get; init; }
    public string TipAnaliza { get; init; } = "Laborator"; // Categoria din nomenclator
    public string? Prioritate { get; init; }
    public bool EsteCito { get; init; }
    public string? IndicatiiClinice { get; init; }
    public string? ObservatiiMedic { get; init; }
    public Guid CreatDe { get; init; }
}
