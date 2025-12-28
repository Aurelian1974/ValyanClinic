namespace ValyanClinic.Domain.Entities;

public class FilterOption
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class PersonalMedicalFilterMetadataDto
{
    public List<FilterOption> AvailableDepartamente { get; set; } = new();
    public List<FilterOption> AvailablePozitii { get; set; } = new();
    public List<FilterOption> AvailableStatuses { get; set; } = new()
    {
        new FilterOption { Value = "true", Text = "Activ" },
        new FilterOption { Value = "false", Text = "Inactiv" }
    };
}

public class PersonalMedicalStatisticsDto
{
    public int TotalActiv { get; set; }
    public int TotalInactiv { get; set; }
}
