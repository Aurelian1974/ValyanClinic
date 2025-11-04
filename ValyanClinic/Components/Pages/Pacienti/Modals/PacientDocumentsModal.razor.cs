using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ValyanClinic.Components.Pages.Pacienti.Modals;

public partial class PacientDocumentsModal : ComponentBase
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
    private string SelectedCategory { get; set; } = "toate";
    private string ViewMode { get; set; } = "grid"; // grid or list
    #endregion

    // Data
    private List<MedicalDocument> AllDocuments { get; set; } = new();
    private List<MedicalDocument> FilteredDocuments => SelectedCategory == "toate"
        ? AllDocuments
        : AllDocuments.Where(d => d.Category.ToLower() == SelectedCategory).ToList();

    // Storage
    private long TotalStorageUsed => AllDocuments.Sum(d => d.FileSize);
    private long MaxStorage => 500 * 1024 * 1024; // 500 MB
    private double StoragePercentage => Math.Min((double)TotalStorageUsed / MaxStorage * 100, 100);

    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible && PacientId.HasValue)
        {
            await LoadDocuments();
        }
    }

    private async Task LoadDocuments()
    {
        IsLoading = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            // TODO: Replace with actual API call
            await Task.Delay(500); // Simulate API call

            // Mock data
            PacientNume = "Popescu Ion";
            AllDocuments = GenerateMockDocuments();
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

    private void SetCategory(string category)
    {
        SelectedCategory = category;
    }

    private int GetCategoryCount(string category)
    {
        return AllDocuments.Count(d => d.Category.ToLower() == category);
    }

    private string GetCategoryClass(string category)
    {
        return category.ToLower() switch
        {
            "rezultate" => "category-results",
            "imagistica" => "category-imaging",
            "retete" => "category-prescription",
            "rapoarte" => "category-reports",
            "altele" => "category-other",
            _ => string.Empty
        };
    }

    private string GetFileTypeIcon(string fileType)
    {
        return fileType.ToLower() switch
        {
            "pdf" => "pdf",
            "doc" or "docx" => "word",
            "xls" or "xlsx" => "excel",
            "jpg" or "jpeg" or "png" or "gif" => "image",
            "zip" or "rar" => "archive",
            _ => "file"
        };
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }

    private async Task OpenUploadModal()
    {
        await JSRuntime.InvokeVoidAsync("alert", "Funcționalitate în dezvoltare: Încărcare document");
    }

    private async Task ViewDocument(MedicalDocument doc)
    {
        await JSRuntime.InvokeVoidAsync("alert", $"Vizualizare document: {doc.FileName}");
    }

    private async Task DownloadDocument(MedicalDocument doc)
    {
        await JSRuntime.InvokeVoidAsync("alert", $"Descărcare document: {doc.FileName}");
    }

    private async Task ShareDocument(MedicalDocument doc)
    {
        await JSRuntime.InvokeVoidAsync("alert", $"Partajare document: {doc.FileName}");
    }

    private async Task DeleteDocument(MedicalDocument doc)
    {
        var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
            $"Sunteți sigur că doriți să ștergeți documentul '{doc.FileName}'?");

        if (confirmed)
        {
            AllDocuments.Remove(doc);
            StateHasChanged();
        }
    }

    private async Task DownloadAll()
    {
        await JSRuntime.InvokeVoidAsync("alert", "Descărcare toate documentele în format ZIP");
    }

    private async Task HandleOverlayClick()
    {
        await Close();
    }

    private async Task Close()
    {
        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
        SelectedCategory = "toate";
        ViewMode = "grid";
    }

    // Mock Data Generator
    private List<MedicalDocument> GenerateMockDocuments()
    {
        return new List<MedicalDocument>
        {
            new MedicalDocument
            {
                Id = Guid.NewGuid(),
                FileName = "Analize_Sange_15012025.pdf",
                Category = "Rezultate",
                FileType = "pdf",
                FileSize = 245 * 1024,
                UploadDate = DateTime.Now.AddDays(-5),
                Description = "Analize complete sange: hemoleucograma, glicemie, colesterol"
            },
            new MedicalDocument
            {
                Id = Guid.NewGuid(),
                FileName = "Radiografie_Torace.jpg",
                Category = "Imagistica",
                FileType = "jpg",
                FileSize = 1024 * 1024,
                UploadDate = DateTime.Now.AddDays(-15),
                Description = "Radiografie toracică AP - normală"
            },
            new MedicalDocument
            {
                Id = Guid.NewGuid(),
                FileName = "Reteta_Cardiologie_2025.pdf",
                Category = "Retete",
                FileType = "pdf",
                FileSize = 156 * 1024,
                UploadDate = DateTime.Now.AddDays(-10),
                Description = "Rețetă medicală - Enalapril 10mg x 3 luni"
            },
            new MedicalDocument
            {
                Id = Guid.NewGuid(),
                FileName = "Raport_Consultatie_Cardiolog.pdf",
                Category = "Rapoarte",
                FileType = "pdf",
                FileSize = 324 * 1024,
                UploadDate = DateTime.Now.AddDays(-20),
                Description = "Raport consultație cardiologie - Dr. Ionescu"
            },
            new MedicalDocument
            {
                Id = Guid.NewGuid(),
                FileName = "Ecografie_Abdominala.pdf",
                Category = "Imagistica",
                FileType = "pdf",
                FileSize = 512 * 1024,
                UploadDate = DateTime.Now.AddMonths(-1),
                Description = "Ecografie abdominală - fără modificări patologice"
            },
            new MedicalDocument
            {
                Id = Guid.NewGuid(),
                FileName = "Analize_Urina_20122024.pdf",
                Category = "Rezultate",
                FileType = "pdf",
                FileSize = 187 * 1024,
                UploadDate = DateTime.Now.AddMonths(-1).AddDays(-5),
                Description = "Sumar de urină - parametri normali"
            },
            new MedicalDocument
            {
                Id = Guid.NewGuid(),
                FileName = "Bilet_Externare_Spital.doc",
                Category = "Rapoarte",
                FileType = "doc",
                FileSize = 78 * 1024,
                UploadDate = DateTime.Now.AddMonths(-2),
                Description = "Bilet de externare după internare chirurgie"
            },
            new MedicalDocument
            {
                Id = Guid.NewGuid(),
                FileName = "Adeverinta_Medicala.pdf",
                Category = "Altele",
                FileType = "pdf",
                FileSize = 95 * 1024,
                UploadDate = DateTime.Now.AddMonths(-3),
                Description = "Adeverință medicală pentru concediu medical"
            },
            new MedicalDocument
            {
                Id = Guid.NewGuid(),
                FileName = "RMN_Craniocerebral.pdf",
                Category = "Imagistica",
                FileType = "pdf",
                FileSize = 2048 * 1024,
                UploadDate = DateTime.Now.AddMonths(-4),
                Description = "RMN craniocerebrală - aspect normal"
            },
            new MedicalDocument
            {
                Id = Guid.NewGuid(),
                FileName = "Certificat_Vaccinare_COVID.pdf",
                Category = "Altele",
                FileType = "pdf",
                FileSize = 145 * 1024,
                UploadDate = DateTime.Now.AddMonths(-6),
                Description = "Certificat vaccinare COVID-19 (3 doze)"
            }
        };
    }

    // Data Model
    public class MedicalDocument
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public string? Description { get; set; }
    }
}
