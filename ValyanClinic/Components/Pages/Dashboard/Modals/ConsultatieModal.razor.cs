using Microsoft.AspNetCore.Components;
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;

namespace ValyanClinic.Components.Pages.Dashboard.Modals;

public partial class ConsultatieModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ConsultatieModal> Logger { get; set; } = default!;

    // Parameters
    [Parameter] public Guid ProgramareID { get; set; }
    [Parameter] public Guid PacientID { get; set; }
    [Parameter] public Guid MedicID { get; set; }
    [Parameter] public EventCallback OnConsultatieCompleted { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    // State
    private bool IsVisible { get; set; }
    private bool IsSaving { get; set; }
    private bool IsLoadingPacient { get; set; }
    
    // Data
    private CreateConsultatieCommand Model { get; set; } = new();
    private PacientDetailDto? PacientInfo { get; set; }

    // UI State
    private string ActiveTab { get; set; } = "motive";
    private string CurrentSection { get; set; } = "motive";
    private List<string> Sections { get; set; } = new()
    {
        "motive",
        "antecedente",
        "examen",
        "investigatii",
        "diagnostic",
        "tratament",
        "concluzie"
    };
    private HashSet<string> CompletedSections { get; set; } = new();

    // Computed Properties
    private string CalculatedIMC
    {
        get
        {
            if (Model.Greutate.HasValue && Model.Inaltime.HasValue && Model.Inaltime > 0)
            {
                var inaltimeMetri = Model.Inaltime.Value / 100;
                var imc = Model.Greutate.Value / (inaltimeMetri * inaltimeMetri);
                return Math.Round(imc, 2).ToString("F2");
            }
            return "-";
        }
    }

    private string IMCInterpretation
    {
        get
        {
            if (Model.Greutate.HasValue && Model.Inaltime.HasValue && Model.Inaltime > 0)
            {
                var inaltimeMetri = Model.Inaltime.Value / 100;
                var imc = Model.Greutate.Value / (inaltimeMetri * inaltimeMetri);

                return imc switch
                {
                    < 18.5m => "Subponderal",
                    >= 18.5m and < 25m => "Normal",
                    >= 25m and < 30m => "Supraponderal",
                    >= 30m and < 35m => "Obezitate I",
                    >= 35m and < 40m => "Obezitate II",
                    >= 40m => "Obezitate morbida"
                };
            }
            return "";
        }
    }

    // Public Methods
    public async Task Open()
    {
        Logger.LogInformation("[ConsultatieModal] Opening modal for Programare: {ProgramareID}", ProgramareID);
        
        IsVisible = true;
        await LoadPacientData();
        InitializeModel();
        
        StateHasChanged();
    }

    public void Close()
    {
        Logger.LogInformation("[ConsultatieModal] Closing modal");
        IsVisible = false;
        ResetModal();
        StateHasChanged();
    }

    // Private Methods
    private async Task LoadPacientData()
    {
        try
        {
            IsLoadingPacient = true;
            StateHasChanged();

            var query = new GetPacientByIdQuery(PacientID);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                PacientInfo = result.Value;
                Logger.LogInformation("[ConsultatieModal] Loaded pacient: {Nume}", PacientInfo.NumeComplet);
            }
            else
            {
                Logger.LogWarning("[ConsultatieModal] Failed to load pacient data: {Errors}", string.Join(", ", result.Errors));
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ConsultatieModal] Error loading pacient data");
        }
        finally
        {
            IsLoadingPacient = false;
            StateHasChanged();
        }
    }

    private void InitializeModel()
    {
        Model = new CreateConsultatieCommand
        {
            ProgramareID = ProgramareID,
            PacientID = PacientID,
            MedicID = MedicID,
            TipConsultatie = "Prima consultatie",
            CreatDe = MedicID.ToString()
        };

        // Pre-populate with existing pacient data if available
        if (PacientInfo != null)
        {
            Model.APP_Alergii = PacientInfo.Alergii;
            Model.APP_BoliAdult = PacientInfo.Boli_Cronice;
        }

        Logger.LogInformation("[ConsultatieModal] Model initialized");
    }

    private void ResetModal()
    {
        Model = new CreateConsultatieCommand();
        PacientInfo = null;
        ActiveTab = "motive";
        CurrentSection = "motive";
        CompletedSections.Clear();
    }

    private async Task HandleSubmit()
    {
        try
        {
            Logger.LogInformation("[ConsultatieModal] Submitting consultatie...");
            IsSaving = true;
            StateHasChanged();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(Model.MotivPrezentare))
            {
                Logger.LogWarning("[ConsultatieModal] MotivPrezentare is required");
                // TODO: Show validation message
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.DiagnosticPozitiv))
            {
                Logger.LogWarning("[ConsultatieModal] DiagnosticPozitiv is required");
                // TODO: Show validation message
                return;
            }

            var result = await Mediator.Send(Model);

            if (result.IsSuccess)
            {
                Logger.LogInformation("[ConsultatieModal] Consultatie created successfully: {Id}", result.Value);
                
                await OnConsultatieCompleted.InvokeAsync();
                Close();
            }
            else
            {
                Logger.LogError("[ConsultatieModal] Failed to create consultatie: {Errors}", string.Join(", ", result.Errors));
                // TODO: Show error message
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ConsultatieModal] Error submitting consultatie");
            // TODO: Show error message
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task SaveDraft()
    {
        Logger.LogInformation("[ConsultatieModal] Saving draft...");
        // TODO: Implement draft saving to local storage or temporary DB table
        await Task.CompletedTask;
    }

    private async Task PreviewScrisoare()
    {
        Logger.LogInformation("[ConsultatieModal] Generating preview...");
        // TODO: Generate PDF preview of scrisoare medicala
        await Task.CompletedTask;
    }

    private async Task CloseModal()
    {
        await OnClose.InvokeAsync();
        Close();
    }

    private async Task HandleOverlayClick()
    {
        // Permite inchiderea modalului prin click pe overlay
        await CloseModal();
    }

    private void CalculateIMC()
    {
        // Trigger recalculation
        StateHasChanged();
    }

    private string GetSectionLabel(string section)
    {
        return section switch
        {
            "motive" => "Motive",
            "antecedente" => "Antecedente",
            "examen" => "Examen",
            "investigatii" => "Investigatii",
            "diagnostic" => "Diagnostic",
            "tratament" => "Tratament",
            "concluzie" => "Concluzie",
            _ => section
        };
    }

    private void MarkSectionCompleted(string section)
    {
        if (!CompletedSections.Contains(section))
        {
            CompletedSections.Add(section);
            Logger.LogDebug("[ConsultatieModal] Section completed: {Section}", section);
        }
    }

    private bool IsSectionCompleted(string section)
    {
        return CompletedSections.Contains(section);
    }
}
