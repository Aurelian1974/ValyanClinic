using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PersonalManagement.Commands.CreatePersonal;

/// <summary>
/// Command pentru crearea unui angajat nou - ALINIAT CU DB REALA
/// </summary>
public record CreatePersonalCommand : IRequest<Result<Guid>>
{
    public string Cod_Angajat { get; init; } = string.Empty;
    public string Nume { get; init; } = string.Empty;
    public string Prenume { get; init; } = string.Empty;
    public string? Nume_Anterior { get; init; }
    public string CNP { get; init; } = string.Empty;
    public DateTime Data_Nasterii { get; init; }
    public string? Locul_Nasterii { get; init; }
    public string Nationalitate { get; init; } = "Romana";
    public string Cetatenie { get; init; } = "Romana";

    // Contact
    public string? Telefon_Personal { get; init; }
    public string? Telefon_Serviciu { get; init; }
    public string? Email_Personal { get; init; }
    public string? Email_Serviciu { get; init; }

    // Adresa Domiciliu
    public string Adresa_Domiciliu { get; init; } = string.Empty;
    public string Judet_Domiciliu { get; init; } = string.Empty;
    public string Oras_Domiciliu { get; init; } = string.Empty;
    public string? Cod_Postal_Domiciliu { get; init; }

    // Adresa Resedinta
    public string? Adresa_Resedinta { get; init; }
    public string? Judet_Resedinta { get; init; }
    public string? Oras_Resedinta { get; init; }
    public string? Cod_Postal_Resedinta { get; init; }

    // Stare civila si pozitie
    public string? Stare_Civila { get; init; }
    public string Functia { get; init; } = string.Empty;
    public string? Departament { get; init; }

    // Carte Identitate
    public string? Serie_CI { get; init; }
    public string? Numar_CI { get; init; }
    public string? Eliberat_CI_De { get; init; }
    public DateTime? Data_Eliberare_CI { get; init; }
    public DateTime? Valabil_CI_Pana { get; init; }

    // Status
    public string Status_Angajat { get; init; } = "Activ";
    public string? Observatii { get; init; }

    // Audit
    public string CreatDe { get; init; } = string.Empty;
}
