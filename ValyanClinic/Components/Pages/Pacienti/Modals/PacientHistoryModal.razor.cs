using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ValyanClinic.Components.Pages.Pacienti.Modals;

public partial class PacientHistoryModal : ComponentBase
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    #region Parameters
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public Guid? PacientId { get; set; }
    #endregion

    #region State
    private bool IsLoading { get; set; }
    private bool HasError { get; set; }
    private string? ErrorMessage { get; set; }
    private string PacientNume { get; set; } = string.Empty;
    private string SelectedFilter { get; set; } = "toate";
    #endregion

    // Data
    private List<MedicalRecord> AllRecords { get; set; } = new();
    private List<MedicalRecord> FilteredRecords => SelectedFilter == "toate"
        ? AllRecords
        : AllRecords.Where(r => r.Type.ToLower() == SelectedFilter.Replace("i", "ie")).ToList();

    // Pagination
    private int CurrentPage { get; set; } = 1;
    private int PageSize { get; set; } = 10;
    private int TotalRecords => FilteredRecords.Count;
    private int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible && PacientId.HasValue)
        {
            await LoadHistoryData();
        }
    }

    private async Task LoadHistoryData()
    {
        IsLoading = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            // TODO: Replace with actual API call
            await Task.Delay(500); // Simulate API call

            // Mock data pentru demonstrație
            PacientNume = "Popescu Ion";
            AllRecords = GenerateMockData();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Eroare: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void SetFilter(string filter)
    {
        SelectedFilter = filter;
        CurrentPage = 1;
    }

    private void LoadPreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
        }
    }

    private void LoadNextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
        }
    }

    private async Task OpenAddRecordModal()
    {
        await JSRuntime.InvokeVoidAsync("alert", "Funcționalitate în dezvoltare: Adăugare înregistrare medicală");
    }

    private async Task EditRecord(MedicalRecord record)
    {
        await JSRuntime.InvokeVoidAsync("alert", $"Editare înregistrare: {record.Title}");
    }

    private async Task DeleteRecord(MedicalRecord record)
    {
        var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
            $"Sunteți sigur că doriți să ștergeți înregistrarea '{record.Title}'?");

        if (confirmed)
        {
            AllRecords.Remove(record);
            StateHasChanged();
        }
    }

    private async Task ViewAttachment(Attachment attachment)
    {
        await JSRuntime.InvokeVoidAsync("alert", $"Vizualizare atașament: {attachment.FileName}");
    }

    private async Task ExportToPdf()
    {
        await JSRuntime.InvokeVoidAsync("alert", "Export PDF în dezvoltare");
    }

    private string GetRecordTypeClass(string type)
    {
        return type.ToLower() switch
        {
            "consultatie" => "type-consultation",
            "analize" => "type-analysis",
            "tratament" => "type-treatment",
            "interventie" => "type-intervention",
            _ => "type-other"
        };
    }

    private string GetRecordTypeIcon(string type)
    {
        return type.ToLower() switch
        {
            "consultatie" => "stethoscope",
            "analize" => "vial",
            "tratament" => "pills",
            "interventie" => "procedures",
            _ => "notes-medical"
        };
    }

    private string GetFileIcon(string extension)
    {
        return extension.ToLower() switch
        {
            ".pdf" => "pdf",
            ".doc" or ".docx" => "word",
            ".xls" or ".xlsx" => "excel",
            ".jpg" or ".jpeg" or ".png" => "image",
            _ => "file"
        };
    }

    private async Task HandleOverlayClick()
    {
        await Close();
    }

    private async Task Close()
    {
        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
        SelectedFilter = "toate";
        CurrentPage = 1;
    }

    // Mock Data Generator
    private List<MedicalRecord> GenerateMockData()
    {
        return new List<MedicalRecord>
        {
            new MedicalRecord
            {
                Id = Guid.NewGuid(),
                Type = "Consultatie",
                Title = "Consultatie Cardiologie",
                Description = "Pacient prezinta dureri toracice intermitente. ECG normal, TA 130/85 mmHg.",
                Doctor = "Dr. Ionescu Maria - Cardiolog",
                Date = DateTime.Now.AddDays(-5),
                Details = new Dictionary<string, string>
                {
                    { "Tensiune", "130/85 mmHg" },
                    { "Puls", "72 bpm" },
                    { "Diagnostic", "Angina pectorala stabila" }
                },
                Attachments = new List<Attachment>
                {
                    new Attachment { FileName = "ECG_20250115.pdf", Extension = ".pdf", Size = 245000 }
                }
            },
            new MedicalRecord
            {
                Id = Guid.NewGuid(),
                Type = "Analize",
                Title = "Analize Sange Complete",
                Description = "Analize de rutina: hemoleucograma, glicemie, colesterol.",
                Doctor = "Laborator MedLife",
                Date = DateTime.Now.AddDays(-10),
                Details = new Dictionary<string, string>
                {
                    { "Hemoglobina", "14.2 g/dL (normal)" },
                    { "Glicemie", "105 mg/dL (usor crescut)" },
                    { "Colesterol total", "220 mg/dL (crescut)" }
                },
                Attachments = new List<Attachment>
                {
                    new Attachment { FileName = "Analize_20250110.pdf", Extension = ".pdf", Size = 512000 }
                }
            },
            new MedicalRecord
            {
                Id = Guid.NewGuid(),
                Type = "Tratament",
                Title = "Prescriptie Medicala",
                Description = "Tratament pentru hipertensiune arteriala.",
                Doctor = "Dr. Popescu Ion - Medic Familie",
                Date = DateTime.Now.AddDays(-15),
                Details = new Dictionary<string, string>
                {
                    { "Medicament", "Enalapril 10mg" },
                    { "Doza", "1 cp/zi dimineata" },
                    { "Durata", "3 luni" }
                }
            },
            new MedicalRecord
            {
                Id = Guid.NewGuid(),
                Type = "Interventie",
                Title = "Interventie Chirurgicala Minora",
                Description = "Indepartare chist sebaceu regiune dorsala.",
                Doctor = "Dr. Vladescu Ana - Chirurg",
                Date = DateTime.Now.AddMonths(-2),
                Details = new Dictionary<string, string>
                {
                    { "Tip interventie", "Excizie chirurgicala" },
                    { "Anestezie", "Locala" },
                    { "Durata", "30 minute" },
                    { "Complicatii", "Niciunele" }
                },
                Attachments = new List<Attachment>
                {
                    new Attachment { FileName = "Raport_Interventie.pdf", Extension = ".pdf", Size = 324000 },
                    new Attachment { FileName = "Fotografie_Preop.jpg", Extension = ".jpg", Size = 156000 }
                }
            },
            new MedicalRecord
            {
                Id = Guid.NewGuid(),
                Type = "Consultatie",
                Title = "Control Post-Operator",
                Description = "Verificare vindecare plaga operatorie. Evolutie favorabila, fara semne de infectie.",
                Doctor = "Dr. Vladescu Ana - Chirurg",
                Date = DateTime.Now.AddMonths(-1).AddDays(-20),
                Details = new Dictionary<string, string>
                {
                    { "Aspect plaga", "Complet vindecata" },
                    { "Recomandate", "Poate relua activitate normala" }
                }
            }
        };
    }

    // Data Models
    public class MedicalRecord
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Doctor { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public Dictionary<string, string>? Details { get; set; }
        public List<Attachment>? Attachments { get; set; }
    }

    public class Attachment
    {
        public string FileName { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}
