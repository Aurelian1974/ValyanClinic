namespace ValyanClinic.Domain.Entities;

public class Departament
{
    public Guid IdDepartament { get; set; }
    public Guid? IdTipDepartament { get; set; }
    public string DenumireDepartament { get; set; } = string.Empty;
    public string? DescriereDepartament { get; set; }

    public TipDepartament? TipDepartament { get; set; }
}
