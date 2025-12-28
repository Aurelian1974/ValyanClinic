using System.ComponentModel.DataAnnotations;

namespace ValyanClinic.Components.Pages.Administrare.Personal.Models;

public class PersonalFormModel
{
    public Guid? Id_Personal { get; set; }

    [Required(ErrorMessage = "Codul de angajat este obligatoriu")]
    [MaxLength(20, ErrorMessage = "Codul nu poate depasi 20 caractere")]
    public string Cod_Angajat { get; set; } = string.Empty;

    [Required(ErrorMessage = "Numele este obligatoriu")]
    [MaxLength(100, ErrorMessage = "Numele nu poate depasi 100 caractere")]
    public string Nume { get; set; } = string.Empty;

    [Required(ErrorMessage = "Prenumele este obligatoriu")]
    [MaxLength(100, ErrorMessage = "Prenumele nu poate depasi 100 caractere")]
    public string Prenume { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Nume_Anterior { get; set; }

    [Required(ErrorMessage = "CNP-ul este obligatoriu")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "CNP-ul trebuie sa contina exact 13 cifre")]
    public string CNP { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data nasterii este obligatorie")]
    public DateTime Data_Nasterii { get; set; } = DateTime.Today.AddYears(-30);

    [MaxLength(200)]
    public string? Locul_Nasterii { get; set; }

    [Required(ErrorMessage = "Nationalitatea este obligatorie")]
    [MaxLength(50)]
    public string Nationalitate { get; set; } = "Romana";

    [Required(ErrorMessage = "Cetatenia este obligatorie")]
    [MaxLength(50)]
    public string Cetatenie { get; set; } = "Romana";

    [RegularExpression(@"^(\+4|0)\d{9}$", ErrorMessage = "Format invalid pentru telefon")]
    public string? Telefon_Personal { get; set; }

    [RegularExpression(@"^(\+4|0)\d{9}$", ErrorMessage = "Format invalid pentru telefon")]
    public string? Telefon_Serviciu { get; set; }

    [EmailAddress(ErrorMessage = "Email invalid")]
    [MaxLength(100)]
    public string? Email_Personal { get; set; }

    [EmailAddress(ErrorMessage = "Email invalid")]
    [MaxLength(100)]
    public string? Email_Serviciu { get; set; }

    [Required(ErrorMessage = "Strada domiciliului este obligatorie")]
    [MaxLength(200)]
    public string Strada_Domiciliu { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Numar_Domiciliu { get; set; }

    [MaxLength(20)]
    public string? Bloc_Domiciliu { get; set; }

    [MaxLength(10)]
    public string? Scara_Domiciliu { get; set; }

    [MaxLength(10)]
    public string? Etaj_Domiciliu { get; set; }

    [MaxLength(10)]
    public string? Apartament_Domiciliu { get; set; }

    /// <summary>
    /// Adresa completă formatată pentru afișare (readonly computed).
    /// </summary>
    public string Adresa_Domiciliu_Complet => FormatAdresa(
        Strada_Domiciliu, Numar_Domiciliu, Bloc_Domiciliu, 
        Scara_Domiciliu, Etaj_Domiciliu, Apartament_Domiciliu);

    [Required(ErrorMessage = "Judetul domiciliului este obligatoriu")]
    [MaxLength(50)]
    public string Judet_Domiciliu { get; set; } = string.Empty;

    [Required(ErrorMessage = "Orasul domiciliului este obligatoriu")]
    [MaxLength(100)]
    public string Oras_Domiciliu { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? Cod_Postal_Domiciliu { get; set; }

    [MaxLength(200)]
    public string? Strada_Resedinta { get; set; }

    [MaxLength(20)]
    public string? Numar_Resedinta { get; set; }

    [MaxLength(20)]
    public string? Bloc_Resedinta { get; set; }

    [MaxLength(10)]
    public string? Scara_Resedinta { get; set; }

    [MaxLength(10)]
    public string? Etaj_Resedinta { get; set; }

    [MaxLength(10)]
    public string? Apartament_Resedinta { get; set; }

    /// <summary>
    /// Adresa completă formatată pentru afișare (readonly computed).
    /// </summary>
    public string? Adresa_Resedinta_Complet => string.IsNullOrEmpty(Strada_Resedinta) 
        ? null 
        : FormatAdresa(Strada_Resedinta, Numar_Resedinta, Bloc_Resedinta, 
                       Scara_Resedinta, Etaj_Resedinta, Apartament_Resedinta);

    [MaxLength(50)]
    public string? Judet_Resedinta { get; set; }

    [MaxLength(100)]
    public string? Oras_Resedinta { get; set; }

    [MaxLength(10)]
    public string? Cod_Postal_Resedinta { get; set; }

    [MaxLength(100)]
    public string? Stare_Civila { get; set; }

    [Required(ErrorMessage = "Functia este obligatorie")]
    [MaxLength(100)]
    public string Functia { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Departament { get; set; }

    [MaxLength(10)]
    public string? Serie_CI { get; set; }

    [MaxLength(20)]
    public string? Numar_CI { get; set; }

    [MaxLength(100)]
    public string? Eliberat_CI_De { get; set; }

    public DateTime? Data_Eliberare_CI { get; set; }

    public DateTime? Valabil_CI_Pana { get; set; }

    [Required(ErrorMessage = "Statusul angajatului este obligatoriu")]
    public string Status_Angajat { get; set; } = "Activ";

    public string? Observatii { get; set; }

    public bool IsEditMode => Id_Personal.HasValue;
    public string ModalTitle => IsEditMode ? "Editeaza Personal" : "Adauga Personal";

    #region Helper Methods

    /// <summary>
    /// Formatează adresa completă din componente individuale.
    /// </summary>
    private static string FormatAdresa(string? strada, string? numar, string? bloc, 
                                        string? scara, string? etaj, string? apartament)
    {
        if (string.IsNullOrWhiteSpace(strada))
            return string.Empty;

        var parts = new List<string> { strada };

        if (!string.IsNullOrWhiteSpace(numar))
            parts.Add($"Nr. {numar}");

        if (!string.IsNullOrWhiteSpace(bloc))
            parts.Add($"Bl. {bloc}");

        if (!string.IsNullOrWhiteSpace(scara))
            parts.Add($"Sc. {scara}");

        if (!string.IsNullOrWhiteSpace(etaj))
            parts.Add($"Et. {etaj}");

        if (!string.IsNullOrWhiteSpace(apartament))
            parts.Add($"Ap. {apartament}");

        return string.Join(", ", parts);
    }

    #endregion
}
