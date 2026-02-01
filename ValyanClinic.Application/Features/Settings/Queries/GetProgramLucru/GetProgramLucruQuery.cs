using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.Settings.Queries.GetProgramLucru;

/// <summary>
/// Query pentru obținerea programului de lucru complet al clinicii
/// </summary>
public record GetProgramLucruQuery : IRequest<Result<List<ProgramLucruDto>>>;
