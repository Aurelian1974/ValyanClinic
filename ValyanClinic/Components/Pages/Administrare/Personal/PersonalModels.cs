using ValyanClinic.Domain.Models;
using ValyanClinic.Domain.Enums;
using PersonalModel = ValyanClinic.Domain.Models.Personal;

namespace ValyanClinic.Components.Pages.Administrare.Personal;

/// <summary>
/// Models și helper classes pentru pagina Personal
/// Similar cu UtilizatoriModels dar adaptat pentru Personal
/// </summary>
public class PersonalModels
{
    public List<PersonalModel> Personal { get; set; } = new();
    public List<PersonalModel> FilteredPersonal { get; set; } = new();
    public int[] PageSizes { get; } = { 10, 20, 50, 100 };

    // Filter options
    public List<FilterOption<Departament?>> DepartmentFilterOptions { get; set; } = new();
    public List<FilterOption<StatusAngajat?>> StatusFilterOptions { get; set; } = new();
    public List<string> ActivityPeriodOptions { get; } = new()
    {
        "", "Ultima zi", "Ultima saptamana", "Ultima luna", "Ultimele 3 luni", "Ultimul an"
    };

    public void SetPersonal(List<PersonalModel> personal)
    {
        Personal = personal;
        FilteredPersonal = personal;
    }

    public void InitializeFilterOptions()
    {
        // Departament filter options
        DepartmentFilterOptions = new List<FilterOption<Departament?>>
        {
            new FilterOption<Departament?>(null, "Toate departamentele")
        };

        foreach (Departament dept in Enum.GetValues<Departament>())
        {
            DepartmentFilterOptions.Add(new FilterOption<Departament?>(dept, GetDepartmentDisplayName(dept)));
        }

        // Status filter options
        StatusFilterOptions = new List<FilterOption<StatusAngajat?>>
        {
            new FilterOption<StatusAngajat?>(null, "Toate statusurile")
        };

        foreach (StatusAngajat status in Enum.GetValues<StatusAngajat>())
        {
            StatusFilterOptions.Add(new FilterOption<StatusAngajat?>(status, GetStatusDisplayName(status)));
        }
    }

    public List<PersonalModel> ApplyFilters(PersonalPageState state)
    {
        var filtered = Personal.AsEnumerable();

        // Text search
        if (!string.IsNullOrEmpty(state.SearchText))
        {
            var searchTerm = state.SearchText.ToLower();
            filtered = filtered.Where(p => 
                p.NumeComplet.ToLower().Contains(searchTerm) ||
                (p.Email_Personal?.ToLower().Contains(searchTerm) == true) ||
                (p.Email_Serviciu?.ToLower().Contains(searchTerm) == true) ||
                (p.Telefon_Personal?.Contains(searchTerm) == true) ||
                (p.CNP?.Contains(searchTerm) == true) ||
                (p.Cod_Angajat?.ToLower().Contains(searchTerm) == true));
        }

        // Department filter
        if (!string.IsNullOrEmpty(state.SelectedDepartment))
        {
            if (Enum.TryParse<Departament>(state.SelectedDepartment, out var dept))
            {
                filtered = filtered.Where(p => p.Departament == dept);
            }
        }

        // Status filter
        if (!string.IsNullOrEmpty(state.SelectedStatus))
        {
            if (Enum.TryParse<StatusAngajat>(state.SelectedStatus, out var status))
            {
                filtered = filtered.Where(p => p.Status_Angajat == status);
            }
        }

        FilteredPersonal = filtered.ToList();
        return FilteredPersonal;
    }

    public PersonalModel CreateNewPersonal()
    {
        return new PersonalModel
        {
            Id_Personal = Guid.NewGuid(),
            Data_Nasterii = DateTime.Now.AddYears(-25), // Default age
            Status_Angajat = StatusAngajat.Activ,
            Data_Crearii = DateTime.Now,
            Data_Ultimei_Modificari = DateTime.Now,
            Nationalitate = "Romana",
            Cetatenie = "Romana"
        };
    }

    public PersonalModel ClonePersonal(PersonalModel original)
    {
        return new PersonalModel
        {
            Id_Personal = original.Id_Personal,
            Cod_Angajat = original.Cod_Angajat,
            CNP = original.CNP,
            Nume = original.Nume,
            Prenume = original.Prenume,
            Nume_Anterior = original.Nume_Anterior,
            Data_Nasterii = original.Data_Nasterii,
            Locul_Nasterii = original.Locul_Nasterii,
            Nationalitate = original.Nationalitate,
            Cetatenie = original.Cetatenie,
            Telefon_Personal = original.Telefon_Personal,
            Telefon_Serviciu = original.Telefon_Serviciu,
            Email_Personal = original.Email_Personal,
            Email_Serviciu = original.Email_Serviciu,
            Adresa_Domiciliu = original.Adresa_Domiciliu,
            Judet_Domiciliu = original.Judet_Domiciliu,
            Oras_Domiciliu = original.Oras_Domiciliu,
            Cod_Postal_Domiciliu = original.Cod_Postal_Domiciliu,
            Adresa_Resedinta = original.Adresa_Resedinta,
            Judet_Resedinta = original.Judet_Resedinta,
            Oras_Resedinta = original.Oras_Resedinta,
            Cod_Postal_Resedinta = original.Cod_Postal_Resedinta,
            Stare_Civila = original.Stare_Civila,
            Functia = original.Functia,
            Departament = original.Departament,
            Serie_CI = original.Serie_CI,
            Numar_CI = original.Numar_CI,
            Eliberat_CI_De = original.Eliberat_CI_De,
            Data_Eliberare_CI = original.Data_Eliberare_CI,
            Valabil_CI_Pana = original.Valabil_CI_Pana,
            Status_Angajat = original.Status_Angajat,
            Observatii = original.Observatii,
            Data_Crearii = original.Data_Crearii,
            Data_Ultimei_Modificari = original.Data_Ultimei_Modificari,
            Creat_De = original.Creat_De,
            Modificat_De = original.Modificat_De
        };
    }

    private string GetDepartmentDisplayName(Departament department) => department switch
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
        _ => "Necunoscut"
    };

    private string GetStatusDisplayName(StatusAngajat status) => status switch
    {
        StatusAngajat.Activ => "Activ",
        StatusAngajat.Inactiv => "Inactiv",
        _ => status.ToString()
    };

    /// <summary>
    /// Helper classes
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
}
