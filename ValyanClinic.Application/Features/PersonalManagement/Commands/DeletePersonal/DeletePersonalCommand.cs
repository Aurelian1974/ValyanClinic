using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PersonalManagement.Commands.DeletePersonal;

/// <summary>
/// Command pentru stergerea unui angajat
/// </summary>
public record DeletePersonalCommand(Guid Id, string StersDe) : IRequest<Result<bool>>;
