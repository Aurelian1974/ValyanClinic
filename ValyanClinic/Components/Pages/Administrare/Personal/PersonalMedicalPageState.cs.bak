using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using PersonalMedicalModel = ValyanClinic.Domain.Models.PersonalMedical;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

/// <summary>
/// State management class pentru pagina PersonalMedical
/// Similar cu PersonalPageState dar adaptat pentru PersonalMedical cu departamente din DB
/// SIMPLIFIED VERSION - Removed kebab menu and advanced filtering
/// </summary>
public class PersonalMedicalPageState
{
    // Loading and error state
    public bool IsLoading { get; private set; }
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string? ErrorMessage { get; private set; }

    // Data - folosind PersonalMedical în loc de Personal
    public PersonalMedicalPagedResult? PagedResult { get; set; }
    public PersonalMedicalDropdownOptions? DropdownOptions { get; set; }

    // Modal state pentru PersonalMedical
    public bool IsModalVisible { get; set; }
    public PersonalMedicalModel? SelectedPersonalMedical { get; set; }
    public bool IsAddEditModalVisible { get; set; }
    public PersonalMedicalModel? EditingPersonalMedical { get; set; }
    public PersonalMedicalModel? SelectedPersonalMedicalForEdit { get; set; }
    public bool IsEditMode { get; set; }

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int[] PageSizes { get; } = { 10, 20, 50, 100 };

    // Filtering specific pentru PersonalMedical - BASIC ONLY
    public string? SearchText { get; set; }
    public string? SelectedDepartment { get; set; } // String pentru departamente din DB
    public PozitiePersonalMedical? SelectedPozitie { get; set; } // Enum pentru poziții
    public bool? SelectedStatus { get; set; } // Bool pentru EsteActiv

    // Departamente medicale options - DIN DB, nu enum-uri
    public List<DepartamentFilterItem> DepartmentOptions { get; set; } = new();
    
    // Poziții options - DOAR pentru poziții (enum)
    public List<PozitieFilterItem> PozitieOptions { get; } = new()
    {
        new(null, "Toate pozițiile")
    };

    // Status options pentru PersonalMedical (EsteActiv)
    public List<StatusFilterItem> StatusOptions { get; } = new()
    {
        new(null, "Toate statusurile"),
        new(true, "Activ"),
        new(false, "Inactiv")
    };

    // Computed properties - SIMPLIFIED
    public bool IsAnyFilterActive => 
        !string.IsNullOrEmpty(SearchText) || 
        !string.IsNullOrEmpty(SelectedDepartment) || 
        SelectedPozitie.HasValue ||
        SelectedStatus.HasValue;

    // Constructor pentru inițializarea pozițiilor
    public PersonalMedicalPageState()
    {
        InitializePozitieOptions();
    }

    private void InitializePozitieOptions()
    {
        foreach (PozitiePersonalMedical pozitie in Enum.GetValues<PozitiePersonalMedical>())
        {
            PozitieOptions.Add(new PozitieFilterItem(pozitie, pozitie.GetDisplayName()));
        }
    }

    // State management methods
    public void SetLoading(bool loading)
    {
        IsLoading = loading;
        if (loading) ErrorMessage = null;
    }

    public void SetError(string error)
    {
        ErrorMessage = error;
        IsLoading = false;
    }

    public void ClearError()
    {
        ErrorMessage = null;
    }

    public void ClearFilters()
    {
        SearchText = null;
        SelectedDepartment = null;
        SelectedPozitie = null;
        SelectedStatus = null;
        CurrentPage = 1;
    }

    // Departament options management - CRUCIAL pentru PersonalMedical
    public void SetDepartmentOptions(List<DepartamentMedical> departamenteMedicale)
    {
        DepartmentOptions.Clear();
        DepartmentOptions.Add(new DepartamentFilterItem("", "Toate departamentele"));

        // Grupează departamentele după nume pentru a evita duplicatele
        var uniqueDepartments = departamenteMedicale
            .Where(d => !string.IsNullOrWhiteSpace(d.Nume))
            .GroupBy(d => d.Nume)
            .Select(g => g.First())
            .OrderBy(d => d.Nume);

        foreach (var dept in uniqueDepartments)
        {
            DepartmentOptions.Add(new DepartamentFilterItem(dept.Nume, dept.DisplayName));
        }
    }

    // Modal management pentru PersonalMedical
    public string GetModalTitle() => IsEditMode ? "Editează Personal Medical" : "Adaugă Personal Medical Nou";
    public string GetModalSubtitle() => IsEditMode 
        ? $"Modifică informațiile pentru {EditingPersonalMedical?.NumeComplet}"
        : "Completează formularul pentru a adăuga personal medical nou";

    // Metode pentru business logic specific PersonalMedical
    public bool NeedsDepartmentFromDatabase => true; // Întotdeauna pentru PersonalMedical
    public bool SupportsSpecializari => true;
    public bool RequiresLicensaMedicala => SelectedPozitie?.RequiresLicensaMedicala() == true;
}

/// <summary>
/// Helper classes pentru dropdown-uri - specifice PersonalMedical
/// </summary>
public class DepartamentFilterItem
{
    public DepartamentFilterItem(string value, string text)
    {
        Value = value;
        Text = text;
    }

    public string Value { get; set; }
    public string Text { get; set; }
}

public class PozitieFilterItem
{
    public PozitieFilterItem(PozitiePersonalMedical? value, string text)
    {
        Value = value;
        Text = text;
    }

    public PozitiePersonalMedical? Value { get; set; }
    public string Text { get; set; }
}

public class StatusFilterItem
{
    public StatusFilterItem(bool? value, string text)
    {
        Value = value;
        Text = text;
    }

    public bool? Value { get; set; }
    public string Text { get; set; }
}

/// <summary>
/// Models pentru rezultate paginare - PersonalMedical specific
/// </summary>
public class PersonalMedicalPagedResult
{
    public IEnumerable<PersonalMedicalModel> Data { get; set; } = new List<PersonalMedicalModel>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Models pentru dropdown options - PersonalMedical specific
/// </summary>
public class PersonalMedicalDropdownOptions
{
    public List<DepartamentMedical> DepartamenteMedicale { get; set; } = new();
    public List<DepartamentMedical> Categorii { get; set; } = new();
    public List<DepartamentMedical> Specializari { get; set; } = new();
    public List<DepartamentMedical> Subspecializari { get; set; } = new();

    // Computed properties pentru verificări
    public bool HasDepartamenteMedicale => DepartamenteMedicale.Any();
    public bool HasCategorii => Categorii.Any();
    public bool HasSpecializari => Specializari.Any();
    public bool HasSubspecializari => Subspecializari.Any();

    // Metode pentru căutare și găsirea elementelor
    public DepartamentMedical? FindDepartamentByName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        return DepartamenteMedicale.FirstOrDefault(d => 
            string.Equals(d.Nume, name, StringComparison.OrdinalIgnoreCase));
    }

    public DepartamentMedical? FindCategorieById(Guid? id)
    {
        if (!id.HasValue) return null;
        return Categorii.FirstOrDefault(c => c.DepartamentID == id.Value);
    }

    public DepartamentMedical? FindSpecializareById(Guid? id)
    {
        if (!id.HasValue) return null;
        return Specializari.FirstOrDefault(s => s.DepartamentID == id.Value);
    }

    public DepartamentMedical? FindSubspecializareById(Guid? id)
    {
        if (!id.HasValue) return null;
        return Subspecializari.FirstOrDefault(s => s.DepartamentID == id.Value);
    }
}
