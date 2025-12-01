using Microsoft.AspNetCore.Components;

namespace ValyanClinic.Components.Shared.Consultatie;

public partial class ConsultatieTabs : ComponentBase
{
    [Parameter] public List<string> Tabs { get; set; } = new();
    [Parameter] public string ActiveTab { get; set; } = "";
    [Parameter] public HashSet<string> CompletedTabs { get; set; } = new();
    [Parameter] public EventCallback<string> OnTabChanged { get; set; }
    [Parameter] public bool IsDisabled { get; set; } = false;
    [Parameter] public Dictionary<string, int>? TabBadges { get; set; }

    private async Task OnTabClick(string tab)
    {
        if (!IsDisabled && tab != ActiveTab)
        {
            await OnTabChanged.InvokeAsync(tab);
        }
    }

    private bool IsTabCompleted(string tab)
    {
        return CompletedTabs.Contains(tab);
    }

    private string GetTabLabel(string tab)
    {
        return tab switch
        {
            "motive" => "Motive Prezentare",
            "antecedente" => "Antecedente",
            "examen" => "Examen Obiectiv",
            "investigatii" => "Investigații",
            "diagnostic" => "Diagnostic",
            "tratament" => "Tratament",
            "concluzie" => "Concluzie",
            _ => tab
        };
    }

    private string GetTabIcon(string tab)
    {
        return tab switch
        {
            "motive" => "fas fa-file-medical",
            "antecedente" => "fas fa-history",
            "examen" => "fas fa-stethoscope",
            "investigatii" => "fas fa-vials",
            "diagnostic" => "fas fa-diagnoses",
            "tratament" => "fas fa-pills",
            "concluzie" => "fas fa-clipboard-check",
            _ => "fas fa-circle"
        };
    }

    private int GetTabBadge(string tab)
    {
        if (TabBadges == null) return 0;
        return TabBadges.TryGetValue(tab, out var count) ? count : 0;
    }
}
