namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList;

public class PersonalMedicalListMeta
{
    public PersonalMedicalFilterMetadata Filters { get; set; } = new();
    public PersonalMedicalStatistics Stats { get; set; } = new();
}
