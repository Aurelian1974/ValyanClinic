using MediatR;
using ValyanClinic.Application.Common.Results;

namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Commands.UpdatePersonalMedical;

public record UpdatePersonalMedicalCommand : IRequest<Result<bool>>
{
    public Guid PersonalID { get; init; }
    public string Nume { get; init; } = string.Empty;
    public string Prenume { get; init; } = string.Empty;
    public string? Specializare { get; init; }
    public string? NumarLicenta { get; init; }
    public string? Telefon { get; init; }
    public string? Email { get; init; }
    public string? Departament { get; init; }
    public string? Pozitie { get; init; }
    public bool EsteActiv { get; init; } = true;
    public Guid? CategorieID { get; init; }
    public Guid? PozitieID { get; init; }
    public Guid? SpecializareID { get; init; }
    public Guid? SubspecializareID { get; init; }
}
