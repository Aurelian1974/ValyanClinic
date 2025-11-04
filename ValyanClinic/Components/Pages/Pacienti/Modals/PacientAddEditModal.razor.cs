using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ValyanClinic.Application.Features.PacientManagement.Commands.CreatePacient;
using ValyanClinic.Application.Features.PacientManagement.Commands.UpdatePacient;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById;

namespace ValyanClinic.Components.Pages.Pacienti.Modals;

public partial class PacientAddEditModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public EventCallback OnSaved { get; set; }
    [Parameter] public Guid? PacientId { get; set; }

    // State
    private bool IsEditMode => PacientId.HasValue && PacientId != Guid.Empty;
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }
    private bool HasError { get; set; }
    private string? ErrorMessage { get; set; }
    private string ActiveTab { get; set; } = "personal";

    // Form Model
    private PacientFormModel FormModel { get; set; } = new();

    // Dropdown Options
    private List<string> SexOptions { get; set; } = new() { "M", "F" };

    // Judete list (ar trebui să vină din API)
    private List<string> JudeteList { get; set; } = new()
    {
        "Bucuresti", "Alba", "Arad", "Arges", "Bacau", "Bihor", "Bistrita-Nasaud",
        "Botosani", "Brasov", "Braila", "Buzau", "Caras-Severin", "Calarasi",
        "Cluj", "Constanta", "Covasna", "Dambovita", "Dolj", "Galati", "Giurgiu",
        "Gorj", "Harghita", "Hunedoara", "Ialomita", "Iasi", "Ilfov", "Maramures",
        "Mehedinti", "Mures", "Neamt", "Olt", "Prahova", "Satu Mare", "Salaj",
        "Sibiu", "Suceava", "Teleorman", "Timis", "Tulcea", "Vaslui", "Valcea", "Vrancea"
    };

    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible && IsEditMode)
        {
            await LoadPacientData();
        }
        else if (IsVisible && !IsEditMode)
        {
            ResetForm();
        }
    }

    private async Task LoadPacientData()
    {
        IsLoading = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            var query = new GetPacientByIdQuery(PacientId!.Value);
            var result = await Mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                var pacient = result.Value;
                FormModel = new PacientFormModel
                {
                    Nume = pacient.Nume,
                    Prenume = pacient.Prenume,
                    CNP = pacient.CNP,
                    Cod_Pacient = pacient.Cod_Pacient,
                    Data_Nasterii = pacient.Data_Nasterii,
                    Sex = pacient.Sex,
                    Telefon = pacient.Telefon,
                    Telefon_Secundar = pacient.Telefon_Secundar,
                    Email = pacient.Email,
                    Judet = pacient.Judet,
                    Localitate = pacient.Localitate,
                    Adresa = pacient.Adresa,
                    Cod_Postal = pacient.Cod_Postal,
                    Asigurat = pacient.Asigurat,
                    CNP_Asigurat = pacient.CNP_Asigurat,
                    Nr_Card_Sanatate = pacient.Nr_Card_Sanatate,
                    Casa_Asigurari = pacient.Casa_Asigurari,
                    Alergii = pacient.Alergii,
                    Boli_Cronice = pacient.Boli_Cronice,
                    Medic_Familie = pacient.Medic_Familie,
                    Persoana_Contact = pacient.Persoana_Contact,
                    Telefon_Urgenta = pacient.Telefon_Urgenta,
                    Relatie_Contact = pacient.Relatie_Contact,
                    Activ = pacient.Activ,
                    Observatii = pacient.Observatii
                };
            }
            else
            {
                HasError = true;
                ErrorMessage = result.FirstError ?? "Eroare la încărcarea datelor pacientului.";
            }
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

    private void ResetForm()
    {
        FormModel = new PacientFormModel
        {
            Data_Nasterii = DateTime.Now.AddYears(-30),
            Activ = true,
            Asigurat = false
        };
        ActiveTab = "personal";
        HasError = false;
        ErrorMessage = null;
    }

    private void SetActiveTab(string tab)
    {
        ActiveTab = tab;
    }

    private async Task HandleSubmit()
    {
        IsSaving = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            if (IsEditMode)
            {
                await UpdatePacient();
            }
            else
            {
                await CreatePacient();
            }
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task CreatePacient()
    {
        var command = new CreatePacientCommand
        {
            Nume = FormModel.Nume,
            Prenume = FormModel.Prenume,
            CNP = FormModel.CNP,
            Cod_Pacient = FormModel.Cod_Pacient,
            Data_Nasterii = FormModel.Data_Nasterii,
            Sex = FormModel.Sex,
            Telefon = FormModel.Telefon,
            Telefon_Secundar = FormModel.Telefon_Secundar,
            Email = FormModel.Email,
            Judet = FormModel.Judet,
            Localitate = FormModel.Localitate,
            Adresa = FormModel.Adresa,
            Cod_Postal = FormModel.Cod_Postal,
            Asigurat = FormModel.Asigurat,
            CNP_Asigurat = FormModel.CNP_Asigurat,
            Nr_Card_Sanatate = FormModel.Nr_Card_Sanatate,
            Casa_Asigurari = FormModel.Casa_Asigurari,
            Alergii = FormModel.Alergii,
            Boli_Cronice = FormModel.Boli_Cronice,
            Medic_Familie = FormModel.Medic_Familie,
            Persoana_Contact = FormModel.Persoana_Contact,
            Telefon_Urgenta = FormModel.Telefon_Urgenta,
            Relatie_Contact = FormModel.Relatie_Contact,
            Activ = FormModel.Activ,
            Observatii = FormModel.Observatii,
            CreatDe = "System" // TODO: Get from authentication
        };

        var result = await Mediator.Send(command);

        if (result.IsSuccess)
        {
            await JSRuntime.InvokeVoidAsync("alert", result.SuccessMessage ?? "Pacient creat cu succes!");
            await Close();
            await OnSaved.InvokeAsync();
        }
        else
        {
            HasError = true;
            ErrorMessage = string.Join("\n", result.Errors);
            await JSRuntime.InvokeVoidAsync("alert", $"Eroare la salvare:\n{ErrorMessage}");
        }
    }

    private async Task UpdatePacient()
    {
        var command = new UpdatePacientCommand
        {
            Id = PacientId!.Value,
            Nume = FormModel.Nume,
            Prenume = FormModel.Prenume,
            CNP = FormModel.CNP,
            Data_Nasterii = FormModel.Data_Nasterii,
            Sex = FormModel.Sex,
            Telefon = FormModel.Telefon,
            Telefon_Secundar = FormModel.Telefon_Secundar,
            Email = FormModel.Email,
            Judet = FormModel.Judet,
            Localitate = FormModel.Localitate,
            Adresa = FormModel.Adresa,
            Cod_Postal = FormModel.Cod_Postal,
            Asigurat = FormModel.Asigurat,
            CNP_Asigurat = FormModel.CNP_Asigurat,
            Nr_Card_Sanatate = FormModel.Nr_Card_Sanatate,
            Casa_Asigurari = FormModel.Casa_Asigurari,
            Alergii = FormModel.Alergii,
            Boli_Cronice = FormModel.Boli_Cronice,
            Medic_Familie = FormModel.Medic_Familie,
            Persoana_Contact = FormModel.Persoana_Contact,
            Telefon_Urgenta = FormModel.Telefon_Urgenta,
            Relatie_Contact = FormModel.Relatie_Contact,
            Activ = FormModel.Activ,
            Observatii = FormModel.Observatii,
            ModificatDe = "System" // TODO: Get from authentication
        };

        var result = await Mediator.Send(command);

        if (result.IsSuccess)
        {
            await JSRuntime.InvokeVoidAsync("alert", result.SuccessMessage ?? "Pacient actualizat cu succes!");
            await Close();
            await OnSaved.InvokeAsync();
        }
        else
        {
            HasError = true;
            ErrorMessage = string.Join("\n", result.Errors);
            await JSRuntime.InvokeVoidAsync("alert", $"Eroare la salvare:\n{ErrorMessage}");
        }
    }

    private async Task HandleOverlayClick()
    {
        var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", 
            "Sunteți sigur că doriți să închideți? Modificările nesalvate vor fi pierdute.");
        
        if (confirmed)
        {
            await Close();
        }
    }

    private async Task Close()
    {
        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
        ResetForm();
    }

    // Form Model Class
    public class PacientFormModel
    {
        [Required(ErrorMessage = "Numele este obligatoriu")]
        [StringLength(100, ErrorMessage = "Numele nu poate depăși 100 de caractere")]
        public string Nume { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prenumele este obligatoriu")]
        [StringLength(100, ErrorMessage = "Prenumele nu poate depăși 100 de caractere")]
        public string Prenume { get; set; } = string.Empty;

        [StringLength(13, MinimumLength = 13, ErrorMessage = "CNP-ul trebuie să conțină exact 13 cifre")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "CNP-ul trebuie să conțină doar cifre")]
        public string? CNP { get; set; }

        public string? Cod_Pacient { get; set; }

        [Required(ErrorMessage = "Data nașterii este obligatorie")]
        public DateTime Data_Nasterii { get; set; }

        [Required(ErrorMessage = "Sexul este obligatoriu")]
        public string Sex { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Format telefon invalid")]
        public string? Telefon { get; set; }

        [Phone(ErrorMessage = "Format telefon invalid")]
        public string? Telefon_Secundar { get; set; }

        [EmailAddress(ErrorMessage = "Format email invalid")]
        public string? Email { get; set; }

        public string? Judet { get; set; }
        public string? Localitate { get; set; }
        public string? Adresa { get; set; }
        
        [StringLength(6, ErrorMessage = "Codul poștal nu poate depăși 6 caractere")]
        public string? Cod_Postal { get; set; }

        public bool Asigurat { get; set; }
        
        [StringLength(13, MinimumLength = 13, ErrorMessage = "CNP Asigurat trebuie să conțină exact 13 cifre")]
        public string? CNP_Asigurat { get; set; }

        public string? Nr_Card_Sanatate { get; set; }
        public string? Casa_Asigurari { get; set; }
        public string? Alergii { get; set; }
        public string? Boli_Cronice { get; set; }
        public string? Medic_Familie { get; set; }
        public string? Persoana_Contact { get; set; }
        
        [Phone(ErrorMessage = "Format telefon invalid")]
        public string? Telefon_Urgenta { get; set; }
        
        public string? Relatie_Contact { get; set; }
        public bool Activ { get; set; }
        public string? Observatii { get; set; }
    }
}
