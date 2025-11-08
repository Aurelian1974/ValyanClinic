using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.UserSessions.Commands.EndSession;

/// <summary>
/// Command pentru închiderea forțată a unei sesiuni (logout forțat)
/// </summary>
public record EndSessionCommand(
    Guid SessionID,
    string ModificatDe = "Admin"
) : IRequest<Result<bool>>;
