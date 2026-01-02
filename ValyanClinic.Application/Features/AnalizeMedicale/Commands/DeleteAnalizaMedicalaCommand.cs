using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands;

/// <summary>
/// Command pentru ștergerea unei analize medicale recomandate
/// </summary>
public record DeleteAnalizaMedicalaCommand(Guid Id) : IRequest<Result<bool>>;
