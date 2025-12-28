namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList;

public record ColumnFilterDto
{
    public string Column { get; init; } = string.Empty; // e.g., "Nume", "Specializare", "NumarLicenta"
    public string Operator { get; init; } = "Contains"; // Contains | Equals | StartsWith | EndsWith
    public string Value { get; init; } = string.Empty;
} 