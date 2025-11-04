namespace ValyanClinic.Application.Features.DepartamentManagement.Queries.GetDepartamentById;

public class DepartamentDetailDto
{
    public Guid IdDepartament { get; set; }
    public Guid? IdTipDepartament { get; set; }
    public string DenumireDepartament { get; set; } = string.Empty;
    public string? DescriereDepartament { get; set; }
    public string? DenumireTipDepartament { get; set; }
}
