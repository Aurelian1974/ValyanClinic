using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.AnalizeMedicale.Commands.DeleteAnalizaRecomandata;

/// <summary>
/// Command pentru ștergere analiză recomandată
/// </summary>
public record DeleteAnalizaRecomandataCommand(Guid AnalizaId) : IRequest<Result<bool>>;
