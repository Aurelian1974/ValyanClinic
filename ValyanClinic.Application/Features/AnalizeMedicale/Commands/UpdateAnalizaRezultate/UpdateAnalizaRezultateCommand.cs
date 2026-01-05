using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands.UpdateAnalizaRezultate;

/// <summary>
/// Command pentru actualizare rezultate analiză
/// </summary>
public record UpdateAnalizaRezultateCommand : IRequest<Result<bool>>
{
    public Guid AnalizaId { get; init; }
    
    // Rezultate
    public DateTime DataEfectuare { get; init; }
    public string? NumeLaborator { get; init; }
    public string? ValoareRezultat { get; init; }
    public string? UnitatiMasura { get; init; }
    public decimal? ValoareNormalaMin { get; init; }
    public decimal? ValoareNormalaMax { get; init; }
    public bool EsteInAfaraLimitelor { get; init; }
    
    // Interpretare
    public string? InterpretareMedic { get; init; }
    
    // Document
    public string? CaleFisierRezultat { get; init; }
    
    // Audit
    public Guid ModificatDe { get; init; }
}
