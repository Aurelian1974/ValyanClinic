using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.SpecializareManagement.Commands.DeleteSpecializare;

public record DeleteSpecializareCommand(Guid Id) : IRequest<Result<bool>>;
