namespace ValyanClinic.Domain.Entities;

/// <summary>
/// Entity pentru ICD-10 Favorite per medic
/// Tabel: ICD10_Favorites
/// Relație: PersonalMedical -> ICD10_Codes (Many-to-Many prin această tabelă)
/// </summary>
public class ICD10Favorite
{
    /// <summary>
    /// ID unic al favoritei
    /// </summary>
    public Guid FavoriteId { get; set; }

    /// <summary>
    /// ID-ul medicului (FK la PersonalMedical)
    /// </summary>
    public Guid PersonalID { get; set; }

    /// <summary>
    /// ID-ul codului ICD-10 (FK la ICD10_Codes)
    /// </summary>
    public Guid ICD10_ID { get; set; }

    /// <summary>
    /// Ordine de afișare (pentru sortare personalizată)
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Data adăugării la favorite
    /// </summary>
    public DateTime CreatedAt { get; set; }

    // ==================== Navigation Properties ====================

    /// <summary>
    /// Codul ICD-10 asociat
    /// </summary>
    public virtual ICD10Code? ICD10Code { get; set; }
}
