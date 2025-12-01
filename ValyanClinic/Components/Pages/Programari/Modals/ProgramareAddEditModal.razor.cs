using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientList;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList;
using ValyanClinic.Application.Features.ProgramareManagement.Commands.CreateProgramare;
using ValyanClinic.Application.Features.ProgramareManagement.Commands.UpdateProgramare;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;
using ValyanClinic.Application.Features.ProgramareManagement.Queries.CheckProgramareConflict;
using ValyanClinic.Application.Features.ProgramareManagement.Queries.GetProgramareById;
using ValyanClinic.Services;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Calendars;

namespace ValyanClinic.Components.Pages.Programari.Modals;

public partial class ProgramareAddEditModal : ComponentBase
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public Guid? ProgramareId { get; set; }
    [Parameter] public EventCallback OnSaved { get; set; }

    // ✅ ADDED: Parametri pentru pre-fill data/ora din cell click
    [Parameter] public DateTime? PrefilledStartTime { get; set; }
    [Parameter] public DateTime? PrefilledEndTime { get; set; }

    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private INotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<ProgramareAddEditModal> Logger { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    private bool IsEditMode => ProgramareId.HasValue;
    private bool IsLoading = false;
    private bool IsSaving = false;
    private bool HasConflict = false;
    private string ConflictDoctorName = string.Empty;
    private string ErrorMessage = string.Empty;

    // ✅ UPDATED: Validare formular permite PacientID gol pentru SlotBlocat
    private bool IsFormValid => Model != null &&
           (Model.TipProgramare == "SlotBlocat" || Model.PacientID != Guid.Empty) &&
         Model.DoctorID != Guid.Empty;

    private CreateProgramareDto? Model { get; set; }
    private List<PacientDropdownDto> PacientiList = new();
    private List<DoctorDropdownDto> DoctorsList = new();

    private int DurataMinute => Model != null ? (int)(Model.OraSfarsit - Model.OraInceput).TotalMinutes : 0;

    // ✅ Dropdown options pentru Syncfusion
    private List<TipProgramareOption> TipProgramareOptions = new()
    {
        new() { Value = "ConsultatieInitiala", Text = "Consultație Inițială (45 min)" },
     new() { Value = "ControlPeriodic", Text = "Control Periodic (30 min)" },
        new() { Value = "Consultatie", Text = "Consultație (30 min)" },
  new() { Value = "Investigatie", Text = "Investigație (20 min)" },
  new() { Value = "Procedura", Text = "Procedură (60 min)" },
     new() { Value = "Urgenta", Text = "Urgență (15 min)" },
        new() { Value = "Telemedicina", Text = "Telemedicină (20 min)" },
  new() { Value = "LaDomiciliu", Text = "La Domiciliu (60 min)" },
  new() { Value = "SlotBlocat", Text = "🚫 Blocat (pauză/întâlnire)" }
    };

    private List<StatusOption> StatusOptions = new()
    {
        new() { Value = "Programata", Text = "Programată" },
    new() { Value = "Confirmata", Text = "Confirmată" },
        new() { Value = "CheckedIn", Text = "Check-in efectuat" },
        new() { Value = "InConsultatie", Text = "În consultație" },
        new() { Value = "Finalizata", Text = "Finalizată" },
        new() { Value = "Anulata", Text = "Anulată" },
        new() { Value = "NoShow", Text = "Nu s-a prezentat" }
    };

    // Debounce timer for conflict checking
    private System.Threading.Timer? _conflictCheckTimer;
    private const int CONFLICT_CHECK_DELAY_MS = 800;

    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible)
        {
            Logger.LogInformation("ProgramareAddEditModal opened. Mode: {Mode}, ID: {ID}",
           IsEditMode ? "Edit" : "Add", ProgramareId);

            await InitializeModalAsync();
            StateHasChanged();
        }
    }

    private async Task InitializeModalAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            HasConflict = false;
            StateHasChanged();

            // Load dropdown data
            await Task.WhenAll(
             LoadPacientiListAsync(),
              LoadDoctorsListAsync()
                    );

            Logger.LogInformation("Loaded {PacientiCount} pacienti and {DoctoriCount} doctori",
        PacientiList.Count, DoctorsList.Count);

            // Initialize model
            if (IsEditMode && ProgramareId.HasValue)
            {
                await LoadProgramareDataAsync(ProgramareId.Value);
            }
            else
            {
                await InitializeNewProgramareAsync();
            }

            Logger.LogInformation("Model initialized successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la inițializarea modalului");
            ErrorMessage = "Eroare la încărcarea datelor. Vă rugăm să încercați din nou.";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadPacientiListAsync()
    {
        try
        {
            var query = new GetPacientListQuery
            {
                PageNumber = 1,
                PageSize = 1000,
                Activ = true
            };

            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null && result.Value.Value != null)
            {
                PacientiList = result.Value.Value
                         .Select(p => new PacientDropdownDto
                         {
                             Id = p.Id,
                             NumeComplet = $"{p.Nume} {p.Prenume} (CNP: {p.CNP})"
                         })
                   .ToList();

                Logger.LogInformation("✅ Loaded {Count} pacienti successfully", PacientiList.Count);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Eroare la încărcarea pacienților");
        }
    }

    private async Task LoadDoctorsListAsync()
    {
        try
        {
            var query = new GetPersonalMedicalListQuery
            {
                PageNumber = 1,
                PageSize = 1000
            };

            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                DoctorsList = result.Value
           .Select(d => new DoctorDropdownDto
           {
               PersonalID = d.PersonalID,
               NumeComplet = $"Dr. {d.Nume} {d.Prenume} - {d.Specializare}"
           })
       .ToList();

                Logger.LogInformation("✅ Loaded {Count} doctori successfully", DoctorsList.Count);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Eroare la încărcarea medicilor");
        }
    }

    private async Task LoadProgramareDataAsync(Guid programareId)
    {
        try
        {
            var query = new GetProgramareByIdQuery(programareId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                var programare = result.Value;

                Model = new CreateProgramareDto
                {
                    PacientID = programare.PacientID,
                    DoctorID = programare.DoctorID,
                    DataProgramare = programare.DataProgramare,
                    OraInceput = programare.OraInceput,
                    OraSfarsit = programare.OraSfarsit,
                    TipProgramare = programare.TipProgramare,
                    Status = programare.Status,
                    Observatii = programare.Observatii
                };
            }
            else
            {
                ErrorMessage = "Nu s-au putut încărca datele programării.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Eroare la încărcarea programării {ProgramareID}", programareId);
            ErrorMessage = "Eroare la încărcarea programării.";
        }
    }

    private async Task InitializeNewProgramareAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var userIdClaim = authState.User.FindFirst("PersonalMedicalID");

        Guid userId = Guid.Empty;
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        // ✅ Use prefilled values if available (from cell click), otherwise use defaults
        DateTime dataProgramare;
        TimeSpan oraInceput;
        TimeSpan oraSfarsit;

        if (PrefilledStartTime.HasValue && PrefilledEndTime.HasValue)
        {
            // Use values from cell click
            dataProgramare = PrefilledStartTime.Value.Date;
            oraInceput = PrefilledStartTime.Value.TimeOfDay;
            oraSfarsit = PrefilledEndTime.Value.TimeOfDay;

            Logger.LogInformation("✅ Using prefilled values from cell click: {Date} {Start}-{End}",
          dataProgramare.ToString("yyyy-MM-dd"), oraInceput, oraSfarsit);
        }
        else
        {
            // Use defaults (today, 9:00-9:30)
            dataProgramare = DateTime.Today;
            oraInceput = new TimeSpan(9, 0, 0);
            oraSfarsit = new TimeSpan(9, 30, 0);

            Logger.LogInformation("Using default values: Today 9:00-9:30");
        }

        Model = new CreateProgramareDto
        {
            DataProgramare = dataProgramare,
            OraInceput = oraInceput,
            OraSfarsit = oraSfarsit,
            Status = "Programata",
            CreatDe = userId
        };

        // Auto-fill start/end time if available
        if (PrefilledStartTime.HasValue)
        {
            Model.OraInceput = PrefilledStartTime.Value.TimeOfDay;
        }

        if (PrefilledEndTime.HasValue)
        {
            Model.OraSfarsit = PrefilledEndTime.Value.TimeOfDay;
        }
    }

    // ✅ Syncfusion Event Handlers
    private async Task OnPacientChanged(ChangeEventArgs<Guid, PacientDropdownDto> args)
    {
        if (Model != null && args.Value != Guid.Empty)
        {
            Model.PacientID = args.Value;
            StateHasChanged();
        }
    }

    private async Task OnDoctorChanged(ChangeEventArgs<Guid, DoctorDropdownDto> args)
    {
        if (Model != null && args.Value != Guid.Empty)
        {
            Model.DoctorID = args.Value;
            await CheckConflictDebounced();
            StateHasChanged();
        }
    }

    private async Task OnDataProgramareChanged(ChangedEventArgs<DateTime> args)
    {
        if (Model != null)
        {
            Model.DataProgramare = args.Value;
            await CheckConflictDebounced();
            StateHasChanged();
        }
    }

    // ✅ HELPER: Convert TimeSpan to DateTime? for Syncfusion TimePicker
    private DateTime? GetTimeAsDateTime(TimeSpan timeSpan)
    {
        if (timeSpan == default) return null;
        return DateTime.Today.Add(timeSpan);
    }

    // ✅ HELPER: Convert DateTime? to TimeSpan for Model
    private TimeSpan GetTimeSpanFromDateTime(DateTime? dateTime)
    {
        if (dateTime == null) return TimeSpan.Zero;
        return dateTime.Value.TimeOfDay;
    }

    // ✅ NEW: Event handler for OraInceput (DateTime? version)
    private async Task OnOraInceputChangedV2(ChangeEventArgs<DateTime?> args)
    {
        if (Model != null && args.Value.HasValue)
        {
            Model.OraInceput = GetTimeSpanFromDateTime(args.Value);

            // Auto-calculate OraSfarsit if TipProgramare is set
            if (!string.IsNullOrEmpty(Model.TipProgramare))
            {
                var durata = GetDurataForTip(Model.TipProgramare);
                Model.OraSfarsit = Model.OraInceput.Add(TimeSpan.FromMinutes(durata));
            }

            await CheckConflictDebounced();
            StateHasChanged();
        }
    }

    // ✅ NEW: Event handler for OraSfarsit (DateTime? version)
    private async Task OnOraSfarsitChangedV2(ChangeEventArgs<DateTime?> args)
    {
        if (Model != null && args.Value.HasValue)
        {
            Model.OraSfarsit = GetTimeSpanFromDateTime(args.Value);
            await CheckConflictDebounced();
            StateHasChanged();
        }
    }

    // ✅ OLD: Deprecated - kept for backward compatibility
    private async Task OnOraInceputChanged(ChangeEventArgs<TimeSpan> args)
    {
        if (Model != null)
        {
            Model.OraInceput = args.Value;

            // Auto-calculate OraSfarsit if TipProgramare is set
            if (!string.IsNullOrEmpty(Model.TipProgramare))
            {
                var durata = GetDurataForTip(Model.TipProgramare);
                Model.OraSfarsit = Model.OraInceput.Add(TimeSpan.FromMinutes(durata));
            }

            await CheckConflictDebounced();
            StateHasChanged();
        }
    }

    // ✅ OLD: Deprecated - kept for backward compatibility
    private async Task OnOraSfarsitChanged(ChangeEventArgs<TimeSpan> args)
    {
        if (Model != null)
        {
            Model.OraSfarsit = args.Value;
            await CheckConflictDebounced();
            StateHasChanged();
        }
    }

    private async Task OnTipProgramareChanged(ChangeEventArgs<string, TipProgramareOption> args)
    {
        if (Model != null && !string.IsNullOrEmpty(args.Value))
        {
            Model.TipProgramare = args.Value;
            var durata = GetDurataForTip(args.Value);
            Model.OraSfarsit = Model.OraInceput.Add(TimeSpan.FromMinutes(durata));
            await CheckConflictDebounced();
            StateHasChanged();
        }
    }

    private int GetDurataForTip(string tipProgramare)
    {
        return tipProgramare switch
        {
            "ConsultatieInitiala" => 45,
            "ControlPeriodic" => 30,
            "Consultatie" => 30,
            "Investigatie" => 20,
            "Procedura" => 60,
            "Urgenta" => 15,
            "Telemedicina" => 20,
            "LaDomiciliu" => 60,
            "SlotBlocat" => 60,    // ✅ ADDED: 60 min pentru slot blocat
            _ => 30
        };
    }

    private async Task CheckConflictDebounced()
    {
        _conflictCheckTimer?.Dispose();
        _conflictCheckTimer = new System.Threading.Timer(async _ =>
          {
              await InvokeAsync(async () =>
             {
              await CheckConflictAsync();
              StateHasChanged();
          });
          }, null, CONFLICT_CHECK_DELAY_MS, Timeout.Infinite);
    }

    private async Task CheckConflictAsync()
    {
        if (Model == null || Model.DoctorID == Guid.Empty || Model.DataProgramare == default)
        {
            HasConflict = false;
            return;
        }

        try
        {
            var query = new CheckProgramareConflictQuery(
      Model.DoctorID,
   Model.DataProgramare,
          Model.OraInceput,
     Model.OraSfarsit,
        ProgramareId
     );

            var result = await Mediator.Send(query);

            if (result.IsSuccess)
            {
                HasConflict = result.Value;

                if (HasConflict)
                {
                    var doctor = DoctorsList.FirstOrDefault(d => d.PersonalID == Model.DoctorID);
                    ConflictDoctorName = doctor?.NumeComplet ?? "Medicul selectat";
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la verificarea conflictului");
            HasConflict = false;
        }
    }

    private async Task HandleSubmit()
    {
        if (Model == null || !IsFormValid || IsSaving)
            return;

        try
        {
            IsSaving = true;
            ErrorMessage = string.Empty;

            if (IsEditMode && ProgramareId.HasValue)
            {
                await UpdateProgramareAsync();
            }
            else
            {
                await CreateProgramareAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Eroare la salvarea programării");
            ErrorMessage = "Eroare la salvarea programării. Vă rugăm să încercați din nou.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task CreateProgramareAsync()
    {
        var command = new CreateProgramareCommand
        {
            PacientID = Model!.PacientID,
            DoctorID = Model.DoctorID,
            DataProgramare = Model.DataProgramare,
            OraInceput = Model.OraInceput,
            OraSfarsit = Model.OraSfarsit,
            TipProgramare = Model.TipProgramare,
            Status = Model.Status,
            Observatii = Model.Observatii,
            CreatDe = Model.CreatDe
        };

        var result = await Mediator.Send(command);

        if (result.IsSuccess)
        {
            await NotificationService.ShowSuccessAsync("Programarea a fost creată cu succes!");
            await CloseModal();
            await OnSaved.InvokeAsync();
        }
        else
        {
            ErrorMessage = result.Errors?.FirstOrDefault() ?? "Eroare la crearea programării.";
            await NotificationService.ShowErrorAsync(ErrorMessage);
        }
    }

    private async Task UpdateProgramareAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var userIdClaim = authState.User.FindFirst("PersonalMedicalID");

        Guid userId = Guid.Empty;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out userId))
        {
            ErrorMessage = "Nu s-a putut identifica utilizatorul curent.";
            return;
        }

        var command = new UpdateProgramareCommand
        {
            ProgramareID = ProgramareId!.Value,
            PacientID = Model!.PacientID,
            DoctorID = Model.DoctorID,
            DataProgramare = Model.DataProgramare,
            OraInceput = Model.OraInceput,
            OraSfarsit = Model.OraSfarsit,
            TipProgramare = Model.TipProgramare,
            Status = Model.Status,
            Observatii = Model.Observatii,
            ModificatDe = userId
        };

        var result = await Mediator.Send(command);

        if (result.IsSuccess)
        {
            await NotificationService.ShowSuccessAsync("Programarea a fost actualizată cu succes!");
            await CloseModal();
            await OnSaved.InvokeAsync();
        }
        else
        {
            ErrorMessage = result.Errors?.FirstOrDefault() ?? "Eroare la actualizarea programării.";
            await NotificationService.ShowErrorAsync(ErrorMessage);
        }
    }

    private async Task CloseModal()
    {
        Model = null;
        ErrorMessage = string.Empty;
        HasConflict = false;
        await IsVisibleChanged.InvokeAsync(false);
    }

    private async Task HandleOverlayClick()
    {
        // ❌ DEZACTIVAT: Nu închide modalul la click pe overlay
        // Acest modal conține date importante care nu trebuie pierdute
        // await CloseModal();

        // 📝 Pentru a proteja datele introduse în formulare
        return;
    }

    private string GetValidationClass(string fieldName)
    {
        return string.Empty;
    }

    public void Dispose()
    {
        _conflictCheckTimer?.Dispose();
    }

    // ✅ Helper classes pentru Syncfusion DropDownList
    public class PacientDropdownDto
    {
        public Guid Id { get; set; }
        public string NumeComplet { get; set; } = string.Empty;
    }

    public class DoctorDropdownDto
    {
        public Guid PersonalID { get; set; }
        public string NumeComplet { get; set; } = string.Empty;
    }

    public class TipProgramareOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class StatusOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
