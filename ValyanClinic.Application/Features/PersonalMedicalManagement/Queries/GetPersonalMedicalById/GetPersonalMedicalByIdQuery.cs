using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalById;

public record GetPersonalMedicalByIdQuery(Guid PersonalID) : IRequest<Result<PersonalMedicalDetailDto>>;
