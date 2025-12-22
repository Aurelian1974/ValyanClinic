using MediatR;
using Microsoft.AspNetCore.Components;
using ValyanClinic.Application.Features.AnalizeMedicale.Queries;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Application.ViewModels;

namespace ValyanClinic.Components.Shared.Consultatie.Tabs;

/// <summary>
/// Tab Investigații pentru consultație
/// Include: Laborator, Imagistice, EKG, Alte investigații + Analize Medicale OCR
/// </summary>
public partial class InvestigatiiTab : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    
    [Parameter] public CreateConsultatieCommand Model { get; set; } = new();
    [Parameter] public bool IsActive { get; set; }
    [Parameter] public EventCallback OnChanged { get; set; }
    [Parameter] public EventCallback OnSectionCompleted { get; set; }
    [Parameter] public bool ShowValidation { get; set; } = false;
    [Parameter] public Guid? PacientId { get; set; }

    private bool IsSectionCompleted => IsInvestigatiiCompleted();
    
    // Analize medicale state
    private List<AnalizeMedicaleGroupDto>? AnalizeMedicaleGroups { get; set; }
    private bool IsLoadingAnalize { get; set; } = false;

    protected override async Task OnParametersSetAsync()
    {
        // Încarcă analizele când se setează PacientId
        if (PacientId.HasValue && AnalizeMedicaleGroups == null && !IsLoadingAnalize)
        {
            await LoadAnalizeMedicaleAsync();
        }
    }

    private async Task LoadAnalizeMedicaleAsync()
    {
        if (!PacientId.HasValue) return;
        
        IsLoadingAnalize = true;
        StateHasChanged();
        
        try
        {
            var result = await Mediator.Send(new GetAnalizeMedicaleByPacientQuery(PacientId.Value));
            if (result.IsSuccess)
            {
                AnalizeMedicaleGroups = result.Value;
            }
        }
        catch
        {
            // Silently fail - analizele sunt opționale
            AnalizeMedicaleGroups = new List<AnalizeMedicaleGroupDto>();
        }
        finally
        {
            IsLoadingAnalize = false;
            StateHasChanged();
        }
    }

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
    /// Verifică dacă secțiunea Investigații este completă
    /// Considerăm completă dacă cel puțin două tipuri de investigații sunt completate
    /// </summary>
    private bool IsInvestigatiiCompleted()
    {
        var completedCount = 0;

        if (!string.IsNullOrWhiteSpace(Model.InvestigatiiLaborator)) completedCount++;
        if (!string.IsNullOrWhiteSpace(Model.InvestigatiiImagistice)) completedCount++;
        if (!string.IsNullOrWhiteSpace(Model.InvestigatiiEKG)) completedCount++;
        if (!string.IsNullOrWhiteSpace(Model.AlteInvestigatii)) completedCount++;

        // Cel puțin 2 tipuri de investigații completate
        return completedCount >= 2;
    }
}
