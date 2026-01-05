using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands.DeleteAnalizaEfectuata;

/// <summary>
/// Command pentru ștergerea unei analize efectuate (cu rezultate)
/// </summary>
public record DeleteAnalizaEfectuataCommand(Guid Id) : IRequest<Result<bool>>;
