using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands;

/// <summary>
/// Command pentru crearea unei analize medicale recomandate în consultație
/// </summary>
public record CreateAnalizaMedicalaCommand : IRequest<Result<Guid>>
{
    public Guid ConsultatieID { get; init; }
    public string TipAnaliza { get; init; } = string.Empty;
    public string NumeAnaliza { get; init; } = string.Empty;
    public string? CodAnaliza { get; init; }
    public string? Prioritate { get; init; }
    public bool EsteCito { get; init; }
    public string? IndicatiiClinice { get; init; }
    public string? ObservatiiRecomandare { get; init; }
    public Guid CreatDe { get; init; }
}
