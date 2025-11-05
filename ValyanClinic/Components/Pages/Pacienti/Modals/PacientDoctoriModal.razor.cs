using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.DTOs;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Queries.GetDoctoriByPacient;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.RemoveRelatie;

namespace ValyanClinic.Components.Pages.Pacienti.Modals;

public partial class PacientDoctoriModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<PacientDoctoriModal> Logger { get; set; } = default!;

    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public Guid? PacientID { get; set; }
    [Parameter] public string PacientNume { get; set; } = string.Empty;

    private bool IsLoading { get; set; }
    private bool HasError { get; set; }
    private string? ErrorMessage { get; set; }
    private bool ShowAddDoctorModal { get; set; }

    private List<DoctorAsociatDto> AllDoctori { get; set; } = new();
    private List<DoctorAsociatDto> DoctoriActivi => AllDoctori.Where(d => d.EsteActiv).ToList();
    private List<DoctorAsociatDto> DoctoriInactivi => AllDoctori.Where(d => !d.EsteActiv).ToList();

    protected override async Task OnParametersSetAsync()
    {
        Logger.LogInformation("[PacientDoctoriModal] OnParametersSetAsync - IsVisible: {IsVisible}, PacientID: {PacientID}, PacientNume: {PacientNume}",
            IsVisible, PacientID, PacientNume);

   if (IsVisible && PacientID.HasValue)
        {
     Logger.LogInformation("[PacientDoctoriModal] Loading doctori for PacientID: {PacientID}", PacientID);
  await LoadDoctori();
        }
        else
        {
       Logger.LogWarning("[PacientDoctoriModal] NOT loading - IsVisible: {IsVisible}, HasValue: {HasValue}",
     IsVisible, PacientID.HasValue);
        }
  }

    private async Task LoadDoctori()
    {
     IsLoading = true;
     HasError = false;
     ErrorMessage = null;

    try
        {
      Logger.LogInformation("[PacientDoctoriModal] Calling GetDoctoriByPacientQuery for PacientID: {PacientID}", PacientID);
            
 var query = new GetDoctoriByPacientQuery(PacientID!.Value, ApenumereActivi: false);
            var result = await Mediator.Send(query);

            Logger.LogInformation("[PacientDoctoriModal] Query result: IsSuccess={IsSuccess}, Count={Count}",
     result.IsSuccess, result.Value?.Count ?? 0);

       if (result.IsSuccess)
{
           AllDoctori = result.Value ?? new List<DoctorAsociatDto>();
        Logger.LogInformation("[PacientDoctoriModal] Loaded {Count} doctori ({Active} activi, {Inactive} inactivi)",
 AllDoctori.Count, DoctoriActivi.Count, DoctoriInactivi.Count);
    }
      else
          {
             HasError = true;
          ErrorMessage = result.FirstError;
          Logger.LogError("[PacientDoctoriModal] Error loading doctori: {Error}", ErrorMessage);
  }
        }
     catch (Exception ex)
        {
 HasError = true;
        ErrorMessage = $"Eroare: {ex.Message}";
Logger.LogError(ex, "[PacientDoctoriModal] Exception loading doctori");
}
   finally
   {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Deschide modalul pentru adăugare doctor NOU.
    /// 
    /// LOGICA BUTOANELOR:
    /// - Buton 1 "+ Adaugă doctor" (sus dreapta în header) - ÎNTOTDEAUNA VIZIBIL
    /// - Buton 2 "+ Adaugă primul doctor" (centru în empty state) - DOAR când Count = 0
    /// 
    /// AMBELE BUTOANE APELEAZĂ ACEASTĂ METODĂ!
    /// Rezultat: Se deschide același modal AddDoctorToPacientModal
    /// </summary>
    private void OpenAddDoctorModal()
    {
        Logger.LogInformation("[PacientDoctoriModal] Opening AddDoctorModal");
        ShowAddDoctorModal = true;
 }

    /// <summary>
    /// Event handler când un doctor nou a fost adăugat cu succes.
    /// 
  /// FLOW:
    /// 1. AddDoctorToPacientModal salvează doctorul în DB
    /// 2. Apelează OnDoctorAdded (acest callback)
    /// 3. Închidem AddDoctorToPacientModal
    /// 4. Reîncărcăm lista de doctori (LoadDoctori)
    /// 5. StateHasChanged pentru re-render
    /// 
    /// REZULTAT: Lista se actualizează automat cu noul doctor
 /// </summary>
    private async Task OnDoctorAdded()
    {
        Logger.LogInformation("[PacientDoctoriModal] Doctor added - reloading list");
   ShowAddDoctorModal = false;
        await LoadDoctori();
        StateHasChanged();
    }

    private async Task RemoveDoctor(DoctorAsociatDto doctor)
    {
        var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
          $"Sunteți sigur că doriți să dezactivați relația cu {doctor.DoctorNumeComplet}?");

        if (!confirmed) return;

        try
        {
      Logger.LogInformation("[PacientDoctoriModal] Removing doctor: {DoctorName}, RelatieID: {RelatieID}",
       doctor.DoctorNumeComplet, doctor.RelatieID);

  var command = new RemoveRelatieCommand(RelatieID: doctor.RelatieID);
    var result = await Mediator.Send(command);

            if (result.IsSuccess)
         {
    await LoadDoctori();
                await JSRuntime.InvokeVoidAsync("alert", "Relație dezactivată cu succes!");
            }
     else
            {
      await JSRuntime.InvokeVoidAsync("alert", $"Eroare: {result.FirstError}");
      }
        }
        catch (Exception ex)
        {
         Logger.LogError(ex, "[PacientDoctoriModal] Error removing doctor");
            await JSRuntime.InvokeVoidAsync("alert", $"Eroare: {ex.Message}");
     }
    }

    private string GetBadgeClass(string? tipRelatie)
    {
    return tipRelatie switch
        {
            "MedicPrimar" => "primary",
     "Specialist" => "info",
    "MedicConsultant" => "success",
       "MedicDeGarda" => "warning",
_ => "secondary"
     };
    }

    private async Task Close()
    {
      Logger.LogInformation("[PacientDoctoriModal] Closing modal");
 IsVisible = false;
    await IsVisibleChanged.InvokeAsync(false);
        ShowAddDoctorModal = false;
    }
}
