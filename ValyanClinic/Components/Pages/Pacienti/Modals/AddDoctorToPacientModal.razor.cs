using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.AddRelatie;
using ValyanClinic.Services; // ✅ ADDED

namespace ValyanClinic.Components.Pages.Pacienti.Modals;

public partial class AddDoctorToPacientModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<AddDoctorToPacientModal> Logger { get; set; } = default!;
    [Inject] private INotificationService NotificationService { get; set; } = default!; // ✅ ADDED

    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public Guid? PacientID { get; set; }
    [Parameter] public string PacientNume { get; set; } = string.Empty;
    [Parameter] public EventCallback OnDoctorAdded { get; set; }

    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }
    private string? ValidationError { get; set; }

    private List<PersonalMedicalListDto> DoctoriDisponibili { get; set; } = new();
    private Guid? SelectedDoctorId { get; set; }
    private string? TipRelatie { get; set; }
    private string? Motiv { get; set; }
    private string? Observatii { get; set; }

    private bool IsFormValid => SelectedDoctorId.HasValue && !string.IsNullOrEmpty(TipRelatie);

    private List<TipRelatieOption> TipuriRelatie { get; set; } = new()
    {
      new TipRelatieOption { Value = "MedicPrimar", Text = "Medic Primar" },
        new TipRelatieOption { Value = "Specialist", Text = "Specialist" },
    new TipRelatieOption { Value = "MedicConsultant", Text = "Medic Consultant" },
        new TipRelatieOption { Value = "MedicDeGarda", Text = "Medic de Gardă" },
        new TipRelatieOption { Value = "MedicFamilie", Text = "Medic de Familie" },
        new TipRelatieOption { Value = "AsistentMedical", Text = "Asistent Medical" }
 };

    protected override async Task OnParametersSetAsync()
    {
        Logger.LogInformation("[AddDoctorToPacientModal] OnParametersSetAsync - IsVisible: {IsVisible}, PacientID: {PacientID}, PacientNume: {PacientNume}",
         IsVisible, PacientID, PacientNume);

        if (IsVisible && DoctoriDisponibili.Count == 0)
        {
            Logger.LogInformation("[AddDoctorToPacientModal] Loading doctori disponibili...");
            await LoadDoctori();
        }
    }

    private async Task LoadDoctori()
    {
        IsLoading = true;

        try
        {
            Logger.LogInformation("[AddDoctorToPacientModal] Calling GetPersonalMedicalListQuery...");

            var query = new GetPersonalMedicalListQuery
            {
                PageNumber = 1,
                PageSize = 1000,
                GlobalSearchText = null,
                FilterDepartament = null,
                FilterPozitie = null,
                FilterEsteActiv = true,
                SortColumn = "Nume",
                SortDirection = "ASC"
            };

            var result = await Mediator.Send(query);

            if (result != null && result.Value != null)
            {
                DoctoriDisponibili = result.Value.ToList();
                Logger.LogInformation("[AddDoctorToPacientModal] Loaded {Count} doctori disponibili", DoctoriDisponibili.Count);
            }
            else
            {
                Logger.LogWarning("[AddDoctorToPacientModal] No doctori returned from query");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[AddDoctorToPacientModal] Error loading doctori");
            // ✅ CHANGED: alert() → ShowErrorAsync()
            await NotificationService.ShowErrorAsync(
            $"Eroare la încărcarea doctorilor: {ex.Message}",
                  "Eroare");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SaveDoctor()
    {
        ValidationError = null;

        if (!IsFormValid)
        {
            ValidationError = "Vă rugăm completați toate câmpurile obligatorii.";
            Logger.LogWarning("[AddDoctorToPacientModal] Form validation failed");
            return;
        }

        IsSaving = true;

        try
        {
            Logger.LogInformation("[AddDoctorToPacientModal] Saving doctor: PacientID={PacientID}, DoctorID={DoctorID}, TipRelatie={TipRelatie}",
     PacientID, SelectedDoctorId, TipRelatie);

            var command = new AddRelatieCommand(
              PacientID: PacientID!.Value,
            PersonalMedicalID: SelectedDoctorId!.Value,
         TipRelatie: TipRelatie,
               Observatii: Observatii,
            Motiv: Motiv,
           CreatDe: null // Will use SYSTEM_USER
                 );

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                Logger.LogInformation("[AddDoctorToPacientModal] Doctor saved successfully");
                // ✅ CHANGED: alert() → ShowSuccessAsync()
                await NotificationService.ShowSuccessAsync("Doctor adăugat cu succes!");
                await OnDoctorAdded.InvokeAsync();
                ResetForm();
                await Close();
            }
            else
            {
                ValidationError = result.FirstError;
                Logger.LogError("[AddDoctorToPacientModal] Error saving doctor: {Error}", ValidationError);
            }
        }
        catch (Exception ex)
        {
            ValidationError = $"Eroare: {ex.Message}";
            Logger.LogError(ex, "[AddDoctorToPacientModal] Exception saving doctor");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void ResetForm()
    {
        Logger.LogInformation("[AddDoctorToPacientModal] Resetting form");
        SelectedDoctorId = null;
        TipRelatie = null;
        Motiv = null;
        Observatii = null;
        ValidationError = null;
    }

    private async Task Close()
    {
        Logger.LogInformation("[AddDoctorToPacientModal] Closing modal");
        ResetForm();
        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
    }

    public class TipRelatieOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
