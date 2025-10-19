using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PacientManagement.Commands.CreatePacient;

/// <summary>
/// Command pentru crearea unui pacient nou
/// </summary>
public record CreatePacientCommand : IRequest<Result<Guid>>
{
    // Date personale obligatorii
    public string Nume { get; init; } = string.Empty;
    public string Prenume { get; init; } = string.Empty;
    public DateTime Data_Nasterii { get; init; }
    public string Sex { get; init; } = string.Empty; // 'M' sau 'F'
    
    // Identificare (opționale)
    public string? CNP { get; init; }
    public string? Cod_Pacient { get; init; } // Auto-generat dacă null
    
    // Date de contact (opționale)
    public string? Telefon { get; init; }
    public string? Telefon_Secundar { get; init; }
    public string? Email { get; init; }
    
    // Adresă (opționale)
    public string? Judet { get; init; }
    public string? Localitate { get; init; }
    public string? Adresa { get; init; }
    public string? Cod_Postal { get; init; }
    
    // Informații asigurare (opționale)
    public bool Asigurat { get; init; } = false;
    public string? CNP_Asigurat { get; init; }
    public string? Nr_Card_Sanatate { get; init; }
    public string? Casa_Asigurari { get; init; }
    
    // Date medicale de bază (opționale)
    public string? Alergii { get; init; }
    public string? Boli_Cronice { get; init; }
    public string? Medic_Familie { get; init; }
    
    // Contact urgență (opționale)
    public string? Persoana_Contact { get; init; }
    public string? Telefon_Urgenta { get; init; }
    public string? Relatie_Contact { get; init; }
    
    // Status și observații
    public bool Activ { get; init; } = true;
    public string? Observatii { get; init; }
    
    // Audit
    public string CreatDe { get; init; } = string.Empty;
}
