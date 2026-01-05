using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands.AddAnalizaToConsultatie;

/// <summary>
/// Command pentru adăugare analiză recomandată în consultație
/// </summary>
public record AddAnalizaToConsultatieCommand : IRequest<Result<Guid>>
{
    public Guid ConsultatieID { get; init; }
    public Guid PacientID { get; init; }
    public Guid? AnalizaNomenclatorId { get; init; } // Opțional - dacă e din nomenclator
    
    // Informații analiză
    public string NumeAnaliza { get; init; } = string.Empty;
    public string? Categorie { get; init; }
    public string? CodAnaliza { get; init; }
    
    // Status și prioritate
    public string Prioritate { get; init; } = "Normala";
    public bool EsteCito { get; init; }
    
    // Prescripție
    public string? IndicatiiMedic { get; init; }
    public DateTime? DataProgramata { get; init; }
    
    // Audit
    public Guid CreatDe { get; init; }
}
