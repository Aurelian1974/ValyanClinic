using MediatR;
using ValyanClinic.Application.Common.Results;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

namespace ValyanClinic.Application.Features.ConsultatieManagement.Queries.GetConsulatieByProgramare;

/// <summary>
/// Query pentru obtinerea consultatiei asociate cu o programare
/// Folosit pentru a verifica daca o programare are deja consultatie creata
/// </summary>
public record GetConsulatieByProgramareQuery(Guid ProgramareID) : IRequest<Result<ConsulatieDetailDto?>>;
