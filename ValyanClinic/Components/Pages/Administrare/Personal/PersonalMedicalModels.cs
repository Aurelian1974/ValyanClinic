using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using PersonalMedicalModel = ValyanClinic.Domain.Models.PersonalMedical;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

/// <summary>
/// Models si helper classes pentru pagina PersonalMedical
/// Similar cu PersonalModels dar adaptat pentru PersonalMedical cu departamente din DB
/// </summary>
public class PersonalMedicalModels
{
    public List<PersonalMedicalModel> PersonalMedical { get; set; } = new();
    public List<PersonalMedicalModel> FilteredPersonalMedical { get; set; } = new();
    public int[] PageSizes { get; } = { 10, 20, 50, 100 };

    // Filter options - IMPORTANT: Departamente din DB, nu enum-uri
    public List<FilterOption<string?>> DepartmentFilterOptions { get; set; } = new();
    public List<FilterOption<PozitiePersonalMedical?>> PozitieFilterOptions { get; set; } = new();
    public List<FilterOption<bool?>> StatusFilterOptions { get; set; } = new();
    public List<string> ActivityPeriodOptions { get; } = new()
    {
        "", "Ultima zi", "Ultima saptamana", "Ultima luna", "Ultimele 3 luni", "Ultimul an"
    };

    public void SetPersonalMedical(List<PersonalMedicalModel> personalMedical)
    {
        PersonalMedical = personalMedical;
        FilteredPersonalMedical = personalMedical;
    }

    public void InitializeFilterOptions(List<DepartamentMedical> departamenteMedicale)
    {
        // Departament filter options - din DB, nu enum-uri
        DepartmentFilterOptions = new List<FilterOption<string?>>
        {
            new FilterOption<string?>(null, "Toate departamentele")
        };

        // Grupeaza departamentele unice dupa nume
        var uniqueDepartments = departamenteMedicale
            .Where(d => !string.IsNullOrWhiteSpace(d.Nume))
            .GroupBy(d => d.Nume)
            .Select(g => g.First())
            .OrderBy(d => d.Nume);

        foreach (var dept in uniqueDepartments)
        {
            DepartmentFilterOptions.Add(new FilterOption<string?>(dept.Nume, dept.DisplayName));
        }

        // Pozitie filter options - din enum-uri (singura exceptie pentru PersonalMedical)
        PozitieFilterOptions = new List<FilterOption<PozitiePersonalMedical?>>
        {
            new FilterOption<PozitiePersonalMedical?>(null, "Toate pozitiile")
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

        // Pozitie filter
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
            Pozitie = PozitiePersonalMedical.AsistentMedical // Pozitie implicita
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
    /// Helper classes pentru filtrarea si afisarea datelor
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

    /// <summary>
    /// Metode helper pentru validari si business logic specifice medicale
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
                errors.Add($"Numarul de licenta este obligatoriu pentru pozitia {model.Pozitie?.GetDisplayName()}");

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
