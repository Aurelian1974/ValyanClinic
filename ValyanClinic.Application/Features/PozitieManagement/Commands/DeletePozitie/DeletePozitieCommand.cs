using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PozitieManagement.Commands.DeletePozitie;

public record DeletePozitieCommand(Guid Id) : IRequest<Result<bool>>;
