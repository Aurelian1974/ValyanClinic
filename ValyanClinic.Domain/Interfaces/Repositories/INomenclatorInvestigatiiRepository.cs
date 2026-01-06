using ValyanClinic.Domain.Entities.Investigatii;

namespace ValyanClinic.Domain.Interfaces.Repositories;

/// <summary>
/// Repository pentru nomenclatorul de Investigații Imagistice
/// </summary>
public interface INomenclatorInvestigatiiImagisticeRepository
{
    /// <summary>
    /// Obține toate investigațiile imagistice active
    /// </summary>
    Task<IEnumerable<NomenclatorInvestigatieImagistica>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține investigațiile imagistice după categorie
    /// </summary>
    Task<IEnumerable<NomenclatorInvestigatieImagistica>> GetByCategorieAsync(string categorie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Caută investigații imagistice după termen
    /// </summary>
    Task<IEnumerable<NomenclatorInvestigatieImagistica>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține o investigație după ID
    /// </summary>
    Task<NomenclatorInvestigatieImagistica?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pentru nomenclatorul de Explorări Funcționale
/// </summary>
public interface INomenclatorExplorariFuncRepository
{
    /// <summary>
    /// Obține toate explorările funcționale active
    /// </summary>
    Task<IEnumerable<NomenclatorExplorareFunc>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține explorările funcționale după categorie
    /// </summary>
    Task<IEnumerable<NomenclatorExplorareFunc>> GetByCategorieAsync(string categorie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Caută explorări funcționale după termen
    /// </summary>
    Task<IEnumerable<NomenclatorExplorareFunc>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține o explorare după ID
    /// </summary>
    Task<NomenclatorExplorareFunc?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pentru nomenclatorul de Endoscopii
/// </summary>
public interface INomenclatorEndoscopiiRepository
{
    /// <summary>
    /// Obține toate endoscopiile active
    /// </summary>
    Task<IEnumerable<NomenclatorEndoscopie>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține endoscopiile după categorie
    /// </summary>
    Task<IEnumerable<NomenclatorEndoscopie>> GetByCategorieAsync(string categorie, CancellationToken cancellationToken = default);

    /// <summary>
    /// Caută endoscopii după termen
    /// </summary>
    Task<IEnumerable<NomenclatorEndoscopie>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obține o endoscopie după ID
    /// </summary>
    Task<NomenclatorEndoscopie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
