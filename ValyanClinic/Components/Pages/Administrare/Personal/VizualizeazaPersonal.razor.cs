using Microsoft.AspNetCore.Components;
using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using ValyanClinic.Application.Services;
using PersonalModel = ValyanClinic.Domain.Models.Personal;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

/// <summary>
/// Code-behind pentru componenta de vizualizare detalii Personal
/// Separare completa markup/logic conform best practices
/// </summary>
public partial class VizualizeazaPersonal : ComponentBase
{
    [Inject] private IPersonalService PersonalService { get; set; } = default!;
    
    [Parameter] public Guid? PersonalId { get; set; }
    [Parameter] public PersonalModel? PersonalData { get; set; }

    // UI State Properties - SIMILAR CU VizualizeazUtilizator
    public bool HasError { get; set; } = false;
    public string ErrorMessage { get; set; } = string.Empty;
    public bool IsLoading { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        if (PersonalId.HasValue && PersonalData == null)
        {
            try
            {
                IsLoading = true;
                PersonalData = await PersonalService.GetPersonalByIdAsync(PersonalId.Value);
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Eroare la încărcarea datelor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (PersonalId.HasValue && PersonalData == null)
        {
            try
            {
                IsLoading = true;
                PersonalData = await PersonalService.GetPersonalByIdAsync(PersonalId.Value);
                HasError = false;
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Eroare la încărcarea datelor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }
    }

    #region Helper Methods for Display

    private string GetStareCivilaDisplay(StareCivila? stareCivila)
    {
        return stareCivila switch
        {
            StareCivila.Necasatorit => "Necăsătorit/ă",
            StareCivila.Casatorit => "Căsătorit/ă",
            StareCivila.Divortat => "Divorțat/ă",
            StareCivila.Vaduv => "Văduvă/Văduv",
            StareCivila.UniuneConsensuala => "Uniune Consensuală",
            _ => "Nu este specificată"
        };
    }

    private string GetDepartamentDisplay(Departament? departament)
    {
        return departament switch
        {
            Departament.Administratie => "Administrație",
            Departament.Financiar => "Financiar",
            Departament.IT => "IT",
            Departament.Intretinere => "Întreținere",
            Departament.Logistica => "Logistică",
            Departament.Marketing => "Marketing",
            Departament.Receptie => "Recepție",
            Departament.ResurseUmane => "Resurse Umane",
            Departament.Securitate => "Securitate",
            Departament.Transport => "Transport",
            Departament.Juridic => "Juridic",
            Departament.RelatiiClienti => "Relații Clienți",
            Departament.Calitate => "Calitate",
            Departament.CallCenter => "Call Center",
            _ => "Nu este specificat"
        };
    }

    private string GetStatusDisplay(StatusAngajat status)
    {
        return status switch
        {
            StatusAngajat.Activ => "Activ",
            StatusAngajat.Inactiv => "Inactiv",
            _ => status.ToString()
        };
    }

    // SIMILAR CU VizualizeazUtilizator - System Age Calculation
    private string GetSystemAge(DateTime createdDate)
    {
        var age = DateTime.Now - createdDate;
        
        if (age.TotalDays < 1)
        {
            return "Astăzi";
        }
        else if (age.TotalDays < 30)
        {
            var days = (int)age.TotalDays;
            return $"{days} {(days == 1 ? "zi" : "zile")}";
        }
        else if (age.TotalDays < 365)
        {
            var months = (int)(age.TotalDays / 30);
            return $"{months} {(months == 1 ? "lună" : "luni")}";
        }
        else
        {
            var years = (int)(age.TotalDays / 365);
            var remainingMonths = (int)((age.TotalDays % 365) / 30);
            
            if (remainingMonths == 0)
            {
                return $"{years} {(years == 1 ? "an" : "ani")}";
            }
            else
            {
                return $"{years} {(years == 1 ? "an" : "ani")} și {remainingMonths} {(remainingMonths == 1 ? "lună" : "luni")}";
            }
        }
    }

    #endregion
}
