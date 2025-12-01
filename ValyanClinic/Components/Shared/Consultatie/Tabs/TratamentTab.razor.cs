using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;

namespace ValyanClinic.Components.Shared.Consultatie.Tabs;

/// <summary>
/// Tab Tratament pentru consultație
/// Include: Tratament medicamentos, nemedicamentos, recomandări
/// </summary>
public partial class TratamentTab : ComponentBase
{
    [Parameter] public CreateConsultatieCommand Model { get; set; } = new();
    [Parameter] public bool IsActive { get; set; }
    [Parameter] public EventCallback OnChanged { get; set; }
    [Parameter] public EventCallback OnSectionCompleted { get; set; }
    [Parameter] public bool ShowValidation { get; set; } = false;

    private bool IsSectionCompleted => IsTratamentCompleted();

    private async Task OnFieldChanged()
    {
        await OnChanged.InvokeAsync();

        // Check if section is completed
        if (IsSectionCompleted)
        {
            await OnSectionCompleted.InvokeAsync();
        }
    }

    /// <summary>
    /// Verifică dacă secțiunea Tratament este completă
    /// Considerăm completă dacă tratamentul medicamentos este completat (obligatoriu)
    /// </summary>
    private bool IsTratamentCompleted()
    {
        // Tratamentul medicamentos este obligatoriu
        if (string.IsNullOrWhiteSpace(Model.TratamentMedicamentos))
            return false;

        // Opțional: cel puțin o recomandare completată
        var hasRecommendations = !string.IsNullOrWhiteSpace(Model.TratamentNemedicamentos) ||
                                !string.IsNullOrWhiteSpace(Model.RecomandariDietetice) ||
                                !string.IsNullOrWhiteSpace(Model.RecomandariRegimViata) ||
                                !string.IsNullOrWhiteSpace(Model.InvestigatiiRecomandate) ||
                                !string.IsNullOrWhiteSpace(Model.ConsulturiSpecialitate) ||
                                !string.IsNullOrWhiteSpace(Model.DataUrmatoareiProgramari) ||
                                !string.IsNullOrWhiteSpace(Model.RecomandariSupraveghere);

        return hasRecommendations;
    }
}
