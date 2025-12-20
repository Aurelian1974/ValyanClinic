using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Commands.FinalizeConsultatie;

/// <summary>
/// Command pentru finalizarea unei consultatii medicale
/// Schimba statusul din "In desfasurare" în "Finalizata"
/// Valideaza campurile obligatorii (MotivPrezentare, DiagnosticPozitiv)
/// Actualizeaza status-ul programarii asociate
/// </summary>
public class FinalizeConsulatieCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// ID-ul consultatiei de finalizat
    /// </summary>
    public Guid ConsultatieID { get; set; }

    /// <summary>
    /// Durata efectiva a consultatiei (in minute)
    /// Calculata din timer-ul UI
    /// </summary>
    public int DurataMinute { get; set; }

    /// <summary>
    /// ID-ul utilizatorului care finalizeaza consultatia
    /// </summary>
    public Guid ModificatDe { get; set; }
}
