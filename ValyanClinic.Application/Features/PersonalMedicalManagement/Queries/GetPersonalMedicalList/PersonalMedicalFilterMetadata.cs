namespace ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList;

public class FilterOptionDto
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class PersonalMedicalFilterMetadata
{
    public List<FilterOptionDto> AvailableDepartamente { get; set; } = new();
    public List<FilterOptionDto> AvailablePozitii { get; set; } = new();
    public List<FilterOptionDto> AvailableStatuses { get; set; } = new()
    {
        new FilterOptionDto { Value = "true", Text = "Activ" },
        new FilterOptionDto { Value = "false", Text = "Inactiv" }
    };
}

public class PersonalMedicalStatistics
{
    public int TotalActiv { get; set; }
    public int TotalInactiv { get; set; }
    // Add more stats as needed
}
