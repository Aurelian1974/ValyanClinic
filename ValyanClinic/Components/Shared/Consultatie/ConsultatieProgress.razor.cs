using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Shared.Consultatie;

public partial class ConsultatieProgress : ComponentBase
{
    [Parameter] public List<string> Sections { get; set; } = new();
    [Parameter] public HashSet<string> CompletedSections { get; set; } = new();
    [Parameter] public string ActiveSection { get; set; } = "";
    
    private int ProgressPercentage
    {
        get
        {
            if (Sections.Count == 0) return 0;
            return (int)((CompletedSections.Count / (double)Sections.Count) * 100);
        }
    }
    
    private bool IsSectionCompleted(string section)
    {
        return CompletedSections.Contains(section);
    }
    
    private string GetSectionLabel(string section)
    {
        return section switch
        {
            "motive" => "Motive",
            "antecedente" => "Antecedente",
            "examen" => "Examen",
            "investigatii" => "Investigații",
            "diagnostic" => "Diagnostic",
            "tratament" => "Tratament",
            "concluzie" => "Concluzie",
            _ => section
        };
    }
}
