using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;

namespace ValyanClinic.Components.Shared.Consultatie.Tabs;

/// <summary>
/// Tab Antecedente pentru consultație
/// Include: AHC, AF, APP, Condiții Socio-Economice
/// </summary>
public partial class AntecedenteTab : ComponentBase
{
    [Parameter] public CreateConsultatieCommand Model { get; set; } = new();
    [Parameter] public string? PacientSex { get; set; }
    [Parameter] public bool IsActive { get; set; }
    [Parameter] public EventCallback OnChanged { get; set; }
    [Parameter] public EventCallback OnSectionCompleted { get; set; }
    [Parameter] public bool ShowValidation { get; set; } = false;
    
    private bool IsSectionCompleted => IsAntecedenteCompleted();
    
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
    /// Verifică dacă secțiunea Antecedente este completă
    /// Considerăm completă dacă cel puțin un câmp din fiecare subsecțiune este completat
    /// </summary>
    private bool IsAntecedenteCompleted()
    {
        // AHC - cel puțin un câmp completat
        var hasAHC = !string.IsNullOrWhiteSpace(Model.AHC_Mama) ||
                     !string.IsNullOrWhiteSpace(Model.AHC_Tata) ||
                     !string.IsNullOrWhiteSpace(Model.AHC_Frati) ||
                     !string.IsNullOrWhiteSpace(Model.AHC_Bunici) ||
                     !string.IsNullOrWhiteSpace(Model.AHC_Altele);
        
        // AF - cel puțin un câmp completat
        var hasAF = !string.IsNullOrWhiteSpace(Model.AF_Nastere) ||
                    !string.IsNullOrWhiteSpace(Model.AF_Dezvoltare) ||
                    !string.IsNullOrWhiteSpace(Model.AF_Menstruatie) ||
                    !string.IsNullOrWhiteSpace(Model.AF_Sarcini) ||
                    !string.IsNullOrWhiteSpace(Model.AF_Alaptare);
        
        // APP - cel puțin două câmpuri completate (considerăm minim 2 pentru completitudine)
        var appCount = 0;
        if (!string.IsNullOrWhiteSpace(Model.APP_BoliCopilarieAdolescenta)) appCount++;
        if (!string.IsNullOrWhiteSpace(Model.APP_BoliAdult)) appCount++;
        if (!string.IsNullOrWhiteSpace(Model.APP_Interventii)) appCount++;
        if (!string.IsNullOrWhiteSpace(Model.APP_Traumatisme)) appCount++;
        if (!string.IsNullOrWhiteSpace(Model.APP_Transfuzii)) appCount++;
        if (!string.IsNullOrWhiteSpace(Model.APP_Alergii)) appCount++;
        if (!string.IsNullOrWhiteSpace(Model.APP_Medicatie)) appCount++;
        var hasAPP = appCount >= 2;
        
        // Condiții socio-economice - cel puțin două câmpuri
        var socioCount = 0;
        if (!string.IsNullOrWhiteSpace(Model.Profesie)) socioCount++;
        if (!string.IsNullOrWhiteSpace(Model.ConditiiMunca)) socioCount++;
        if (!string.IsNullOrWhiteSpace(Model.ConditiiLocuinta)) socioCount++;
        if (!string.IsNullOrWhiteSpace(Model.ObiceiuriAlimentare)) socioCount++;
        if (!string.IsNullOrWhiteSpace(Model.Toxice)) socioCount++;
        var hasSocio = socioCount >= 2;
        
        // Secțiunea e completă dacă toate subsecțiunile au cel puțin ceva completat
        return hasAHC && hasAF && hasAPP && hasSocio;
    }
}
