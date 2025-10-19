using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PacientManagement.Commands.UpdatePacient;

/// <summary>
/// Command pentru actualizarea unui pacient existent
/// </summary>
public record UpdatePacientCommand : IRequest<Result<Guid>>
{
    // ID pentru identificare
    public Guid Id { get; init; }
    
    // Date personale obligatorii
    public string Nume { get; init; } = string.Empty;
    public string Prenume { get; init; } = string.Empty;
    public DateTime Data_Nasterii { get; init; }
    public string Sex { get; init; } = string.Empty;
    
    // Identificare (CNP poate fi null)
    public string? CNP { get; init; }
    // Cod_Pacient nu se modifică după creare
    
    // Date de contact
    public string? Telefon { get; init; }
    public string? Telefon_Secundar { get; init; }
    public string? Email { get; init; }
    
    // Adresă
    public string? Judet { get; init; }
    public string? Localitate { get; init; }
    public string? Adresa { get; init; }
    public string? Cod_Postal { get; init; }
    
    // Informații asigurare
    public bool Asigurat { get; init; }
    public string? CNP_Asigurat { get; init; }
    public string? Nr_Card_Sanatate { get; init; }
    public string? Casa_Asigurari { get; init; }
    
    // Date medicale de bază
    public string? Alergii { get; init; }
    public string? Boli_Cronice { get; init; }
    public string? Medic_Familie { get; init; }
    
    // Contact urgență
    public string? Persoana_Contact { get; init; }
    public string? Telefon_Urgenta { get; init; }
    public string? Relatie_Contact { get; init; }
    
    // Status și observații
    public bool Activ { get; init; }
    public string? Observatii { get; init; }
    
    // Audit
    public string ModificatDe { get; init; } = string.Empty;
}
