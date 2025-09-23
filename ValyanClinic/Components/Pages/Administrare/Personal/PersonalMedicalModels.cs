using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using PersonalMedicalModel = ValyanClinic.Domain.Models.PersonalMedical;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

/// <summary>
/// Models și helper classes pentru pagina PersonalMedical
/// Similar cu PersonalModels dar adaptat pentru PersonalMedical cu departamente din DB
/// </summary>
public class PersonalMedicalModels
{
    public List<PersonalMedicalModel> PersonalMedical { get; set; } = new();
    public List<PersonalMedicalModel> FilteredPersonalMedical { get; set; } = new();
    public List<StatisticCard> PersonalMedicalStatistics { get; set; } = new();
    public int[] PageSizes { get; } = { 10, 20, 50, 100 };

    // Filter options - IMPORTANT: Departamente din DB, nu enum-uri
    public List<FilterOption<string?>> DepartmentFilterOptions { get; set; } = new();
    public List<FilterOption<PozitiePersonalMedical?>> PozitieFilterOptions { get; set; } = new();
    public List<FilterOption<bool?>> StatusFilterOptions { get; set; } = new();
    public List<string> ActivityPeriodOptions { get; } = new()
    {
        "", "Ultima zi", "Ultima săptămână", "Ultima lună", "Ultimele 3 luni", "Ultimul an"
    };

    public void SetPersonalMedical(List<PersonalMedicalModel> personalMedical)
    {
        PersonalMedical = personalMedical;
        FilteredPersonalMedical = personalMedical;
        CalculateStatistics();
    }

    public void CalculateStatistics()
    {
        if (PersonalMedical == null || !PersonalMedical.Any())
        {
            PersonalMedicalStatistics = CreateEmptyStatistics();
            return;
        }

        var totalPersonalMedical = PersonalMedical.Count;
        var personalMedicalActiv = PersonalMedical.Count(p => p.EsteActiv);
        var personalMedicalInactiv = PersonalMedical.Count(p => !p.EsteActiv);

        // Statistici pe departamente medicale
        var departamenteMedicaleCount = PersonalMedical
            .Where(p => !string.IsNullOrWhiteSpace(p.DepartamentDisplay))
            .GroupBy(p => p.DepartamentDisplay)
            .Count();

        // Doctori și asistenți (poziții medicale principale)
        var doctoriSiAsistenti = PersonalMedical.Count(p => p.EsteDoctorSauAsistent);

        // Personal nou adăugat (ultima lună)
        var personalNou = PersonalMedical.Count(p => p.DataCreare > DateTime.Now.AddMonths(-1));

        PersonalMedicalStatistics = new List<StatisticCard>
        {
            new("Total Personal Medical", totalPersonalMedical, "fas fa-user-md", "primary"),
            new("Personal Activ", personalMedicalActiv, "fas fa-user-check", "success"),
            new("Personal Inactiv", personalMedicalInactiv, "fas fa-user-times", "danger"),
            new("Doctori & Asistenți", doctoriSiAsistenti, "fas fa-stethoscope", "info"),
            new("Departamente Medicale", departamenteMedicaleCount, "fas fa-hospital", "warning"),
            new("Adăugat recent", personalNou, "fas fa-user-plus", "secondary")
        };
    }

    private List<StatisticCard> CreateEmptyStatistics()
    {
        return new List<StatisticCard>
        {
            new("Total Personal Medical", 0, "fas fa-user-md", "primary"),
            new("Personal Activ", 0, "fas fa-user-check", "success"),
            new("Personal Inactiv", 0, "fas fa-user-times", "danger"),
            new("Doctori & Asistenți", 0, "fas fa-stethoscope", "info"),
            new("Departamente Medicale", 0, "fas fa-hospital", "warning"),
            new("Adăugat recent", 0, "fas fa-user-plus", "secondary")
        };
    }

    public void InitializeFilterOptions(List<DepartamentMedical> departamenteMedicale)
    {
        // Departament filter options - din DB, nu enum-uri
        DepartmentFilterOptions = new List<FilterOption<string?>>
        {
            new FilterOption<string?>(null, "Toate departamentele")
        };

        // Grupează departamentele unice după nume
        var uniqueDepartments = departamenteMedicale
            .Where(d => !string.IsNullOrWhiteSpace(d.Nume))
            .GroupBy(d => d.Nume)
            .Select(g => g.First())
            .OrderBy(d => d.Nume);

        foreach (var dept in uniqueDepartments)
        {
            DepartmentFilterOptions.Add(new FilterOption<string?>(dept.Nume, dept.DisplayName));
        }

        // Poziție filter options - din enum-uri (singura excepție pentru PersonalMedical)
        PozitieFilterOptions = new List<FilterOption<PozitiePersonalMedical?>>
        {
            new FilterOption<PozitiePersonalMedical?>(null, "Toate pozițiile")
        };

        foreach (PozitiePersonalMedical pozitie in Enum.GetValues<PozitiePersonalMedical>())
        {
            PozitieFilterOptions.Add(new FilterOption<PozitiePersonalMedical?>(pozitie, pozitie.GetDisplayName()));
        }

        // Status filter options
        StatusFilterOptions = new List<FilterOption<bool?>>
        {
            new FilterOption<bool?>(null, "Toate statusurile"),
            new FilterOption<bool?>(true, "Activ"),
            new FilterOption<bool?>(false, "Inactiv")
        };
    }

    public List<PersonalMedicalModel> ApplyFilters(PersonalMedicalPageState state)
    {
        var filtered = PersonalMedical.AsEnumerable();

        // Text search
        if (!string.IsNullOrEmpty(state.SearchText))
        {
            var searchTerm = state.SearchText.ToLower();
            filtered = filtered.Where(p => 
                p.NumeComplet.ToLower().Contains(searchTerm) ||
                (p.Email?.ToLower().Contains(searchTerm) == true) ||
                (p.Telefon?.Contains(searchTerm) == true) ||
                (p.NumarLicenta?.ToLower().Contains(searchTerm) == true) ||
                (p.Specializare?.ToLower().Contains(searchTerm) == true) ||
                (p.CategorieName?.ToLower().Contains(searchTerm) == true) ||
                (p.SpecializareName?.ToLower().Contains(searchTerm) == true) ||
                (p.Departament?.ToLower().Contains(searchTerm) == true));
        }

        // Department filter - din DB
        if (!string.IsNullOrEmpty(state.SelectedDepartment))
        {
            filtered = filtered.Where(p => p.MatchesDepartament(state.SelectedDepartment));
        }

        // Poziție filter
        if (state.SelectedPozitie.HasValue)
        {
            filtered = filtered.Where(p => p.MatchesPozitie(state.SelectedPozitie));
        }

        // Status filter
        if (state.SelectedStatus.HasValue)
        {
            filtered = filtered.Where(p => p.MatchesStatus(state.SelectedStatus));
        }

        FilteredPersonalMedical = filtered.ToList();
        return FilteredPersonalMedical;
    }

    public PersonalMedicalModel CreateNewPersonalMedical()
    {
        return new PersonalMedicalModel
        {
            PersonalID = Guid.NewGuid(),
            EsteActiv = true,
            DataCreare = DateTime.Now,
            Pozitie = PozitiePersonalMedical.AsistentMedical // Poziție implicită
        };
    }

    public PersonalMedicalModel ClonePersonalMedical(PersonalMedicalModel original)
    {
        return new PersonalMedicalModel
        {
            PersonalID = original.PersonalID,
            Nume = original.Nume,
            Prenume = original.Prenume,
            Specializare = original.Specializare,
            NumarLicenta = original.NumarLicenta,
            Telefon = original.Telefon,
            Email = original.Email,
            Departament = original.Departament,
            Pozitie = original.Pozitie,
            EsteActiv = original.EsteActiv,
            DataCreare = original.DataCreare,
            CategorieID = original.CategorieID,
            SpecializareID = original.SpecializareID,
            SubspecializareID = original.SubspecializareID,
            CategorieName = original.CategorieName,
            SpecializareName = original.SpecializareName,
            SubspecializareName = original.SubspecializareName
        };
    }

    /// <summary>
    /// Helper classes pentru filtrarea și afișarea datelor
    /// </summary>
    public class FilterOption<T>
    {
        public FilterOption(T value, string text)
        {
            Value = value;
            Text = text;
        }

        public T Value { get; set; }
        public string Text { get; set; }
    }

    public class StatisticCard
    {
        public StatisticCard(string label, int value, string iconClass, string colorClass)
        {
            Label = label;
            Value = value;
            IconClass = iconClass;
            ColorClass = colorClass;
        }

        public string Label { get; set; }
        public int Value { get; set; }
        public string IconClass { get; set; }
        public string ColorClass { get; set; }
    }

    /// <summary>
    /// Metode helper pentru validări și business logic specifice medicale
    /// </summary>
    public static class PersonalMedicalValidationHelpers
    {
        public static bool ValidateNumarLicenta(string? numarLicenta, PozitiePersonalMedical? pozitie)
        {
            if (!pozitie.HasValue) return true;

            bool needsLicense = pozitie.Value.RequiresLicensaMedicala();
            
            if (needsLicense)
            {
                return !string.IsNullOrWhiteSpace(numarLicenta) && numarLicenta.Length >= 5;
            }

            return true;
        }

        public static List<string> GetValidationErrors(PersonalMedicalModel model)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(model.Nume))
                errors.Add("Numele este obligatoriu");

            if (string.IsNullOrWhiteSpace(model.Prenume))
                errors.Add("Prenumele este obligatoriu");

            if (!ValidateNumarLicenta(model.NumarLicenta, model.Pozitie))
                errors.Add($"Numărul de licență este obligatoriu pentru poziția {model.Pozitie?.GetDisplayName()}");

            if (!string.IsNullOrWhiteSpace(model.Email) && !IsValidEmail(model.Email))
                errors.Add("Formatul email-ului este invalid");

            return errors;
        }

        private static bool IsValidEmail(string email)
        {
            return email.Contains('@') && email.Contains('.');
        }
    }
}
