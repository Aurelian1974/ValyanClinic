using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.ProgramareManagement.Commands.DeleteProgramare;

/// <summary>
/// Command pentru ștergerea (anularea) unei programări.
/// Implementează soft delete - marchează programarea ca "Anulata".
/// </summary>
public class DeleteProgramareCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// ID-ul programării de șters.
    /// </summary>
    public Guid ProgramareID { get; set; }

    /// <summary>
    /// ID-ul utilizatorului care efectuează ștergerea.
    /// </summary>
    public Guid ModificatDe { get; set; }

    /// <summary>
    /// Motiv anulare (opțional, pentru audit).
    /// </summary>
    public string? MotivAnulare { get; set; }
}
