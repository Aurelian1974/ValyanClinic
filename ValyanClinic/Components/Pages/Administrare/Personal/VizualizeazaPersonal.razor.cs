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
    
    // CALLBACK PENTRU TOAST iN MODAL - SOLUtIA PENTRU TOAST BLURAT
    [Parameter] public EventCallback<(string Title, string Message, string CssClass)> OnToastMessage { get; set; }

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
                
                // Toast de success pentru incarcarea datelor
                if (OnToastMessage.HasDelegate && PersonalData != null)
                {
                    await OnToastMessage.InvokeAsync(("Succes", "Datele au fost incarcate cu succes", "e-toast-success"));
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Eroare la incarcarea datelor: {ex.Message}";
                
                // Toast de eroare
                if (OnToastMessage.HasDelegate)
                {
                    await OnToastMessage.InvokeAsync(("Eroare", ErrorMessage, "e-toast-danger"));
                }
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
                
                // Toast de success pentru incarcarea datelor
                if (OnToastMessage.HasDelegate && PersonalData != null)
                {
                    await OnToastMessage.InvokeAsync(("Succes", "Datele au fost incarcate cu succes", "e-toast-success"));
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Eroare la incarcarea datelor: {ex.Message}";
                
                // Toast de eroare
                if (OnToastMessage.HasDelegate)
                {
                    await OnToastMessage.InvokeAsync(("Eroare", ErrorMessage, "e-toast-danger"));
                }
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
            StareCivila.Necasatorit => "Necasatorit/a",
            StareCivila.Casatorit => "Casatorit/a",
            StareCivila.Divortat => "Divortat/a",
            StareCivila.Vaduv => "Vaduva/Vaduv",
            StareCivila.UniuneConsensuala => "Uniune Consensuala",
            _ => "Nu este specificata"
        };
    }

    private string GetDepartamentDisplay(Departament? departament)
    {
        return departament switch
        {
            Departament.Administratie => "Administratie",
            Departament.Financiar => "Financiar",
            Departament.IT => "IT",
            Departament.Intretinere => "Intretinere",
            Departament.Logistica => "Logistica",
            Departament.Marketing => "Marketing",
            Departament.Receptie => "Receptie",
            Departament.ResurseUmane => "Resurse Umane",
            Departament.Securitate => "Securitate",
            Departament.Transport => "Transport",
            Departament.Juridic => "Juridic",
            Departament.RelatiiClienti => "Relatii Clienti",
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
            return "Astazi";
        }
        else if (age.TotalDays < 30)
        {
            var days = (int)age.TotalDays;
            return $"{days} {(days == 1 ? "zi" : "zile")}";
        }
        else if (age.TotalDays < 365)
        {
            var months = (int)(age.TotalDays / 30);
            return $"{months} {(months == 1 ? "luna" : "luni")}";
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
                return $"{years} {(years == 1 ? "an" : "ani")} si {remainingMonths} {(remainingMonths == 1 ? "luna" : "luni")}";
            }
        }
    }

    #endregion
}
