using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Commands.DeletePersonalMedical;

public record DeletePersonalMedicalCommand(Guid PersonalID, string StersDe) : IRequest<Result<bool>>;
