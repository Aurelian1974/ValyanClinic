using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.Settings.Queries.GetActiveSessions;

public record GetActiveSessionsQuery : IRequest<Result<List<UserSessionDto>>>;
